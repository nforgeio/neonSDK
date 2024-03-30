//-----------------------------------------------------------------------------
// FILE:        HyperVWmiDriver.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Commands;
using Microsoft.PowerShell;
using Microsoft.Vhd.PowerShell.Cmdlets;

using Neon.Common;
using Neon.Net;
using Neon.Time;

namespace Neon.HyperV
{
    //-------------------------------------------------------------------------
    // IMPLEMENTATION NOTE:
    //
    // Microsoft doesn't really have a nice Hyper-V API other than the PowerShell
    // cmdlets.  Hyper-V can be controlled via WMI but I couldn't find much in the
    // way of documentation for how that works.  I was able to hack around and get
    // some things working a few months late December 2022 but I abandoned that 
    // effort in favor of a new approach described here:
    //
    //      https://github.com/jscarle/HyperV.NET
    //
    // The gist of this is to use ILSpy to decompile the Hyper-V cmdlets and adapt
    // that code here.
    //
    // Here's an explaination of how to obtain the source code for a cmdlet:
    //
    //      https://learn.microsoft.com/en-us/archive/blogs/luisdem/get-the-source-code-of-the-powershell-cmdlets
    //
    // The basic approach is to use this PowerShell command to locate the cmdlet DLL
    // that includes the code implementing a specific Hyper-V related PowerShell COMMAND:
    //
    //      (Get-Command COMMAND).dll
    //
    // and then fire up ILSpy to have a look.

    /// <summary>
    /// Uses WMI to implement <see cref="IHyperVDriver"/>.
    /// </summary>
    internal sealed class HyperVWmiDriver : IHyperVDriver
    {
        //---------------------------------------------------------------------
        // Private types

        /// <summary>
        /// Holds arguments being passed to a cmdlet.
        /// </summary>
        private struct CmdletArgs
        {
            //-----------------------------------------------------------------
            // Static members

            private static readonly object switchValue = "is-switch";

            //-----------------------------------------------------------------
            // Instance members

            private Dictionary<string, object> args = new Dictionary<string, object>();

            /// <summary>
            /// Default constructor.
            /// </summary>
            public CmdletArgs()
            {
            }

            /// <summary>
            /// Adds a switch argument.
            /// </summary>
            /// <param name="name">Specifies the switch name.</param>
            public void AddSwitch(string name)
            {
                Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name), nameof(name));

                args.Add(name, switchValue);
            }

