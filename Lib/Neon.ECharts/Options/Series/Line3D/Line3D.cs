// -----------------------------------------------------------------------------
// FILE:	    Line3D.cs
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

namespace Neon.ECharts.Options.Series.Line3D
{
    /// <summary>
    /// Represents a 3D line series in ECharts GL.
    /// </summary>
    public record Line3D : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "line3D";

        /// <summary>
        /// Initializes a new instance of the <see cref="Line3D"/> class.
        /// </summary>
        public Line3D() : base(_Type) { }

        /// <summary>
        /// Gets or sets the coordinate system used.
        /// </summary>
        public string CoordinateSystem { get; set; }

        /// <summary>
        /// Gets or sets the index of the 3D grid.
        /// </summary>
        public int? Grid3DIndex { get; set; }

        /// <summary>
        /// Gets or sets the line style.
        /// </summary>
        public LineStyle LineStyle { get; set; }

        /// <summary>
        /// Gets or sets the item style.
        /// </summary>
        public ItemStyle ItemStyle { get; set; }
    }
}
