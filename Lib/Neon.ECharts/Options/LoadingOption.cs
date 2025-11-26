// -----------------------------------------------------------------------------
// FILE:	    LoadingOption.cs
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
    /// <summary>
    /// Represents the loading options for ECharts.
    /// </summary>
    public record LoadingOption
    {
        /// <summary>
        /// Gets or sets the text to display while loading.
        /// </summary>
        public string Text { get; set; } = "loading";

        /// <summary>
        /// Gets or sets the color of the loading spinner.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color Color { get; set; } = Color.Red;

        /// <summary>
        /// Gets or sets the color of the loading text.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color TextColor { get; set; } = Color.Black;

        /// <summary>
        /// Gets or sets the color of the loading mask.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color MaskColor { get; set; } = Color.FromArgb(255, Color.White);

        /// <summary>
        /// Gets or sets the z-level of the loading element.
        /// </summary>
        public int Zlevel { get; set; } = 0;

        /// <summary>
        /// Gets or sets the font size of the loading text.
        /// </summary>
        public int FontSize { get; set; } = 12;

        /// <summary>
        /// Gets or sets a value indicating whether to show the loading spinner.
        /// </summary>
        public bool ShowSpinner { get; set; } = true;

        /// <summary>
        /// Gets or sets the radius of the loading spinner.
        /// </summary>
        public int SpinnerRadius { get; set; } = 10;

        /// <summary>
        /// Gets or sets the line width of the loading spinner.
        /// </summary>
        public int LineWidth { get; set; } = 5;

        /// <summary>
        /// Gets or sets the font weight of the loading text.
        /// </summary>
        public string FontWeight { get; set; } = "normal";

        /// <summary>
        /// Gets or sets the font style of the loading text.
        /// </summary>
        public string FontStyle { get; set; } = "normal";

        /// <summary>
        /// Gets or sets the font family of the loading text.
        /// </summary>
        public string FontFamily { get; set; } = "sans-serif";

        /// <inheritdoc/>
        public override string ToString()
        {
            return EChartsOptionSerializer.Default.Serialize(this);
        }
    }
}
