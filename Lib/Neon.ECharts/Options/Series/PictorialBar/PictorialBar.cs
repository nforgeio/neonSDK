// -----------------------------------------------------------------------------
// FILE:	    PictorialBar.cs
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

namespace Neon.ECharts.Options.Series.PictorialBar
{
    /// <summary>
    /// Represents a pictorial bar series in ECharts, which uses symbols to represent data.
    /// </summary>
    public record PictorialBar : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "pictorialBar";

        /// <summary>
        /// Initializes a new instance of the <see cref="PictorialBar"/> class.
        /// </summary>
        public PictorialBar() : base(_Type) { }

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
        /// Gets or sets the symbol type.
        /// </summary>
        public object Symbol { get; set; }

        /// <summary>
        /// Gets or sets the symbol size.
        /// </summary>
        public object SymbolSize { get; set; }

        /// <summary>
        /// Gets or sets the symbol position. Valid values: 'start', 'end', 'center'.
        /// </summary>
        public string SymbolPosition { get; set; }

        /// <summary>
        /// Gets or sets the symbol offset.
        /// </summary>
        public object SymbolOffset { get; set; }

        /// <summary>
        /// Gets or sets the symbol rotation in degrees.
        /// </summary>
        public object SymbolRotate { get; set; }

        /// <summary>
        /// Gets or sets whether the symbol is repeated.
        /// </summary>
        public object SymbolRepeat { get; set; }

        /// <summary>
        /// Gets or sets the direction of symbol repeat.
        /// </summary>
        public string SymbolRepeatDirection { get; set; }

        /// <summary>
        /// Gets or sets the margin between repeated symbols.
        /// </summary>
        public object SymbolMargin { get; set; }

        /// <summary>
        /// Gets or sets whether the symbol is clipped.
        /// </summary>
        public bool? SymbolClip { get; set; }

        /// <summary>
        /// Gets or sets the bar gap.
        /// </summary>
        public string BarGap { get; set; }

        /// <summary>
        /// Gets or sets the bar category gap.
        /// </summary>
        public object BarCategoryGap { get; set; }

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
