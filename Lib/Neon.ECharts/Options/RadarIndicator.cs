// -----------------------------------------------------------------------------
// FILE:	    RadarIndicator.cs
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
    public class RadarIndicator
    {
        /// <summary>
        /// Gets or sets the name of the radar indicator.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        /// Gets or sets the maximum value of the radar indicator.
        /// </summary>
        public int? Max { set; get; }

        /// <summary>
        /// Gets or sets the minimum value of the radar indicator.
        /// </summary>
        public int? Min { set; get; }

        /// <summary>
        /// Gets or sets the color of the radar indicator.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? Color { set; get; }
    }
}
