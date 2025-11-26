// -----------------------------------------------------------------------------
// FILE:	    LightSpec.cs
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
    public record LightSpec
    {
        /// <summary>
        /// Gets or sets the color of the light.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? Color { get; set; }

        /// <summary>
        /// Gets or sets the intensity of the light.
        /// </summary>
        public double? Intensity { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the light has a shadow.
        /// </summary>
        public bool? Shadow { get; set; }

        /// <summary>
        /// Gets or sets the quality of the shadow.
        /// </summary>
        public string ShadowQuality { get; set; }

        /// <summary>
        /// Gets or sets the alpha value of the light.
        /// </summary>
        public int? Alpha { get; set; }

        /// <summary>
        /// Gets or sets the beta value of the light.
        /// </summary>
        public int? Beta { get; set; }
    }
}
