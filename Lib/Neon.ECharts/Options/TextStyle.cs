// -----------------------------------------------------------------------------
// FILE:	    TextStyle.cs
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
using Neon.ECharts.Options.Series;

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents the text style options for ECharts.
    /// </summary>
    public class TextStyle
    {
        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? Color { set; get; }

        /// <summary>
        /// Gets or sets the background color of the text.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? BackgroundColor { set; get; }

        /// <summary>
        /// Gets or sets the font style of the text.
        /// </summary>
        public FontStyle? FontStyle { set; get; }

        /// <summary>
        /// Gets or sets the font weight of the text.
        /// </summary>
        public object FontWeight { set; get; }

        /// <summary>
        /// Gets or sets the font family of the text.
        /// </summary>
        public string FontFamily { set; get; }

        /// <summary>
        /// Gets or sets the font size of the text.
        /// </summary>
        public int? FontSize { set; get; }

        /// <summary>
        /// Gets or sets the border radius of the text.
        /// </summary>
        public object BorderRadius { set; get; }

        /// <summary>
        /// Gets or sets the padding of the text.
        /// </summary>
        public object Padding { set; get; }

        /// <summary>
        /// Gets or sets the line height of the text.
        /// </summary>
        public int? LineHeight { set; get; }

        /// <summary>
        /// Gets or sets the width of the text.
        /// </summary>
        public object Width { set; get; }

        /// <summary>
        /// Gets or sets the height of the text.
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
        /// Gets or sets the rich text options for the text.
        /// </summary>
        public object Rich { set; get; }

        /// <summary>
        /// Gets or sets the emphasis options for the text.
        /// </summary>
        public Emphasis Emphasis { set; get; }
    }
}
