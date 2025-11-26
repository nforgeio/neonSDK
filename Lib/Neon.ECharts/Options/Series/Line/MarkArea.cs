// -----------------------------------------------------------------------------
// FILE:	    MarkArea.cs
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

namespace Neon.ECharts.Options.Series.Line
{
    /// <summary>
    /// Represents the mark area configuration for a line series.
    /// </summary>
    public class MarkArea
    {
        /// <summary>
        /// Gets or sets a value indicating whether the mark area is silent.
        /// </summary>
        public bool? Silent { set; get; }

        /// <summary>
        /// Gets or sets the data for the mark area.
        /// </summary>
        public List<List<MarkAreaData>> Data { set; get; }
    }

    /// <summary>
    /// Represents the data for a mark area.
    /// </summary>
    public class MarkAreaData
    {
        /// <summary>
        /// Gets or sets the x-axis value for the mark area data.
        /// </summary>
        public string XAxis { set; get; }
    }
}
