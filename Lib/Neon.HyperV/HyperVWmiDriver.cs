//-----------------------------------------------------------------------------
// FILE:	    HyperVWmiDriver.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Vhd.PowerShell.Cmdlets;
using Microsoft.HyperV.PowerShell.Commands;

using Neon.Common;
using Neon.Net;
using Microsoft.HyperV.PowerShell;
using System.Runtime.CompilerServices;
using Microsoft.Virtualization.Client.Management;
using OpenTelemetry;

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

        private HyperVClient        client;
        private readonly TimeSpan   timeout      = TimeSpan.FromMinutes(10);
        private readonly TimeSpan   pollInterval = TimeSpan.FromSeconds(1);
        private readonly TimeSpan   waitTime     = TimeSpan.FromSeconds(0);

        /// <summary>
        /// Constructor.
        /// </summary>
        public HyperVWmiDriver(HyperVClient client)
        {
            Covenant.Requires<ArgumentNullException>(client != null, nameof(client));

            this.client = client;

            if (!NeonHelper.IsWindows)
            {
                throw new NotSupportedException($"{nameof(HyperVWmiDriver)} is only supported on Windows.");
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~HyperVWmiDriver()
        {
            Dispose();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            client = null;
        }

        /// <summary>
        /// Releases all associated resources.
        /// </summary>
        /// <param name="disposing">Pass <c>true</c> if we're disposing, <c>false</c> if we're finalizing.</param>
        private void Dispose(bool disposing)
        {
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
                var cmdletAttr          = typeof(TCmdlet).GetCustomAttribute<CmdletAttribute>();
                var cmdletName          = $"{cmdletAttr.VerbName}-{cmdletAttr.NounName}";
                var initialSessionState = InitialSessionState.Create();

                initialSessionState.Commands.Add(new SessionStateCmdletEntry(cmdletName, typeof(TCmdlet), null));

                using (var runspace = RunspaceFactory.CreateRunspace(initialSessionState))
                {
                    runspace.Open();

                    using (var powershell = PowerShell.Create(runspace))
                    {
                        powershell.Runspace = runspace;

                        powershell.AddCommand(cmdletName);
                        args.CopyArgsTo(powershell);

                        var result = powershell.Invoke();
                        var error  = powershell.Streams.Error.FirstOrDefault();

                        if (error != null)
                        {
                            throw error.Exception;
                        }

                        return result;
                    }
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
            bool        checkPointDrives = false)
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

            SetVm(machineName, processorCount: processorCount, checkpointDrives: false);
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

            Thread.Sleep(waitTime);
        }

        /// <inheritdoc/>
        public void EjectDvdDrive(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

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
                args.Add("VMDvdDrive", drive);

                Invoke<RemoveVMDvdDrive>(args);
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

            Thread.Sleep(waitTime);
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualNat> ListNats()
        {
            CheckDisposed();

            throw new NotImplementedException();
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
        public IEnumerable<VirtualNetworkAdapter> ListVmNetAdapters(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            var args = new CmdletArgs();

            args.Add("VMName", machineName);

            var adapters = new List<VirtualNetworkAdapter>();

            foreach (var rawAdapter in Invoke<GetVMNetworkAdapter>(args))
            {
                var adapter = new VirtualNetworkAdapter()
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
        public void NewNetIPAddress(string switchName, IPAddress address, NetworkCidr subnet)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(switchName), nameof(switchName));
            Covenant.Requires<ArgumentNullException>(address != null, nameof(address));
            Covenant.Requires<ArgumentNullException>(subnet != null, nameof(subnet));

            var args = new CmdletArgs();

            args.Add("IPAddress", address);
            args.Add("PrefixLength", subnet.PrefixLength);
            args.Add("InterfaceAlias", $"vEthernet ({switchName})");

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void NewNat(string switchName, NetworkCidr subnet)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void NewVhd(string drivePath, bool isDynamic, long sizeBytes, int blockSizeBytes)
        {
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
        public void NewSwitch(string switchName, string targetAdapter = null, bool @internal = false)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(switchName), nameof(switchName));

            var args = new CmdletArgs();

            args.Add("Name", switchName);

            if (targetAdapter != null)
            {
                args.Add("NetAdapterName", targetAdapter);
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
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));

            var args = new CmdletArgs();

            args.Add("Path", drivePath);

            Invoke<OptimizeVhd>(args);
        }

        /// <inheritdoc/>
        public void RemoveNat(string natName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveVm(string machineName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            WaitForVmReady(machineName);

            var args = new CmdletArgs();

            args.Add("Name", machineName);
            args.AddSwitch("Force");

            Invoke<RemoveVM>(args, waitFor: () => !ListVms().Any(vm => vm.Name == machineName));
        }

        /// <inheritdoc/>
        public void RemoveSwitch(string switchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(switchName), nameof(switchName));

            var args = new CmdletArgs();

            args.Add("NetAdapterName", switchName);

            Invoke<RemoveVMSwitch>(args, waitFor: () => !ListSwitches().Any(@switch => @switch.Name.Equals(switchName, StringComparison.InvariantCultureIgnoreCase)));
        }

        /// <inheritdoc/>
        public void ResizeVhd(string drivePath, long sizeBytes)
        {
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
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            WaitForVmReady(machineName);

            var args = new CmdletArgs();

            args.Add("Name", machineName);

            Invoke<SaveVM>(args, waitFor: () => CheckVm(machineName, VirtualMachineState.Saved));
        }

        /// <inheritdoc/>
        public void SetVm(string machineName, int? processorCount = null, long? startupMemoryBytes = null, bool? checkpointDrives = null)
        {
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

            Invoke<SetVM>(args,
                waitFor: () =>
                {
                    var vm = ListVms().First();

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
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            WaitForVmReady(machineName);

            var args = new CmdletArgs();

            args.Add("Name", machineName);

            Invoke<StartVM>(args, waitFor: () => CheckVm(machineName, VirtualMachineState.Running));
        }

        /// <inheritdoc/>
        public void StopVm(string machineName, bool turnOff = false)
        {
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

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<string> ListHostAdapters()
        {
            CheckDisposed();

            throw new NotImplementedException();
        }
    }
}
