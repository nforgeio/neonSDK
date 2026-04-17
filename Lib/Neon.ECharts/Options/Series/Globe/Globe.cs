// -----------------------------------------------------------------------------
// FILE:	    Globe.cs
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

namespace Neon.ECharts.Options.Series.Globe
{
    /// <summary>
    /// Represents a globe series in ECharts GL, used for 3D globe visualization.
    /// </summary>
    public record Globe : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "globe";

        /// <summary>
        /// Initializes a new instance of the <see cref="Globe"/> class.
        /// </summary>
        public Globe() : base(_Type) { }

        /// <summary>
        /// Gets or sets the base texture of the globe (image URL or HTMLImageElement).
        /// </summary>
        public object BaseTexture { get; set; }

        /// <summary>
        /// Gets or sets the height texture for terrain.
        /// </summary>
        public object HeightTexture { get; set; }

        /// <summary>
        /// Gets or sets the displacement scale for the height texture.
        /// </summary>
        public double? DisplacementScale { get; set; }

        /// <summary>
        /// Gets or sets the shading style (e.g., 'color', 'lambert', 'realistic').
        /// </summary>
        public string Shading { get; set; }

        /// <summary>
        /// Gets or sets the environment texture.
        /// </summary>
        public object Environment { get; set; }

        /// <summary>
        /// Gets or sets the light configuration.
        /// </summary>
        public object Light { get; set; }

        /// <summary>
        /// Gets or sets the view control configuration.
        /// </summary>
        public object ViewControl { get; set; }
    }
}
