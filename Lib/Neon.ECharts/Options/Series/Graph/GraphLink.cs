// -----------------------------------------------------------------------------
// FILE:	    GraphLink.cs
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

namespace Neon.ECharts.Options.Series.Graph
{
    /// <summary>
    /// Represents a link in a graph.
    /// </summary>
    public record GraphLink
    {
        /// <summary>
        /// Gets or sets the source node index of the link.
        /// </summary>
        public int Source { get; set; }

        /// <summary>
        /// Gets or sets the target node index of the link.
        /// </summary>
        public int Target { get; set; }

        /// <summary>
        /// Gets or sets the value associated with the link.
        /// </summary>
        public string Value { get; set; }
    }
}
