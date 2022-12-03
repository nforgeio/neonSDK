//-----------------------------------------------------------------------------
// FILE:	    WmiHyperVClient.VHD.cs
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
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime;
using System.Text;
using System.Threading;

using Neon.Common;
using Neon.Diagnostics;
using static System.Net.WebRequestMethods;

namespace Neon.HyperV
{
    internal partial class WmiHyperVClient
    {
        /// <summary>
        /// Validates a VHD or VHDX disk image.
        /// </summary>
        /// <param name="path">Path to the disk image file.</param>
        /// <exception cref="HyperVException">Thrown when the disk is not valid.</exception>
        public void ValidateVHD(string path)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path));
            CheckDisposed();

            InvokeJob(WmiClass.ImageManagementService, "ValidateVirtualHardDisk",
                new Dictionary<string, object>()
                {
                    { "Path", path }
                });
        }

        /// <summary>
        /// Creates a new virtual drive.
        /// </summary>
        /// <param name="path">
        /// <para>
        /// Specifies the path for the new virtual disk file.
        /// </para>
        /// <note>
        /// The file extension must be <b>".vhd"</b> or <b>".vhdx"</b> and this determines
        /// the type of virtual drive being created.
        /// </note>
        /// </param>
        /// <param name="dynamic">Specifies that the disk will be dynamically sized vs. fixed.</param>
        /// <param name="size">
        /// Specifies the maximum disk size in bytes.  This cannot be less than 100 MiB and 
        /// VHD disks are limited mto 1 TiB and VHDX disks are limited to 64 TiB.
        /// </param>
        /// <param name="blockSize">
        /// Optionally specifies the underlying block size to be used for VHDX disks.
        /// This defaults to a reasonable value and is ignored for VHD disks.
        /// </param>
        /// <exception cref="HyperVException"></exception>
        public void NewVHD(string path, bool dynamic, long size, int blockSize = 0)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path));
            Covenant.Requires<ArgumentException>(size >= 100 * ByteUnits.MebiBytes, nameof(size), "Virtual disks must be at least [100 MiB].");
            Covenant.Requires<ArgumentException>(blockSize >= 0, nameof(blockSize));

            var extension = Path.GetExtension(path).ToLowerInvariant();

            Covenant.Requires<ArgumentException>(extension == ".vhd" || extension == ".vhdx", nameof(path));
            Covenant.Requires<ArgumentException>(size > 0, nameof(size));
            CheckDisposed();

            var vhdFormat = extension == ".vhd";

            if (vhdFormat)
            {
                Covenant.Requires<ArgumentException>(size <= 1 * ByteUnits.TebiBytes, nameof(size), "VHD disks can not excceed [1 TiB].");
            }
            else
            {
                Covenant.Requires<ArgumentException>(size <= 64 * ByteUnits.TebiBytes, nameof(size), "VHDX disks can not excceed [64 TiB].");
            }

            var service = GetService(WmiClass.ImageManagementService);
            var values  = new Dictionary<string, object>()
            {
                { "Format",          vhdFormat ? WmiDiskFormat.VHD : WmiDiskFormat.VHDX },
                { "MaxInternalSize", (ulong)size },
                { "Path",            path },
                { "Type",            dynamic ? WmiDiskType.Dynamic : WmiDiskType.Fixed },
            };

            if (!vhdFormat && blockSize > 0)
            {
                values["BlockSize"] = (uint)blockSize;
            }

            using (var settings = CreateSettings(service, WmiClass.VirtualHardDiskSettingData, values))
            {
                InvokeJob(WmiClass.ImageManagementService, "CreateVirtualHardDisk",
                    new Dictionary<string, object>()
                    {
                        { "VirtualDiskSettingData", settings.GetText(TextFormat.WmiDtd20) }
                    });
            }
        }

        /// <summary>
        /// Resizes a virtual disk.
        /// </summary>
        /// <param name="path">Path to the virtual disk file.</param>
        /// <param name="size">
        /// Specifies the new maximim disk size in bytes.  This cannot be less than 100 MiB and 
        /// VHD disks are limited mto 1 TiB and VHDX disks are limited to 64 TiB.
        /// </param>
        /// <exception cref="HyperVException"></exception>
        public void ResizeVHD(string path, long size)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path));
            Covenant.Requires<ArgumentException>(size >= 100 * ByteUnits.MebiBytes, nameof(size), "Virtual disks must be at least [100 MiB].");

            var extension = Path.GetExtension(path).ToLowerInvariant();

            Covenant.Requires<ArgumentException>(extension == ".vhd" || extension == ".vhdx", nameof(path));
            Covenant.Requires<ArgumentException>(size > 0, nameof(size));
            CheckDisposed();

            var vhdFormat = extension == ".vhd";

            if (vhdFormat)
            {
                Covenant.Requires<ArgumentException>(size <= 1 * ByteUnits.TebiBytes, nameof(size), "VHD disks can not excceed [1 TiB].");
            }
            else
            {
                Covenant.Requires<ArgumentException>(size <= 64 * ByteUnits.TebiBytes, nameof(size), "VHDX disks can not excceed [64 TiB].");
            }

            InvokeJob(WmiClass.ImageManagementService, "ResizeVirtualHardDisk",
                new Dictionary<string, object>()
                {
                    { "Path", path },
                    { "MaxInternalSize", size }
                });
        }

        /// <summary>
        /// <para>
        /// Compacts a virtual disk using <b>full</b> compaction mode.
        /// </para>
        /// <note>
        /// Only these disk types may be compacted: Fixed VHDX, Dynamic VHD,
        /// Dynamic VHDX, Differencing VHD, and Differencing VHDX.
        /// </note>
        /// </summary>
        /// <param name="path">Path to the virtual disk file.</param>
        /// <exception cref="HyperVException"></exception>
        public void CompactVHD(string path)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path));

            var extension = Path.GetExtension(path).ToLowerInvariant();

            Covenant.Requires<ArgumentException>(extension == ".vhd" || extension == ".vhdx", nameof(path));
            CheckDisposed();

            InvokeJob(WmiClass.ImageManagementService, "CompactVirtualHardDisk",
                new Dictionary<string, object>()
                {
                    { "Path", path },
                    { "Mode", WmiDiskCompactMode.Full }
                });
        }
    }
}
