// -----------------------------------------------------------------------------
// FILE:	    Grid3D.cs
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
    /// Represents a 3D grid for ECharts.
    /// </summary>
    public record Grid3D : Grid
    {
        /// <summary>
        /// Gets or sets the width of each box in the grid.
        /// </summary>
        public int? BoxWidth { set; get; }

        /// <summary>
        /// Gets or sets the depth of each box in the grid.
        /// </summary>
        public int? BoxDepth { set; get; }

        /// <summary>
        /// Gets or sets the light settings for the grid.
        /// </summary>
        public Light Light { get; set; }

        /// <summary>
        /// Gets or sets the axis pointer settings for the grid.
        /// </summary>
        public AxisPointer AxisPointer { get; set; }

        /// <summary>
        /// Gets or sets the axis line settings for the grid.
        /// </summary>
        public AxisLine AxisLine { get; set; }

        /// <summary>
        /// Gets or sets the view control settings for the grid.
        /// </summary>
        public ViewControl ViewControl { get; set; }
    }
}
