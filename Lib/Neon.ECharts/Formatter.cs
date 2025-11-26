// -----------------------------------------------------------------------------
// FILE:	    Formatter.cs
// CONTRIBUTOR: NEONFORGE Team
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

using Neon.ECharts.Options;

namespace Neon.ECharts
{
    /// <summary>
    /// Represents a formatter used in Neon ECharts library.
    /// </summary>
    public class Formatter
    {
        /// <summary>
        /// Gets or sets the JFunc associated with the formatter.
        /// </summary>
        public JFunc JFunc { get; set; }

        /// <summary>
        /// Gets or sets the string format used by the formatter.
        /// </summary>
        public string StringFormat { get; set; }

        /// <summary>
        /// Implicitly converts a string to a Formatter object.
        /// </summary>
        /// <param name="v">The string value to convert.</param>
        /// <returns>A new instance of the Formatter class.</returns>
        public static implicit operator Formatter(string v) => new Formatter() { StringFormat = v };
    }
}
