// -----------------------------------------------------------------------------
// FILE:	    OnePasswordItem.cs
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
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using Neon.Common;

namespace Neon.Deployment
{
    /// <summary>
    /// Describes a 1Password item.
    /// </summary>
    public class OnePasswordItem
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Specifies the 1Password item name.</param>
        /// <param name="fields">Specifies the 1password item fields.</param>
        public OnePasswordItem(string name, IEnumerable<OnePasswordField> fields)
        {
            Covenant.Requires<NullReferenceException>(!string.IsNullOrEmpty(name), nameof(name));
            Covenant.Requires<NullReferenceException>(fields != null, nameof(fields));

            var itemFields = new Dictionary<string, OnePasswordField>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var field in fields)
            {
                itemFields.Add(field.Name, field);
            }

            Name   = name; 
            Fields = new ReadOnlyDictionary<string, OnePasswordField>(itemFields);
        }

        /// <summary>
        /// Returns the 1Password item name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Returns a read-only dictionary with the item fields keyed by case-insensitive name.
        /// </summary>
        public ReadOnlyDictionary<string, OnePasswordField> Fields { get; private set; }
    }
}
