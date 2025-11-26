// -----------------------------------------------------------------------------
// FILE:	    Radar.cs
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

namespace Neon.ECharts.Options.Series.Radar
{
    /// <summary>
    /// Represents a radar series in ECharts.
    /// </summary>
    public record Radar : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "radar";

        /// <summary>
        /// Initializes a new instance of the <see cref="Radar"/> class.
        /// </summary>
        public Radar() : base(_Type) { }

        /// <summary>
        /// Gets or sets the line style of the radar series.
        /// </summary>
        public LineStyle LineStyle { set; get; }

        /// <summary>
        /// Gets or sets the symbol of the radar series.
        /// </summary>
        public object Symbol { set; get; }

        /// <summary>
        /// Gets or sets the item style of the radar series.
        /// </summary>
        public ItemStyle ItemStyle { set; get; }

        /// <summary>
        /// Gets or sets the area style of the radar series.
        /// </summary>
        public AreaStyle AreaStyle { set; get; }
    }
}
