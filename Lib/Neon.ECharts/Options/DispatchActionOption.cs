// -----------------------------------------------------------------------------
// FILE:	    DispatchActionOption.cs
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

using System.Collections.Generic;

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents the options for dispatching an action in ECharts.
    /// </summary>
    public record DispatchActionOption
    {
        /// <summary>
        /// Gets or sets the type of the action.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the index of the series.
        /// </summary>
        public int? SeriesIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the data.
        /// </summary>
        public int? DataIndex { get; set; }

        /// <summary>
        /// Gets or sets the list of areas.
        /// </summary>
        public List<Area> Areas { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return EChartsOptionSerializer.Default.Serialize(this);
        }
    }
}
