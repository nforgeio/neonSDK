// -----------------------------------------------------------------------------
// FILE:	    OnePasswordVault.cs
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
    /// Describes a 1Password vault including any related secret items.
    /// </summary>
    public class OnePasswordVault
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">Specifies the 1Password vault UUID.</param>
        /// <param name="name">Specifies the 1Password vault name.</param>
        public OnePasswordVault(string id, string name)
        {
            Covenant.Requires<NullReferenceException>(!string.IsNullOrEmpty(id), nameof(id));
            Covenant.Requires<NullReferenceException>(!string.IsNullOrEmpty(name), nameof(name));

            this.Id    = id;
            this.Name  = name;
            this.Items = new Dictionary<string, OnePasswordItem>();
        }

        /// <summary>
        /// Returns the 1Password vault UUID.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Returns the 1Password vault name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Returns dictionary of the vault items keyed by case-insensitive item name.
        /// </summary>
        public Dictionary<string, OnePasswordItem> Items { get; private set;}
    }
}
