//-----------------------------------------------------------------------------
// FILE:        WmiHelper.cs
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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Helpers for the Cmdlet port.
/// </summary>
internal static class WmiHelper
{
    /// <summary>
    /// Returns the fully qualified name to be used to reference an assembly reference.
    /// </summary>
    /// <param name="name">Specifies the unqualified name.</param>
    /// <returns>The fully qualified resource name.</returns>
    public static string GetResourceName(string name)
    {
        Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name), nameof(name));

        return $"Neon.HyperV.HyperVWmiDriver.{name}";
    }
}
