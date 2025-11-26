// -----------------------------------------------------------------------------
// FILE:	    JFunc.cs
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

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents a JFunc object.
    /// </summary>
    public record JFunc
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JFunc"/> class.
        /// </summary>
        public JFunc() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JFunc"/> class with the specified raw value.
        /// </summary>
        /// <param name="raw">The raw value.</param>
        public JFunc(string raw) => RAW = raw;

        /// <summary>
        /// Gets or sets the raw value.
        /// </summary>
        public string RAW { get; set; }

        /// <summary>
        /// Implicitly converts a string to a <see cref="JFunc"/> object.
        /// </summary>
        /// <param name="v">The string value.</param>
        /// <returns>The <see cref="JFunc"/> object.</returns>
        public static implicit operator JFunc(string v) => new JFunc() { RAW = v };
    }
}
