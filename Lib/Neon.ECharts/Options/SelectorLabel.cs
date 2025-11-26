// -----------------------------------------------------------------------------
// FILE:	    SelectorLabel.cs
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

using System.Collections.Generic;
using System.Drawing;
using System.Text.Json.Serialization;

using Neon.ECharts.Options.Enum;

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents the label options for a selector.
    /// </summary>
    public class SelectorLabel
    {
        /// <summary>
        /// Gets or sets a value indicating whether to show the label.
        /// </summary>
        public bool? Show { set; get; }

        /// <summary>
        /// Gets or sets the distance between the label and the selector.
        /// </summary>
        public int? Distance { set; get; }

        /// <summary>
        /// Gets or sets the rotation angle of the label.
        /// </summary>
        public int? Rotate { set; get; }

        /// <summary>
        /// Gets or sets the offset of the label from the selector.
        /// </summary>
        public List<int> Offset { set; get; }

        /// <summary>
        /// Gets or sets the color of the label.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? Color { set; get; }

        /// <summary>
        /// Gets or sets the font style of the label.
        /// </summary>
        public FontStyle? FontStyle { set; get; }

        /// <summary>
        /// Gets or sets the font weight of the label.
        /// </summary>
        public FontWeight? FontWeight { set; get; }

        /// <summary>
        /// Gets or sets the font family of the label.
        /// </summary>
        public string FontFamily { set; get; }

        /// <summary>
        /// Gets or sets the font size of the label.
        /// </summary>
        public int? FontSize { set; get; }

        /// <summary>
        /// Gets or sets the horizontal alignment of the label.
        /// </summary>
        public Align2? Align { set; get; }

        /// <summary>
        /// Gets or sets the vertical alignment of the label.
        /// </summary>
        public VerticalAlign? VerticalAlign { set; get; }

        /// <summary>
        /// Gets or sets the line height of the label.
        /// </summary>
        public int? LineHeight { set; get; }

        /// <summary>
        /// Gets or sets the background color of the label.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? BackgroundColor { set; get; }

        /// <summary>
        /// Gets or sets the border color of the label.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? BorderColor { set; get; }

        /// <summary>
        /// Gets or sets the border width of the label.
        /// </summary>
        public int? BorderWidth { set; get; }

        /// <summary>
        /// Gets or sets the border radius of the label.
        /// </summary>
        public object BorderRadius { set; get; }

        /// <summary>
        /// Gets or sets the padding of the label.
        /// </summary>
        public object Padding { set; get; }

        /// <summary>
        /// Gets or sets the shadow blur of the label.
        /// </summary>
        public int? ShadowBlur { set; get; }

        /// <summary>
        /// Gets or sets the shadow color of the label.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? ShadowColor { set; get; }

        /// <summary>
        /// Gets or sets the shadow offset on the x-axis of the label.
        /// </summary>
        public int? ShadowOffsetX { set; get; }

        /// <summary>
        /// Gets or sets the shadow offset on the y-axis of the label.
        /// </summary>
        public int? ShadowOffsetY { set; get; }

        /// <summary>
        /// Gets or sets the width of the label.
        /// </summary>
        public object Width { set; get; }

        /// <summary>
        /// Gets or sets the height of the label.
        /// </summary>
        public object Height { set; get; }

        /// <summary>
        /// Gets or sets the border color of the text.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? TextBorderColor { set; get; }

        /// <summary>
        /// Gets or sets the border width of the text.
        /// </summary>
        public int? TextBorderWidth { set; get; }

        /// <summary>
        /// Gets or sets the shadow color of the text.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? TextShadowColor { set; get; }

        /// <summary>
        /// Gets or sets the shadow blur of the text.
        /// </summary>
        public int? TextShadowBlur { set; get; }

        /// <summary>
        /// Gets or sets the shadow offset on the x-axis of the text.
        /// </summary>
        public int? TextShadowOffsetX { set; get; }

        /// <summary>
        /// Gets or sets the shadow offset on the y-axis of the text.
        /// </summary>
        public int? TextShadowOffsetY { set; get; }

        /// <summary>
        /// Gets or sets the rich text styles for the label.
        /// </summary>
        public object Rich { set; get; }
    }
}
