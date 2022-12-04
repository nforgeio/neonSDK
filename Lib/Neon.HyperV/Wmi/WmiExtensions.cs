//-----------------------------------------------------------------------------
// FILE:	    WmiExtensions.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
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
using OpenTelemetry.Trace;

namespace Neon.HyperV
{
    /// <summary>
    /// WMI related extension methods.
    /// </summary>
    internal static class WmiExtensions
    {
        /// <summary>
        /// Persists the properties from a dictionary into a <see cref="ManagementBaseObject"/>.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="properties">The dictionary with the properties.  This may be empty or <c>null</c>.</param>
        public static void SetProperties(this ManagementBaseObject target, Dictionary<string, object> properties)
        {
            if (properties == null)
            {
                return;
            }

            foreach (var property in properties)
            {
                target[property.Key] = property.Value;
            }
        }

        /// <summary>
        /// Returns a dictionary holding the properties of a <see cref="ManagementBaseObject"/>.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>The property dictionary.</returns>
        public static Dictionary<string, object> ToDictionary(this ManagementBaseObject source)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var item in source.Properties)
            {
                dictionary[item.Name] = item.Value;
            }

            return dictionary;
        }

        /// <summary>
        /// Returns the single object from a <see cref="ManagementObjectCollection"/>.
        /// </summary>
        /// <param name="collection">The management object collection.</param>
        /// <param name="predicate">Optionally specifies a predicate to to select the collection item.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown if no items satisfy the predicate.</exception>
        public static ManagementObject Single(this ManagementObjectCollection collection, Func<ManagementObject, bool> predicate = null)
        {
            var matches = new List<ManagementObject>();

            if (predicate == null)
            {
                predicate = new Func<ManagementObject, bool>(item => true);
            }

            foreach (ManagementObject item in collection)
            {
                if (predicate(item))
                {
                    matches.Add(item);
                }
            }

            return matches.Single();
        }
    }
}
