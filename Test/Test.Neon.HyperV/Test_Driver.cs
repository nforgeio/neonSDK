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

#undef TEST_POWERSHELL_DRIVER
#define TEST_WMI_DRIVER

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
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
        /// Ensures that a virtual machine is in a known state.
        /// </summary>
        /// <param name="client">Specifies the <see cref="HyperVClient"/>.</param>
        /// <param name="vmName">Specifies the virtual machine name.</param>
        /// <param name="state">Specifies the desired state.</param>
        /// <exception cref="TimeoutException">Thrown if the desired state was not achieved in the required time.</exception>
        private void VerifyVmState(HyperVClient client, string vmName, VirtualMachineState state)
        {
            Covenant.Requires<ArgumentNullException>(client != null, nameof(client));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(vmName), nameof(vmName));

            var vm = client.FindVm(vmName);

            Assert.Equal(state, vm.State);
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
        [InlineData(DriverType.PowerShell)]
#endif
#if TEST_WMI_DRIVER
        [InlineData(DriverType.Wmi)]
#endif
        [Repeat(100)]
        public void VirtualMachine(DriverType driverType)
        {
            driverType = DriverType.Wmi;

            // Verify virtual machine operations.

            using (var tempFolder = new TempFolder())
            {
                var bootDiskPath = Path.Combine(tempFolder.Path, "boot.vhdx");
                var dataDiskPath = Path.Combine(tempFolder.Path, "data.vhdx");

                File.Copy(TestHelper.GetUbuntuTestVhdxPath(), bootDiskPath);

                //###############################
                // $debug(jefflill): DELETE THIS!
                var logPath = @"C:\Temp\debug.log";

                //if (File.Exists(logPath))
                //{
                //    File.Delete(logPath);
                //}

                using var log = new StreamWriter(logPath) { AutoFlush = true };
                    
                var stopwatch = new Stopwatch();

                stopwatch.Start();
                log.WriteLine();
                //###############################

                using (var client = new HyperVClient())
                {
                    using (var driver = CreateDriver(client, driverType))
                    {
                        try
                        {
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 0: ");
                            // Scan for an existing test VM and stop/remove it when present.

                            if (client.FindVm(testVmName) != null)
                            {
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 1: ");
                                driver.StopVm(testVmName, turnOff: true);
                                driver.RemoveVm(testVmName);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 2: ");
                            }

                            // Create a test VM with a boot disk and then verify that it exists and is not running.

log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 3: ");
                            driver.NewVM(testVmName, processorCount: 1, startupMemoryBytes: 2 * (long)ByteUnits.GibiBytes);
                            driver.AddVmDrive(testVmName, bootDiskPath);
                            driver.EnableVmNestedVirtualization(testVmName);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 4: ");

                            var vm = client.FindVm(testVmName);

                            Assert.NotNull(vm);
                            Assert.Equal(VirtualMachineState.Off, vm.State);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 5: ");

                            // Start the VM and wait for it to transition to running.

log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 6A: ");
                            driver.StartVm(testVmName);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 6B: ");
                            VerifyVmState(client, testVmName, VirtualMachineState.Running);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 6C: ");

                            // Verify the VM memory and processor count.

                            vm = client.FindVm(testVmName);

                            Assert.Equal(1, vm.ProcessorCount);
                            Assert.Equal(2 * (long)ByteUnits.GibiBytes, vm.MemorySizeBytes);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 7: ");

                            // Save the VM, wait for it to report being saved and then restart it.

log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 7A: ");
                            driver.SaveVm(testVmName);
                            VerifyVmState(client, testVmName, VirtualMachineState.Saved);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 7B: ");
                            driver.StartVm(testVmName);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 7C: ");
                            VerifyVmState(client, testVmName, VirtualMachineState.Running);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 8D: ");

                            // List the attached data drives.  There should only be the boot disk.

                            var drives = driver.ListVmDrives(testVmName).ToList();

                            Assert.Single(drives);
                            Assert.Contains(bootDiskPath, drives);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 9: ");

                            // Create a DVD ISO file and verify that we can add and remove it
                            // from the VM.

                            var isoBuilder = new CDBuilder();
                            var isoPath    = Path.Combine(tempFolder.Path, "test.iso");

                            isoBuilder.AddFile("hello.txt", Encoding.UTF8.GetBytes("HELLO WORLD!"));
                            isoBuilder.Build(isoPath);

                            driver.InsertVmDvdDrive(testVmName, isoPath);
                            driver.EjectDvdDrive(testVmName);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 10A: ");

                            // Stop the VM gracefully, add a data drive and increase the processors to 4
                            // and the memory to 3 GiB and then restart the VM to verify the processors/memory
                            // and the new drive.

                            driver.StopVm(testVmName);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 10B: ");
                            driver.NewVhd(dataDiskPath, isDynamic: true, sizeBytes: 64 * (long)ByteUnits.MebiBytes, blockSizeBytes: (int)ByteUnits.MebiBytes);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 10C: ");
                            driver.AddVmDrive(testVmName, dataDiskPath);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 11: ");
                            driver.SetVm(testVmName, processorCount: 4, startupMemoryBytes: 3 * (long)ByteUnits.GibiBytes);
                            driver.StartVm(testVmName);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 12: ");

                            vm = client.FindVm(testVmName);

                            Assert.Equal(4, vm.ProcessorCount);
                            Assert.Equal(3 * (long)ByteUnits.GibiBytes, vm.MemorySizeBytes);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 13: ");

                            drives = driver.ListVmDrives(testVmName).ToList();

                            Assert.Equal(2, drives.Count);
                            Assert.Contains(bootDiskPath, drives);
                            Assert.Contains(dataDiskPath, drives);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 14: ");

                            // List the VM's network adapters.

                            var adapters = driver.ListVmNetAdapters(testVmName);

                            Assert.NotEmpty(adapters);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 15A: ");

                            // Stop and remove the VM and verify.

                            driver.StopVm(testVmName, turnOff: true);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 15B: ");
                            driver.RemoveVm(testVmName);
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 15C: ");
                            Assert.Null(client.FindVm(testVmName));
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 16: ");
                        }
                        finally
                        {
                            // Forcefully turn off any existing VM and remove it.

log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 17: ");
                            if (client.FindVm(testVmName) != null)
                            {
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 18: ");
                                driver.StopVm(testVmName, turnOff: true);
                                driver.RemoveVm(testVmName);
                            }
log.WriteLine($"[{stopwatch.Elapsed.RoundToSeconds()}]: 19: ");
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

        [MaintainerTheory]
#if TEST_POWERSHELL_DRIVER
        [InlineData(DriverType.PowerShell)]
#endif
#if TEST_WMI_DRIVER
        [InlineData(DriverType.Wmi)]
#endif
        public void Switches(DriverType driverType)
        {
            // Verify that switch related operations don't crash.

            using (var client = new HyperVClient())
            {
                using (var driver = CreateDriver(client, driverType))
                {
                    driver.ListSwitches();
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
        public void DEVTEST(DriverType driverType)
        {
            using (var client = new HyperVClient())
            {
                using (var driver = CreateDriver(client, driverType))
                {
                    driver.NewSwitch("foo");
                    driver.RemoveSwitch("foo");
                }
            }
        }
    }
}
