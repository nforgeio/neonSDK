//-----------------------------------------------------------------------------
// FILE:        Test_CodeSigner.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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
        private UsbTokenProfile usbTokenProfile;
        private AzureProfile    azureProfile;

        public Test_CodeSigner()
        {
#pragma warning disable CS0618 // Type or member is obsolete

            // Fetch the required secrets and construct the signing profiles.

            var profileClient = new MaintainerProfile();

            usbTokenProfile = new UsbTokenProfile(
                provider:     profileClient.GetSecretValue("CODESIGN_TOKEN[provider]",     vault: "group-devops"),
                certBase64:   profileClient.GetSecretValue("CODESIGN_TOKEN[pubcert]",      vault: "group-devops"),
                container:    profileClient.GetSecretValue("CODESIGN_TOKEN[container]",    vault: "group-devops"),
                timestampUri: profileClient.GetSecretValue("CODESIGN_TOKEN[timestampuri]", vault: "group-devops"),
                password:     profileClient.GetSecretValue("CODESIGN_TOKEN[password]",     vault: "group-devops"));

            azureProfile = new AzureProfile(
                azureTenantId:              profileClient.GetSecretValue("CODESIGN_AZURE[AZURE_TENANT_ID]", vault: "group-devops"),
                azureClientId:              profileClient.GetSecretValue("CODESIGN_AZURE[AZURE_CLIENT_ID]", vault: "group-devops"),
                azureClientSecret:          profileClient.GetSecretValue("CODESIGN_AZURE[AZURE_CLIENT_SECRET]", vault: "group-devops"),
                codeSigningAccountEndpoint: profileClient.GetSecretValue("CODESIGN_AZURE[CODESIGNING_ACCOUNT_ENDPOINT]", vault: "group-devops"),
                codeSigningAccountName:     profileClient.GetSecretValue("CODESIGN_AZURE[CODESIGNING_ACCOUNT_NAME]", vault: "group-devops"),
                certificateProfileName:     profileClient.GetSecretValue("CODESIGN_AZURE[CERTIFICATE_PROFILE_NAME]", vault: "group-devops"));

#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Fact(Skip = "Needs to be run manually by a maintainer")]
        //[Fact]
        public void IsReady()
        {
            Assert.True(CodeSigner.IsReady(usbTokenProfile));
        }

        //[MaintainerFact(Skip = "Needs to be run manually by a maintainer")]
        [MaintainerFact]
        public void Sign_WithAzure()
        {
            var signingCacheFolder = NeonHelper.NeonSdkAzureCodeSigningFolder;

            using (var tempFile = new TempFile(suffix: ".exe"))
            {
                ExtractTestBinaryTo(tempFile.Path);
                Assert.True(File.Exists(tempFile.Path));

                var beforeHash = CryptoHelper.ComputeMD5StringFromFile(tempFile.Path);

                CodeSigner.Sign(azureProfile, tempFile.Path);

                var afterHash = CryptoHelper.ComputeMD5StringFromFile(tempFile.Path);

                Assert.NotEqual(beforeHash, afterHash);

                // Perform another signing operation and verify that the cached 
                // client signing tools were not downloaded by verifying that none
                // of the cached files have been modified.

                var cachedFiles = new Dictionary<string, DateTime>();

                foreach (var file in Directory.GetFiles(signingCacheFolder, "*.*", SearchOption.AllDirectories))
                {
                    cachedFiles.Add(file, File.GetLastWriteTimeUtc(file));
                }

                ExtractTestBinaryTo(tempFile.Path);
                Assert.True(File.Exists(tempFile.Path));
                CodeSigner.Sign(azureProfile, tempFile.Path);

                foreach (var item in cachedFiles)
                {
                    Assert.Equal(item.Value, File.GetLastWriteTimeUtc(item.Key));
                }

                // Clear the signing cache folder, set the cached [version.txt] file to
                // an invalid version, and re-sign the binary to verify that we reinstall
                // the client signing tools.

                NeonHelper.DeleteFolderContents(signingCacheFolder);
                File.WriteAllText(Path.Combine(signingCacheFolder, "version.txt"), "INVALID");

                ExtractTestBinaryTo(tempFile.Path);
                Assert.True(File.Exists(tempFile.Path));

                beforeHash = CryptoHelper.ComputeMD5StringFromFile(tempFile.Path);

                CodeSigner.Sign(azureProfile, tempFile.Path);

                afterHash = CryptoHelper.ComputeMD5StringFromFile(tempFile.Path);

                Assert.NotEqual(beforeHash, afterHash);
            }
        }

        //[MaintainerFact(Skip = "Needs to be run manually by a maintainer")]
        [MaintainerFact]
        public void Sign_WithAzure_AndCorrelationId()
        {
            var profile = new AzureProfile(
                azureTenantId:              azureProfile.AzureTenantId,
                azureClientId:              azureProfile.AzureClientId,
                azureClientSecret:          azureProfile.AzureClientSecret,
                codeSigningAccountEndpoint: azureProfile.CodeSigningAccountEndpoint,
                codeSigningAccountName:     azureProfile.CodeSigningAccountName,
                certificateProfileName:     azureProfile.CertificateProfileName,
                correlationId:              "my-correlation-id");

            using (var tempFile = new TempFile(suffix: ".exe"))
            {
                ExtractTestBinaryTo(tempFile.Path);
                Assert.True(File.Exists(tempFile.Path));

                var beforeHash = CryptoHelper.ComputeMD5StringFromFile(tempFile.Path);

                CodeSigner.Sign(profile, tempFile.Path);

                var afterHash = CryptoHelper.ComputeMD5StringFromFile(tempFile.Path);

                Assert.NotEqual(beforeHash, afterHash);
            }
        }

        [MaintainerFact(Skip = "Needs to be run manually by a maintainer")]
        //[MaintainerFact]
        public void Sign_WithUsbToken()
        {
            // Verify that signing an executable actually changes the file.

            using (var tempFile = new TempFile(suffix: ".exe"))
            {
                ExtractTestBinaryTo(tempFile.Path);

                var beforeHash = CryptoHelper.ComputeMD5StringFromFile(tempFile.Path);

                CodeSigner.Sign(usbTokenProfile, tempFile.Path);

                var afterHash = CryptoHelper.ComputeMD5StringFromFile(tempFile.Path);

                Assert.NotEqual(beforeHash, afterHash);

                // Clear the signing cache folder, set the cached [version.txt] file to
                // an invalid version, and re-sign the binary to verify that we reinstall
                // the client signing tools.

                var signingCacheFolder = NeonHelper.NeonSdkUsbCodeSigningFolder;

                NeonHelper.DeleteFolderContents(signingCacheFolder);
                File.WriteAllText(Path.Combine(signingCacheFolder, "version.txt"), "INVALID");

                ExtractTestBinaryTo(tempFile.Path);
                Assert.True(File.Exists(tempFile.Path));

                beforeHash = CryptoHelper.ComputeMD5StringFromFile(tempFile.Path);

                CodeSigner.Sign(azureProfile, tempFile.Path);

                afterHash = CryptoHelper.ComputeMD5StringFromFile(tempFile.Path);

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
