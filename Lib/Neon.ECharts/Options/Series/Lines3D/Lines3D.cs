// -----------------------------------------------------------------------------
// FILE:	    Lines3D.cs
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

namespace Neon.ECharts.Options.Series.Lines3D
{
    /// <summary>
    /// Represents a 3D lines series in ECharts GL, used for drawing 3D flight lines.
    /// </summary>
    public record Lines3D : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "lines3D";

        /// <summary>
        /// Initializes a new instance of the <see cref="Lines3D"/> class.
        /// </summary>
        public Lines3D() : base(_Type) { }

        /// <summary>
        /// Gets or sets the coordinate system used.
        /// </summary>
        public string CoordinateSystem { get; set; }

        /// <summary>
        /// Gets or sets the index of the globe.
        /// </summary>
        public int? GlobeIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the geo3D.
        /// </summary>
        public int? Geo3DIndex { get; set; }

        /// <summary>
        /// Gets or sets whether it is a polyline.
        /// </summary>
        public bool? Polyline { get; set; }

        /// <summary>
        /// Gets or sets the line style.
        /// </summary>
        public LineStyle LineStyle { get; set; }

        /// <summary>
        /// Gets or sets the effect configuration for flight lines.
        /// </summary>
        public object Effect { get; set; }
    }
}
