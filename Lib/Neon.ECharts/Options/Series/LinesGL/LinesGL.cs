// -----------------------------------------------------------------------------
// FILE:	    LinesGL.cs
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

namespace Neon.ECharts.Options.Series.LinesGL
{
    /// <summary>
    /// Represents a GPU-accelerated lines series in ECharts GL, optimized for large-scale line rendering.
    /// </summary>
    public record LinesGL : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "linesGL";

        /// <summary>
        /// Initializes a new instance of the <see cref="LinesGL"/> class.
        /// </summary>
        public LinesGL() : base(_Type) { }

        /// <summary>
        /// Gets or sets the coordinate system used.
        /// </summary>
        public string CoordinateSystem { get; set; }

        /// <summary>
        /// Gets or sets whether it is a polyline.
        /// </summary>
        public bool? Polyline { get; set; }

        /// <summary>
        /// Gets or sets the line style.
        /// </summary>
        public LineStyle LineStyle { get; set; }

        /// <summary>
        /// Gets or sets the post-effect configuration.
        /// </summary>
        public object PostEffect { get; set; }
    }
}
