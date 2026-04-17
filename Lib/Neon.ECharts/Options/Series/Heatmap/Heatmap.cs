// -----------------------------------------------------------------------------
// FILE:	    Heatmap.cs
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

namespace Neon.ECharts.Options.Series.Heatmap
{
    /// <summary>
    /// Represents a heatmap series in ECharts.
    /// </summary>
    public record Heatmap : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "heatmap";

        /// <summary>
        /// Initializes a new instance of the <see cref="Heatmap"/> class.
        /// </summary>
        public Heatmap() : base(_Type) { }

        /// <summary>
        /// Gets or sets the coordinate system used.
        /// </summary>
        public string CoordinateSystem { get; set; }

        /// <summary>
        /// Gets or sets the index of the x-axis.
        /// </summary>
        public int? XAxisIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the y-axis.
        /// </summary>
        public int? YAxisIndex { get; set; }

        /// <summary>
        /// Gets or sets the point size of each data point in the heatmap.
        /// </summary>
        public int? PointSize { get; set; }

        /// <summary>
        /// Gets or sets the blur size of each data point.
        /// </summary>
        public int? BlurSize { get; set; }

        /// <summary>
        /// Gets or sets the minimum opacity.
        /// </summary>
        public double? MinOpacity { get; set; }

        /// <summary>
        /// Gets or sets the maximum opacity.
        /// </summary>
        public double? MaxOpacity { get; set; }

        /// <summary>
        /// Gets or sets the item style.
        /// </summary>
        public ItemStyle ItemStyle { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        public Label Label { get; set; }

        /// <summary>
        /// Gets or sets the emphasis style.
        /// </summary>
        public Emphasis Emphasis { get; set; }
    }
}
