// -----------------------------------------------------------------------------
// FILE:	    Bar3D.cs
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

namespace Neon.ECharts.Options.Series.Bar3D
{
    /// <summary>
    /// Represents a 3D bar series in ECharts.
    /// </summary>
    public record Bar3D : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "bar3D";

        /// <summary>
        /// Initializes a new instance of the <see cref="Bar3D"/> class.
        /// </summary>
        public Bar3D() : base(_Type) { }

        /// <summary>
        /// Gets or sets the smoothness of the bar.
        /// </summary>
        public object Smooth { set; get; }

        /// <summary>
        /// Gets or sets the gap between categories of bars.
        /// </summary>
        public object BarCategoryGap { set; get; }

        /// <summary>
        /// Gets or sets the line style of the bar.
        /// </summary>
        public object LineStyle { set; get; }

        /// <summary>
        /// Gets or sets the item style of the bar.
        /// </summary>
        public ItemStyle ItemStyle { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the bar is silent.
        /// </summary>
        public bool? Silent { get; set; }

        /// <summary>
        /// Gets or sets the gap between bars in the same category.
        /// </summary>
        public string BarGap { get; set; }

        /// <summary>
        /// Gets or sets the z-index of the bar.
        /// </summary>
        public int? Z { set; get; }

        /// <summary>
        /// Gets or sets the index of the x-axis that the bar belongs to.
        /// </summary>
        public int? XAxisIndex { set; get; }

        /// <summary>
        /// Gets or sets the index of the y-axis that the bar belongs to.
        /// </summary>
        public int? YAxisIndex { set; get; }

        /// <summary>
        /// Gets or sets the width of the bar.
        /// </summary>
        public string BarWidth { get; set; }

        /// <summary>
        /// Gets or sets the encoding scheme used for the bar.
        /// </summary>
        public Encode Encode { get; set; }

        /// <summary>
        /// Gets or sets the stack name of the bar.
        /// </summary>
        public string Stack { get; set; }

        /// <summary>
        /// Gets or sets the shading style of the bar.
        /// </summary>
        public string Shading { get; set; }

        /// <summary>
        /// Gets or sets the label of the bar.
        /// </summary>
        public Label Label { get; set; }

        /// <summary>
        /// Gets or sets the emphasis style of the bar.
        /// </summary>
        public Emphasis Emphasis { get; set; }
    }
}
