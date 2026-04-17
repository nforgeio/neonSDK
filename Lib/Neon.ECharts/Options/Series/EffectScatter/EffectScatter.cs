// -----------------------------------------------------------------------------
// FILE:	    EffectScatter.cs
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

namespace Neon.ECharts.Options.Series.EffectScatter
{
    /// <summary>
    /// Represents an effect scatter series in ECharts, which displays scatter points with ripple effects.
    /// </summary>
    public record EffectScatter : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "effectScatter";

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectScatter"/> class.
        /// </summary>
        public EffectScatter() : base(_Type) { }

        /// <summary>
        /// Gets or sets the coordinate system used.
        /// </summary>
        public string CoordinateSystem { get; set; }

        /// <summary>
        /// Gets or sets the index of the x-axis.
        /// </summary>
        public int? XAxisIndex { get; set; }

        /// <summary>
        /// Gets or sets the index of the y-axis.
        /// </summary>
        public int? YAxisIndex { get; set; }

        /// <summary>
        /// Gets or sets the symbol type.
        /// </summary>
        public object Symbol { get; set; }

        /// <summary>
        /// Gets or sets the symbol size.
        /// </summary>
        public object SymbolSize { get; set; }

        /// <summary>
        /// Gets or sets the ripple effect configuration.
        /// </summary>
        public object RippleEffect { get; set; }

        /// <summary>
        /// Gets or sets when to show the effect. Valid values: 'render', 'emphasis'.
        /// </summary>
        public string ShowEffectOn { get; set; }

        /// <summary>
        /// Gets or sets the item style.
        /// </summary>
        public ItemStyle ItemStyle { get; set; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        public Label Label { get; set; }

        /// <summary>
        /// Gets or sets the emphasis style.
        /// </summary>
        public Emphasis Emphasis { get; set; }
    }
}
