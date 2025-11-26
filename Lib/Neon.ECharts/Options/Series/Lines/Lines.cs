// -----------------------------------------------------------------------------
// FILE:	    Lines.cs
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

using Neon.ECharts.Options.Enum;

namespace Neon.ECharts.Options.Series.Lines
{
    /// <summary>
    /// Represents a lines series in ECharts.
    /// </summary>
    public record Lines : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "lines";

        /// <summary>
        /// Initializes a new instance of the <see cref="Lines"/> class.
        /// </summary>
        public Lines() : base(_Type) { }

        /// <summary>
        /// Gets or sets the coordinate system used for the lines series.
        /// </summary>
        public CoordinateSystem? CoordinateSystem { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the lines are represented as polylines.
        /// </summary>
        public bool? Polyline { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the lines series is silent.
        /// </summary>
        public bool? Silent { set; get; }

        /// <summary>
        /// Gets or sets the line style for the lines series.
        /// </summary>
        public LineStyle LineStyle { get; set; }

        /// <summary>
        /// Gets or sets the progressive threshold for rendering the lines series.
        /// </summary>
        public int? ProgressiveThreshold { get; set; }

        /// <summary>
        /// Gets or sets the progressive value for rendering the lines series.
        /// </summary>
        public int? Progressive { get; set; }

        /// <summary>
        /// Gets or sets the effect for the lines series.
        /// </summary>
        public Effect Effect { get; set; }
    }
}
