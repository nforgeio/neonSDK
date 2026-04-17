// -----------------------------------------------------------------------------
// FILE:	    ThemeRiver.cs
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

namespace Neon.ECharts.Options.Series.ThemeRiver
{
    /// <summary>
    /// Represents a theme river series in ECharts.
    /// </summary>
    public record ThemeRiver : SeriesBase, IPosition
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "themeRiver";

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeRiver"/> class.
        /// </summary>
        public ThemeRiver() : base(_Type) { }

        /// <summary>
        /// Gets or sets the left position.
        /// </summary>
        public object Left { get; set; }

        /// <summary>
        /// Gets or sets the top position.
        /// </summary>
        public object Top { get; set; }

        /// <summary>
        /// Gets or sets the right position.
        /// </summary>
        public object Right { get; set; }

        /// <summary>
        /// Gets or sets the bottom position.
        /// </summary>
        public object Bottom { get; set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        public object Width { get; set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public object Height { get; set; }

        /// <summary>
        /// Gets or sets the coordinate system used.
        /// </summary>
        public string CoordinateSystem { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        public Label Label { get; set; }

        /// <summary>
        /// Gets or sets the item style.
        /// </summary>
        public ItemStyle ItemStyle { get; set; }

        /// <summary>
        /// Gets or sets the emphasis style.
        /// </summary>
        public Emphasis Emphasis { get; set; }
    }
}
