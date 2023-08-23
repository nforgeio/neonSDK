// -----------------------------------------------------------------------------
// FILE:	    AzureProfile.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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

namespace Neon.Deployment.CodeSigning
{
    /// <summary>
    /// Defines parameters required for code signing via <b>Azure Code Signing</b>.
    /// </summary>
    public class AzureProfile
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="azureTenantId">Specifies the Azure tenent ID for the key vault.</param>
        /// <param name="azureClientId">Specifies the Azure client ID used for authenticating the signing request.</param>
        /// <param name="azureClientSecret">Specifies the Azure client secret used for authenticating the signing request. </param>
        /// <param name="codeSigningAccountEndpoint">Specifies the code signing account endpoint.</param>
        /// <param name="codeSigningAccountName">Specifies the name of the Azure code signing account being used.</param>
        /// <param name="certificateProfileName">Specifies the signing certificate name within the code signing account.</param>
        public AzureProfile(
            string      azureTenantId,
            string      azureClientId,
            string      azureClientSecret,
            string      codeSigningAccountEndpoint,
            string      codeSigningAccountName,
            string      certificateProfileName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(azureTenantId), nameof(azureTenantId));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(azureClientId), nameof(azureClientId));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(azureClientSecret), nameof(azureClientSecret));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(codeSigningAccountEndpoint), nameof(codeSigningAccountEndpoint));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(codeSigningAccountName), nameof(codeSigningAccountName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(certificateProfileName), nameof(certificateProfileName));

            this.AzureTenantId              = azureTenantId;
            this.AzureClientId              = azureClientId;
            this.AzureClientSecret          = azureClientSecret;
            this.CodeSigningAccountEndpoint = codeSigningAccountEndpoint;
            this.CodeSigningAccountName     = codeSigningAccountName;
            this.CertificateProfileName     = certificateProfileName;
        }

        /// <summary>
        /// Returns the Azure tenent ID for the key vault.
        /// </summary>
        public string AzureTenantId { get; private set; }

        /// <summary>
        /// Returns the client ID used for authenticating the signing request.
        /// </summary>
        public string AzureClientId { get; private set; }

        /// <summary>
        /// Returns the secret used for authenticating the signing request. 
        /// </summary>
        public string AzureClientSecret { get; private set; }

        /// <summary>
        /// Returns the URL for the Azure key vault holding the signing certificate.
        /// </summary>
        public string CodeSigningAccountEndpoint { get; private set; }

        /// <summary>
        /// Returns the name of the Azure code signing account being used.
        /// </summary>
        public string CodeSigningAccountName { get; private set; }

        /// <summary>
        /// Returns the signing certificate name within the key vault.
        /// </summary>
        public string CertificateProfileName { get; private set; }
    }
}
