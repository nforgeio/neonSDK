//-----------------------------------------------------------------------------
// FILE:	    CodeSigner.cs
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
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

using Neon.Common;
using Neon.IO;

namespace Neon.Deployment
{
    /// <summary>
    /// Implements code signing.
    /// </summary>
    public static class CodeSigner
    {
        /// <summary>
        /// Signs an EXE or MSI file using a code signing certificate and the Windows
        /// <b>signtool.exe</b> program.
        /// </summary>
        /// <param name="targetPath">Specifies the path to the file being signed.</param>
        /// <param name="provider">Specifies the certificate provider, like: "eToken Base Cryptographic Provider"</param>
        /// <param name="thumbPrint">Specifies the certificate's SHA1 thumbprint.</param>
        /// <param name="certBase64">Specifies the base64 encoded public certificate (multi-line values are allowed).</param>
        /// <param name="container">Specifies the certificate container, like: "Sectigo_20220830143311"</param>
        /// <param name="password">Specifies the certificate password.</param>
        /// <param name="timestampUri">Specifies the URI for the certificate timestamp service, like: http://timestamp.sectigo.com</param>
        /// <param name="signToolPath">Optionally specifies the path to the <b>signtool.exe</b>.  We'll look for this on the path by default.</param>
        public static void Sign(
            string targetPath, 
            string provider, 
            string thumbPrint, 
            string certBase64, 
            string container, 
            string password, 
            string timestampUri,
            string signToolPath = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(targetPath), nameof(targetPath));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(provider), nameof(provider));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(thumbPrint), nameof(thumbPrint));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(certBase64), nameof(certBase64));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(container), nameof(container));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(password), nameof(password));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(timestampUri), nameof(timestampUri));

            signToolPath ??= "signtool.exe";

            // Strip out any CR/LFs from the certificate base64, convert to bytes 
            // and write to a temporary file so we'll be able to pass its path
            // to signtool.exe

            certBase64 = certBase64.Replace("\r", string.Empty);
            certBase64 = certBase64.Replace("\n", string.Empty);

            using (var tempCertFile = new TempFile())
            {
                File.WriteAllBytes(tempCertFile.Path, Convert.FromBase64String(certBase64));

                NeonHelper.ExecuteCapture(signToolPath,
                new object[]
                {
                    "sign",
                    "/f", tempCertFile.Path,
                    "/fd", "sha256",
                    "/tr", timestampUri,
                    "/td", "sha256",
                    "/csp", provider,
                    "/sha1", thumbPrint,
                    "/k", $"[{{{{{password}}}}}]={container}",
                    targetPath
                })
                .EnsureSuccess();
            }
        }
    }
}
