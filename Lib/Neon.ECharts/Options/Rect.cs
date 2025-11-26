// -----------------------------------------------------------------------------
// FILE:	    Rect.cs
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
    /// Represents a rectangular shape.
    /// </summary>
    public record Rect
    {
        /// <summary>
        /// Gets or sets the X-coordinate of the top-left corner of the rectangle.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y-coordinate of the top-left corner of the rectangle.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the width of the rectangle. Can be null.
        /// </summary>
        public double? Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the rectangle. Can be null.
        /// </summary>
        public double? Height { get; set; }
    }
}
