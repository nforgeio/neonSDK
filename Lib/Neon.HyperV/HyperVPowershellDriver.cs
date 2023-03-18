//-----------------------------------------------------------------------------
// FILE:	    HyperVPowershellDriver.cs
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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.HyperV.PowerShell;

using Neon.Common;
using Neon.Net;
using Neon.Windows;

using Newtonsoft.Json.Linq;

namespace Neon.HyperV
{
    /// <summary>
    /// Uses Powershell to implement <see cref="IHyperVDriver"/>.
    /// </summary>
    internal sealed class HyperVPowershellDriver : IHyperVDriver
    {
        //---------------------------------------------------------------------
        // Static members

        /// <summary>
        /// The Hyper-V cmdlet namespace prefix used to avoid conflicts with things
        /// like the VMware cmdlets.
        /// </summary>
        private const string HyperVNamespace = @"Hyper-V\";

        /// <summary>
        /// The Hyper-V namespace prefix for the TCP/IP related cmdlets.
        /// </summary>
        private const string NetTcpIpNamespace = @"NetTCPIP\";

        /// <summary>
        /// The Hyper-V namespace prefix for the NAT related cmdlets.
        /// </summary>
        private const string NetNatNamespace = @"NetNat\";

        //---------------------------------------------------------------------
        // Instance members

        private HyperVClient        client;
        private PowerShell          powershell;
        private readonly TimeSpan   timeout = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Constructor.
        /// </summary>
        public HyperVPowershellDriver(HyperVClient client)
        {
            Covenant.Requires<ArgumentNullException>(client != null, nameof(client));

            this.client = client;

            if (!NeonHelper.IsWindows)
            {
                throw new NotSupportedException($"{nameof(HyperVPowershellDriver)} is only supported on Windows.");
            }

            powershell = new PowerShell();
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~HyperVPowershellDriver()
        {
            Dispose();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            powershell?.Dispose();
            powershell = null;

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
                powershell?.Dispose();
                powershell = null;

                GC.SuppressFinalize(this);
            }

            powershell = null;
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
        /// Extracts virtual machine properties from a dynamic PowerShell result.
        /// </summary>
        /// <param name="rawMachine">The dynamic machine properties.</param>
        /// <returns>The parsed <see cref="VirtualMachine"/>.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        private VirtualMachine ExtractVm(JObject rawMachine)
        {
            Covenant.Requires<ArgumentNullException>(rawMachine != null, nameof(rawMachine));

            var vm = new VirtualMachine();

            // Extract the VM name.

            vm.Name = (string)rawMachine.Property("VMName");

            // Extract the processor count and memory size.

            vm.ProcessorCount  = (int)rawMachine.Property("ProcessorCount");
            vm.MemorySizeBytes = (long)rawMachine.Property("MemoryStartup");

            // Extract the VM state.

            switch ((string)rawMachine.Property("State"))
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

            var operationalStatus = (JArray)rawMachine.Property("OperationalStatus").Value;

            vm.Ready  = (string)operationalStatus[0] == "Ok";

            var uptimeObject = (JObject)rawMachine.Property("Uptime").Value;

            vm.Uptime = TimeSpan.FromTicks((long)uptimeObject.Property("Ticks"));

            // Extract the connected switch name from the first network adapter (if any).

            // $note(jefflill):
            // 
            // We don't currently support VMs with multiple network adapters and will only
            // capture the name of the switch connected to the first adapter if any).

            var adapters = (JArray)rawMachine.Property("NetworkAdapters").Value;

            if (adapters.Count > 0)
            {
                var firstAdapter = (JObject)adapters[0];

                vm.SwitchName = (string)firstAdapter.Property("SwitchName").Value;
            }

            return vm;
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

            var command = $"{HyperVNamespace}New-VM -Name '{machineName}' -MemoryStartupBytes {startupMemoryBytes} -Generation {generation}";

            if (!string.IsNullOrEmpty(drivePath))
            {
                command += $" -VHDPath '{drivePath}'";
            }

            if (!string.IsNullOrEmpty(switchName))
            {
                command += $" -SwitchName '{switchName}'";
            }

            try
            {
                powershell.Execute(command);
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }

            // Disable drive checkpointing.

            SetVm(machineName, processorCount: processorCount, checkpointDrives: false);
        }

        /// <inheritdoc/>
        public void AddVmDrive(string machineName, string drivePath)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));

