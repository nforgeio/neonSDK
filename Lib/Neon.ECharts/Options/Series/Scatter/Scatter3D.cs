// -----------------------------------------------------------------------------
// FILE:	    Scatter3D.cs
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

using System.Collections.Generic;

namespace Neon.ECharts.Options.Series.Scatter
{
    /// <summary>
    /// Represents a 3D scatter series in ECharts.
    /// </summary>
    public record Scatter3D : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "scatter3D";

        /// <summary>
        /// Initializes a new instance of the <see cref="Scatter3D"/> class.
        /// </summary>
        public Scatter3D() : base(_Type) { }

        /// <summary>
        /// Gets or sets the dimensions of the scatter series.
        /// </summary>
        public List<string> Dimensions { get; set; }

        /// <summary>
        /// Gets or sets the item style of the scatter series.
        /// </summary>
        public ItemStyle ItemStyle { get; set; }

        /// <summary>
        /// Gets or sets the emphasis of the scatter series.
        /// </summary>
        public Emphasis Emphasis { get; set; }

        /// <summary>
        /// Gets or sets the symbol size of the scatter series.
        /// </summary>
        public int SymbolSize { get; set; }
    }
}
