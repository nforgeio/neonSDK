// -----------------------------------------------------------------------------
// FILE:	    Color.cs
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
    /// <summary>
    /// Represents a gradation color.
    /// </summary>
    public record GradationColor
    {
        /// <summary>
        /// Gets or sets the type of the color.
        /// </summary>
        public ColorType? Type { set; get; }

        /// <summary>
        /// Gets or sets the X coordinate.
        /// </summary>
        public double? X { set; get; }

        /// <summary>
        /// Gets or sets the Y coordinate.
        /// </summary>
        public double? Y { set; get; }

        /// <summary>
        /// Gets or sets the ending X coordinate.
        /// </summary>
        public double? X2 { set; get; }

        /// <summary>
        /// Gets or sets the ending Y coordinate.
        /// </summary>
        public double? Y2 { set; get; }

        /// <summary>
        /// Gets or sets the list of color stops.
        /// </summary>
        public List<ColorStops> ColorStops { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether the color is global.
        /// </summary>
        public bool? Global { set; get; }
    }
}
