// -----------------------------------------------------------------------------
// FILE:	    Graphic.cs
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
    /// Represents a graphic element in ECharts.
    /// </summary>
    public record Graphic
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Graphic"/> class.
        /// </summary>
        public Graphic() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graphic"/> class with the specified type.
        /// </summary>
        /// <param name="type">The type of the graphic.</param>
        public Graphic(string type) { Type = type; }

        /// <summary>
        /// Gets or sets the type of the graphic.
        /// </summary>
        public string Type { set; get; }

        /// <summary>
        /// Gets or sets the ID of the graphic.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        /// Gets or sets the left position of the graphic.
        /// </summary>
        public object Left { set; get; }

        /// <summary>
        /// Gets or sets the right position of the graphic.
        /// </summary>
        public object Right { set; get; }

        /// <summary>
        /// Gets or sets the top position of the graphic.
        /// </summary>
        public object Top { set; get; }

        /// <summary>
        /// Gets or sets the bottom position of the graphic.
        /// </summary>
        public object Bottom { set; get; }

        /// <summary>
        /// Gets or sets the z-index of the graphic.
        /// </summary>
        public int? Z { set; get; }

        /// <summary>
        /// Gets or sets the bounding of the graphic.
        /// </summary>
        public object Bounding { set; get; }

        /// <summary>
        /// Gets or sets the origin of the graphic.
        /// </summary>
        public int[] Origin { set; get; }

        /// <summary>
        /// Gets or sets the style of the graphic.
        /// </summary>
        public GraphicStyle Style { set; get; }

        /// <summary>
        /// Gets or sets the rotation of the graphic.
        /// </summary>
        public object Rotation { set; get; }
    }
}
