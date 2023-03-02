//-----------------------------------------------------------------------------
// FILE:	    IHyperV.cs
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
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Net;

namespace Neon.HyperV
{
    /// <summary>
    /// <para>
    /// Describes the behavior of our Hyper-V API abstraction.  We have two implementations
    /// of this: <see cref="HyperVPowershell"/> which implements these via Hyper-V Powershell
    /// Cmdlets and <see cref="HyperVWmi"/> which implements these via direct WMI calls.
    /// </para>
    /// <para>
    /// <see cref="HyperVPowershell"/> is the older and much slower implementation which also
    /// requires Powershell 7+ to be installed.  This was our initial implementation and we've 
    /// been using this for several years (probably from 2016).  We're replacing this with
    /// <see cref="HyperVWmi"/> which will be much faster and won't require a Powershell 7+,
    /// which will save 100MB for installers and something like 300MB after installation.
    /// </para>
    /// <para>
    /// We're going to deprecate the <see cref="HyperVPowershell"/> implement, but keep it
    /// around just in case we need it later.
    /// </para>
    /// <note>
    /// These methods are (mostly) patterned after the Powershell cmdlets to make porting
    /// existing code easier.
    /// </note>
    /// </summary>
    internal interface IHyperV : IDisposable
    {
        /// <summary>
        /// Creates a new virtual machine.
        /// </summary>
        /// <param name="name">Specifies the virtual machine name.</param>
        /// <param name="startupMemoryBytes">Specifies the machine RAM in bytes.</param>
        /// <param name="generation">Specifies the virtual machine generation (defaults to <b>1</b>).</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void AddVM(
            string  name,
            long    startupMemoryBytes,
            int     generation = 1);

