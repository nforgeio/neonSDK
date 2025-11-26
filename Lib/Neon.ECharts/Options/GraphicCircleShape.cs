// -----------------------------------------------------------------------------
// FILE:	    GraphicCircle.cs
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
    /// Represents a circle shape in a graphic element.
    /// </summary>
    public record GraphicCircleShape
    {
        /// <summary>
        /// Gets or sets the x-coordinate of the center of the circle.
        /// </summary>
        public int? Cx { get; set; }

        /// <summary>
        /// Gets or sets the y-coordinate of the center of the circle.
        /// </summary>
        public int? Cy { get; set; }

        /// <summary>
        /// Gets or sets the radius of the circle.
        /// </summary>
        public double? R { get; set; }
    }
}
