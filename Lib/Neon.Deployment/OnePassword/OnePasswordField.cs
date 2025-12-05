// -----------------------------------------------------------------------------
// FILE:	    OnePasswordField.cs
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
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using Neon.Common;

namespace Neon.Deployment
{
    /// <summary>
    /// Descrives a 1Password item field.
    /// </summary>
    public class OnePasswordField
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Specifies the field name.</param>
        /// <param name="value">Specifies the field value.</param>
        public OnePasswordField(string name, string value)
        {
            Covenant.Requires<NullReferenceException>(!string.IsNullOrEmpty(name), nameof(name));
            Covenant.Requires<NullReferenceException>(value != null, nameof(value));

            this.Name  = name;
            this.Value = value;
        }

        /// <summary>
        /// Returns the 1Password field name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Returns the 1Password field value.
        /// </summary>
        public string Value { get; private set; }
    }
}
