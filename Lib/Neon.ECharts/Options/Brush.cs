// -----------------------------------------------------------------------------
// FILE:	    Brush.cs
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
    /// Represents a Brush object.
    /// </summary>
    public class Brush
    {
        /// <summary>
        /// Gets or sets the X axis index.
        /// </summary>
        public object XAxisIndex { set; get; }

        /// <summary>
        /// Gets or sets the series index.
        /// </summary>
        public object SeriesIndex { set; get; }

        /// <summary>
        /// Gets or sets the brush type.
        /// </summary>
        public string BrushType { set; get; }

        /// <summary>
        /// Gets or sets the brush mode.
        /// </summary>
        public string BrushMode { set; get; }

        /// <summary>
        /// Gets or sets the brush link.
        /// </summary>
        public object BrushLink { set; get; }

        /// <summary>
        /// Gets or sets the out of brush.
        /// </summary>
        public object OutOfBrush { set; get; }

        /// <summary>
        /// Gets or sets the throttle type.
        /// </summary>
        public string ThrottleType { get; set; }

        /// <summary>
        /// Gets or sets the throttle delay.
        /// </summary>
        public int ThrottleDelay { get; set; }
    }
}
