// -----------------------------------------------------------------------------
// FILE:	    Grid.cs
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

namespace Neon.ECharts.Options
{
    public record Grid : IPosition
    {
        /// <summary>
        /// Gets or sets the ID of the grid.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid is shown.
        /// </summary>
        public bool? Show { set; get; }

        /// <summary>
        /// Gets or sets the z-level of the grid.
        /// </summary>
        public int? Zlevel { set; get; }

        /// <summary>
        /// Gets or sets the z-index of the grid.
        /// </summary>
        public int? Z { set; get; }

        /// <summary>
        /// Gets or sets the left position of the grid.
        /// </summary>
        public object Left { set; get; }

        /// <summary>
        /// Gets or sets the top position of the grid.
        /// </summary>
        public object Top { set; get; }

        /// <summary>
        /// Gets or sets the right position of the grid.
        /// </summary>
        public object Right { set; get; }

        /// <summary>
        /// Gets or sets the bottom position of the grid.
        /// </summary>
        public object Bottom { set; get; }

        /// <summary>
        /// Gets or sets the width of the grid.
        /// </summary>
        public object Width { set; get; }

        /// <summary>
        /// Gets or sets the height of the grid.
        /// </summary>
        public object Height { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the grid contains labels.
        /// </summary>
        public bool? ContainLabel { set; get; }

        /// <summary>
        /// Gets or sets the background color of the grid.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? BackgroundColor { set; get; }

        /// <summary>
        /// Gets or sets the border color of the grid.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? BorderColor { set; get; }

        /// <summary>
        /// Gets or sets the border width of the grid.
        /// </summary>
        public int? BorderWidth { set; get; }

        /// <summary>
        /// Gets or sets the shadow blur of the grid.
        /// </summary>
        public int? ShadowBlur { set; get; }

        /// <summary>
        /// Gets or sets the shadow color of the grid.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? ShadowColor { set; get; }

        /// <summary>
        /// Gets or sets the shadow offset on the x-axis of the grid.
        /// </summary>
        public int? ShadowOffsetX { set; get; }

        /// <summary>
        /// Gets or sets the shadow offset on the y-axis of the grid.
        /// </summary>
        public int? ShadowOffsetY { set; get; }

        /// <summary>
        /// Gets or sets the tooltip configuration for the grid.
        /// </summary>
        public Tooltip Tooltip { set; get; }
    }
}
