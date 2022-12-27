//-----------------------------------------------------------------------------
// FILE:        Test_Git.cs
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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Git;
using Neon.IO;
using Neon.Xunit;

using Xunit;

namespace TestCryptography
{
    [Trait(TestTrait.Category, TestArea.NeonGit)]
    public class Test_Git
    {
        /// <summary>
        /// Identifies the GitHub repo we'll use for testing the <b>Neon.Git</b> library.
        /// </summary>
        private const string TestRepo = "neontest/neon-git";

        [Fact]
        public async Task Clone()
        {
            // Verify that we can clone the repo to a local temporary folder.

            using (var tempFolder = new TempFolder())
            {
            }

            await Task.CompletedTask;
        }
    }
}