            /// <summary>
            /// Adds a named argument.
            /// </summary>
            /// <param name="name">Specifes the argument name.</param>
            /// <param name="value">Specifies the argument value which may be <c>null</c>.</param>
            public void Add(string name, object value)
            {
                Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name), nameof(name));

                args.Add(name, value);
            }

            /// <summary>
            /// Copies the arguments to a <see cref="PowerShell"/> instance to prepare for
            /// a command execution.
            /// </summary>
            /// <param name="powershell"></param>
            public void CopyArgsTo(PowerShell powershell)
            {
                Covenant.Requires<ArgumentNullException>(powershell != null, nameof(powershell));

                foreach (var arg in args)
                {
                    if (object.ReferenceEquals(arg.Value, switchValue))
                    {
                        powershell.AddParameter(arg.Key);
                    }
                    else
                    {
                        powershell.AddParameter(arg.Key, arg.Value);
                    }
                }
            }

            /// <summary>
            /// Clears and existing arguments.
            /// </summary>
            public void Clear()
            {
                args.Clear();
            }
        }

        //---------------------------------------------------------------------
        // Implementation

        private HyperVClient                client;
        private ManagementScope             cim2Scope;
        private InitialSessionState         iss;
        private RunspacePool                rsp;
        private Dictionary<Type, string>    typeToCommand = new Dictionary<Type, string>();
        private readonly TimeSpan           timeout       = TimeSpan.FromMinutes(10);
        private readonly TimeSpan           pollInterval  = TimeSpan.FromSeconds(1);

        // $hack(jefflill):
        //
        // We're seeing problems with inserting and then immediately ejecting a
        // DVD into a virtual machine: the VM gets into a bad state where it's
        // unresponsive to subsequent commands from PowerShell/WMI or even the
        // Hyper-V Manager UI.
        //
        // This isn't something we really see in real life because we typically
        // let the VM do some other things after inserting the DVD before we
        // eject it.
        //
        // The workaround is to ensure that we wait at least 5 seconds after a
        // DVD is inserted before ejecting a drive.  This dictionary will be used
        // to keep track of the DVD insertion time (SysTime) so a subsequent 
        // eject operation can delay as necessary.
        //
        // Note that access to this can be multi-threaded so we'll acquire a
        // lock on the dictionary before manipulating it.

        private readonly ConcurrentDictionary<string, DateTime> machineToDvdInsertTime = new ConcurrentDictionary<string, DateTime>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Constructor.
        /// </summary>
        public HyperVWmiDriver(HyperVClient client)
        {
            Covenant.Requires<ArgumentNullException>(client != null, nameof(client));

            if (!NeonHelper.IsWindows)
            {
                throw new NotSupportedException($"{nameof(HyperVWmiDriver)} is only supported on Windows.");
            }

            this.client    = client;
            this.cim2Scope = new ManagementScope(@"\\.\root\StandardCimv2");

            // Configure the initial session state for the PowerShell runspaces.

            iss = InitialSessionState.Create();

            iss.ExecutionPolicy = ExecutionPolicy.Unrestricted;

            AddCommand<AddVMHardDiskDrive>(iss);
            AddCommand<DisableVMConsoleSupport>(iss);
            AddCommand<DismountVHD>(iss);
            AddCommand<GetVM>(iss);
            AddCommand<GetVMDvdDrive>(iss);
            AddCommand<GetVMHardDiskDrive>(iss);
            AddCommand<GetVMIntegrationService>(iss);
            AddCommand<GetVMNetworkAdapter>(iss);
            AddCommand<GetVMSwitch>(iss);
            AddCommand<MountVhd>(iss);
            AddCommand<NewVhd>(iss);
            AddCommand<NewVM>(iss);
            AddCommand<NewVMSwitch>(iss);
            AddCommand<OptimizeVhd>(iss);
            AddCommand<RemoveVM>(iss);
            AddCommand<RemoveVMDvdDrive>(iss);
            AddCommand<RemoveVMSwitch>(iss);
            AddCommand<ResizeVhd>(iss);
            AddCommand<SaveVM>(iss);
            AddCommand<SetVM>(iss);
            AddCommand<SetVMDvdDrive>(iss);
            AddCommand<SetVMNetworkAdapter>(iss);
            AddCommand<SetVMProcessor>(iss);
            AddCommand<StartVM>(iss);
            AddCommand<StopVM>(iss);

            // Create and configure an runspace pool.  We'll use this to pool
            // and reuse runspaces rather than creating fresh ones for every
            // command invocation, which is slow (~18 seconds each).

            rsp = RunspaceFactory.CreateRunspacePool(iss);

            // $hack(jefflill): Hardcoding pool parameters.

            rsp.CleanupInterval = TimeSpan.FromMinutes(5);
            rsp.SetMinRunspaces(1);
            rsp.SetMaxRunspaces(4);
            rsp.Open();
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~HyperVWmiDriver()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases all associated resources.
        /// </summary>
        /// <param name="disposing">Pass <c>true</c> if we're disposing, <c>false</c> if we're finalizing.</param>
        private void Dispose(bool disposing)
        {
            if (client == null)
            {
                return;
            }

            rsp?.Dispose();
            rsp = null;

            client    = null;
            cim2Scope = null;
            iss       = null;

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Ensures that the instance has not been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the instance has been disposed.</exception>
        private void CheckDisposed()
        {
            if (client == null)
            {
                throw new ObjectDisposedException(nameof(HyperVClient));
            }
        }

        /// <summary>
        /// Adds a cmdlet type to the initial session state and also adds a type/command
        /// name mapping to <see cref="typeToCommand"/>.
        /// </summary>
        /// <typeparam name="TCmdlet">Specifies the cmdlet type.</typeparam>
        /// <param name="iss">The target session state.</param>
        private void AddCommand<TCmdlet>(InitialSessionState iss)
            where TCmdlet : Cmdlet
        {
            var cmdletAttr = typeof(TCmdlet).GetCustomAttribute<CmdletAttribute>();
            var cmdletName = $"{cmdletAttr.VerbName}-{cmdletAttr.NounName}";

            iss.Commands.Add(new SessionStateCmdletEntry(cmdletName, typeof(TCmdlet), null));
            typeToCommand.Add(typeof(TCmdlet), cmdletName);
        }

        /// <summary>
        /// Invokes the <typeparamref name="TCmdlet"/> cmdlet with the parameters passed.
        /// </summary>
        /// <typeparam name="TCmdlet">Specifies the target cmdlet implementation.</typeparam>
        /// <param name="args">Specifies any arguments to be passed.</param>
        /// <param name="waitFor">
        /// Optionally specifies a predicate that needs to retunr <c>true</c> before the
        /// operation is to be considered complete.
        /// </param>
        /// <returns>The comand results.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        /// <exception cref="TimeoutException">
        /// Thrown when <paramref name="waitFor"/> is passed and doesn't return <c>true</c>
        /// within 10 minutes.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Some Hyper-V related cmdlets just initiate the operaton and don't
        /// wait until the operation completes.  This can cause Hyper-V virtual
        /// machines to transition to a state where additional operations can't
        /// be performed (even using the Hyper-V Manager user interface).
        /// </para>
        /// <para>
        /// We'll pass a <paramref name="waitFor"/> predicate in these cases that
        /// returns <c>true</c> when the operation is considered to be complete.
        /// This method calls the predicate periodically until it returnes <c>true</c>
        /// and waits up to 10 minutes for this to happe, before throwning a
        /// <see cref="TimeoutException"/>.
        /// </para>
        /// </remarks>
        private IList<PSObject> Invoke<TCmdlet>(CmdletArgs args, Func<bool> waitFor = null)
            where TCmdlet : PSCmdlet, new()
        {
            try
            {
                using (var powershell = PowerShell.Create())
                {
                    powershell.RunspacePool = rsp;

                    powershell.AddCommand(typeToCommand[typeof(TCmdlet)]);
                    args.CopyArgsTo(powershell);

                    var result = powershell.Invoke();
                    var errors = powershell.Streams.Error.FirstOrDefault();

                    if (errors != null)
                    {
                        throw errors.Exception;
                    }

                    return result;
                }
            }
            catch (Exception e)
            {
                throw new HyperVException(e);
            }
            finally
            {
                if (waitFor != null)
                {
                    NeonHelper.WaitFor(waitFor, timeout: timeout, pollInterval: pollInterval);
                }
            }
        }

        /// <summary>
        /// Executes a built-in CORE cmdlet.
        /// </summary>
        /// <param name="cmdletName">Specifies the cmdlet name.</param>
        /// <param name="args">Specifies any cmdlet arguments.</param>
        /// <returns>The cmdlet execution result.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        private IList<PSObject> Invoke(string cmdletName, CmdletArgs args)
        {
            try
            {
                using (var powershell = PowerShell.Create())
                {
                    powershell.RunspacePool = rsp;

                    powershell.AddCommand(cmdletName);
                    args.CopyArgsTo(powershell);

                    var result = powershell.Invoke();
                    var errors = powershell.Streams.Error.FirstOrDefault();

                    if (errors != null)
                    {
                        throw errors.Exception;
                    }

                    return result;
                }
            }
            catch (Exception e)
            {
                throw new HyperVException(e);
            }
        }

        /// <summary>
        /// Waits for a VM to indicate that it's ready to perform an operation.
        /// </summary>
        /// <param name="machineName">Specifies the machine name.</param>
        /// <param name="shutdownRequired">Optionally requires that the <b>Shutdown</b> integration guest service is enabled.</param>
        /// <exception cref="HyperVException">
        /// Thrown if <paramref name="shutdownRequired"/> is <c>true</c> but <b>Shutdown</b>
        /// services aren't enabled for this machine.
        /// </exception>
        private void WaitForVmReady(string machineName, bool shutdownRequired = false)
        {
            while (true)
            {
                var vm = ListVms().SingleOrDefault(vm => vm.Name.Equals(machineName, StringComparison.InvariantCultureIgnoreCase));

                if (vm == null)
                {
                    throw new HyperVException($"Virtual machine [{machineName}] does not exist.");
                }

                if (vm.Ready)
                {
                    break;
                }

                Thread.Sleep(pollInterval);
            }

            if (shutdownRequired)
            {
                var args          = new CmdletArgs();
                var shutdownReady = false;
                var vm            = ListVms().SingleOrDefault(vm => vm.Name.Equals(machineName, StringComparison.InvariantCultureIgnoreCase));

                args.Clear();
                args.Add("VMName", machineName);

                var integrationServices = (Collection<PSObject>)Invoke<GetVMIntegrationService>(args);

                foreach (var service in integrationServices)
                {
                    if ((string)service.Members["Name"].Value == "Shutdown")
                    {
                        if ((bool)service.Members["Enabled"].Value)
                        {
                            shutdownReady = true;
                            break;
                        }
                    }
                }

                if (!shutdownReady)
                {
                    throw new HyperVException($"Cannot gracefully shutdown [{machineName}] because shudown guest services are not enabled.");
                }
            }
        }

        /// <summary>
        /// Determines whether a VM exists and optionally verifies that it has a specific state.
        /// </summary>
        /// <param name="machineName">Specifies the machine name.</param>
        /// <param name="state">Optionally specifies the required state.</param>
        /// <returns><c>true</c> when the VM exists and satisfies any state constraint.</returns>
        private bool CheckVm(string machineName, VirtualMachineState? state = null)
        {
            var vm = ListVms().SingleOrDefault(vm => vm.Name.Equals(machineName, StringComparison.InvariantCultureIgnoreCase));

            if (vm == null)
            {
                return false;
            }

            if (state.HasValue)
            {
                return vm.State == state;
            }

            return true;
        }

        /// <inheritdoc/>
        public void NewVM(
            string      machineName,
            int         processorCount,
            long        startupMemoryBytes,
            int         generation       = 1,
            string      drivePath        = null,
            string      switchName       = null,
            bool        checkPointDrives = false,
            string      notes            = null)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));
            Covenant.Requires<ArgumentException>(processorCount > 0, nameof(processorCount));
            Covenant.Requires<ArgumentException>(startupMemoryBytes > 0, nameof(startupMemoryBytes));
            Covenant.Requires<ArgumentException>(generation == 1 || generation == 2, nameof(generation));

            var args = new CmdletArgs();

            args.Add("Name", machineName);
            args.Add("MemoryStartupBytes", startupMemoryBytes);
            args.Add("Generation", generation);

            if (!string.IsNullOrEmpty(drivePath))
            {
                args.Add("VHDPath", drivePath);
            }

            if (!string.IsNullOrEmpty(switchName))
            {
                args.Add("SwitchName", switchName);
            }

            Invoke<NewVM>(args, waitFor: () => CheckVm(machineName));

            // Set the processor count and disable drive checkpointing.

            SetVm(machineName, processorCount: processorCount, checkpointDrives: false, notes: notes);
        }

        /// <inheritdoc/>
        public void AddVmDrive(string machineName, string drivePath)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));

            WaitForVmReady(machineName);

            var args = new CmdletArgs();

            args.Add("VMName", machineName);
            args.Add("Path", drivePath);

            Invoke<AddVMHardDiskDrive>(args, waitFor: () => ListVmDrives(machineName).Any(drivePath => drivePath.Equals(drivePath, StringComparison.InvariantCultureIgnoreCase)));
        }

        /// <inheritdoc/>
        public void DismountVhd(string drivePath)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));

            var args = new CmdletArgs();

            args.Add("Path", drivePath);

            Invoke<DismountVHD>(args);
        }

        /// <inheritdoc/>
        public void EnableVmNestedVirtualization(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            WaitForVmReady(machineName);

            // Enable nested virtualization for the VM.

            var args = new CmdletArgs();

            args.Add("VMName", machineName);
            args.Add("ExposeVirtualizationExtensions", true);

            Invoke<SetVMProcessor>(args);

            // Enable MAC address spoofing for the VMs network adapter.

            args.Clear();
            args.Add("VMName", machineName);
            args.Add("MacAddressSpoofing", "On");

            Invoke<SetVMNetworkAdapter>(args);
        }

        /// <inheritdoc/>
        public void InsertVmDvdDrive(string machineName, string isoPath)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(isoPath), nameof(isoPath));

            WaitForVmReady(machineName);

            var args = new CmdletArgs();

            args.Add("VMName", machineName);

            var drives = Invoke<GetVMDvdDrive>(args)
                .Select(drive => drive.Members["Path"].Value)
                .Where(path => path != null);

            if (drives.Any())
            {
                throw new HyperVException($"Virtual machine [{machineName}] already has an attached DVD drive.");
            }

            args.Clear();
            args.Add("VMName", machineName);
            args.Add("Path", isoPath);

            Invoke<SetVMDvdDrive>(args,
                waitFor: () =>
                {
                    args.Clear();
                    args.Add("VMName", machineName);

                    var drives = Invoke<GetVMDvdDrive>(args)
                        .Select(drive => (string)drive.Members["Path"].Value);

                    return drives.Any(drive => drive.Equals(isoPath, StringComparison.InvariantCultureIgnoreCase));
                });

            machineToDvdInsertTime[machineName] = SysTime.Now;
        }

        /// <inheritdoc/>
        public void EjectDvdDrive(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            // $hack(jefflill):
            //
            // We need to wait at least 5 seconds since a DVD was last inserted
            // to keep the VM from transitioning into a bad state.

            if (machineToDvdInsertTime.TryGetValue(machineName, out var sysInsertTime))
            {
                var waitTime = TimeSpan.FromSeconds(5) - (SysTime.Now - sysInsertTime);

                if (waitTime > TimeSpan.Zero)
                {
                    Thread.Sleep(waitTime);
                }

                machineToDvdInsertTime.Remove(machineName, out _);
            }

            WaitForVmReady(machineName);

            var args = new CmdletArgs();

            args.Add("VMName", machineName);

            var drives = Invoke<GetVMDvdDrive>(args)
                .Where(drive => drive.Members["Path"].Value != null);

            if (!drives.Any())
            {
                // No attached DVD drives.

                return;
            }

            foreach (var drive in drives)
            {
                args.Clear();
                args.Add("VMName", machineName);
                args.Add("ControllerNumber", (int)drive.Properties["ControllerNumber"].Value);
                args.Add("ControllerLocation", (int)drive.Properties["ControllerLocation"].Value);
                args.Add("Path", null);

                Invoke<SetVMDvdDrive>(args);
            }

            // Wait until there are no remaining DVD drives attached to the VM.

            NeonHelper.WaitFor(
                () =>
                {
                    args.Clear();
                    args.Add("VMname", machineName);

                    var drives = Invoke<GetVMDvdDrive>(args)
                        .Select(drive => drive.Members["Path"].Value)
                        .Where(path => path != null);

                    return !drives.Any();
                },
                timeout:      timeout,
                pollInterval: pollInterval);
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualNat> ListNats()
        {
            CheckDisposed();

            // $note(jefflill):
            //
            // The [NetNat] cmdlets aren't working so we'll use WMI instead.

            var nats = new List<VirtualNat>();

            foreach (var rawNat in Wmi.Query("select * from MSFT_NetNat", cim2Scope))
            {
                var subnet = (string)rawNat.Properties["InternalIPInterfaceAddressPrefix"].Value;

                if (subnet == null)
                {
                    subnet = (string)rawNat.Properties["ExternalIPInterfaceAddressPrefix"].Value;
                }

                nats.Add(
                    new VirtualNat()
                    {
                        Name   = (string)rawNat.Properties["Name"].Value,
                        Subnet = subnet
                    });
            }

            return nats;
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualSwitch> ListSwitches()
        {
            CheckDisposed();

            var switches = new List<VirtualSwitch>();

            foreach (var @switch in Invoke<GetVMSwitch>(new CmdletArgs()))
            {
                switches.Add(
                    new VirtualSwitch()
                    {
                         Name = (string)@switch.Members["Name"].Value,
                         Type = (VirtualSwitchType)@switch.Members["SwitchType"].Value
                    });
            }

            return switches;
        }

        /// <inheritdoc/>
        public IEnumerable<string> ListVmDrives(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            var args = new CmdletArgs();

            args.Add("VMName", machineName);

            var drives = new List<string>();

            foreach (var drive in Invoke<GetVMHardDiskDrive>(args))
            {
                drives.Add((string)drive.Members["Path"].Value);
            }

            return drives;
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualMachineNetworkAdapter> ListVirtualMachineNetAdapters(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            var args = new CmdletArgs();

            args.Add("VMName", machineName);

            var adapters = new List<VirtualMachineNetworkAdapter>();

            foreach (var rawAdapter in Invoke<GetVMNetworkAdapter>(args))
            {
                var adapter = new VirtualMachineNetworkAdapter()
                {
                    Name           = (string)rawAdapter.Members["Name"].Value,
                    VMName         = (string)rawAdapter.Members["VMName"].Value,
                    IsManagementOs = (bool)rawAdapter.Members["IsManagementOs"].Value,
                    SwitchName     = (string)rawAdapter.Members["SwitchName"].Value,
                    MacAddress     = (string)rawAdapter.Members["MacAddress"].Value
                };

                var statusItems = (Microsoft.HyperV.PowerShell.VMNetworkAdapterOperationalStatus[])rawAdapter.Members["Status"].Value;

                if (statusItems?.Length > 0)
                {
                    adapter.Status = statusItems.First().ToString();
                }

                // Parse the IP addresses.

                var addresses = (string[])rawAdapter.Members["IPAddresses"].Value;

                if (addresses.Length > 0)
                {
                    foreach (string address in addresses)
                    {
                        if (!string.IsNullOrEmpty(address))
                        {
                            var ipAddress = IPAddress.Parse(address.Trim());

                            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                            {
                                adapter.Addresses.Add(IPAddress.Parse(address.Trim()));
                            }
                        }
                    }
                }

                adapters.Add(adapter);
            }

            return adapters;
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualMachine> ListVms()
        {
            CheckDisposed();

            var machines = new List<VirtualMachine>();

            foreach (var rawMachine in Invoke<GetVM>(new CmdletArgs()))
            {
                var vm                = new VirtualMachine();
                var operationalStatus = (VMOperationalStatus[])rawMachine.Members["OperationalStatus"].Value;

                vm.Name            = (string)rawMachine.Members["Name"].Value;
                vm.Notes           = (string)rawMachine.Members["Notes"].Value;
                vm.ProcessorCount  = (int)(long)rawMachine.Members["ProcessorCount"].Value;
                vm.MemorySizeBytes = (long)rawMachine.Members["MemoryStartup"].Value;
                vm.Ready           = operationalStatus[0] == VMOperationalStatus.Ok;
                vm.Uptime          = (TimeSpan)rawMachine.Members["Uptime"].Value;

                switch (rawMachine.Members["State"].Value.ToString())
                {
                    case "Off":

                        vm.State = VirtualMachineState.Off;
                        break;

                    case "Starting":

                        vm.State = VirtualMachineState.Starting;
                        break;

                    case "Running":

                        vm.State = VirtualMachineState.Running;
                        break;

                    case "Paused":

                        vm.State = VirtualMachineState.Paused;
                        break;

                    case "Saved":

                        vm.State = VirtualMachineState.Saved;
                        break;

                    default:

                        vm.State = VirtualMachineState.Unknown;
                        break;
                }

                // Extract the connected switch name from the first network adapter (if any).

                // $note(jefflill):
                // 
                // We don't currently support VMs with multiple network adapters and will
                // only capture the name of the switch connected to the first adapter.

                var adapters = (IList<VMNetworkAdapter>)rawMachine.Members["NetworkAdapters"].Value;

                if (adapters.Count > 0)
                {
                    vm.SwitchName = adapters[0].SwitchName;
                }

                machines.Add(vm);
            }

            return machines;
        }

        /// <inheritdoc/>
        public void MountVhd(string drivePath, bool readOnly = false)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));

            var args = new CmdletArgs();

            args.Add("Path", drivePath);

            if (readOnly)
            {
                args.AddSwitch("ReadOnly");
            }

            Invoke<MountVhd>(args);
        }

        /// <inheritdoc/>
        public void NewNetIPAddress(string switchName, IPAddress gatewayAddress, NetworkCidr subnet)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(switchName), nameof(switchName));
            Covenant.Requires<ArgumentNullException>(gatewayAddress != null, nameof(gatewayAddress));
            Covenant.Requires<ArgumentNullException>(subnet != null, nameof(subnet));

            var @switch = ListSwitches().SingleOrDefault(@switch => @switch.Name.Equals(switchName, StringComparison.InvariantCultureIgnoreCase));

            if (@switch == null)
            {
                throw new HyperVException($"Switch [{switchName}] does not exist.");
            }

            var adapterName = $"vEthernet ({switchName})";
            var adapter     = ListNetAdapters().SingleOrDefault(adapter => adapter.Name.Equals(adapterName, StringComparison.InvariantCultureIgnoreCase));

            if (adapter == null)
            {
                throw new HyperVException($"Host network adapter for switch [{switchName}] cannot be located.");
            }

            // $note(jefflill):
            //
            // The [NetTcpIp] cmdlets aren't working so we'll use WMI instead.

            var newIPAddressClass = new ManagementClass($"{cim2Scope.Path}:MSFT_NetIPAddress");
            var netIPAddress      = newIPAddressClass.CreateInstance();

            netIPAddress.Properties["AddressFamily"].Value  = (ushort)2;                     // IPv4
            netIPAddress.Properties["AddressOrigin"].Value  = (ushort)0;                     // Unknown
            netIPAddress.Properties["InterfaceIndex"].Value = (uint)adapter.InterfaceIndex;
            netIPAddress.Properties["IPv4Address"].Value    = gatewayAddress;
            netIPAddress.Properties["PrefixLength"].Value   = (byte)subnet.PrefixLength;
            netIPAddress.Properties["PrefixOrigin"].Value   = 1;                            // Manual
            netIPAddress.Properties["SuffixOrigin"].Value   = 1;                            // Manual
            netIPAddress.Properties["ProtocolIFType"].Value = (ushort)4096;                 // IPv4
            netIPAddress.Properties["SkipAsSource"].Value   = false;
            netIPAddress.Properties["Type"].Value           = 1;                            // Unicast

            try
            {
                netIPAddress.Put();
            }
            catch (Exception e)
            {
                throw new HyperVException(e);
            }
        }

        /// <inheritdoc/>
        public void NewNat(string natName, bool @internal, NetworkCidr subnet)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(natName), nameof(natName));
            Covenant.Requires<ArgumentNullException>(subnet != null, nameof(subnet));

            // Verify that the NAT doesn't already exist or that no other NAT 
            // is associated with the subnet.

            var existingNats = ListNats();

            if (existingNats.Any(nat => nat.Name.Equals(natName, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new HyperVException($"NetNat [{natName}] already exists.");
            }

            foreach (var nat in existingNats)
            {
                if (nat.Subnet == subnet)
                {
                    throw new HyperVException($"Subnet [{subnet}] is already assocated with existing NetNat [{nat.Name}]. ");
                }
            }

            // $note(jefflill):
            //
            // The [NetNat] cmdlets aren't working so we'll use WMI instead.

            var netNatClass = new ManagementClass($"{cim2Scope.Path}:MSFT_NetNat");
            var netNat      = netNatClass.CreateInstance();

            netNat.Properties["Name"].Value                            = natName;
            netNat.Properties["IcmpQueryTimeout"].Value                = (uint)30;
            netNat.Properties["TcpEstablishedConnectionTimeout"].Value = (uint)1800;
            netNat.Properties["TcpTransientConnectionTimeout"].Value   = (uint)120;
            netNat.Properties["TcpFilteringBehavior"].Value            = (byte)1;       // AddressDependentFiltering
            netNat.Properties["UdpFilteringBehavior"].Value            = (byte)1;       // AddressDependentFiltering
            netNat.Properties["UdpIdleSessionTimeout"].Value           = (uint)120;
            netNat.Properties["UdpInboundRefresh"].Value               = false;
            netNat.Properties["Active"].Value                          = true;

            if (@internal)
            {
                netNat.Properties["InternalIPInterfaceAddressPrefix"].Value = subnet.ToString();
            }
            else
            {
                netNat.Properties["ExternalIPInterfaceAddressPrefix"].Value = subnet.ToString();
            }

            try
            {
                netNat.Put();
            }
            catch (Exception e)
            {
                throw new HyperVException(e);
            }
        }

        /// <inheritdoc/>
        public void NewVhd(string drivePath, bool isDynamic, long sizeBytes, int blockSizeBytes)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));
            Covenant.Requires<ArgumentException>(sizeBytes > 0, nameof(sizeBytes));
            Covenant.Requires<ArgumentException>(blockSizeBytes > 0, nameof(blockSizeBytes));

            var args = new CmdletArgs();

            if (isDynamic)
            {
                args.AddSwitch("Dynamic");
            }
            else
            {
                args.AddSwitch("Fixed");
            }

            args.Add("Path", drivePath);
            args.Add("SizeBytes", sizeBytes);
            args.Add("BlockSizeBytes", blockSizeBytes);

            Invoke<NewVhd>(args);
        }

        /// <inheritdoc/>
        public void NewSwitch(string switchName, NetAdapter hostAdapter = null, bool @internal = false)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(switchName), nameof(switchName));

            if (ListSwitches().Any(@switch => @switch.Name.Equals(switchName, StringComparison.InvariantCultureIgnoreCase)))
            {
                return; // Looks like the switch already exists.
            }

            var args = new CmdletArgs();

            args.Add("Name", switchName);

            if (hostAdapter != null)
            {
                args.Add("NetAdapterName", hostAdapter.Name);
            }

            if (@internal)
            {
                args.Add("SwitchType", "Internal");
            }

            Invoke<NewVMSwitch>(args);
        }

        /// <inheritdoc/>
        public void OptimizeVhd(string drivePath)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));

            var args = new CmdletArgs();

            args.Add("Path", drivePath);

            Invoke<OptimizeVhd>(args);
        }

        /// <inheritdoc/>
        public void RemoveNat(string natName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(natName), nameof(natName));

            // $note(jefflill):
            //
            // The [NetNat] cmdlets aren't working so we'll use WMI instead.

            var rawNat = Wmi.Query($"select * from MSFT_NetNat where Name = '{natName}'", cim2Scope).SingleOrDefault();

            rawNat?.Delete();
        }

        /// <inheritdoc/>
        public void RemoveVm(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            WaitForVmReady(machineName);

            var args = new CmdletArgs();

            args.Add("Name", machineName);
            args.AddSwitch("Force");

            machineToDvdInsertTime.Remove(machineName, out _);

            Invoke<RemoveVM>(args, waitFor: () => !ListVms().Any(vm => vm.Name == machineName));
        }

        /// <inheritdoc/>
        public void RemoveSwitch(string switchName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(switchName), nameof(switchName));

            var args = new CmdletArgs();

            args.Add("Name", switchName);
            // args.Add("NetAdapterName", switchName);
            args.AddSwitch("-Force");

            Invoke<RemoveVMSwitch>(args, waitFor: () => !ListSwitches().Any(@switch => @switch.Name.Equals(switchName, StringComparison.InvariantCultureIgnoreCase)));
        }

        /// <inheritdoc/>
        public void ResizeVhd(string drivePath, long sizeBytes)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));
            Covenant.Requires<ArgumentException>(sizeBytes > 0, nameof(sizeBytes));

            var args = new CmdletArgs();

            args.Add("Path", drivePath);
            args.Add("SizeBytes", sizeBytes);

            Invoke<ResizeVhd>(args);
        }

        /// <inheritdoc/>
        public void SaveVm(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            WaitForVmReady(machineName);

            var args = new CmdletArgs();

            args.Add("Name", machineName);

            Invoke<SaveVM>(args, waitFor: () => CheckVm(machineName, VirtualMachineState.Saved));
        }

        /// <inheritdoc/>
        public void SetVm(string machineName, int? processorCount = null, long? startupMemoryBytes = null, bool? checkpointDrives = null, string notes = null)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            WaitForVmReady(machineName);

            var args = new CmdletArgs();

            args.Add("Name", machineName);
            args.AddSwitch("StaticMemory");

            if (processorCount.HasValue)
            {
                args.Add("ProcessorCount", processorCount);
            }

            if (startupMemoryBytes.HasValue)
            {
                args.Add("MemoryStartupBytes", startupMemoryBytes);
            }

            if (checkpointDrives.HasValue)
            {
                if (checkpointDrives.Value)
                {
                    args.Add("CheckpointType", "Enabled");
                }
                else
                {
                    args.Add("CheckpointType", "Disabled");
                }
            }

            if (!string.IsNullOrEmpty(notes))
            {
                args.Add("Notes", notes);
            }

            Invoke<SetVM>(args,
                waitFor: () =>
                {
                    var vm = ListVms().First(vm => vm.Name.Equals(machineName, StringComparison.InvariantCultureIgnoreCase));

                    if (processorCount.HasValue && vm.ProcessorCount != processorCount.Value)
                    {
                        return false;
                    }

                    if (startupMemoryBytes.HasValue && vm.MemorySizeBytes != startupMemoryBytes.Value)
                    {
                        return false;
                    }

                    // $hack(jefflill): Do we need to verify [checkpointDrives] too?

                    return true;
                });
        }

        /// <inheritdoc/>
        public void StartVm(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            WaitForVmReady(machineName);

            var args = new CmdletArgs();

            args.Add("Name", machineName);

            Invoke<StartVM>(args, waitFor: () => CheckVm(machineName, VirtualMachineState.Running));
        }

        /// <inheritdoc/>
        public void StopVm(string machineName, bool turnOff = false)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            // $note(jefflill):
            //
            // It appears that shutdown commands sent to the VM may be ignored
            // when guest integration services haven't started yet.  This can
            // happen when these aren't enabled or when the machine has just
            // booted and these services aren't running yet.
            //
            // We're going to handle this by:
            //
            //  * passing [shutdownRequired=true] to [WaitForVmReady()]
            //    which throws an exception if shutdown isn't enabled on
            //    the host side.
            //
            //  * Retry sending shutdowns for up to 2 minutes when the VM
            //    doesn't respond, giving the guest services a chance to
            //    start.  We assume here that potentially invoking a shutdown
            //    multiple times will be OK.
            //
            // Note that we don't need to retry when we're turning a VM off.

            WaitForVmReady(machineName, shutdownRequired: !turnOff);

            var args = new CmdletArgs();

            args.Add("Name", machineName);

            if (turnOff)
            {
                args.AddSwitch("TurnOff");
                Invoke<StopVM>(args, waitFor: () => CheckVm(machineName, VirtualMachineState.Off));
                return;
            }

            var stopwatch = new Stopwatch();
            var bootLimit = TimeSpan.FromMinutes(2);

            stopwatch.Start();

            while (true)
            {
                args.AddSwitch("Force");

                Invoke<StopVM>(args);
                Thread.Sleep(pollInterval);

                if (CheckVm(machineName, VirtualMachineState.Off) || stopwatch.Elapsed >= bootLimit)
                {
                    break;
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualIPAddress> ListIPAddresses()
        {
            CheckDisposed();

            // $note(jefflill):
            //
            // Invoking [NetTCPIP\Get-NetIPAddress] returns an empty list so
            // will make a low-level WMI call instead.

            var addresses   = new List<VirtualIPAddress>();
            var switchRegex = new Regex(@"^.*\((?<switch>.+)\)$");

            foreach (var rawAddress in Wmi.Query("select * from MSFT_NetIPAddress", cim2Scope))
            {
                // We're only listing IPv4  addresses.

                var addressFamily = (ushort)rawAddress.Properties["AddressFamily"].Value;

                if (addressFamily != 2) // IPv4
                {
                    continue;
                }

                var address = (string)rawAddress.Properties["IPv4Address"].Value;

                if (string.IsNullOrEmpty(address))
                {
                    continue;
                }

                // Extract the interface/switch name from the [InterfaceAlias] field,
                // which will look something like:
                //
                //      vEthernet (neonkube)
                //
                // We'll extract the name within the parens if present, otherwise we'll
                // take the entire property value as the name.

                var interfaceAlias = (string)rawAddress.Properties["InterfaceAlias"].Value;
                var match          = switchRegex.Match(interfaceAlias);
                var interfaceName  = string.Empty;

                if (match.Success)
                {
                    interfaceName = match.Groups["switch"].Value;
                }
                else
                {
                    interfaceName = interfaceAlias;
                }

                var prefixLength = rawAddress.Properties["PrefixLength"].Value;

                var virtualIPAddress
                    = new VirtualIPAddress()
                    {
                        Address        = address,
                        Subnet         = NetworkCidr.Parse($"{address}/{prefixLength}"),
                        InterfaceName  = interfaceName
                    };

                    addresses.Add(virtualIPAddress);
            }

            return addresses;
        }

        /// <inheritdoc/>
        public IEnumerable<NetAdapter> ListNetAdapters()
        {
            CheckDisposed();

            // $note(jefflill):
            //
            // Invoking [NetAdapter\Get-NetAdapter] returns an empty list so
            // will make a low-level WMI call instead.

            var adapters = new List<NetAdapter>();

            foreach (var rawAdapter in Wmi.Query("select * from MSFT_NetAdapter", cim2Scope))
            {
                adapters.Add(
                    new NetAdapter()
                    {
                        Name           = (string)rawAdapter["Name"],
                        InterfaceIndex = (int)(uint)rawAdapter["InterfaceIndex"]
                    });
            }

            return adapters;
        }
    }
}
