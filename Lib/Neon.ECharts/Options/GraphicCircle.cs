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
    /// Represents a circle graphic element.
    /// </summary>
    public record GraphicCircle : Graphic
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicCircle"/> class with the default shape type "circle".
        /// </summary>
        public GraphicCircle() : base("circle") { }

        /// <summary>
        /// Gets or sets the position of the circle.
        /// </summary>
        public object Position { get; set; }

        /// <summary>
        /// Gets or sets the shape of the circle.
        /// </summary>
        public GraphicCircleShape Shape { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the circle is invisible.
        /// </summary>
        public bool? Invisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the circle is draggable.
        /// </summary>
        public bool? Draggable { get; set; }

        /// <summary>
        /// Gets or sets the JavaScript function to be executed when the circle is dragged.
        /// </summary>
        public JFunc Ondrag { get; set; }

        /// <summary>
        /// Gets or sets the JavaScript function to be executed when the mouse moves over the circle.
        /// </summary>
        public JFunc Onmousemove { get; set; }

        /// <summary>
        /// Gets or sets the JavaScript function to be executed when the mouse moves out of the circle.
        /// </summary>
        public JFunc Onmouseout { get; set; }
    }
}
