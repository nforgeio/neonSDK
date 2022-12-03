//-----------------------------------------------------------------------------
// FILE:	    Test_WmiHyperV.Disk.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

using Neon.Common;
using Neon.Cryptography;
using Neon.Deployment;
using Neon.IO;
using Neon.HyperV;
using Neon.Xunit;

namespace TestHyperV
{
    public partial class Test_WmiHyperV
    {
        private const long  diskSize  = (long) (100 * ByteUnits.MebiBytes);
        private const int   blockSize = (int) ( 1 * ByteUnits.MebiBytes);

        [Fact]
        public void VHD_ValidateVHD()
        {
            using (var wmiClient = new WmiHyperVClient())
            {
                using (var tempDisk = HyperVTestHelper.CreateTempAlpineVhdx())
                {
                    // Validate a good disk image.

                    wmiClient.ValidateVHD(tempDisk.Path);

                    // Detect a bad disk image.

                    File.WriteAllBytes(tempDisk.Path, new byte[4096]);
                    Assert.Throws<HyperVException>(() => wmiClient.ValidateVHD(tempDisk.Path));
                }
            }
        }

        [Fact]
        public void VHD_New_Fixed()
        {
            using (var wmiClient = new WmiHyperVClient())
            {
                using (var tempDisk = new TempFile(suffix: ".vhd"))
                {
                    // Create a new VHD.

                    wmiClient.NewVHD(tempDisk.Path, dynamic: false, diskSize, blockSize);

                    // Validate the new disk.

                    wmiClient.ValidateVHD(tempDisk.Path);
                }
            }
        }

        [Fact]
        public void VHD_New_Dynamic()
        {
            using (var wmiClient = new WmiHyperVClient())
            {
                using (var tempDisk = new TempFile(suffix: ".vhd"))
                {
                    // Create a new VHD.

                    wmiClient.NewVHD(tempDisk.Path, dynamic: true, diskSize, blockSize);

                    // Validate the new disk.

                    wmiClient.ValidateVHD(tempDisk.Path);
                }
            }
        }

        [Fact]
        public void VHDX_New_Fixed()
        {
            using (var wmiClient = new WmiHyperVClient())
            {
                using (var tempDisk = new TempFile(suffix: ".vhdx"))
                {
                    // Create a new VHD.

                    wmiClient.NewVHD(tempDisk.Path, dynamic: false, diskSize, blockSize);

                    // Validate the new disk.

                    wmiClient.ValidateVHD(tempDisk.Path);
                }
            }
        }

        [Fact]
        public void VHDX_New_Dynamic()
        {
            using (var wmiClient = new WmiHyperVClient())
            {
                using (var tempDisk = new TempFile(suffix: ".vhdx"))
                {
                    // Create a new VHD.

                    wmiClient.NewVHD(tempDisk.Path, dynamic: true, diskSize, blockSize);

                    // Validate the new disk.

                    wmiClient.ValidateVHD(tempDisk.Path);
                }
            }
        }

        [Fact]
        public void VHD_Resize()
        {
            using (var wmiClient = new WmiHyperVClient())
            {
                using (var tempDisk = new TempFile(suffix: ".vhd"))
                {
                    // Create a new VHD and validate.

                    wmiClient.NewVHD(tempDisk.Path, dynamic: true, diskSize);
                    wmiClient.ValidateVHD(tempDisk.Path);

                    // Resize the disk and validate.

                    wmiClient.ResizeVHD(tempDisk.Path, diskSize * 2);
                    wmiClient.ValidateVHD(tempDisk.Path);
                }
            }
        }

        [Fact]
        public void VHDX_Resize()
        {
            using (var wmiClient = new WmiHyperVClient())
            {
                using (var tempDisk = new TempFile(suffix: ".vhdx"))
                {
                    // Create a new VHD and validate.

                    wmiClient.NewVHD(tempDisk.Path, dynamic: true, diskSize);
                    wmiClient.ValidateVHD(tempDisk.Path);

                    // Resize the disk and validate.

                    wmiClient.ResizeVHD(tempDisk.Path, diskSize * 2);
                    wmiClient.ValidateVHD(tempDisk.Path);
                }
            }
        }

        [Fact]
        public void VHD_Compact()
        {
            using (var wmiClient = new WmiHyperVClient())
            {
                using (var tempDisk = new TempFile(suffix: ".vhd"))
                {
                    // Create a new VHD and validate.

                    wmiClient.NewVHD(tempDisk.Path, dynamic: true, diskSize);
                    wmiClient.ValidateVHD(tempDisk.Path);

                    // Compact the disk and validate.

                    wmiClient.CompactVHD(tempDisk.Path);
                    wmiClient.ValidateVHD(tempDisk.Path);
                }
            }
        }

        [Fact]
        public void VHDX_Compact()
        {
            using (var wmiClient = new WmiHyperVClient())
            {
                using (var tempDisk = new TempFile(suffix: ".vhdx"))
                {
                    // Create a new VHD and validate.

                    wmiClient.NewVHD(tempDisk.Path, dynamic: true, diskSize);
                    wmiClient.ValidateVHD(tempDisk.Path);

                    // Compact the disk and validate.

                    wmiClient.CompactVHD(tempDisk.Path);
                    wmiClient.ValidateVHD(tempDisk.Path);
                }
            }
        }
    }
}
