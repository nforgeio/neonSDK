//-----------------------------------------------------------------------------
// FILE:	    HyperVPowershell.cs
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
    /// Uses Powershell to implement <see cref="IHyperV"/>.
    /// </summary>
    internal class HyperVPowershell : IHyperV
    {
        /// <inheritdoc/>
        public void AddVM(string name, long startupMemoryBytes, int generation = 1)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void AddVmDvdDrive(string name, string path, int controllerLocation, int controllerNumber)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void AddVmHardDiskDrive(string name, string path)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void DismountVhd(string path)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void EnableVmNestedVirtualization(string name)
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
        public IEnumerable<string> ListVmDrives(string name)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualNetworkAdapter> ListVmNetworkAdapters(string name, bool waitForAddresses = false)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualMachine> ListVms()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void MountVhd(string path, bool readOnly = false)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void NewNetIPAddress(string switchName, IPAddress address, NetworkCidr subnet, string interfaceAlias)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void NewNetNat(string name, NetworkCidr subnet)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void NewVhd(string path, long sizeBytes, int blockSizeBytes)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void NewVmSwitch(string name, string targetAdapter = null, bool @internal = false)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void OptimizeVhd(string path)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveNetNat(string name)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveVm(string name)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveVmDvdDrive(string name, int? controllerLocation, int? controllerNumber)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveVmSwitch(string name)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void ResizeVhd(string path, long sizeBytes)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SaveVm(string name)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetVM(string name, int? processorCount = null, long? startupMemoryBytes = null, bool? checkpointDrives = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void StartVm(string name)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void StopVm(string name, bool turnOff = false)
        {
            throw new NotImplementedException();
        }
    }
}
