// -----------------------------------------------------------------------------
// FILE:	    Funnel.cs
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

using Neon.ECharts.Options.Enum;

namespace Neon.ECharts.Options.Series.Funnel
{
    /// <summary>
    /// Represents a funnel series in ECharts.
    /// </summary>
    public record Funnel : SeriesBase, IPosition
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "funnel";

        /// <summary>
        /// Initializes a new instance of the <see cref="Funnel"/> class.
        /// </summary>
        public Funnel() : base(_Type) { }

        /// <summary>
        /// Gets or sets the left position of the funnel.
        /// </summary>
        public object Left { get; set; }

        /// <summary>
        /// Gets or sets the top position of the funnel.
        /// </summary>
        public object Top { get; set; }

        /// <summary>
        /// Gets or sets the right position of the funnel.
        /// </summary>
        public object Right { get; set; }

        /// <summary>
        /// Gets or sets the bottom position of the funnel.
        /// </summary>
        public object Bottom { get; set; }

        /// <summary>
        /// Gets or sets the width of the funnel.
        /// </summary>
        public object Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the funnel.
        /// </summary>
        public object Height { get; set; }

        /// <summary>
        /// Gets or sets the minimum value of the funnel.
        /// </summary>
        public int Min { get; set; } = 0;

        /// <summary>
        /// Gets or sets the maximum value of the funnel.
        /// </summary>
        public int Max { get; set; } = 100;

        /// <summary>
        /// Gets or sets the minimum size of the funnel.
        /// </summary>
        public string MinSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of the funnel.
        /// </summary>
        public string MaxSize { get; set; }

        /// <summary>
        /// Gets or sets the sort type of the funnel.
        /// </summary>
        public SortType Sort { get; set; }

        /// <summary>
        /// Gets or sets the gap between each item in the funnel.
        /// </summary>
        public int Gap { get; set; } = 0;

        /// <summary>
        /// Gets or sets the label settings for the funnel.
        /// </summary>
        public Label Label { set; get; }

        /// <summary>
        /// Gets or sets the item style settings for the funnel.
        /// </summary>
        public ItemStyle ItemStyle { set; get; }

        /// <summary>
        /// Gets or sets the emphasis settings for the funnel.
        /// </summary>
        public Emphasis Emphasis { set; get; }
    }
}
