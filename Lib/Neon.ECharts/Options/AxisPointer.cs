// -----------------------------------------------------------------------------
// FILE:	    AxisPointer.cs
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
    /// Represents an axis pointer in ECharts options.
    /// </summary>
    public class AxisPointer
    {
        /// <summary>
        /// Gets or sets the ID of the axis pointer.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the axis pointer is shown.
        /// </summary>
        public bool? Show { set; get; }

        /// <summary>
        /// Gets or sets the type of the axis pointer.
        /// </summary>
        public AxisPointerType? Type { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the axis pointer snaps to the data points.
        /// </summary>
        public bool? Snap { set; get; }

        /// <summary>
        /// Gets or sets the z-index of the axis pointer.
        /// </summary>
        public int? Z { set; get; }

        /// <summary>
        /// Gets or sets the label of the axis pointer.
        /// </summary>
        public AxisPointerLabel Label { set; get; }

        /// <summary>
        /// Gets or sets the link configuration of the axis pointer.
        /// </summary>
        public AxisPointerLink Link { set; get; }

        /// <summary>
        /// Gets or sets the line style of the axis pointer.
        /// </summary>
        public LineStyle LineStyle { set; get; }

        /// <summary>
        /// Whether to trigger emphasis of series.
        /// </summary>
        public bool? TriggerEmphasis { get; set; }

        /// <summary>
        /// Whether to trigger tooltip.
        /// </summary>
        public bool? TriggerTooltip { get; set; }
    }
    
}
