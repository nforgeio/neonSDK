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

#define TEST_POWERSHELL_DRIVER
#undef TEST_WMI_DRIVER

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DiscUtils.Iso9660;
using Neon.Common;
using Neon.Deployment;
using Neon.HyperV;
using Neon.IO;
using Neon.Xunit;

using Xunit;

namespace TestHyperV
{
    [Trait(TestTrait.Category, TestArea.NeonHyperV)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public class Test_Driver
    {
        //---------------------------------------------------------------------
        // Private types

        public enum DriverType
        {
            PowerShell,
            Wmi
        }

        //---------------------------------------------------------------------
        // Implementation

        private const string testVmName = "neon-hyperv-unit-test";

        private readonly TimeSpan   timeout      = TimeSpan.FromSeconds(15);
        private readonly TimeSpan   pollInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Creates a Hyper-V driver of the specified type.
        /// </summary>
        /// <param name="client">Specifies the parent client.</param>
        /// <param name="driverType">Specifies the desired driver type.</param>
        /// <returns>The <see cref="IHyperVDriver"/> implementations.</returns>
        private IHyperVDriver CreateDriver(HyperVClient client, DriverType driverType)
        {
            Covenant.Requires<ArgumentNullException>(client != null, nameof(client));

            switch (driverType)
            {
                case DriverType.PowerShell:

                    return new HyperVPowershellDriver(client);

                case DriverType.Wmi:

                    return new HyperVWmiDriver(client);

                default:

                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Waits for a virtual machine to transition to a specific state.
        /// </summary>
        /// <param name="client">Specififies the <see cref="HyperVClient"/>.</param>
        /// <param name="vmName">Specifies the virtual machine name.</param>
        /// <param name="state">Specifies the desired state.</param>
        /// <exception cref="TimeoutException">Thrown if the desired state was not achieved in the required time.</exception>
        private void WaitForVmState(HyperVClient client, string vmName, VirtualMachineState state)
        {
            Covenant.Requires<ArgumentNullException>(client != null, nameof(client));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(vmName), nameof(vmName));

            NeonHelper.WaitFor(
                () =>
                {
                    var vm = client.FindVm(vmName);

                    return vm.State == state;
                },
                timeout:      timeout,
                pollInterval: pollInterval);
        }

        [MaintainerTheory]
#if TEST_POWERSHELL_DRIVER
        [InlineData(DriverType.PowerShell, true)]
        [InlineData(DriverType.PowerShell, false)]
#endif
#if TEST_WMI_DRIVER
        [InlineData(DriverType.Wmi, true)]
        [InlineData(DriverType.Wmi, false)]
#endif
        public void Disk(DriverType driverType, bool isDynamic)
        {
            // Verify standalone virtual disk operations.

            using (var tempFolder = new TempFolder())
            {
                var drivePath = Path.Combine(tempFolder.Path, "test.vhdx");
                var driveSize = (long)ByteUnits.MebiBytes * 64;
                var blockSize = (int)ByteUnits.MebiBytes;

                using (var client = new HyperVClient())
                {
                    using (var driver = CreateDriver(client, driverType))
                    {
                        driver.NewVhd(drivePath, isDynamic, sizeBytes: driveSize, blockSizeBytes: blockSize);
                        Assert.True(File.Exists(drivePath));

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
        [InlineData(DriverType.PowerShell)]
#endif
#if TEST_WMI_DRIVER
        [InlineData(DriverType.Wmi)]
#endif
        public void VirtualMachine(DriverType driverType)
        {
            // Verify virtual machine operations.

            using (var tempFolder = new TempFolder())
            {
                var bootDiskPath = Path.Combine(tempFolder.Path, "boot.vhdx");
                var dataDiskPath = Path.Combine(tempFolder.Path, "data.vhdx");

                File.Copy(TestHelper.GetUbuntuTestVhdxPath(), bootDiskPath);

                using (var client = new HyperVClient())
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

                            // Create a test VM with a boot dist and then verify that it exists and is not running.

                            driver.NewVM(testVmName, processorCount: 1, startupMemoryBytes: 2 * (long)ByteUnits.GibiBytes);
                            driver.AddVmDrive(testVmName, bootDiskPath);
                            driver.EnableVmNestedVirtualization(testVmName);

                            var vm = client.FindVm(testVmName);

                            Assert.NotNull(vm);
                            Assert.Equal(VirtualMachineState.Off, vm.State);

                            // Start the VM and wait for it to transition to running.

                            driver.StartVm(testVmName);
                            WaitForVmState(client, testVmName, VirtualMachineState.Running);

                            // Verify the VM memory and processor count.

                            vm = client.FindVm(testVmName);

                            Assert.Equal(1, vm.ProcessorCount);
                            Assert.Equal(2 * (long)ByteUnits.GibiBytes, vm.MemorySizeBytes);

                            // Save the VM, wait for it to report being saved and then restart it.

                            driver.SaveVm(testVmName);
                            WaitForVmState(client, testVmName, VirtualMachineState.Saved);
                            driver.StartVm(testVmName);
                            WaitForVmState(client, testVmName, VirtualMachineState.Running);

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

                            driver.InsertVmDvdDrive(testVmName, isoPath);
                            driver.EjectDvdDrive(testVmName);

                            // Stop the VM gracefully, add a data drive and increase the processors to 4
                            // and the memory to 3 GiB and then restart the VM to verify the processors/memory
                            // and the new drive.

                            driver.StopVm(testVmName);
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

                            // Remove the VM and verify.

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
        [InlineData(DriverType.PowerShell)]
#endif
#if TEST_WMI_DRIVER
        [InlineData(DriverType.Wmi)]
#endif
        public void ListHostAdapters(DriverType driverType)
        {
            // Verify that we can list host network adapters.

            using (var client = new HyperVClient())
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
        [InlineData(DriverType.PowerShell)]
#endif
#if TEST_WMI_DRIVER
        [InlineData(DriverType.Wmi)]
#endif
        public void ListIPAddresses(DriverType driverType)
        {
            // Verify that we can list IP addresses.

            using (var client = new HyperVClient())
            {
                using (var driver = CreateDriver(client, driverType))
                {
                    var addresses = driver.ListIPAddresses();

                    Assert.NotEmpty(addresses);
                }
            }
        }

        // $todo(jefflill):
        //
        // We need to add unit tests for switches and NATs at some point.
    }
}
