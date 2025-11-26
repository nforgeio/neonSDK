// -----------------------------------------------------------------------------
// FILE:	    AreaStyle.cs
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
    /// Represents the area style options for a chart.
    /// </summary>
    public record AreaStyle
    {
        /// <summary>
        /// Gets or sets the color of the area.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? Color { set; get; }

        /// <summary>
        /// Gets or sets the blur radius of the shadow.
        /// </summary>
        public int? ShadowBlur { set; get; }

        /// <summary>
        /// Gets or sets the color of the shadow.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? ShadowColor { set; get; }

        /// <summary>
        /// Gets or sets the horizontal offset of the shadow.
        /// </summary>
        public int? ShadowOffsetX { set; get; }

        /// <summary>
        /// Gets or sets the vertical offset of the shadow.
        /// </summary>
        public int? ShadowOffsetY { set; get; }

        /// <summary>
        /// Gets or sets the opacity of the area.
        /// </summary>
        public double? Opacity { set; get; }

        /// <summary>
        /// Gets or sets the origin of the area.
        /// </summary>
        public Origin? Origin { set; get; }
    }
}
