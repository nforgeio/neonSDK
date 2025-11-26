// -----------------------------------------------------------------------------
// FILE:	    ItemStyle.cs
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
    /// <summary>
    /// Represents the item style options for a series in ECharts.
    /// </summary>
    public record ItemStyle
    {
        /// <summary>
        /// Gets or sets the color of the item.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? Color { set; get; }

        /// <summary>
        /// Gets or sets the color of the item when the value is 0.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? Color0 { set; get; }

        /// <summary>
        /// Gets or sets the border color of the item.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? BorderColor { set; get; }

        /// <summary>
        /// Gets or sets the border width of the item.
        /// </summary>
        public int? BorderWidth { set; get; }

        /// <summary>
        /// Gets or sets the border color of the item when the value is 0.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? BorderColor0 { set; get; }

        /// <summary>
        /// Gets or sets the text border width of the item.
        /// </summary>
        public int? TextBorderWidth { set; get; }

        /// <summary>
        /// Gets or sets the border type of the item.
        /// </summary>
        public LineStyleType? BorderType { set; get; }

        /// <summary>
        /// Gets or sets the shadow blur of the item.
        /// </summary>
        public int? ShadowBlur { set; get; }

        /// <summary>
        /// Gets or sets the shadow color of the item.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? ShadowColor { set; get; }

        /// <summary>
        /// Gets or sets the shadow offset on the x-axis of the item.
        /// </summary>
        public int? ShadowOffsetX { set; get; }

        /// <summary>
        /// Gets or sets the shadow offset on the y-axis of the item.
        /// </summary>
        public int? ShadowOffsetY { set; get; }
    }
}
