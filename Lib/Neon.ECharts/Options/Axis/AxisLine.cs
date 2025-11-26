// -----------------------------------------------------------------------------
// FILE:	    AxisLine.cs
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
    /// Represents the axis line options.
    /// </summary>
    public class AxisLine
    {
        /// <summary>
        /// Gets or sets a value indicating whether to show the axis line.
        /// </summary>
        public bool? Show { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the axis line should be positioned at zero.
        /// </summary>
        public bool? OnZero { set; get; }

        /// <summary>
        /// Gets or sets the line style of the axis line.
        /// </summary>
        public LineStyle LineStyle { set; get; }
    }
}
