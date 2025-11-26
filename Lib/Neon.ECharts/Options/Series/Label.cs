// -----------------------------------------------------------------------------
// FILE:	    Label.cs
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

namespace Neon.ECharts.Options.Series
{
    public record Label
    {
        /// <summary>
        /// Gets or sets a value indicating whether the label is shown.
        /// </summary>
        public bool? Show { set; get; }

        /// <summary>
        /// Gets or sets the position of the label.
        /// </summary>
        public LabelPosition? Position { set; get; }

        /// <summary>
        /// Gets or sets the emphasis style of the label.
        /// </summary>
        public Emphasis Emphasis { set; get; }

        /// <summary>
        /// Gets or sets the formatter function or string for the label.
        /// </summary>
        public object Formatter { set; get; }

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
        public double? BorderWidth { set; get; }

        /// <summary>
        /// Gets or sets the border radius of the label.
        /// </summary>
        public object BorderRadius { set; get; }

        /// <summary>
        /// Gets or sets the rich text styles for the label.
        /// </summary>
        public object Rich { set; get; }

        /// <summary>
        /// Gets or sets the alignment of the label.
        /// </summary>
        public Align2 Align { set; get; }

        /// <summary>
        /// Gets or sets the color of the label.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? Color { set; get; }

        /// <summary>
        /// Gets or sets the width of the label.
        /// </summary>
        public object Width { set; get; }

        /// <summary>
        /// Gets or sets the height of the label.
        /// </summary>
        public object Height { set; get; }

        /// <summary>
        /// Gets or sets the padding of the label.
        /// </summary>
        public object Padding { set; get; }

        /// <summary>
        /// Gets or sets the font size of the label.
        /// </summary>
        public int? FontSize { set; get; }

        /// <summary>
        /// Gets or sets the rotation angle of the label.
        /// </summary>
        public object Rotate { get; set; }

        /// <summary>
        /// Gets or sets the text style of the label.
        /// </summary>
        public TextStyle TextStyle { get; set; }
    }
}
