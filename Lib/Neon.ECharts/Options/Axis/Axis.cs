// -----------------------------------------------------------------------------
// FILE:	    Axis.cs
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

using Neon.ECharts.Options.Enum;

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents an axis in the ECharts library.
    /// </summary>
    public record Axis
    {
        /// <summary>
        /// Gets or sets the name of the axis.
        /// </summary>
        public string Name { set; get; }

        /// <summary>
        /// Gets or sets the location of the axis name.
        /// </summary>
        public Location? NameLocation { set; get; }

        /// <summary>
        /// Gets or sets the gap between the axis name and the axis line.
        /// </summary>
        public object NameGap { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the axis is inverted.
        /// </summary>
        public bool? Inverse { set; get; }

        /// <summary>
        /// Gets or sets the maximum value of the axis.
        /// </summary>
        public object Max { set; get; }

        /// <summary>
        /// Gets or sets the minimum value of the axis.
        /// </summary>
        public object Min { set; get; }

        /// <summary>
        /// Gets or sets the type of the axis.
        /// </summary>
        public AxisType? Type { set; get; }

        /// <summary>
        /// Gets or sets the data for the axis.
        /// </summary>
        public object Data { set; get; }

        /// <summary>
        /// Gets or sets the gap between data and the boundary of the axis.
        /// </summary>
        public object BoundaryGap { set; get; }

        /// <summary>
        /// Gets or sets the line style of the axis line.
        /// </summary>
        public AxisLine AxisLine { set; get; }

        /// <summary>
        /// Gets or sets the index of the grid that the axis belongs to.
        /// </summary>
        public int? GridIndex { set; get; }

        /// <summary>
        /// Gets or sets the line style of the split line.
        /// </summary>
        public SplitLine SplitLine { set; get; }

        /// <summary>
        /// Gets or sets the label of the axis.
        /// </summary>
        public AxisLabel AxisLabel { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the axis is silent.
        /// </summary>
        public bool? Silent { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to scale the axis.
        /// </summary>
        public bool? Scale { set; get; }

        /// <summary>
        /// Gets or sets the number of splits on the axis.
        /// </summary>
        public int? SplitNumber { set; get; }

        /// <summary>
        /// Gets or sets the style of the axis pointer.
        /// </summary>
        public AxisPointer AxisPointer { set; get; }

        /// <summary>
        /// Gets or sets the style of the axis tick.
        /// </summary>
        public AxisTick AxisTick { set; get; }

        /// <summary>
        /// Gets or sets the style of the split area.
        /// </summary>
        public SplitArea SplitArea { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to align the ticks of multiple axes.
        /// </summary>
        public bool? AlignTicks { set; get; }

        /// <summary>
        /// Gets or sets the offset of the axis.
        /// </summary>
        public double? Offset { set; get; }
    }
}
