// -----------------------------------------------------------------------------
// FILE:	    Parallel.cs
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

namespace Neon.ECharts.Options.Series.Parallel
{
    /// <summary>
    /// Represents a parallel coordinates series in ECharts.
    /// </summary>
    public record Parallel : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "parallel";

        /// <summary>
        /// Initializes a new instance of the <see cref="Parallel"/> class.
        /// </summary>
        public Parallel() : base(_Type) { }

        /// <summary>
        /// Gets or sets the coordinate system used.
        /// </summary>
        public string CoordinateSystem { get; set; }

        /// <summary>
        /// Gets or sets the index of the parallel coordinate system.
        /// </summary>
        public int? ParallelIndex { get; set; }

        /// <summary>
        /// Gets or sets the line style.
        /// </summary>
        public LineStyle LineStyle { get; set; }

        /// <summary>
        /// Gets or sets the emphasis style.
        /// </summary>
        public Emphasis Emphasis { get; set; }

        /// <summary>
        /// Gets or sets whether to enable smooth interpolation.
        /// </summary>
        public object Smooth { get; set; }

        /// <summary>
        /// Gets or sets whether to clip the content outside the coordinate area.
        /// </summary>
        public bool? Clip { get; set; }
    }
}
