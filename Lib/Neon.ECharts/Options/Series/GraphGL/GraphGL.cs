// -----------------------------------------------------------------------------
// FILE:	    GraphGL.cs
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

using System.Collections.Generic;

using Neon.ECharts.Options.Series.Graph;

namespace Neon.ECharts.Options.Series.GraphGL
{
    /// <summary>
    /// Represents a graph GL series in ECharts GL, used for large-scale graph visualization.
    /// </summary>
    public record GraphGL : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "graphGL";

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphGL"/> class.
        /// </summary>
        public GraphGL() : base(_Type) { }

        /// <summary>
        /// Gets or sets the layout algorithm (e.g., 'forceAtlas2').
        /// </summary>
        public string Layout { get; set; }

        /// <summary>
        /// Gets or sets the force atlas2 layout configuration.
        /// </summary>
        public object ForceAtlas2 { get; set; }

        /// <summary>
        /// Gets or sets the symbol type.
        /// </summary>
        public object Symbol { get; set; }

        /// <summary>
        /// Gets or sets the symbol size.
        /// </summary>
        public object SymbolSize { get; set; }

        /// <summary>
        /// Gets or sets the item style.
        /// </summary>
        public ItemStyle ItemStyle { get; set; }

        /// <summary>
        /// Gets or sets the line style.
        /// </summary>
        public LineStyle LineStyle { get; set; }

        /// <summary>
        /// Gets or sets the links of the graph.
        /// </summary>
        public List<GraphLink> Links { get; set; }
    }
}
