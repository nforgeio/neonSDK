// -----------------------------------------------------------------------------
// FILE:	    Map3D.cs
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

namespace Neon.ECharts.Options.Series.Map3D
{
    /// <summary>
    /// Represents a 3D map series in ECharts GL.
    /// </summary>
    public record Map3D : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "map3D";

        /// <summary>
        /// Initializes a new instance of the <see cref="Map3D"/> class.
        /// </summary>
        public Map3D() : base(_Type) { }

        /// <summary>
        /// Gets or sets the map type (e.g., 'china', 'world').
        /// </summary>
        public string MapType { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        public Label Label { get; set; }

        /// <summary>
        /// Gets or sets the item style.
        /// </summary>
        public ItemStyle ItemStyle { get; set; }

        /// <summary>
        /// Gets or sets the emphasis style.
        /// </summary>
        public Emphasis Emphasis { get; set; }

        /// <summary>
        /// Gets or sets the shading style (e.g., 'color', 'lambert', 'realistic').
        /// </summary>
        public string Shading { get; set; }

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
