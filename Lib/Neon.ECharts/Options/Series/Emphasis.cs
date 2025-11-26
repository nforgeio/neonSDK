// -----------------------------------------------------------------------------
// FILE:	    Emphasis.cs
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

namespace Neon.ECharts.Options.Series
{
    public record Emphasis
    {
        /// <summary>
        /// Gets or sets a value indicating whether to show the emphasis.
        /// </summary>
        public bool? Show { set; get; }

        /// <summary>
        /// Gets or sets the label for the emphasis.
        /// </summary>
        public Label Label { set; get; }

        /// <summary>
        /// Gets or sets the text style for the emphasis.
        /// </summary>
        public TextStyle TextStyle { set; get; }

        /// <summary>
        /// Gets or sets the item style for the emphasis.
        /// </summary>
        public ItemStyle ItemStyle { set; get; }

        /// <summary>
        /// Gets or sets the shadow blur for the emphasis.
        /// </summary>
        public int? ShadowBlur { get; set; }

        /// <summary>
        /// Gets or sets the shadow color for the emphasis.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? ShadowColor { get; set; }
    }
}
