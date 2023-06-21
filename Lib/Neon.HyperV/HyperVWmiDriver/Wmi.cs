//-----------------------------------------------------------------------------
// FILE:        Wmi.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
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
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;

using Neon.Common;
using Neon.Diagnostics;

namespace Neon.HyperV
{
    /// <summary>
    /// Global WMI utilities.
    /// </summary>
    internal static class Wmi
    {
        /// <summary>
        /// Queries a management scope for <see cref="ManagementObject"/> instances using an <see cref="ObjectQuery"/>.
        /// </summary>
        /// <param name="query">The query object.</param>
        /// <param name="scope">Optionally specifies the scope.  This defaults to <b>"\\.\root\cimv2"</b></param>
        /// <returns>The query results.</returns>
        public static IEnumerable<ManagementObject> Query(ObjectQuery query, ManagementScope scope = null)
        {
            Covenant.Requires<ArgumentNullException>(query != null, nameof(query));

            scope ??= new ManagementScope();

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            {
                using (var results = searcher.Get())
                {
                    return results.OfType<ManagementObject>().ToArray();
                }
            }
        }

        /// <summary>
        /// Queries a management scope for <see cref="ManagementObject"/> instances using query string.
        /// </summary>
        /// <param name="query">The query string.</param>
        /// <param name="scope">Optionally specifies the scope.  This defaults to <b>"\\.\root\cimv2"</b></param>
        /// <returns>The query results.</returns>
        public static IEnumerable<ManagementObject> Query(string query, ManagementScope scope = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(query), nameof(query));

            return Query(new ObjectQuery(query), scope);
        }
    }
}
