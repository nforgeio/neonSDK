// -----------------------------------------------------------------------------
// FILE:	    PageIcons.cs
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

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents the page icons for a chart.
    /// </summary>
    public class PageIcons
    {
        /// <summary>
        /// Gets or sets the list of horizontal page icons.
        /// </summary>
        public List<string> Horizontal { set; get; }

        /// <summary>
        /// Gets or sets the list of vertical page icons.
        /// </summary>
        public List<string> Vertical { set; get; }
    }
}
