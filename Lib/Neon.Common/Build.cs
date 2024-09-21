//-----------------------------------------------------------------------------
// FILE:        Build.cs
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

namespace Neon
{
    /// <summary>
    /// Neon build constants.
    /// </summary>
    public static partial class Build
    {
        /// <summary>
        /// The company name to use for all Neon assemblies.
        /// </summary>
        public const string Company = "NEONFORGE LLC";

        /// <summary>
        /// The copyright statement to be included in all assemblies.
        /// </summary>
        public const string Copyright = "Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.";

        /// <summary>
        /// Trademark statement.
        /// </summary>
        public const string Trademark = "NeonSDK, NeonKUBE, and NeonDESKTOP are trademarks of NEONFORGE LLC";

        /// <summary> 
        /// The product name.
        /// </summary>
        public const string ProductName = "NeonSDK";

        /// <summary>
        /// <para>
        /// The released library/package version.
        /// </para>
        /// <note>
        /// <b>WARNING:</b> Do not rename this constant without updating the relevant 
        /// release scripts to match.
        /// </note>
        /// </summary>
        public const string NeonSdkVersion = "4.0.13-preview.28";

        /// <summary>
        /// The product license.
        /// </summary>
        public const string ProductLicense = "Apache License, Version 2.0";

        /// <summary>
        /// The product license URL.
        /// </summary>
        public const string ProductLicenseUrl = "http://www.apache.org/licenses/LICENSE-2.0";

        /// <summary>
        /// The build configuration.
        /// </summary>
        public const string Configuration =
#if DEBUG
            "Debug";
#else
            "Release";
#endif
    }
}
