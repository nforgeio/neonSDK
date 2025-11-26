// -----------------------------------------------------------------------------
// FILE:	    Selected.cs
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

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents the selected state of a series in ECharts.
    /// </summary>
    public class Selected
    {
        /// <summary>
        /// Gets or sets the series ID.
        /// </summary>
        public string SeriesId { get; set; }

        /// <summary>
        /// Gets or sets the series index.
        /// </summary>
        public int SeriesIndex { get; set; }

        /// <summary>
        /// Gets or sets the series name.
        /// </summary>
        public string SeriesName { get; set; }

        /// <summary>
        /// Gets or sets the array of data indices.
        /// </summary>
        public int[] DataIndex { get; set; }
    }
}
