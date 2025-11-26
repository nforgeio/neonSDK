// -----------------------------------------------------------------------------
// FILE:	    Toolbox.cs
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

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents the toolbox options for the ECharts library.
    /// </summary>
    public record Toolbox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the toolbox is shown.
        /// </summary>
        public bool? Show { set; get; }

        /// <summary>
        /// Gets or sets the features available in the toolbox.
        /// </summary>
        public Feature Feature { set; get; }

        /// <summary>
        /// Gets or sets the left position of the toolbox.
        /// </summary>
        public object Left { set; get; }

        /// <summary>
        /// Gets or sets the top position of the toolbox.
        /// </summary>
        public object Top { set; get; }

        /// <summary>
        /// Gets or sets the orientation of the toolbox.
        /// </summary>
        public Orient Orient { set; get; }
    }
}
