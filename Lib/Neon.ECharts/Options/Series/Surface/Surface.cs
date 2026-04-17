// -----------------------------------------------------------------------------
// FILE:	    Surface.cs
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

namespace Neon.ECharts.Options.Series.Surface
{
    /// <summary>
    /// Represents a surface series in ECharts GL, used for 3D surface visualization.
    /// </summary>
    public record Surface : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "surface";

        /// <summary>
        /// Initializes a new instance of the <see cref="Surface"/> class.
        /// </summary>
        public Surface() : base(_Type) { }

        /// <summary>
        /// Gets or sets the coordinate system used.
        /// </summary>
        public string CoordinateSystem { get; set; }

        /// <summary>
        /// Gets or sets the index of the 3D grid.
        /// </summary>
        public int? Grid3DIndex { get; set; }

        /// <summary>
        /// Gets or sets the parametric equation for the surface.
        /// </summary>
        public object Equation { get; set; }

        /// <summary>
        /// Gets or sets whether the surface is parametric.
        /// </summary>
        public bool? Parametric { get; set; }

        /// <summary>
        /// Gets or sets the item style.
        /// </summary>
        public ItemStyle ItemStyle { get; set; }

        /// <summary>
        /// Gets or sets the shading style (e.g., 'color', 'lambert', 'realistic').
        /// </summary>
        public string Shading { get; set; }

        /// <summary>
        /// Gets or sets whether to enable wireframe rendering.
        /// </summary>
        public object Wireframe { get; set; }
    }
}
