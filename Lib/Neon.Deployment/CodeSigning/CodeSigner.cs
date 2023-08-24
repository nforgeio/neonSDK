//-----------------------------------------------------------------------------
// FILE:        CodeSigner.cs
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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;

using ICSharpCode.SharpZipLib.Zip;

using Neon.Common;
using Neon.Deployment.CodeSigning;
using Neon.Diagnostics;
using Neon.IO;

namespace Neon.Deployment.CodeSigning
{
    /// <summary>
    /// Implements code signing.
    /// </summary>
    public static class CodeSigner
    {
        /// <summary>
        /// Verifies that the current machine is ready for code signing using a USB code signing certificate and the Microsoft Built Tools <b>signtool</b> program.
        /// </summary>
        /// <param name="profile">Specifies a <see cref="UsbTokenProfile"/> with the required signing prarameters.</param>
        /// <returns><c>true</c> when the signing token is available and the profile ius correct.</returns>
        /// <exception cref="PlatformNotSupportedException">Thrown when executed on a non 64-bit Windows machine.</exception>
        /// <remarks>
        /// <note>
        /// <b>WARNING!</b> Be very careful when using this method with Extended Validation (EV) code signing 
        /// USB tokens.  Using an incorrect password can brkick EV tokens since thay typically allow only a 
        /// very limited number of signing attempts with invalid passwords.
        /// </note>
        /// </remarks>
        public static bool IsReady(UsbTokenProfile profile)
        {
            Covenant.Requires<ArgumentNullException>(profile != null, nameof(profile));
            Covenant.Requires<PlatformNotSupportedException>(NeonHelper.IsWindows && NeonHelper.Is64BitOS, "This is supported only for 64-bit Windows.");

            // We're going to verify that code signing can complete by signing
            // a copy of a small embedded executable.  This verifies that the parameters
            // are correct and also that the code-signing token is actually available.

            try
            {
                using (var tempFile = new TempFile(suffix: ".exe"))
                {
                    ExtractTestBinaryTo(tempFile.Path);
                    Sign(profile, tempFile.Path);
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Signs an EXE, DLL or MSI file using a USB code signing certificate and the <b>SignTool</b> from the Microsoft Built Tools.
        /// </summary>
        /// <param name="profile">Specifies a <see cref="UsbTokenProfile"/> with the required signing prarameters.</param>
        /// <param name="targetPath">Specifies the path to the file being signed.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown when executed on a non 64-bit Windows machine.</exception>
        /// <remarks>
        /// <note>
        /// <b>WARNING!</b> Be very careful when using this method with Extended Validation (EV) code signing 
        /// USB tokens.  Using an incorrect password can brick EV tokens since thay typically allow only a 
        /// very limited number of signing attempts with invalid passwords.
        /// </note>
        /// </remarks>
        public static void Sign(UsbTokenProfile profile, string targetPath)
        {
            Covenant.Requires<ArgumentNullException>(profile != null, nameof(profile));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(targetPath), nameof(targetPath));

            // Strip out any CR/LFs from the certificate base64, convert to bytes 
            // and write to a temporary file so we'll be able to pass its path
            // to signtool.exe

            var certBase64 = profile.CertBase64;

            certBase64 = profile.CertBase64.Replace("\r", string.Empty);
            certBase64 = certBase64.Replace("\n", string.Empty);

            using (var tempFolder = new TempFolder())
            {
                var tempCertPath = Path.Combine(tempFolder.Path, "certificate.cer");
                var signToolPath = InstallSignTool(tempFolder.Path);

                File.WriteAllBytes(tempCertPath, Convert.FromBase64String(certBase64));

                NeonHelper.ExecuteCapture(signToolPath,
                    new object[]
                    {
                        "sign",
                        "/f", tempCertPath,
                        "/fd", "sha256",
                        "/tr", profile.TimestampUri,
                        "/td", "sha256",
                        "/csp", profile.Provider,
                        "/k", $"[{{{{{profile.Password}}}}}]={profile.Container}",
                        targetPath
                    })
                .EnsureSuccess();
            }
        }

        /// <summary>
        /// Signs an EXE, DLL or MSI file using Azure Code Signing using the <b>AzureSignTool</b>.
        /// </summary>
        /// <param name="profile">Specifies a <see cref="UsbTokenProfile"/> with the required signing prarameters.</param>
        /// <param name="targetPath">Specifies the path to the file being signed.</param>
        /// <exception cref="PlatformNotSupportedException">Thrown when executed on a non 64-bit Windows machine.</exception>
        public static void Sign(AzureProfile profile, string targetPath)
        {
            Covenant.Requires<ArgumentNullException>(profile != null, nameof(profile));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(targetPath), nameof(targetPath));

            using (var tempFolder = new TempFolder())
            {
                // Verify that a .NET CORE 6.x runtime is installed.

                var response = NeonHelper.ExecuteCapture("dotnet", new object[] { "--list-runtimes" })
                    .EnsureSuccess();

                if (!response.OutputText.ToLines().Any(line => line.StartsWith("Microsoft.NETCore.App 6.")))
                {
                    throw new NotSupportedException(".NET 6.x runtime is required to use Azure Code Signing.");
                }

                // Install the SignTool and the signing DLL to the temp folder.

                var signToolPath = InstallSignTool(tempFolder.Path);
                var signDllPath  = InstallSigningDll(tempFolder.Path);

                // Create the metadata file.

                var metadataPath = Path.Combine(tempFolder.Path, "metadata.json");
                var metadata     =
$@"{{
    ""Endpoint"": ""{profile.CodeSigningAccountEndpoint}"",
    ""CodeSigningAccountName"": ""{profile.CodeSigningAccountName}"",
    ""CertificateProfileName"": ""{profile.CertificateProfileName}""
}}
";
                File.WriteAllText(metadataPath, metadata);

                // We're going to present the [code-signer] Azure service principal
                // credentials to SignTool as environment variables.

                var azureCredentials = new Dictionary<string, string>()
                {
                    { "AZURE_TENANT_ID", profile.AzureTenantId },
                    { "AZURE_CLIENT_ID", profile.AzureClientId },
                    { "AZURE_CLIENT_SECRET", profile.AzureClientSecret}
                };

                // Ensure that the referenced files actually exist.

                Covenant.Assert(File.Exists(signToolPath), $"signtool not found: {signToolPath}");
                Covenant.Assert(File.Exists(signDllPath), $"signing DLL not found: {signDllPath}");
                Covenant.Assert(File.Exists(metadataPath), $"metadata file not found: {metadataPath}");
                Covenant.Assert(File.Exists(targetPath), $"target file not found: {targetPath}");

                // Sign the binary.

                response = NeonHelper.ExecuteCapture(signToolPath,
                    new object[]
                    {
                        "sign",
                        "/debug",
                        "/v",
                        "/fd", "SHA256",
                        "/tr", "http://timestamp.acs.microsoft.com",
                        "/td", "SHA256",
                        "/dlib", signDllPath,
                        "/dmdf", metadataPath,
                        targetPath
                    },
                    environmentVariables: azureCredentials);

                response.EnsureSuccess();
            }
        }

