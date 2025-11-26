// -----------------------------------------------------------------------------
// FILE:	    SplitLine.cs
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
    /// Represents the split line options for the chart.
    /// </summary>
    public class SplitLine
    {
        /// <summary>
        /// Gets or sets a value indicating whether to show the split line.
        /// </summary>
        public bool? Show { set; get; }

        /// <summary>
        /// Gets or sets the interval between split lines.
        /// </summary>
        public object Interval { set; get; }

        /// <summary>
        /// Gets or sets the line style of the split line.
        /// </summary>
        public LineStyle LineStyle { set; get; }
    }
}
