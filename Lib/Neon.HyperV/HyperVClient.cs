//-----------------------------------------------------------------------------
// FILE:        HyperVClient.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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

using Microsoft.Win32;
using Neon.Common;
using Neon.Net;
using Neon.Retry;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace Neon.HyperV
{
    /// <summary>
    /// <para>
    /// Abstracts management of local Hyper-V virtual machines and components
    /// on Windows via PowerShell.
    /// </para>
    /// <note>
    /// This class requires elevated administrative rights.
    /// </note>
    /// </summary>
    /// <threadsafety instance="false"/>
    public class HyperVClient : IDisposable
    {
        //---------------------------------------------------------------------
        // Static members

        /// <summary>
        /// Returns the path to the user's default Hyper-V virtual drive folder.
        /// </summary>
        public static string DefaultDriveFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Hyper-V", "Virtual hard disks");

        //---------------------------------------------------------------------
        // Instance members

        private IHyperVDriver     hypervDriver;

        /// <summary>
        /// Default constructor to be used to manage Hyper-V objects
        /// on the local Windows machine.
        /// </summary>
        /// <param name="driverType">
        /// Optionally overrides the default Hyper-V driver implementation. This
        /// defaults to <see cref="HyperVDriverType.Wmi"/> and is generally overridden
        /// only by unit tests.
        /// </param>
        public HyperVClient(HyperVDriverType driverType = HyperVDriverType.Wmi)
        {
            if (!NeonHelper.IsWindows)
            {
                throw new NotSupportedException($"{nameof(HyperVClient)} is only supported on Windows.");
            }

            switch (driverType)
            {
                case HyperVDriverType.PowerShell:

                    hypervDriver = new HyperVPowershellDriver(this);
                    break;

                case HyperVDriverType.Wmi:

                    hypervDriver = new HyperVWmiDriver(this);
                    break;

                default:

                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Releases all resources associated with the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases all associated resources.
        /// </summary>
        /// <param name="disposing">Pass <c>true</c> if we're disposing, <c>false</c> if we're finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                hypervDriver?.Dispose();
                hypervDriver = null;

                GC.SuppressFinalize(this);
            }

            hypervDriver = null;
        }

        /// <summary>
        /// Ensures that the instance has not been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the instance has been disposed.</exception>
        private void CheckDisposed()
        {
            if (hypervDriver == null)
            {
                throw new ObjectDisposedException(nameof(HyperVClient));
            }
        }

        /// <summary>
        /// Determines whether the current machine is already running as a Hyper-V
        /// virtual machine and that any Hyper-V VMs deployed on this machine can
        /// be considered to be nested.
        /// </summary>
        /// <remarks>
        /// <para>
        /// We use the presence of this registry value to detect VM nesting:
        /// </para>
        /// <example>
        /// HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Virtual Machine\Auto\OSName
        /// </example>
        /// </remarks>
#pragma warning disable CA1416
        public bool IsNestedVirtualization => Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Virtual Machine\Auto", "OSName", null) != null;
#pragma warning restore CA1416

        /// <summary>
        /// Creates a virtual machine. 
        /// </summary>
        /// <param name="machineName">The machine name.</param>
        /// <param name="memorySize">
        /// A string specifying the memory size.  This can be a long byte count or a
        /// byte count or a number with units like <b>512MiB</b>, <b>0.5GiB</b>, <b>2GiB</b>, 
        /// or <b>1TiB</b>.  This defaults to <b>2GiB</b>.
        /// </param>
        /// <param name="processorCount">
        /// The number of virutal processors to assign to the machine.  This defaults to <b>4</b>.
        /// </param>
        /// <param name="driveSize">
        /// A string specifying the primary disk size.  This can be a long byte count or a
        /// byte count or a number with units like <b>512MB</b>, <b>0.5GiB</b>, <b>2GiB</b>, 
        /// or <b>1TiB</b>.  Pass <c>null</c> to leave the disk alone.  This defaults to <c>null</c>.
        /// </param>
        /// <param name="drivePath">
        /// Optionally specifies the path where the virtual hard drive will be located.  Pass 
        /// <c>null</c> or empty to default to <b>MACHINE-NAME.vhdx</b> located in the default
        /// Hyper-V virtual machine drive folder.
        /// </param>
        /// <param name="checkpointDrives">Optionally enables drive checkpoints.  This defaults to <c>false</c>.</param>
        /// <param name="templateDrivePath">
        /// If this is specified and <paramref name="drivePath"/> is not <c>null</c> then
        /// the hard drive template at <paramref name="templateDrivePath"/> will be copied
        /// to <paramref name="drivePath"/> before creating the machine.
        /// </param>
        /// <param name="switchName">Optional name of the virtual switch.</param>
        /// <param name="extraDrives">
        /// Optionally specifies any additional virtual drives to be created and 
        /// then attached to the new virtual machine.
        /// </param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        /// <remarks>
        /// <note>
        /// The <see cref="VirtualDrive.Path"/> property of <paramref name="extraDrives"/> may be
        /// passed as <c>null</c> or empty.  In this case, the drive location will default to
        /// being located in the standard Hyper-V virtual drivers folder and will be named
        /// <b>MACHINE-NAME-#.vhdx</b>, where <b>#</b> is the one-based index of the drive
        /// in the enumeration.
        /// </note>
        /// </remarks>
        public void AddVm(
            string                      machineName, 
            string                      memorySize        = "2GiB", 
            int                         processorCount    = 4,
            string                      driveSize         = null,
            string                      drivePath         = null,
            bool                        checkpointDrives  = false,
            string                      templateDrivePath = null, 
            string                      switchName        = null,
            IEnumerable<VirtualDrive>   extraDrives       = null)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(memorySize), nameof(memorySize));
            Covenant.Requires<ArgumentException>(processorCount > 0, nameof(processorCount));

            if (templateDrivePath != null && !File.Exists(templateDrivePath))
            {
                throw new HyperVException($"Virtual machine drive template [{templateDrivePath}] does not exist.");
            }

            if (VmExists(machineName))
            {
                throw new HyperVException($"Virtual machine [{machineName}] already exists.");
            }

            memorySize = ByteUnits.Parse(memorySize).ToString();

            if (driveSize != null)
            {
                driveSize = ByteUnits.Parse(driveSize).ToString();
            }

            var driveFolder = DefaultDriveFolder;

            if (string.IsNullOrEmpty(drivePath))
            {
                drivePath = Path.Combine(driveFolder, $"{machineName}-[0].vhdx");
            }
            else
            {
                driveFolder = Path.GetDirectoryName(Path.GetFullPath(drivePath));
            }

            if (VmExists(machineName))
            {
                throw new HyperVException($"Virtual machine [{machineName}] already exists.");
            }

            // Copy the template VHDX file.

            if (templateDrivePath != null)
            {
                File.Copy(templateDrivePath, drivePath);
            }

            // Resize the VHDX if requested.

            if (driveSize != null)
            {
                hypervDriver.ResizeVhd(drivePath, ((long)ByteUnits.Parse(driveSize)));
            }

            // Create the virtual machine.

            hypervDriver.NewVM(
                machineName, 
                processorCount:     processorCount, 
                startupMemoryBytes: (long)ByteUnits.Parse(memorySize), 
                generation:         1, 
                drivePath:          drivePath,
                switchName:         switchName, 
                checkpointDrives:   checkpointDrives);

            // We need to do some extra configuration for nested virtual machines:
            //
            //      https://docs.microsoft.com/en-us/virtualization/hyper-v-on-windows/user-guide/nested-virtualization

            if (IsNestedVirtualization)
            {
                hypervDriver.EnableVmNestedVirtualization(machineName);
            }

            // Create and attach any additional drives as required.

            if (extraDrives != null)
            {
                var diskNumber = 1;

                foreach (var drive in extraDrives)
                {
                    if (string.IsNullOrEmpty(drive.Path))
                    {
                        drive.Path = Path.Combine(driveFolder, $"{machineName}-[{diskNumber}].vhdx");
                    }

                    if (drive.Size <= 0)
                    {
                        throw new ArgumentException("Virtual drive size must be greater than 0.", nameof(drive));
                    }

                    NeonHelper.DeleteFile(drive.Path);

                    hypervDriver.NewVhd(drivePath, drive.IsDynamic, (long)drive.Size, (int)ByteUnits.MebiBytes);
                    hypervDriver.AddVmDrive(machineName, drivePath);

                    diskNumber++;
                }
            }
        }

        /// <summary>
        /// Removes a named virtual machine and all of its drives (by default).
        /// </summary>
        /// <param name="machineName">The machine name.</param>
        /// <param name="keepDrives">Optionally retains the VM disk files.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public void RemoveVm(string machineName, bool keepDrives = false)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            var vm = FindVm(machineName);

            if (vm == null)
            {
                throw new HyperVException($"Virtual machine [{machineName}] does not exist.");
            }

            // Only non-running VMs may be removed.

            switch (vm.State)
            {
                case VirtualMachineState.Running:
                case VirtualMachineState.Starting:

                    throw new HyperVException($"Cannot remove running or starting virtual machine: {machineName}");
            }

            // Remove the machine along with any of of its virtual hard drive files.

            var drives = ListVmDrives(machineName);

            hypervDriver.RemoveVm(machineName);

            if (!keepDrives)
            {
                foreach (var drivePath in drives)
                {
                    NeonHelper.DeleteFile(drivePath);
                }
            }
        }

        /// <summary>
        /// Stops and removes all virtual machines whose name includes a prefix.
        /// </summary>
        /// <param name="namePrefix">Specifies the name prefix.</param>
        public void RemoveVmsWithPrefix(string namePrefix)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(namePrefix), nameof(namePrefix));

            foreach (var vm in ListVms()
                .Where(vm => vm.Name.StartsWith(namePrefix)))
            {
                if (vm.State == VirtualMachineState.Running || vm.State == VirtualMachineState.Starting)
                {
                    StopVm(vm.Name);
                }

                RemoveVm(vm.Name);
            }
        }

        /// <summary>
        /// Lists the virtual machines.
        /// </summary>
        /// <returns><see cref="IEnumerable{VirtualMachine}"/>.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public IEnumerable<VirtualMachine> ListVms()
        {
            CheckDisposed();

            return hypervDriver.ListVms();
        }

        /// <summary>
        /// Gets the current status for a named virtual machine.
        /// </summary>
        /// <param name="machineName">The machine name.</param>
        /// <returns>The <see cref="VirtualMachine"/> or <c>null</c> when the virtual machine doesn't exist.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public VirtualMachine FindVm(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            return ListVms().SingleOrDefault(vm => vm.Name.Equals(machineName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Determines whether a named virtual machine exists.
        /// </summary>
        /// <param name="machineName">The machine name.</param>
        /// <returns><c>true</c> if the machine exists.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public bool VmExists(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            return ListVms().Count(vm => vm.Name.Equals(machineName, StringComparison.InvariantCultureIgnoreCase)) > 0;
        }

        /// <summary>
        /// Starts the named virtual machine.
        /// </summary>
        /// <param name="machineName">The machine name.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public void StartVm(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            if (!VmExists(machineName))
            {
                throw new HyperVException($"Virtual machine [{machineName}] does not exist.");
            }

            hypervDriver.StartVm(machineName);
        }

        /// <summary>
        /// Stops the named virtual machine.
        /// </summary>
        /// <param name="machineName">The machine name.</param>
        /// <param name="turnOff">
        /// <para>
        /// Optionally just turns the VM off without performing a graceful shutdown first.
        /// </para>
        /// <note>
        /// <b>WARNING!</b> This could result in corruption or the the loss of unsaved data.
        /// </note>
        /// </param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public void StopVm(string machineName, bool turnOff = false)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            if (!VmExists(machineName))
            {
                throw new HyperVException($"Virtual machine [{machineName}] does not exist.");
            }

            hypervDriver.StopVm(machineName, turnOff: turnOff);
        }

        /// <summary>
        /// Persists the state of a running virtual machine and then stops it.  This is 
        /// equivalent to hibernation for a physical machine.
        /// </summary>
        /// <param name="machineName">The machine name.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public void SaveVm(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            if (!VmExists(machineName))
            {
                throw new HyperVException($"Virtual machine [{machineName}] does not exist.");
            }

            hypervDriver.SaveVm(machineName);
        }

        /// <summary>
        /// Returns host file system paths to any virtual drives attached to a virtual machine.
        /// </summary>
        /// <param name="machineName">The machine name.</param>
        /// <returns>The list of fully qualified virtual drive file paths.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public IEnumerable<string> ListVmDrives(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            if (!VmExists(machineName))
            {
                throw new HyperVException($"Virtual machine [{machineName}] does not exist.");
            }

            return hypervDriver.ListVmDrives(machineName);
        }

        /// <summary>
        /// Creates a new virtual drive and adds it to a virtual machine.
        /// </summary>
        /// <param name="machineName">The target virtual machine name.</param>
        /// <param name="drive">The new drive information.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public void AddVmDrive(string machineName, VirtualDrive drive)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));
            Covenant.Requires<ArgumentNullException>(drive != null, nameof(drive));

            if (!VmExists(machineName))
            {
                throw new HyperVException($"Virtual machine [{machineName}] does not exist.");
            }

            // Delete the drive file if it already exists.

            NeonHelper.DeleteFile(drive.Path);

            // Create and attach the drive.

            hypervDriver.NewVhd(drive.Path, drive.IsDynamic, (long)drive.Size, (int)ByteUnits.MebiBytes);
            hypervDriver.AddVmDrive(machineName, drive.Path);
        }

        /// <summary>
        /// <para>
        /// Compacts a dynamic VHD or VHDX virtual disk file.
        /// </para>
        /// <note>
        /// The disk may be mounted to a VM but the VM cannot be running.
        /// </note>
        /// </summary>
        /// <param name="drivePath">Path to the virtual drive file.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public void CompactDrive(string drivePath)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));

            hypervDriver.OptimizeVhd(drivePath);
        }

        /// <summary>
        /// Inserts an ISO file as the DVD for a virtual machine, ejecting any
        /// existing disc first.
        /// </summary>
        /// <param name="machineName">The machine name.</param>
        /// <param name="isoPath">Path to the DVD ISO file.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public void InsertVmDvd(string machineName, string isoPath)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(isoPath), nameof(isoPath));

            if (!VmExists(machineName))
            {
                throw new HyperVException($"Virtual machine [{machineName}] does not exist.");
            }

            EjectVmDvd(machineName);
            hypervDriver.InsertVmDvdDrive(machineName, isoPath);
        }

        /// <summary>
        /// Ejects any DVD that's currently inserted into a virtual machine.
        /// </summary>
        /// <param name="machineName">The machine name.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public void EjectVmDvd(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            if (!VmExists(machineName))
            {
                throw new HyperVException($"Virtual machine [{machineName}] does not exist.");
            }

            hypervDriver.EjectDvdDrive(machineName);
        }

        /// <summary>
        /// Returns information for a Hyper-V switch by name.
        /// </summary>
        /// <param name="switchName">The switch name.</param>
        /// <returns>The <see cref="VirtualSwitch"/> when present or <c>null</c>.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public VirtualSwitch FindSwitch(string switchName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(switchName), nameof(switchName));

            return ListSwitches().FirstOrDefault(@switch => @switch.Name.Equals(switchName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Lists the virtual switches.
        /// </summary>
        /// <returns>The switches.</returns>
        public IEnumerable<VirtualSwitch> ListSwitches()
        {
            CheckDisposed();

            return hypervDriver.ListSwitches();
        }

        /// <summary>
        /// Adds a virtual Hyper-V switch that has external connectivity.
        /// </summary>
        /// <param name="switchName">The new switch name.</param>
        /// <param name="gateway">Address of the LAN gateway, used to identify the connected network interface.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public void NewExternalSwitch(string switchName, IPAddress gateway)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(switchName), nameof(switchName));
            Covenant.Requires<ArgumentNullException>(gateway != null, nameof(gateway));

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                throw new HyperVException($"No network connection detected.  Hyper-V provisioning requires a connected network.");
            }

            // We're going to look for an active (non-loopback) interface that is configured
            // to use the correct upstream gateway and also has at least one nameserver.

            // $todo(jefflill):
            //
            // This may be a problem for machines with multiple active network interfaces
            // because I may choose the wrong one (e.g. the slower card).  It might be
            // useful to have an optional cluster node definition property that explicitly
            // specifies the adapter to use for a given node.
            //
            // Another problem we'll see is for laptops with wi-fi adapters.  Lets say we
            // setup a cluster when wi-fi is connected and then the user docks the laptop,
            // connecting to a new wired adapter.  The cluster's virtual switch will still
            // be configured to use the wi-fi adapter.  The only workaround for this is
            // probably for the user to modify the virtual switch.
            //
            // This last issue is really just another indication that clusters aren't 
            // really portable in the sense that you can't expect to relocate a cluster 
            // from one network environment to another (that's why we bought the portable 
            // routers for motel use). So we'll consider this as by design (for now).

            var connectedAdapter = (NetworkInterface)null;

            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback))
            {
                var nicProperties = nic.GetIPProperties();

                if (nicProperties.DnsAddresses.Count > 0 &&
                    nicProperties.GatewayAddresses.Count(nicGateway => nicGateway.Address.Equals(gateway)) > 0)
                {
                    connectedAdapter = nic;
                    break;
                }
            }

            if (connectedAdapter == null)
            {
                throw new HyperVException($"Cannot identify a connected network adapter.");
            }

            try
            {
                var adapters    = hypervDriver.ListNetAdapters();
                var hostAdapter = (NetAdapter)null;

                foreach (var adapter in adapters)
                {
                    if (adapter.Name.Equals(connectedAdapter.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        hostAdapter = adapter;
                        break;
                    }
                }

                if (hostAdapter == null)
                {
                    throw new HyperVException($"Internal Error: Cannot identify a connected network adapter.");
                }

                hypervDriver.NewSwitch(switchName, hostAdapter: hostAdapter);
                WaitForNetworkSwitch();
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <summary>
        /// Adds an internal Hyper-V switch configured for the specified subnet and gateway as well
        /// as an optional NAT enabling external connectivity.
        /// </summary>
        /// <param name="switchName">The new switch name.</param>
        /// <param name="subnet">Specifies the internal subnet.</param>
        /// <param name="addNat">Optionally configure a NAT to support external routing.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public void NewInternalSwitch(string switchName, NetworkCidr subnet, bool addNat = false)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(switchName), nameof(switchName));
            Covenant.Requires<ArgumentNullException>(subnet != null, nameof(subnet));

            hypervDriver.NewSwitch(switchName, @internal: true);
            hypervDriver.NewNetIPAddress(switchName, subnet.FirstUsableAddress, subnet);

            if (addNat)
            {
                if (FindNatByName(switchName) == null)
                {
                    hypervDriver.NewNat(switchName, @internal: true, subnet: subnet);
                }
            }

            WaitForNetworkSwitch();
        }

        /// <summary>
        /// Waits for network functionality to be restored after creating a new virtual
        /// switch because networking can be disrupted for a period of time after switch
        /// creation.
        /// </summary>
        private void WaitForNetworkSwitch()
        {
            // $hack(jefflill):
            //
            // Creating an internal (and perhaps external) switch may disrupt the network
            // for a brief period of time.  Hyper-V Manager warns about this when creating
            // an internal switch manually.  We're going to pause for 5 seconds to hopefully
            // let this settle out and then perform network pings until one succeeds.
            //
            //      https://github.com/nforgeio/neonSDK/issues/50

            Thread.Sleep(TimeSpan.FromSeconds(10));

            var retry = new LinearRetryPolicy(e => true, retryInterval: TimeSpan.FromSeconds(1), timeout: TimeSpan.FromSeconds(30));

            using (var pinger = new Pinger())
            {
                retry.Invoke(() => pinger.SendPingAsync(IPAddress.Loopback, timeoutMilliseconds: 500).Wait());
            }
        }

        /// <summary>
        /// Removes a named virtual switch, if it exists as well as any associated NAT (with the same name).
        /// </summary>
        /// <param name="switchName">The target switch name.</param>
        /// <param name="ignoreMissing">Optionally ignore missing items.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public void RemoveSwitch(string switchName, bool ignoreMissing = false)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(switchName), nameof(switchName));

            if (ListSwitches().Any(@switch => @switch.Name.Equals(switchName, StringComparison.InvariantCultureIgnoreCase)))
            {
                try
                {
                    hypervDriver.RemoveSwitch(switchName);
                }
                catch
                {
                    if (!ignoreMissing)
                    {
                        throw;
                    }
                }
            }

            if (ListNats().Any(nat => nat.Name.Equals(switchName, StringComparison.InvariantCultureIgnoreCase)))
            {
                try
                {
                    hypervDriver.RemoveNat(switchName);
                }
                catch
                {
                    if (!ignoreMissing)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the virtual network adapters attached to the named virtual machine.
        /// </summary>
        /// <param name="machineName">The machine name.</param>
        /// <returns>The list of network adapters.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public IEnumerable<VirtualMachineNetworkAdapter> ListVmNetworkAdapters(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            return hypervDriver.ListVirtualMachineNetAdapters(machineName);
        }

        /// <summary>
        /// <para>
        /// Lists the virtual IPv4 addresses managed by Hyper-V.
        /// </para>
        /// <note>
        /// Only IPv4 addresses are returned.  IPv6 and any other address types will be ignored.
        /// </note>
        /// </summary>
        /// <returns>A list of <see cref="VirtualIPAddress"/>.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public IEnumerable<VirtualIPAddress> ListIPAddresses()
        {
            CheckDisposed();

            return hypervDriver.ListIPAddresses();
        }

        /// <summary>
        /// Returns information about a virtual IP address.
        /// </summary>
        /// <param name="address">The desired IP address.</param>
        /// <returns>The <see cref="VirtualIPAddress"/> or <c>null</c> when it doesn't exist.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public VirtualIPAddress FindIPAddress(string address)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(address), nameof(address));

            return ListIPAddresses().SingleOrDefault(addr => addr.Address == address);
        }

        /// <summary>
        /// Lists the virtual NATs.
        /// </summary>
        /// <returns>A list of <see cref="VirtualNat"/>.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public IEnumerable<VirtualNat> ListNats()
        {
            CheckDisposed();

            return hypervDriver.ListNats();
        }

        /// <summary>
        /// Looks for a virtual NAT by name.
        /// </summary>
        /// <param name="natName">The desired NAT name.</param>
        /// <returns>The <see cref="VirtualNat"/> or <c>null</c> if the NAT doesn't exist.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public VirtualNat FindNatByName(string natName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(natName), nameof(natName));

            return ListNats().FirstOrDefault(nat => nat.Name.Equals(natName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Looks for a virtual NAT by subnet.
        /// </summary>
        /// <param name="subnet">The desired NAT subnet.</param>
        /// <returns>The <see cref="VirtualNat"/> or <c>null</c> if the NAT doesn't exist.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public VirtualNat FindNatBySubnet(string subnet)
        {
            CheckDisposed();

            return ListNats().FirstOrDefault(nat => nat.Subnet == subnet);
        }
    }
}
