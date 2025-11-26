// -----------------------------------------------------------------------------
// FILE:	    Tree.cs
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

namespace Neon.ECharts.Options.Series.Tree
{
    /// <summary>
    /// Represents a tree series in ECharts.
    /// </summary>
    public record Tree : SeriesBase, IPosition
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "tree";

        /// <summary>
        /// Initializes a new instance of the <see cref="Tree"/> class.
        /// </summary>
        public Tree() : base(_Type) { }

        /// <summary>
        /// Gets or sets the left position of the tree.
        /// </summary>
        public object Left { get; set; }

        /// <summary>
        /// Gets or sets the top position of the tree.
        /// </summary>
        public object Top { get; set; }

        /// <summary>
        /// Gets or sets the right position of the tree.
        /// </summary>
        public object Right { get; set; }

        /// <summary>
        /// Gets or sets the bottom position of the tree.
        /// </summary>
        public object Bottom { get; set; }

        /// <summary>
        /// Gets or sets the width of the tree.
        /// </summary>
        public object Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the tree.
        /// </summary>
        public object Height { get; set; }

        /// <summary>
        /// Gets or sets the symbol size of the tree.
        /// </summary>
        public object SymbolSize { get; set; }

        /// <summary>
        /// Gets or sets the label of the tree.
        /// </summary>
        public Label Label { get; set; }

        /// <summary>
        /// Gets or sets the leaves of the tree.
        /// </summary>
        public Leaves Leaves { get; set; }
    }
}
