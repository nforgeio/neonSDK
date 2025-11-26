// -----------------------------------------------------------------------------
// FILE:	    EChartsEventBatch.cs
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
    /// Represents an event batch in ECharts.
    /// </summary>
    public record EchartsEventBatch
    {
        /// <summary>
        /// Gets or sets the brush ID.
        /// </summary>
        public string BrushId { get; set; }

        /// <summary>
        /// Gets or sets the brush index.
        /// </summary>
        public int BrushIndex { get; set; }

        /// <summary>
        /// Gets or sets the brush name.
        /// </summary>
        public string BrushName { get; set; }

        /// <summary>
        /// Gets or sets the areas.
        /// </summary>
        public Area[] Areas { get; set; }

        /// <summary>
        /// Gets or sets the selected items.
        /// </summary>
        public Selected[] Selected { get; set; }

        /// <summary>
        /// Gets or sets the type of the event batch.
        /// </summary>
        public string Type { get; set; }
    }

    /// <summary>
    /// Represents an area in ECharts.
    /// </summary>
    public record Area
    {
        /// <summary>
        /// Gets or sets the brush type.
        /// </summary>
        public string BrushType { get; set; }

        /// <summary>
        /// Gets or sets the brush mode.
        /// </summary>
        public string BrushMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the area is transformable.
        /// </summary>
        public bool? Transformable { get; set; }

        /// <summary>
        /// Gets or sets the brush style.
        /// </summary>
        public BrushStyle BrushStyle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the area should be removed on click.
        /// </summary>
        public bool? RemoveOnClick { get; set; }

        /// <summary>
        /// Gets or sets the z-index.
        /// </summary>
        public int? Z { get; set; }

        /// <summary>
        /// Gets or sets the panel ID.
        /// </summary>
        public string PanelId { get; set; }

        /// <summary>
        /// Gets or sets the range.
        /// </summary>
        public double[] Range { get; set; }

        /// <summary>
        /// Gets or sets the coordinate ranges.
        /// </summary>
        public double[][] CoordRanges { get; set; }

        /// <summary>
        /// Gets or sets the coordinate range.
        /// </summary>
        public object[] CoordRange { get; set; }

        /// <summary>
        /// Gets or sets the X-axis index.
        /// </summary>
        public int? XAxisIndex { get; set; }
    }
}
