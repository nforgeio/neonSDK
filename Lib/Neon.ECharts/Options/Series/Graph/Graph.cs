// -----------------------------------------------------------------------------
// FILE:	    Graph.cs
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

using Neon.ECharts.Options.Enum;

namespace Neon.ECharts.Options.Series.Graph
{
    /// <summary>
    /// Represents a graph series in ECharts.
    /// </summary>
    public record Graph : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "graph";

        /// <summary>
        /// Initializes a new instance of the <see cref="Graph"/> class.
        /// </summary>
        public Graph() : base(_Type) { }

        /// <summary>
        /// Gets or sets the layout of the graph.
        /// </summary>
        public Layout Layout { get; set; }

        /// <summary>
        /// Gets or sets the symbol size of the graph.
        /// </summary>
        public object SymbolSize { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to focus on node adjacency.
        /// </summary>
        public bool FocusNodeAdjacency { get; set; }

        /// <summary>
        /// Gets or sets the categories of the graph.
        /// </summary>
        public List<GraphCategory> Categories { get; set; }

        /// <summary>
        /// Gets or sets the label of the graph.
        /// </summary>
        public Label Label { get; set; }

        /// <summary>
        /// Gets or sets the force of the graph.
        /// </summary>
        public GraphForce Force { get; set; }

        /// <summary>
        /// Gets or sets the edge label of the graph.
        /// </summary>
        public Label EdgeLabel { get; set; }

        /// <summary>
        /// Gets or sets the links of the graph.
        /// </summary>
        public List<GraphLink> Links { get; set; }

        /// <summary>
        /// Gets or sets the line style of the graph.
        /// </summary>
        public LineStyle LineStyle { get; set; }
    }
}
