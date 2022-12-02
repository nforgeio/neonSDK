//-----------------------------------------------------------------------------
// FILE:	    HyperVWmi.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
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
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;

using Neon.Common;
using Neon.Diagnostics;

namespace Neon.HyperV
{
    /// <summary>
    /// Abstracts access to the low-level Hyper-V WMI capabilities used to implement <see cref="HyperVClient"/>.
    /// </summary>
    internal sealed partial class WmiHyperVClient : IDisposable
    {
        private ManagementScope     scope = new ManagementScope(@"root\virtualization\v2");

        /// <inheritdoc/>
        public void Dispose()
        {
            scope = null;
        }

        /// <summary>
        /// Ensures that the instance is no0t disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the instance has been disposed.</exception>
        private void CheckDisposed()
        {
            if (scope == null)
            {
                throw new ObjectDisposedException(nameof(WmiHyperVClient));
            }
        }

        /// <summary>
        /// Validates a VHD or VHDX disk image.
        /// </summary>
        /// <param name="path">Path to the disk image file.</param>
        /// <exception cref="HyperVException">Thrown when the disk is not valid.</exception>
        public void ValidateDisk(string path)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path));
            CheckDisposed();

            InvokeJob(WmiServiceClassName.ImageManagement, "ValidateVirtualHardDisk", 
                new Dictionary<string, object>() 
                { 
                    { "Path", path } 
                });
        }
    }
}
