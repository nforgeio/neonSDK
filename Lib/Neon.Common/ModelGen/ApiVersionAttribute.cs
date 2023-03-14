//-----------------------------------------------------------------------------
// FILE:	    ApiVersionAttribute.cs
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
using System.Text;

namespace Neon.ModelGen
{
    /// <summary>
    /// <para>
    /// Used to tag an ASP.NET controller interface or method with an API version.  Versions
    /// are formatted like:
    /// </para>
    /// <code>
    /// [VERSIONGROUP.]MAJOR.MINOR[-STATUS]
    ///
    /// or:
    ///
    /// VERSIONGROUP[MAJOR[.MINOR]][-STATUS]
    /// </code>
    /// <para>
    /// where <b>VERSIONGROUP</b> is a date formatted like <b>YYYY-MM-DD</b>, <b>MAJOR</b>
    /// and <b>MINOR</b> are non-negative integers and <b>STATUS</b> is a string of like
    /// <b>alphs</b>, <b>preview</b>, etc. consisting of the following characters:
    /// <b>a-z</b>, <b>A-Z</b>, <b>0-9</b>, <b>"."</b> and <b>"-"</b>.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class ApiVersionAttribute : Attribute
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="version">Specifies the API version string.</param>
        public ApiVersionAttribute(string version)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(version), nameof(version));

            Version = ApiVersion.Parse(version);
        }

        /// <summary>
        /// Returns parsed <see cref="ApiVersion"/>.
        /// </summary>
        public ApiVersion Version { get; private set; }
    }
}
