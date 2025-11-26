// -----------------------------------------------------------------------------
// FILE:	    Effect.cs
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
    /// Represents the effect options for ECharts.
    /// </summary>
    public record Effect
    {
        /// <summary>
        /// Gets or sets the constant speed of the effect.
        /// </summary>
        public int? ConstantSpeed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the effect is shown.
        /// </summary>
        public bool? Show { get; set; }

        /// <summary>
        /// Gets or sets the trail length of the effect.
        /// </summary>
        public double? TrailLength { get; set; }

        /// <summary>
        /// Gets or sets the symbol size of the effect.
        /// </summary>
        public double? SymbolSize { get; set; }
    }
}
