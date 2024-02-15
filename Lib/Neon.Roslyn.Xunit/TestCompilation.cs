// -----------------------------------------------------------------------------
// FILE:	    TestCompilation.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using Microsoft.CodeAnalysis;

namespace Neon.Roslyn.Xunit
{
    /// <summary>
    /// Helper class for working with Roslyn <see cref="Compilation"/> instances.
    /// </summary>
    public class TestCompilation
    {
        /// <summary>
        /// The Compilation instance.
        /// </summary>
        public Compilation Compilation { get; set; }

        /// <summary>
        /// Diagnostic messages.
        /// </summary>
        public List<Diagnostic> Diagnostics { get; set; }

        /// <summary>
        /// Helper method to check if the compilation has a specific output syntax.
        /// The comparison is done by normalizing the syntax trees to a common format,
        /// by replacing \r\n with \n and trimming the result.
        /// </summary>
        /// <param name="expectedSyntax"></param>
        /// <returns></returns>
        public bool HasOutputSyntax(string expectedSyntax)
        {
            return Compilation.SyntaxTrees.Any(st => st.ToString().Normalize() == expectedSyntax.Normalize());
        }
    }

    public static class StringExtensions
    {
        public static string Normalize(this string value)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return value.Replace("\r\n", "\n").Trim();
            }
            else
            {
                return value.Trim();
            }
        }
    }
}
