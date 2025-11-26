// -----------------------------------------------------------------------------
// FILE:	    Legend.cs
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

using System.Drawing;
using System.Text.Json.Serialization;

using Neon.ECharts.Options.Enum;

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents the legend options in ECharts.
    /// </summary>
    public class Legend
    {
        /// <summary>
        /// Gets or sets the ID of the legend.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        /// Gets or sets the type of the legend.
        /// </summary>
        public LegendType? Type { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the legend is shown.
        /// </summary>
        public bool? Show { set; get; }

        /// <summary>
        /// Gets or sets the z-level of the legend.
        /// </summary>
        public int? Zlevel { set; get; }

        /// <summary>
        /// Gets or sets the z value of the legend.
        /// </summary>
        public int? Z { set; get; }

        /// <summary>
        /// Gets or sets the left position of the legend.
        /// </summary>
        public object Left { set; get; }

        /// <summary>
        /// Gets or sets the top position of the legend.
        /// </summary>
        public object Top { set; get; }

        /// <summary>
        /// Gets or sets the right position of the legend.
        /// </summary>
        public object Right { set; get; }

        /// <summary>
        /// Gets or sets the bottom position of the legend.
        /// </summary>
        public object Bottom { set; get; }

        /// <summary>
        /// Gets or sets the width of the legend.
        /// </summary>
        public int? Width { set; get; }

        /// <summary>
        /// Gets or sets the height of the legend.
        /// </summary>
        public int? Height { set; get; }

        /// <summary>
        /// Gets or sets the orientation of the legend.
        /// </summary>
        public Orient? Orient { set; get; }

        /// <summary>
        /// Gets or sets the alignment of the legend.
        /// </summary>
        public Align1? Align { set; get; }

        /// <summary>
        /// Gets or sets the padding of the legend.
        /// </summary>
        public object Padding { set; get; }

        /// <summary>
        /// Gets or sets the gap between legend items.
        /// </summary>
        public int? ItemGap { set; get; }

        /// <summary>
        /// Gets or sets the width of legend items.
        /// </summary>
        public int? ItemWidth { set; get; }

        /// <summary>
        /// Gets or sets the height of legend items.
        /// </summary>
        public int? ItemHeight { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to keep the aspect ratio of legend symbols.
        /// </summary>
        public bool? SymbolKeepAspect { set; get; }

        /// <summary>
        /// Gets or sets the formatter function for legend labels.
        /// </summary>
        public object Formatter { set; get; }

        /// <summary>
        /// Gets or sets the selected mode of the legend.
        /// </summary>
        public object SelectedMode { set; get; }

        /// <summary>
        /// Gets or sets the color of inactive legend items.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? InactiveColor { set; get; }

        /// <summary>
        /// Gets or sets the selected legend items.
        /// </summary>
        public object Selected { set; get; }

        /// <summary>
        /// Gets or sets the text style of the legend.
        /// </summary>
        public TextStyle TextStyle { set; get; }

        /// <summary>
        /// Gets or sets the tooltip options of the legend.
        /// </summary>
        public Tooltip Tooltip { set; get; }

        /// <summary>
        /// Gets or sets the icon of the legend.
        /// </summary>
        public string Icon { set; get; }

        /// <summary>
        /// Gets or sets the horizontal alignment of the legend.
        /// </summary>
        public Align? X { set; get; }

        /// <summary>
        /// Gets or sets the data of the legend.
        /// </summary>
        public object Data { set; get; }

        /// <summary>
        /// Gets or sets the background color of the legend.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? BackgroundColor { set; get; }

        /// <summary>
        /// Gets or sets the border color of the legend.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? BorderColor { set; get; }

        /// <summary>
        /// Gets or sets the border width of the legend.
        /// </summary>
        public int? BorderWidth { set; get; }

        /// <summary>
        /// Gets or sets the border radius of the legend.
        /// </summary>
        public object BorderRadius { set; get; }

        /// <summary>
        /// Gets or sets the shadow blur of the legend.
        /// </summary>
        public int? ShadowBlur { set; get; }

        /// <summary>
        /// Gets or sets the shadow color of the legend.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? ShadowColor { set; get; }

        /// <summary>
        /// Gets or sets the shadow offset on the x-axis of the legend.
        /// </summary>
        public int? ShadowOffsetX { set; get; }

        /// <summary>
        /// Gets or sets the shadow offset on the y-axis of the legend.
        /// </summary>
        public int? ShadowOffsetY { set; get; }

        /// <summary>
        /// Gets or sets the index of the data to scroll to in the legend.
        /// </summary>
        public int? ScrollDataIndex { set; get; }

        /// <summary>
        /// Gets or sets the gap between page buttons in the legend.
        /// </summary>
        public int? PageButtonItemGap { set; get; }

        /// <summary>
        /// Gets or sets the gap between page buttons and the legend.
        /// </summary>
        public int? PageButtonGap { set; get; }

        /// <summary>
        /// Gets or sets the position of the page buttons in the legend.
        /// </summary>
        public PositionY? PageButtonPosition { set; get; }

        /// <summary>
        /// Gets or sets the formatter function for page buttons in the legend.
        /// </summary>
        public object PageFormatter { set; get; }

        /// <summary>
        /// Gets or sets the icons for page buttons in the legend.
        /// </summary>
        public PageIcons PageIcons { set; get; }

        /// <summary>
        /// Gets or sets the color of page icons in the legend.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? PageIconColor { set; get; }

        /// <summary>
        /// Gets or sets the color of inactive page icons in the legend.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? PageIconInactiveColor { set; get; }

        /// <summary>
        /// Gets or sets the size of page icons in the legend.
        /// </summary>
        public object PageIconSize { set; get; }

        /// <summary>
        /// Gets or sets the text style of page buttons in the legend.
        /// </summary>
        public TextStyle PageTextStyle { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether animation is enabled for the legend.
        /// </summary>
        public bool? Animation { set; get; }

        /// <summary>
        /// Gets or sets the duration of the update animation for the legend.
        /// </summary>
        public int? AnimationDurationUpdate { set; get; }

        /// <summary>
        /// Gets or sets the selector options for the legend.
        /// </summary>
        public object Selector { set; get; }

        /// <summary>
        /// Gets or sets the label options for the legend selector.
        /// </summary>
        public SelectorLabel SelectorLabel { set; get; }

        /// <summary>
        /// Gets or sets the position of the legend selector.
        /// </summary>
        public SelectorPosition? SelectorPosition { set; get; }

        /// <summary>
        /// Gets or sets the gap between legend selector items.
        /// </summary>
        public int? SelectorItemGap { set; get; }

        /// <summary>
        /// Gets or sets the gap between legend selector buttons.
        /// </summary>
        public int? SelectorButtonGap { set; get; }
    }
}
