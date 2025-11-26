// -----------------------------------------------------------------------------
// FILE:	    AxisLabel.cs
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
    public record AxisLabel
    {
        /// <summary>
        /// Gets or sets the formatter for the axis label.
        /// </summary>
        public object Formatter { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the axis label.
        /// </summary>
        public bool? Show { set; get; }

        /// <summary>
        /// Gets or sets the rotation angle of the axis label.
        /// </summary>
        public int? Rotate { get; set; }

        /// <summary>
        /// Gets or sets the interval for displaying axis labels.
        /// </summary>
        public object Interval { get; set; }
    }
}
