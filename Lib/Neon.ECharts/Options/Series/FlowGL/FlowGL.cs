// -----------------------------------------------------------------------------
// FILE:	    FlowGL.cs
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

namespace Neon.ECharts.Options.Series.FlowGL
{
    /// <summary>
    /// Represents a flow GL series in ECharts GL, used for vector field visualization.
    /// </summary>
    public record FlowGL : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "flowGL";

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowGL"/> class.
        /// </summary>
        public FlowGL() : base(_Type) { }

        /// <summary>
        /// Gets or sets the coordinate system used.
        /// </summary>
        public string CoordinateSystem { get; set; }

        /// <summary>
        /// Gets or sets the number of particles for the flow visualization.
        /// </summary>
        public object ParticleDensity { get; set; }

        /// <summary>
        /// Gets or sets the particle size.
        /// </summary>
        public double? ParticleSize { get; set; }

        /// <summary>
        /// Gets or sets the particle speed.
        /// </summary>
        public double? ParticleSpeed { get; set; }

        /// <summary>
        /// Gets or sets the particle trail length.
        /// </summary>
        public double? ParticleTrailLength { get; set; }

        /// <summary>
        /// Gets or sets the super sampling factor.
        /// </summary>
        public int? SuperSampling { get; set; }

        /// <summary>
        /// Gets or sets the item style.
        /// </summary>
        public ItemStyle ItemStyle { get; set; }
    }
}
