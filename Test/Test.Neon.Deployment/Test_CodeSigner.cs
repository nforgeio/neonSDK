//-----------------------------------------------------------------------------
// FILE:        Test_CodeSigner.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Neon.Common;
using Neon.Cryptography;
using Neon.Deployment;
using Neon.Deployment.CodeSigning;
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
        private UsbTokenProfile signToolProfile;

        public Test_CodeSigner()
        {
#pragma warning disable CS0618 // Type or member is obsolete

            // Fetch the required secrets and construct the signing profiles.

            var profileClient = new MaintainerProfile();

            signToolProfile = new UsbTokenProfile(
                provider:     profileClient.GetSecretValue("codesign_token[provider]",     vault: "group-devops"),
                certBase64:   profileClient.GetSecretValue("codesign_token[pubcert]",      vault: "group-devops"),
                container:    profileClient.GetSecretValue("codesign_token[container]",    vault: "group-devops"),
                timestampUri: profileClient.GetSecretValue("codesign_token[timestampuri]", vault: "group-devops"),
                password:     profileClient.GetSecretValue("codesign_token[password]",     vault: "group-devops"));

#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Fact(Skip = "Needs to be run manually by a maintainer")]
        //[Fact]
        public void IsReady()
        {
            Assert.True(CodeSigner.IsReady(signToolProfile));
        }

        [MaintainerFact(Skip = "Needs to be run manually by a maintainer")]
        //[MaintainerFact]
        public void Sign()
        {
            // Verify that signing an executable actually changes the file.

            using (var tempFile = new TempFile(suffix: ".exe"))
            {
                ExtractTestBinaryTo(tempFile.Path);

                var beforeHash = CryptoHelper.ComputeMD5StringFromFile(tempFile.Path);

                CodeSigner.Sign(signToolProfile, tempFile.Path);

                var afterHash = CryptoHelper.ComputeMD5StringFromFile(tempFile.Path);

                Assert.NotEqual(beforeHash, afterHash);
            }
        }

        /// <summary>
        /// Extracts the <b>signee.exe</b> binary from the embedded resource
        /// to the specified path.
        /// </summary>
        /// <param name="targetPath">The target path for the binary.</param>
        private static void ExtractTestBinaryTo(string targetPath)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(targetPath));

            var assembly = Assembly.GetExecutingAssembly();

            using (var toolStream = assembly.GetManifestResourceStream("TestDeployment.Resources.Windows.signee.exe"))
            {
                using (var output = File.Create(targetPath))
                {
                    toolStream.CopyTo(output);
                }
            }
        }
    }
}