        /// <summary>
        /// Sets virtual machine properties.
        /// </summary>
        /// <param name="name">Specifies the virtual machine name.</param>
        /// <param name="processorCount">Optionally specifies the number of processors.</param>
        /// <param name="startupMemoryBytes">Optionally specifies the machine RAM in bytes.</param>
        /// <param name="checkpointDrives">Optionally specifies whether drive checking pointing is enabled.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void SetVM(
            string      name,
            int?        processorCount     = null,
            long?       startupMemoryBytes = null,
            bool?       checkpointDrives   = null);

        /// <summary>
        /// Lists the existing virtual machines.
        /// </summary>
        /// <returns>The virtual machines.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        IEnumerable<VirtualMachine> ListVms();

        /// <summary>
        /// Removes a virtual machine.
        /// </summary>
        /// <param name="name">Specifies the virtual machine name.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void RemoveVm(string name);

        /// <summary>
        /// Starts a virtual machine.
        /// </summary>
        /// <param name="name">Specifies the virtual machine name.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void StartVm(string name);

        /// <summary>
        /// Stops a virtually machine, optionally turning it off immediatelly.
        /// </summary>
        /// <param name="name">Specifies the virtual machine name.</param>
        /// <param name="turnOff">
        /// <para>
        /// Optionally just turns the VM off without performing a graceful shutdown first.
        /// </para>
        /// <note>
        /// <b>WARNING!</b> This could result in corruption or the the loss of unsaved data.
        /// </note>
        /// </param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void StopVm(string name, bool turnOff = false);

        /// <summary>
        /// Persists the state of a running virtual machine and then stops it.  This is 
        /// equivalent to hibernation for a physical machine.
        /// </summary>
        /// <param name="name">Specifies the virtual machine name.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void SaveVm(string name);

        /// <summary>
        /// Enables a virtual machine to operate on a Hyper-V host that running also
        /// virtualized (nested virtualization).
        /// </summary>
        /// <param name="name">Specifies the virtual machine name.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void EnableVmNestedVirtualization(string name);

        /// <summary>
        /// Adds an existing virtual drive to a virtual machine.
        /// </summary>
        /// <param name="name">Specifies the virtual machine name.</param>
        /// <param name="path">Specifies the path to the existing virtual drive.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void AddVmHardDiskDrive(string name, string path);

        /// <summary>
        /// Adds a DVD drive to a virtual machine.
        /// </summary>
        /// <param name="name">Specifies the virtual machine name.</param>
        /// <param name="path">Specifies the path to the existing virtual DVD drive (ISO file).</param>
        /// <param name="controllerLocation">
        /// Specifies the number of the location on the controller at which the DVD drive
        /// is to be added.
        /// </param>
        /// <param name="controllerNumber">
        /// Specifies the number of the controller to which the DVD drive is to be added.
        /// </param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void AddVmDvdDrive(
            string      name,
            string      path,
            int         controllerLocation,
            int         controllerNumber);

        /// <summary>
        /// Removes a DVD drive from a virtual machine.
        /// </summary>
        /// <param name="name">Specifies the virtual machine name.</param>
        /// <param name="controllerLocation">
        /// Specifies the number of the location on the controller at which the DVD drive
        /// is to be added.
        /// </param>
        /// <param name="controllerNumber">
        /// Specifies the number of the controller to which the DVD drive is to be added.
        /// </param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void RemoveVmDvdDrive(
            string      name,
            int?        controllerLocation,
            int?        controllerNumber);

        /// <summary>
        /// Lists the paths of the virtual drives attached to a virtual machine.
        /// </summary>
        /// <param name="name">Specifies the virtual machine name.</param>
        /// <returns>The attached drive paths.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        IEnumerable<string> ListVmDrives(string name);

        /// <summary>
        /// Creates a new virtual disk.
        /// </summary>
        /// <param name="path">Specifies the path where the disk will be created.</param>
        /// <param name="sizeBytes">Specifies the size of the virtual disk in bytes.</param>
        /// <param name="blockSizeBytes">Specifies the block size of the disk in bytes.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void NewVhd(
            string      path,
            long        sizeBytes,
            int         blockSizeBytes);

        /// <summary>
        /// Resizes an existing virtual disk.
        /// </summary>
        /// <param name="path">Specifies the path to the existing virtual disk.</param>
        /// <param name="sizeBytes">Specifies the new size for the disk.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void ResizeVhd(
            string      path,
            long        sizeBytes);

        /// <summary>
        /// Mounts a virtual disk onto Hyper-V so it can be manitpulated.
        /// </summary>
        /// <param name="path">Specifies the path to an existing virtual disk.</param>
        /// <param name="readOnly">Optionally specifies that the disk should be mounted as <b>read-only</b>.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void MountVhd(string path, bool readOnly = false);

        /// <summary>
        /// Dismounts a virtual disk from Hyper-V.
        /// </summary>
        /// <param name="path">Specifies the path to the mounted disk.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void DismountVhd(string path);

        /// <summary>
        /// Optimizes a dynamic virtual disk by relocating blocks to the front of the
        /// disk file to squeeze out any unused blocks and then resizing the disk file
        /// to the end of the last used block, potentially reducing the physical size
        /// of the virtual disk on the host file system.
        /// </summary>
        /// <param name="path">Specifies the path to the already mounted disk file.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void OptimizeVhd(string path);

        /// <summary>
        /// Lists the existing virtual switches.
        /// </summary>
        /// <returns>The <see cref="VirtualSwitch"/> instances found.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        IEnumerable<VirtualSwitch> ListSwitches();

        /// <summary>
        /// Adds a new virtual switch.
        /// </summary>
        /// <param name="name">Specifies the switch name.</param>
        /// <param name="targetAdapter">Optionally identifies the network adapter where the switch will be attached.</param>
        /// <param name="internal">Optionally indicates that the switch type is to be <b>internal</b>.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void NewVmSwitch(
            string          name,
            string          targetAdapter = null,
            bool            @internal     = false);

        /// <summary>
        /// Removes a virtual switch.
        /// </summary>
        /// <param name="name">Specifies the name of the switch.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void RemoveVmSwitch(string name);

        /// <summary>
        /// Creates a new network NAT.
        /// </summary>
        /// <param name="name">Specifies the NAT name.</param>
        /// <param name="subnet">Specifies the NAT subnet.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void NewNetNat(string name, NetworkCidr subnet);

        /// <summary>
        /// Removes a network NAT.
        /// </summary>
        /// <param name="name">Specifies the NAT name.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void RemoveNetNat(string name);

        /// <summary>
        /// Lists any network NATs.
        /// </summary>
        /// <returns>The <see cref="VirtualNat"/> instances found.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        IEnumerable<VirtualNat> ListNats();

        /// <summary>
        /// Adds an IP address to an existing virtual switch.
        /// </summary>
        /// <param name="switchName">Specifies the name of the existing switch.</param>
        /// <param name="address">Specifies the new IP address.</param>
        /// <param name="subnet">Specifies the associated subnet.</param>
        /// <param name="interfaceAlias">Identifies the target network interface alias.</param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        void NewNetIPAddress(string switchName, IPAddress address, NetworkCidr subnet, string interfaceAlias);

        /// <summary>
        /// Lists the network adapters attached to a virtual machine.
        /// </summary>
        /// <param name="name">Specifies the virtual machine.</param>
        /// <param name="waitForAddresses">Optionally wait for the adapters to obtain their IP addresses.</param>
        /// <returns>The <see cref="VirtualNetworkAdapter"/> instances.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        IEnumerable<VirtualNetworkAdapter> ListVmNetworkAdapters(string name, bool waitForAddresses = false);
    }
}
