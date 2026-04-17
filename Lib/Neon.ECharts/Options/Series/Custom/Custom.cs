// -----------------------------------------------------------------------------
// FILE:	    Custom.cs
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

namespace Neon.ECharts.Options.Series.Custom
{
    /// <summary>
    /// Represents a custom series in ECharts, allowing user-defined rendering logic.
    /// </summary>
    public record Custom : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "custom";

        /// <summary>
        /// Initializes a new instance of the <see cref="Custom"/> class.
        /// </summary>
        public Custom() : base(_Type) { }

        /// <summary>
        /// Gets or sets the coordinate system used.
        /// </summary>
        public string CoordinateSystem { get; set; }

        /// <summary>
        /// Gets or sets the index of the x-axis.
        /// </summary>
        public int? XAxisIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the y-axis.
        /// </summary>
        public int? YAxisIndex { get; set; }

        /// <summary>
        /// Gets or sets the render item callback function (as a JFunc or string).
        /// </summary>
        public object RenderItem { get; set; }

        /// <summary>
        /// Gets or sets the item style.
        /// </summary>
        public ItemStyle ItemStyle { get; set; }

        /// <summary>
        /// Gets or sets the encoding configuration.
        /// </summary>
        public Encode Encode { get; set; }
    }
}
