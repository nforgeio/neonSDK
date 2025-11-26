// -----------------------------------------------------------------------------
// FILE:	    VisualMapPiecewise.cs
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

namespace Neon.ECharts.Options
{
    public record VisualMapPiecewise
    {
        /// <summary>
        /// Gets or sets a value indicating whether the visual map is shown.
        /// </summary>
        public bool? Show { set; get; }

        /// <summary>
        /// Gets or sets the top position of the visual map.
        /// </summary>
        public object Top { set; get; }

        /// <summary>
        /// Gets or sets the bottom position of the visual map.
        /// </summary>
        public object Bottom { set; get; }

        /// <summary>
        /// Gets or sets the right position of the visual map.
        /// </summary>
        public object Right { set; get; }

        /// <summary>
        /// Gets or sets the pieces of the visual map.
        /// </summary>
        public List<object> Pieces { set; get; }

        /// <summary>
        /// Gets or sets the range of values that are considered out of range.
        /// </summary>
        public ChannelRange OutOfRange { set; get; }

        /// <summary>
        /// Gets or sets the series index that the visual map is associated with.
        /// </summary>
        public object SeriesIndex { set; get; }

        /// <summary>
        /// Gets or sets the dimension to be visualized.
        /// </summary>
        public int? Dimension { set; get; }

        /// <summary>
        /// Gets or sets the minimum value of the visual map.
        /// </summary>
        public double? Min { set; get; }

        /// <summary>
        /// Gets or sets the maximum value of the visual map.
        /// </summary>
        public double? Max { set; get; }

        /// <summary>
        /// Gets or sets the text labels of the visual map.
        /// </summary>
        public string[] Text { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the visual map is updated in real-time.
        /// </summary>
        public bool? Realtime { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the visual map is calculable.
        /// </summary>
        public bool? Calculable { set; get; }

        /// <summary>
        /// Gets or sets the range of values that are considered in range.
        /// </summary>
        public ChannelRange InRange { set; get; }

        /// <summary>
        /// Gets or sets the text style of the visual map.
        /// </summary>
        public TextStyle TextStyle { set; get; }
    }
    
}