            try
            {
                powershell.Execute($"{HyperVNamespace}Add-VMHardDiskDrive -VMName '{machineName}' -Path \"{drivePath}\"");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void InsertVmDvdDrive(string machineName, string isoPath)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            try
            {
                powershell.Execute($"Set-VMDvdDrive -VMName '{machineName}' -Path '{isoPath}'");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void EjectDvdDrive(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            try
            {
                powershell.Execute($"Set-VMDvdDrive -VMName '{machineName}' -Path $null");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void DismountVhd(string drivePath)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));

            try
            {
                powershell.Execute($"Dismount-VHD '{drivePath}'");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void EnableVmNestedVirtualization(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            try
            {
                // Enable nested virtualization for the VM.

                powershell.Execute($"{HyperVNamespace}Set-VMProcessor -VMName '{machineName}' -ExposeVirtualizationExtensions $true");

                // Enable MAC address spoofing for the VMs network adapter.

                powershell.Execute($"{HyperVNamespace}Set-VMNetworkAdapter -VMName '{machineName}' -MacAddressSpoofing On");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualNat> ListNats()
        {
            CheckDisposed();

            try
            {
                var nats    = new List<VirtualNat>();
                var rawNats = powershell.ExecuteJson($"{NetNatNamespace}Get-NetNAT");

                foreach (dynamic rawNat in rawNats)
                {
                    var name   = (string)null;
                    var subnet = (string)null;

                    foreach (dynamic rawProperty in rawNat.CimInstanceProperties)
                    {
                        switch ((string)rawProperty.Name)
                        {
                            case "Name":

                                name = rawProperty.Value;
                                break;

                            case "InternalIPInterfaceAddressPrefix":

                                subnet = rawProperty.Value;
                                break;
                        }

                        if (name != null && subnet != null)
                        {
                            break;
                        }
                    }

                    var nat = new VirtualNat()
                    {
                        Name   = name,
                        Subnet = subnet
                    };

                    nats.Add(nat);
                }

                return nats;
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualSwitch> ListSwitches()
        {
            CheckDisposed();

            try
            {
                var switches    = new List<VirtualSwitch>();
                var rawSwitches = powershell.ExecuteJson($"{HyperVNamespace}Get-VMSwitch");

                foreach (dynamic rawSwitch in rawSwitches)
                {
                    var virtualSwitch
                        = new VirtualSwitch()
                        {
                            Name = rawSwitch.Name
                        };

                    switch (rawSwitch.SwitchType.Value)
                    {
                        case "Internal":

                            virtualSwitch.Type = VirtualSwitchType.Internal;
                            break;

                        case "External":

                            virtualSwitch.Type = VirtualSwitchType.External;
                            break;

                        case "Private":

                            virtualSwitch.Type = VirtualSwitchType.Private;
                            break;

                        default:

                            virtualSwitch.Type = VirtualSwitchType.Unknown;
                            break;
                    }

                    switches.Add(virtualSwitch);
                }

                return switches;
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualNetworkAdapter> ListVmNetAdapters(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            try
            {
                var adapters  = new List<VirtualNetworkAdapter>();
                var rawAdapters = powershell.ExecuteJson($"{HyperVNamespace}Get-VMNetworkAdapter -VMName '{machineName}'");

                foreach (JObject rawAdapter in rawAdapters)
                {
                    var adapter = new VirtualNetworkAdapter()
                    {
                        Name           = (string)rawAdapter.Property("Name"),
                        VMName         = (string)rawAdapter.Property("VMName"),
                        IsManagementOs = ((string)rawAdapter.Property("IsManagementOs")).Equals("True", StringComparison.InvariantCultureIgnoreCase),
                        SwitchName     = (string)rawAdapter.Property("SwitchName"),
                        MacAddress     = (string)rawAdapter.Property("MacAddress"),
                        Status         = (string)((JArray)rawAdapter.Property("Status").Value).FirstOrDefault()
                    };

                    // Parse the IP addresses.

                    var addresses = (JArray)rawAdapter.Property("IPAddresses").Value;

                    if (addresses.Count > 0)
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
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualMachine> ListVms()
        {
            CheckDisposed();

            try
            {
                var machines = new List<VirtualMachine>();
                var table    = powershell.ExecuteJson($"{HyperVNamespace}Get-VM");

                foreach (dynamic rawMachine in table)
                {
                    machines.Add(ExtractVm(rawMachine));
                }

                return machines;
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void MountVhd(string drivePath, bool readOnly = false)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));

            try
            {
                var sbCommand = new StringBuilder($"Mount-VHD '{drivePath}'");

                if (readOnly)
                {
                    sbCommand.AppendWithSeparator("-ReadOnly");
                }

                powershell.Execute(sbCommand.ToString());
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void NewNetIPAddress(string switchName, IPAddress address, NetworkCidr subnet)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(switchName), nameof(switchName));
            Covenant.Requires<ArgumentNullException>(address != null, nameof(address));
            Covenant.Requires<ArgumentNullException>(subnet != null, nameof(subnet));

            try
            {
                powershell.Execute($"{NetTcpIpNamespace}New-NetIPAddress -IPAddress {subnet.FirstUsableAddress} -PrefixLength {subnet.PrefixLength} -InterfaceAlias 'vEthernet ({switchName})'");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void NewNat(string switchName, NetworkCidr subnet)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(switchName), nameof(switchName));
            Covenant.Requires<ArgumentNullException>(subnet != null, nameof(subnet));

            try
            {
                powershell.Execute($"{NetNatNamespace}New-NetNAT -Name '{switchName}' -InternalIPInterfaceAddressPrefix {subnet}");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void NewVhd(string drivePath, bool isDynamic, long sizeBytes, int blockSizeBytes)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));
            Covenant.Requires<ArgumentException>(sizeBytes > 0, nameof(sizeBytes));
            Covenant.Requires<ArgumentException>(blockSizeBytes > 0, nameof(blockSizeBytes));

            try
            {
                var fixedOrDynamic = isDynamic ? "-Dynamic" : "-Fixed";

                powershell.Execute($"{HyperVNamespace}New-VHD -Path '{drivePath}' {fixedOrDynamic} -SizeBytes {sizeBytes} -BlockSizeBytes {blockSizeBytes}");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void NewSwitch(string switchName, string targetAdapter = null, bool @internal = false)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(switchName), nameof(switchName));

            try
            {
                var sbCommand = new StringBuilder($"{HyperVNamespace}New-VMSwitch -Name '{switchName}'");

                if (targetAdapter != null)
                {
                    sbCommand.AppendWithSeparator($"-NetAdapterName '{targetAdapter}'");
                }

                if (@internal)
                {
                    sbCommand.AppendWithSeparator($"-SwitchType Internal");
                }

                powershell.Execute(sbCommand.ToString());
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void OptimizeVhd(string drivePath)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));

            try
            {
                powershell.Execute($"Optimize-VHD '{drivePath}' -Mode Full");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void RemoveNat(string natName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(natName), nameof(natName));

            try
            {
                powershell.Execute($"Remove-NetNat -Name '{natName}' -Confirm:$false");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void RemoveVm(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            try
            {
                powershell.Execute($"{HyperVNamespace}Remove-VM -Name '{machineName}' -Force");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void RemoveSwitch(string switchName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(switchName), nameof(switchName));

            try
            {
                powershell.Execute($"{HyperVNamespace}Remove-VMSwitch -Name '{switchName}' -Force");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void ResizeVhd(string drivePath, long sizeBytes)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));
            Covenant.Requires<ArgumentException>(sizeBytes > 0, nameof(sizeBytes));

            try
            {
                powershell.Execute($"{HyperVNamespace}Resize-VHD -Path '{drivePath}' -SizeBytes {sizeBytes}");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> ListVmDrives(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            try
            {
                var drives    = new List<string>();
                var rawDrives = powershell.ExecuteJson($"{HyperVNamespace}Get-VMHardDiskDrive -VMName '{machineName}'");

                foreach (dynamic rawDrive in rawDrives)
                {
                    drives.Add(rawDrive.Path.ToString());
                }

                return drives;
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void SaveVm(string machineName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));
            CheckDisposed();

            try
            {
                powershell.Execute($"{HyperVNamespace}Save-VM -Name '{machineName}'");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void SetVm(string machineName, int? processorCount = null, long? startupMemoryBytes = null, bool? checkpointDrives = null)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));
            Covenant.Requires<ArgumentException>(!processorCount.HasValue || processorCount.Value > 0, nameof(processorCount));
            Covenant.Requires<ArgumentException>(!startupMemoryBytes.HasValue || startupMemoryBytes.Value > 1, nameof(startupMemoryBytes));

            try
            {
                var sbCommand = new StringBuilder( $"{HyperVNamespace}Set-VM -Name '{machineName}' -StaticMemory");

                if (processorCount.HasValue)
                {
                    sbCommand.AppendWithSeparator($"-ProcessorCount {processorCount}");
                }

                if (startupMemoryBytes.HasValue)
                {
                    sbCommand.AppendWithSeparator($"-MemoryStartupBytes {startupMemoryBytes}");
                }

                if (checkpointDrives.HasValue)
                {
                    if (checkpointDrives.Value)
                    {
                        sbCommand.AppendWithSeparator($"-CheckpointType Enabled");
                    }
                    else
                    {
                        sbCommand.AppendWithSeparator($"-CheckpointType Disabled");
                    }
                }

                powershell.Execute(sbCommand.ToString());
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void StartVm(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            try
            {
                powershell.Execute($"{HyperVNamespace}Start-VM -Name '{machineName}'");
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public void StopVm(string machineName, bool turnOff = false)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            try
            {
                if (turnOff)
                {
                    powershell.Execute($"{HyperVNamespace}Stop-VM -Name '{machineName}' -TurnOff");
                }
                else
                {
                    powershell.Execute($"{HyperVNamespace}Stop-VM -Name '{machineName}'");
                }
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualIPAddress> ListIPAddresses()
        {
            CheckDisposed();

            try
            {
                var addresses    = new List<VirtualIPAddress>();
                var rawAddresses = powershell.ExecuteJson($"{NetTcpIpNamespace}Get-NetIPAddress");
                var switchRegex  = new Regex(@"^.*\((?<switch>.+)\)$");

                foreach (dynamic rawAddress in rawAddresses)
                {
                    // We're only listing IPv4  addresses.

                    var address = (string)rawAddress.IPv4Address;

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

                    var interfaceAlias = (string)rawAddress.InterfaceAlias;
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

                    var virtualIPAddress
                        = new VirtualIPAddress()
                        {
                            Address       = address,
                            Subnet        = NetworkCidr.Parse($"{address}/{rawAddress.PrefixLength}"),
                            InterfaceName = interfaceName
                        };

                    addresses.Add(virtualIPAddress);
                }

                return addresses;
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> ListHostAdapters()
        {
            CheckDisposed();

            try
            {
                return powershell.ExecuteJson($"Get-NetAdapter").Select(adapter => (string)adapter.Name);
            }
            catch (Exception e)
            {
                throw new HyperVException(e.Message, e);
            }
        }
    }
}
