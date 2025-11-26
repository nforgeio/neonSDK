// -----------------------------------------------------------------------------
// FILE:	    Title.cs
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
    /// Represents the title of a chart.
    /// </summary>
    public class Title
    {
        /// <summary>
        /// Gets or sets the ID of the title.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the title is shown.
        /// </summary>
        public bool? Show { set; get; }

        /// <summary>
        /// Gets or sets the text of the title.
        /// </summary>
        public string Text { set; get; }

        /// <summary>
        /// Gets or sets the link URL of the title.
        /// </summary>
        public string Link { set; get; }

        /// <summary>
        /// Gets or sets the target window or frame for the link.
        /// </summary>
        public Target? Target { set; get; }

        /// <summary>
        /// Gets or sets the text style of the title.
        /// </summary>
        public TextStyle TextStyle { set; get; }

        /// <summary>
        /// Gets or sets the subtext of the title.
        /// </summary>
        public string Subtext { set; get; }

        /// <summary>
        /// Gets or sets the link URL of the subtext.
        /// </summary>
        public string Sublink { set; get; }

        /// <summary>
        /// Gets or sets the target window or frame for the subtext link.
        /// </summary>
        public Target? Subtarget { set; get; }

        /// <summary>
        /// Gets or sets the text style of the subtext.
        /// </summary>
        public TextStyle SubtextStyle { set; get; }

        /// <summary>
        /// Gets or sets the horizontal alignment of the title text.
        /// </summary>
        public Align? TextAlign { set; get; }

        /// <summary>
        /// Gets or sets the vertical alignment of the title text.
        /// </summary>
        public Align? TextVerticalAlign { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the title can trigger events.
        /// </summary>
        public bool? TriggerEvent { set; get; }

        /// <summary>
        /// Gets or sets the padding of the title.
        /// </summary>
        public object Padding { set; get; }

        /// <summary>
        /// Gets or sets the gap between items in the title.
        /// </summary>
        public int? ItemGap { set; get; }

        /// <summary>
        /// Gets or sets the z-level of the title.
        /// </summary>
        public int? Zlevel { set; get; }

        /// <summary>
        /// Gets or sets the z-index of the title.
        /// </summary>
        public int? Z { set; get; }

        /// <summary>
        /// Gets or sets the left position of the title.
        /// </summary>
        public object Left { set; get; }

        /// <summary>
        /// Gets or sets the top position of the title.
        /// </summary>
        public object Top { set; get; }

        /// <summary>
        /// Gets or sets the right position of the title.
        /// </summary>
        public object Right { set; get; }

        /// <summary>
        /// Gets or sets the bottom position of the title.
        /// </summary>
        public object Bottom { set; get; }

        /// <summary>
        /// Gets or sets the background color of the title.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? BackgroundColor { set; get; }

        /// <summary>
        /// Gets or sets the border color of the title.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? BorderColor { set; get; }

        /// <summary>
        /// Gets or sets the border width of the title.
        /// </summary>
        public int? BorderWidth { set; get; }

        /// <summary>
        /// Gets or sets the border radius of the title.
        /// </summary>
        public object BorderRadius { set; get; }

        /// <summary>
        /// Gets or sets the shadow blur of the title.
        /// </summary>
        public int? ShadowBlur { set; get; }

        /// <summary>
        /// Gets or sets the shadow color of the title.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? ShadowColor { set; get; }

        /// <summary>
        /// Gets or sets the shadow offset on the x-axis of the title.
        /// </summary>
        public int? ShadowOffsetX { set; get; }

        /// <summary>
        /// Gets or sets the shadow offset on the y-axis of the title.
        /// </summary>
        public int? ShadowOffsetY { set; get; }

        /// <summary>
        /// Gets or sets the horizontal alignment of the title.
        /// </summary>
        public Align? X { set; get; }

        /// <summary>
        /// Gets or sets the horizontal alignment of the title.
        /// </summary>
        public Align? Align { set; get; }

        /// <summary>
        /// Implicitly converts a string to a Title object.
        /// </summary>
        /// <param name="v">The string value to convert.</param>
        /// <returns>A new instance of the Title class.</returns>
        public static implicit operator Title(string v) => new Title() { Text = v };
    }
}
