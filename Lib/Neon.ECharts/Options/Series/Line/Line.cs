// -----------------------------------------------------------------------------
// FILE:	    Line.cs
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

namespace Neon.ECharts.Options.Series.Line
{
    /// <summary>
    /// Represents a line series in ECharts.
    /// </summary>
    public record Line : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "line";

        /// <summary>
        /// Initializes a new instance of the <see cref="Line"/> class.
        /// </summary>
        public Line() : base(_Type) { }

        /// <summary>
        /// Gets or sets the coordinate system used by the line series.
        /// </summary>
        public CoordinateSystem? CoordinateSystem { set; get; }

        /// <summary>
        /// Gets or sets the index of the x-axis that the line series belongs to.
        /// </summary>
        public int? XAxisIndex { set; get; }

        /// <summary>
        /// Gets or sets the index of the y-axis that the line series belongs to.
        /// </summary>
        public int? YAxisIndex { set; get; }

        /// <summary>
        /// Gets or sets the index of the polar coordinate system that the line series belongs to.
        /// </summary>
        public int? PolarIndex { set; get; }

        /// <summary>
        /// Gets or sets the symbol used for each data point in the line series.
        /// </summary>
        public string Symbol { set; get; }

        /// <summary>
        /// Gets or sets the size of the symbol used for each data point in the line series.
        /// </summary>
        public object SymbolSize { set; get; }

        /// <summary>
        /// Gets or sets the rotation angle of the symbol used for each data point in the line series.
        /// </summary>
        public int? SymbolRotate { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep the aspect ratio of the symbol used for each data point in the line series.
        /// </summary>
        public bool? SymbolKeepAspect { set; get; }

        /// <summary>
        /// Gets or sets the offset of the symbol used for each data point in the line series.
        /// </summary>
        public string SymbolOffset { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the symbol for each data point in the line series.
        /// </summary>
        public bool? ShowSymbol { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to show all symbols for each data point in the line series.
        /// </summary>
        public bool? ShowAllSymbol { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable hover animation for the line series.
        /// </summary>
        public bool? HoverAnimation { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable hover link with legend for the line series.
        /// </summary>
        public bool? LegendHoverLink { set; get; }

        /// <summary>
        /// Gets or sets the stack name of the line series.
        /// </summary>
        public string Stack { set; get; }

        /// <summary>
        /// Gets or sets the cursor style when hovering over the line series.
        /// </summary>
        public string Cursor { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to connect null data points in the line series.
        /// </summary>
        public bool? ConnectNulls { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to clip the line series to the coordinate system.
        /// </summary>
        public bool? Clip { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable step line for the line series.
        /// </summary>
        public bool? Step { set; get; }

        /// <summary>
        /// Gets or sets the area style of the line series.
        /// </summary>
        public AreaStyle AreaStyle { set; get; }

        /// <summary>
        /// Gets or sets the line style of the line series.
        /// </summary>
        public LineStyle LineStyle { set; get; }

        /// <summary>
        /// Gets or sets the smoothness of the line series.
        /// </summary>
        public object Smooth { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable animation for the line series.
        /// </summary>
        public bool? Animation { set; get; }

        /// <summary>
        /// Gets or sets the mark area of the line series.
        /// </summary>
        public MarkArea MarkArea { set; get; }

        /// <summary>
        /// Gets or sets the mark point of the line series.
        /// </summary>
        public MarkPoint MarkPoint { set; get; }

        /// <summary>
        /// Gets or sets the sampling method used by the line series.
        /// </summary>
        public Sampling? Sampling { set; get; }

        /// <summary>
        /// Gets or sets the item style of the line series.
        /// </summary>
        public ItemStyle ItemStyle { set; get; }

        /// <summary>
        /// Gets or sets the mark line of the line series.
        /// </summary>
        public MarkLine MarkLine { set; get; }
    }
}
