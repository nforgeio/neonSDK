//-----------------------------------------------------------------------------
// FILE:	    Test_CodeSigner.cs
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Neon.Common;
using Neon.Cryptography;
using Neon.Deployment;
using Neon.IO;
using Neon.Xunit;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

// $note(jefflill):
//
// These tests should be skipped most of the time and be only executed
// manually by maintainers who have [neon-assistant] configured and is
// logged into a machine with a code signing token.

namespace TestDeployment
{
    [Trait(TestTrait.Category, TestArea.NeonDeployment)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public partial class Test_CodeSigner
    {
        private readonly string     signeePath;
        private readonly string     provider;
        private readonly string     certBase64;
        private readonly string     container;
        private readonly string     timestampUri;
        private readonly string     password;

        public Test_CodeSigner()
        {
#pragma warning disable CS0618 // Type or member is obsolete

            // Get the path to the binary we'll copy and sign for testing.

            signeePath = Path.Combine(NeonHelper.GetAssemblyFolder(Assembly.GetExecutingAssembly()), "signee.exe");

            Assert.True(File.Exists(signeePath));

            // Fetch the required secrets.

            var profileClient = new MaintainerProfileClient();

            provider     = profileClient.GetSecretValue("codesign_token[provider]",     vault: "group-devops");
            certBase64   = profileClient.GetSecretValue("codesign_token[pubcert]",      vault: "group-devops");
            container    = profileClient.GetSecretValue("codesign_token[container]",    vault: "group-devops");
            timestampUri = profileClient.GetSecretValue("codesign_token[timestampuri]", vault: "group-devops");
            password     = profileClient.GetSecretValue("codesign_token[password]",     vault: "group-devops");

#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Copies the [signee.exe] binary to the path specified.
        /// </summary>
        /// <param name="targetPath">Specifies the target path.</param>
        private void CopySigneeTo(string targetPath)
        {
            File.Copy(signeePath, targetPath);
        }

        [Fact(Skip = "Needs to be run manually by a maintainer")]
        // [Fact]
        public void IsReady()
        {
            Assert.True(CodeSigner.IsReady(provider, certBase64, container, timestampUri, password));
        }

        [Fact(Skip = "Needs to be run manually by a maintainer")]
        // [Fact]
        public void Sign()
        {
            // Verify that signing an executable actually changes the file.

            using (var tempFile = new TempFile(suffix: ".exe"))
            {
                CopySigneeTo(tempFile.Path);

                var beforeHash = CryptoHelper.ComputeMD5StringFromFile(tempFile.Path);

                CodeSigner.SignBinary(tempFile.Path, provider, certBase64, container, timestampUri, password);

                var afterHash = CryptoHelper.ComputeMD5StringFromFile(tempFile.Path);

                Assert.NotEqual(beforeHash, afterHash);
            }
        }
    }
}
