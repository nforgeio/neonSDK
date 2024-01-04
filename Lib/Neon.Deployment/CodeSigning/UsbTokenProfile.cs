// -----------------------------------------------------------------------------
// FILE:	    UsbTokenProfile.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
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
using System.Text;

using Neon.Common;

namespace Neon.Deployment.CodeSigning
{
    /// <summary>
    /// Defines parameters required for code signing using SignTool with a USB code signing token.
    /// </summary>
    public class UsbTokenProfile
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="provider">Specifies the certificate provider, like: "eToken Base Cryptographic Provider"</param>
        /// <param name="certBase64">Specifies the base64 encoded public certificate (multi-line values are allowed).</param>
        /// <param name="container">Specifies the certificate container, like: "Sectigo_20220830143311"</param>
        /// <param name="timestampUri">Specifies the URI for the certificate timestamp service, like: http://timestamp.sectigo.com</param>
        /// <param name="password">Specifies the certificate password.</param>
        public UsbTokenProfile(
            string provider,
            string certBase64,
            string container,
            string timestampUri,
            string password)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(provider), nameof(provider));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(certBase64), nameof(certBase64));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(container), nameof(container));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(timestampUri), nameof(timestampUri));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(password), nameof(password));
            Covenant.Requires<PlatformNotSupportedException>(NeonHelper.IsWindows && NeonHelper.Is64BitOS, "This is supported only for 64-bit Windows.");

            this.Provider     = provider;
            this.CertBase64   = certBase64;
            this.Container    = container;
            this.TimestampUri = timestampUri;
            this.Password     = password;
        }

        /// <summary>
        /// Returns the path to the file being signed.
        /// </summary>
        internal string TargetPath { get; private set; }

        /// <summary>
        /// Returns the certificate provider, like: "eToken Base Cryptographic Provider"
        /// </summary>
        internal string Provider { get; private set; }

        /// <summary>
        /// Returns the base64 encoded public certificate (multi-line values are allowed).
        /// </summary>
        internal string CertBase64 { get; private set; }

        /// <summary>
        /// Returns the certificate container, like: "Sectigo_20220830143311"
        /// </summary>
        internal string Container { get; private set; }

        /// <summary>
        /// Returns the URI for the certificate timestamp service, like: http://timestamp.sectigo.com
        /// </summary>
        internal string TimestampUri { get; private set; }

        /// <summary>
        /// Returns the certificate password.
        /// </summary>
        internal string Password { get; private set; }
    }
}