        /// <summary>
        /// Downloads and installs the <b>SignTool</b> binary.
        /// </summary>
        /// <param name="installFolder">The folder where the tool will be installed.</param>
        /// <returns>The path to the <b>SignTool</b> binary.</returns>
        private static string InstallSignTool(string installFolder)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(installFolder));

            Directory.CreateDirectory(installFolder);

            // We're going to use the nuget CLI to install the Microsoft Build Tools
            // within the folder specified which creates some subfolders.  The method
            // returns the path to the Windows x64 version of the SignTool binary.

            const string buildToolsVersion = "10.0.20348.19";

            NeonHelper.ExecuteCapture("nuget",
                new object[]
                {
                    "install",
                    "Microsoft.Windows.SDK.BuildTools",
                    "-Version", buildToolsVersion,
                    "-o", installFolder
                })
                .EnsureSuccess();

            var version       = Version.Parse(buildToolsVersion);
            var signToolPath  = Path.Combine(installFolder, $"Microsoft.Windows.SDK.BuildTools.{buildToolsVersion}", "bin", $"{version.Major}.{version.Minor}.{version.Build}.0", "x64", "signtool.exe");

            if (!File.Exists(signToolPath))
            {
                throw new FileNotFoundException($"SignTool not found: {signToolPath}");
            }

            return signToolPath;
        }

        /// <summary>
        /// Downloads and installs the <b>Azure.CodeSigning.Dlib</b> DLL.
        /// </summary>
        /// <param name="installFolder">The folder where the DLL will be installed.</param>
        /// <returns>The path to the installed DLL.</returns>
        private static string InstallSigningDll(string installFolder)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(installFolder));

            Directory.CreateDirectory(installFolder);

            // $note(jefflill):
            //
            // I've uploaded the code signing DLL and related files as a ZIP
            // to our [s3://neon-public/build-assets] bucket folder.

            // Download the [Azure.CodeSigning.Dlib] ZIP file to the install folder. 

            const string version = "1.0.28";
            const string zipFile = $"Azure.CodeSigning.Dlib.{version}.zip";
            const string zipUri  = $"https://neon-public.s3.us-west-2.amazonaws.com/build-assets/{zipFile}";

            var zipPath = Path.Combine(installFolder, zipFile);

            using (var httpClient = new HttpClient())
            {
                using (var zipStream = File.OpenWrite(zipPath))
                {
                    var request  = new HttpRequestMessage(HttpMethod.Get, zipUri);
                    var response = httpClient.SendAsync(request).Result;

                    response.EnsureSuccessStatusCode();
                    response.Content.CopyToAsync(zipStream).Wait();
                }
            }

            // Extract the Azure.CodeSigning.Dlib ZIP file to the install folder.

            new FastZip().ExtractZip(zipPath, Path.Combine(installFolder, "AzureCodeSigning"), fileFilter: null);

            // Return the path the x64 version of the unzipped Azure.CodeSigning.Dlib.dll file.

            var dllPath = Path.Combine(installFolder, "AzureCodeSigning", "bin", "x64", "Azure.CodeSigning.Dlib.dll");

            if (!File.Exists(dllPath))
            {
                throw new FileNotFoundException($"Signing DLL not found: {dllPath}");
            }

            return dllPath;
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

            using (var toolStream = assembly.GetManifestResourceStream("Neon.Deployment.Resources.Windows.signee.exe"))
            {
                using (var output = File.Create(targetPath))
                {
                    toolStream.CopyTo(output);
                }
            }
        }
    }
}
