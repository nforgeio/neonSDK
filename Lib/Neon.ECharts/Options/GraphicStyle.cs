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
    /// Represents the graphic style used in ECharts options.
    /// </summary>
    public record GraphicStyle
    {
        /// <summary>
        /// Gets or sets the image used for the graphic.
        /// </summary>
        public string Image { set; get; }

        /// <summary>
        /// Gets or sets the width of the graphic.
        /// </summary>
        public int? Width { set; get; }

        /// <summary>
        /// Gets or sets the height of the graphic.
        /// </summary>
        public int? Height { set; get; }

        /// <summary>
        /// Gets or sets the opacity of the graphic.
        /// </summary>
        public double? Opacity { set; get; }
    }
}
