//-----------------------------------------------------------------------------
// FILE:	    HyperVTestHelper.cs
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
using Neon.Xunit;
using System.Diagnostics.Contracts;

namespace TestHyperV
{
    /// <summary>
    /// Test helpers.
    /// </summary>
    public static class HyperVTestHelper
    {
        /// <summary>
        /// Creates a temporary VHDX file holding a small (~4MiB) Alpine virtual machine
        /// image.  This is copied from the git repo: <b>$/External/alpine.vhdx</b>
        /// </summary>
        /// <returns>The <see cref="TempFile"/>.</returns>
        public static TempFile CopyTempAlpineVhdx()
        {
            var tempFile = new TempFile(suffix: ".vhdx");

            File.Copy(Path.Combine(Environment.GetEnvironmentVariable("NF_ROOT"), "External", "alpine.vhdx"), tempFile.Path);

            return tempFile;
        }
    }
}
