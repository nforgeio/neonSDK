// -----------------------------------------------------------------------------
// FILE:	    SeriesBase.cs
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

using Neon.ECharts.Options.Series;

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents the base class for series options in ECharts.
    /// </summary>
    public record SeriesBase : ISeries
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SeriesBase"/> class.
        /// </summary>
        /// <param name="type">The type of the series.</param>
        /// <param name="id">The ID of the series.</param>
        /// <param name="name">The name of the series.</param>
        public SeriesBase(string type, string id = null, string name = null)
        {
            Type = type;
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the ID of the series.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        /// Gets or sets the type of the series.
        /// </summary>
        public string Type { set; get; } = "line";

        /// <summary>
        /// Gets or sets the name of the series.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        /// Gets or sets the animation delay of the series.
        /// </summary>
        public object AnimationDelay { set; get; }

        /// <summary>
        /// Gets or sets the data of the series.
        /// </summary>
        public object Data { set; get; }

        /// <summary>
        /// Gets or sets the z-level of the series.
        /// </summary>
        public int? Zlevel { set; get; }

        /// <summary>
        /// Gets or sets the tooltip of the series.
        /// </summary>
        public Tooltip Tooltip { set; get; }
    }
}
