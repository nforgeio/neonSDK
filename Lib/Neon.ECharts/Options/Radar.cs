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

using System.Collections.Generic;

using Neon.ECharts.Options.Enum;

namespace Neon.ECharts.Options
{
    public class Radar
    {
        /// <summary>
        /// Gets or sets the name of the radar.
        /// </summary>
        public RadarName Name { set; get; }

        /// <summary>
        /// Gets or sets the list of radar indicators.
        /// </summary>
        public List<RadarIndicator> Indicator { set; get; }

        /// <summary>
        /// Gets or sets the shape of the radar.
        /// </summary>
        public RadarShape? Shape { set; get; }

        /// <summary>
        /// Gets or sets the number of splits in the radar.
        /// </summary>
        public int? SplitNumber { set; get; }

        /// <summary>
        /// Gets or sets the split line options for the radar.
        /// </summary>
        public SplitLine SplitLine { set; get; }

        /// <summary>
        /// Gets or sets the split area options for the radar.
        /// </summary>
        public SplitArea SplitArea { set; get; }

        /// <summary>
        /// Gets or sets the axis line options for the radar.
        /// </summary>
        public AxisLine AxisLine { set; get; }

        /// <summary>
        /// Name options for radar indicators.
        /// </summary>
        public AxisName AxisName { get; set; }

        /// <summary>
        /// Distance between the indicator's name and axis.
        /// </summary>
        public int NameGap { get; set; }
    }
}
