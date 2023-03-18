//-----------------------------------------------------------------------------
// FILE:        Test_Driver.cs
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

// Define these to enable unit tests for each driver.

#define TEST_POWERSHELL_DRIVER
#define TEST_WMI_DRIVER

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DiscUtils.Iso9660;

using Neon.Common;
using Neon.Deployment;
using Neon.HyperV;
using Neon.IO;
using Neon.Net;
using Neon.Xunit;

using Xunit;

namespace TestHyperV
{
    [Trait(TestTrait.Category, TestArea.NeonHyperV)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public class Test_Driver
    {
        private const string testVmName = "neon-hyperv-unit-test";

        private readonly TimeSpan   timeout      = TimeSpan.FromSeconds(15);
        private readonly TimeSpan   pollInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Creates a Hyper-V driver of the specified type.
        /// </summary>
        /// <param name="client">Specifies the parent client.</param>
        /// <param name="driverType">Specifies the desired driver type.</param>
        /// <returns>The <see cref="IHyperVDriver"/> implementations.</returns>
        private IHyperVDriver CreateDriver(HyperVClient client, HyperVDriverType driverType)
        {
            Covenant.Requires<ArgumentNullException>(client != null, nameof(client));

            switch (driverType)
            {
                case HyperVDriverType.PowerShell:

                    return new HyperVPowershellDriver(client);

                case HyperVDriverType.Wmi:

                    return new HyperVWmiDriver(client);

                default:

                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Ensures that a virtual machine is in a known state.
        /// </summary>
        /// <param name="client">Specifies the <see cref="HyperVClient"/>.</param>
        /// <param name="vmName">Specifies the virtual machine name.</param>
        /// <param name="state">Specifies the desired state.</param>
        /// <exception cref="TimeoutException">Thrown if the desired state was not achieved in the required time.</exception>
        private void EnsureVmState(HyperVClient client, string vmName, VirtualMachineState state)
        {
            Covenant.Requires<ArgumentNullException>(client != null, nameof(client));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(vmName), nameof(vmName));

            var vm = client.FindVm(vmName);

            Assert.Equal(state, vm.State);
        }

        [MaintainerTheory]
#if TEST_POWERSHELL_DRIVER
        [InlineData(HyperVDriverType.PowerShell, true)]
        [InlineData(HyperVDriverType.PowerShell, false)]
#endif
#if TEST_WMI_DRIVER
        [InlineData(HyperVDriverType.Wmi, true)]
        [InlineData(HyperVDriverType.Wmi, false)]
#endif
        public void Disk(HyperVDriverType driverType, bool isDynamic)
        {
            // Verify standalone virtual disk operations.

            using (var tempFolder = new TempFolder())
            {
                var drivePath = Path.Combine(tempFolder.Path, "test.vhdx");
                var driveSize = (long)ByteUnits.MebiBytes * 64;
                var blockSize = (int)ByteUnits.MebiBytes;

                using (var client = new HyperVClient(driverType))
                {
                    using (var driver = CreateDriver(client, driverType))
                    {
                        driver.NewVhd(drivePath, isDynamic, sizeBytes: driveSize, blockSizeBytes: blockSize);
                        Assert.True(File.Exists(drivePath));

                        driver.ResizeVhd(drivePath, sizeBytes: driveSize * 2);
                        driver.MountVhd(drivePath, readOnly: true);
                        driver.OptimizeVhd(drivePath);
                        driver.DismountVhd(drivePath);

                        File.Delete(drivePath);
                    }
                }
            }
        }

        [MaintainerTheory]
#if TEST_POWERSHELL_DRIVER
        [InlineData(HyperVDriverType.PowerShell)]
#endif
#if TEST_WMI_DRIVER
        [InlineData(HyperVDriverType.Wmi)]
#endif
        public void VirtualMachine(HyperVDriverType driverType)
        {
            // Verify virtual machine operations.

            using (var tempFolder = new TempFolder())
            {
                var bootDiskPath = Path.Combine(tempFolder.Path, "boot.vhdx");
                var dataDiskPath = Path.Combine(tempFolder.Path, "data.vhdx");

                File.Copy(TestHelper.GetUbuntuTestVhdxPath(), bootDiskPath);

                using (var client = new HyperVClient(driverType))
                {
                    using (var driver = CreateDriver(client, driverType))
                    {
                        try
                        {
                            // Scan for an existing test VM and stop/remove it when present.

                            if (client.FindVm(testVmName) != null)
                            {
                                driver.StopVm(testVmName, turnOff: true);
                                driver.RemoveVm(testVmName);
                            }

                            // Create a test VM with a boot disk and then verify that it exists and is not running.

                            driver.NewVM(testVmName, processorCount: 1, startupMemoryBytes: 2 * (long)ByteUnits.GibiBytes);
                            driver.AddVmDrive(testVmName, bootDiskPath);
                            driver.EnableVmNestedVirtualization(testVmName);

                            var vm = client.FindVm(testVmName);

                            Assert.NotNull(vm);
                            Assert.Equal(VirtualMachineState.Off, vm.State);

                            // Start the VM and wait for it to transition to running.

                            driver.StartVm(testVmName);
                            EnsureVmState(client, testVmName, VirtualMachineState.Running);

                            // Verify that the VM is ready and also that [Uptime] is being
                            // retrieved.

                            var minUptime = TimeSpan.FromSeconds(1);

                            Thread.Sleep(minUptime);

                            vm = client.FindVm(testVmName);

                            Assert.True(vm.Uptime >= minUptime);
                            Assert.True(vm.Ready);

                            // Verify the VM memory and processor count.

                            vm = client.FindVm(testVmName);

                            Assert.Equal(1, vm.ProcessorCount);
                            Assert.Equal(2 * (long)ByteUnits.GibiBytes, vm.MemorySizeBytes);

                            // Save the VM, wait for it to report being saved and then restart it.

                            driver.SaveVm(testVmName);
                            EnsureVmState(client, testVmName, VirtualMachineState.Saved);
                            driver.StartVm(testVmName);
                            EnsureVmState(client, testVmName, VirtualMachineState.Running);

                            // List the attached data drives.  There should only be the boot disk.

                            var drives = driver.ListVmDrives(testVmName).ToList();

                            Assert.Single(drives);
                            Assert.Contains(bootDiskPath, drives);

                            // Create a DVD ISO file and verify that we can add and remove it
                            // from the VM.

                            var isoBuilder = new CDBuilder();
                            var isoPath    = Path.Combine(tempFolder.Path, "test.iso");

                            isoBuilder.AddFile("hello.txt", Encoding.UTF8.GetBytes("HELLO WORLD!"));
                            isoBuilder.Build(isoPath);

                            //driver.InsertVmDvdDrive(testVmName, isoPath);
                            //driver.EjectDvdDrive(testVmName);

                            // Stop the VM gracefully, add a data drive and increase the processors to 4
                            // and the memory to 3 GiB and then restart the VM to verify the processors/memory
                            // and the new drive.

                            driver.StopVm(testVmName);
                            EnsureVmState(client, testVmName, VirtualMachineState.Off);
                            driver.NewVhd(dataDiskPath, isDynamic: true, sizeBytes: 64 * (long)ByteUnits.MebiBytes, blockSizeBytes: (int)ByteUnits.MebiBytes);
                            driver.AddVmDrive(testVmName, dataDiskPath);
                            driver.SetVm(testVmName, processorCount: 4, startupMemoryBytes: 3 * (long)ByteUnits.GibiBytes);
                            driver.StartVm(testVmName);

                            vm = client.FindVm(testVmName);

                            Assert.Equal(4, vm.ProcessorCount);
                            Assert.Equal(3 * (long)ByteUnits.GibiBytes, vm.MemorySizeBytes);

                            drives = driver.ListVmDrives(testVmName).ToList();

                            Assert.Equal(2, drives.Count);
                            Assert.Contains(bootDiskPath, drives);
                            Assert.Contains(dataDiskPath, drives);

                            // List the VM's network adapters.

                            var adapters = driver.ListVmNetAdapters(testVmName);

                            Assert.NotEmpty(adapters);

                            // Stop and remove the VM and verify.

                            driver.StopVm(testVmName, turnOff: true);
                            driver.RemoveVm(testVmName);
                            Assert.Null(client.FindVm(testVmName));
                        }
                        finally
                        {
                            // Forcefully turn off any existing VM and remove it.

                            if (client.FindVm(testVmName) != null)
                            {
                                driver.StopVm(testVmName, turnOff: true);
                                driver.RemoveVm(testVmName);
                            }
                        }
                    }
                }
            }
        }

        [MaintainerTheory]
#if TEST_POWERSHELL_DRIVER
        [InlineData(HyperVDriverType.PowerShell)]
#endif
#if TEST_WMI_DRIVER
        [InlineData(HyperVDriverType.Wmi)]
#endif
        public void ListHostAdapters(HyperVDriverType driverType)
        {
            // Verify that we can list host network adapters.

            using (var client = new HyperVClient(driverType))
            {
                using (var driver = CreateDriver(client, driverType))
                {
                    var hostAdapters = driver.ListHostAdapters();

                    Assert.NotEmpty(hostAdapters);
                }
            }
        }

        [MaintainerTheory]
#if TEST_POWERSHELL_DRIVER
        [InlineData(HyperVDriverType.PowerShell)]
#endif
#if TEST_WMI_DRIVER
        [InlineData(HyperVDriverType.Wmi)]
#endif
        public void ListIPAddresses(HyperVDriverType driverType)
        {
            // Verify that we can list IP addresses.

            using (var client = new HyperVClient(driverType))
            {
                using (var driver = CreateDriver(client, driverType))
                {
                    var addresses = driver.ListIPAddresses();

                    Assert.NotEmpty(addresses);
                }
            }
        }

        [MaintainerTheory]
#if TEST_POWERSHELL_DRIVER
        [InlineData(HyperVDriverType.PowerShell)]
#endif
#if TEST_WMI_DRIVER
        [InlineData(HyperVDriverType.Wmi)]
#endif
        public void SwitchAndNat(HyperVDriverType driverType)
        {
            // Verify switch related operations.

            using (var client = new HyperVClient(driverType))
            {
                using (var driver = CreateDriver(client, driverType))
                {
                    const string testSwitchName = "d4f28a28-be82-46ec-8411-34c1b92174c2";
                    const string subnet         = "10.202.0.0/24";

                    // Create an internal switch with a NAT.

                    client.NewInternalSwitch(testSwitchName, NetworkCidr.Parse(subnet), addNat: true);
                    Assert.Contains(testSwitchName, driver.ListSwitches().Select(@switch => @switch.Name));

                    // This should have created a NAT with the same name as the switch.

                    var nats   = client.ListNats();
                    var newNat = nats.Where(nat => nat.Name.Equals(testSwitchName)).FirstOrDefault();

                    Assert.NotNull(newNat);
                    Assert.Equal(subnet, newNat.Subnet);

                    // Remove the switch and NAT.

                    client.RemoveSwitch(testSwitchName);
                    Assert.DoesNotContain(testSwitchName,driver.ListSwitches().Select(@switch => @switch.Name));
                    Assert.DoesNotContain(testSwitchName, driver.ListNats().Select(nat => nat.Name));
                }
            }
        }
    }
}
