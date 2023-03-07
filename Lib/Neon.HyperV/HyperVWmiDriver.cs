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
    /// Uses WMI to implement <see cref="IHyperVDriver"/>.
    /// </summary>
    internal sealed class HyperVWmiDriver : IHyperVDriver
    {
        private HyperVClient    client;

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

        /// <inheritdoc/>
        public void NewVM(
            string      machineName, 
            long        startupMemoryBytes,
            int         generation = 1,
            string      drivePath  = null,
            string      switchName = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void AddVmDrive(string machineName, string drivePath)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void DismountVhd(string drivePath)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void EnableVmNestedVirtualization(string machineName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds a DVD drive to a virtual machine.
        /// </summary>
        /// <param name="machineName">Specifies the virtual machine name.</param>
        /// <param name="isoPath">Specifies the path to the existing virtual DVD drive (ISO file).</param>
        /// <param name="controllerLocation">
        /// Specifies the number of the location on the controller at which the DVD drive
        /// is to be added.
        /// </param>
        /// <param name="controllerNumber">
        /// Specifies the number of the controller to which the DVD drive is to be added.
        /// </param>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public void AddVmDvdDrive(
            string      machineName,
            string      isoPath,
            int         controllerLocation,
            int         controllerNumber)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualNat> ListNats()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualSwitch> ListSwitches()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<string> ListVmDrives(string machineName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualNetworkAdapter> ListVmNetAdapters(string machineName, bool waitForAddresses = false)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualMachine> ListVms()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void MountVhd(string drivePath, bool readOnly = false)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void NewNetIPAddress(string switchName, IPAddress address, NetworkCidr subnet)
        {
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
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void NewSwitch(string switchName, string targetAdapter = null, bool @internal = false)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void OptimizeVhd(string drivePath)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveNat(string natName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveVm(string switchName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveVmDvdDrive(string machineName, int controllerLocation = 0, int controllerNumber = 1)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveSwitch(string switchName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void ResizeVhd(string drivePath, long sizeBytes)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SaveVm(string drivePath)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetVM(string machineName, int? processorCount = null, long? startupMemoryBytes = null, bool? checkpointDrives = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void StartVm(string machineName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void StopVm(string machineName, bool turnOff = false)
        {
            throw new NotImplementedException();
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
