// -----------------------------------------------------------------------------
// FILE:	    ChannelRange.cs
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
using System.Drawing;

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents a channel range in ECharts options.
    /// </summary>
    public record ChannelRange
    {
        /// <summary>
        /// Gets or sets the symbol size for the channel range.
        /// </summary>
        public int[] SymbolSize { set; get; }

        /// <summary>
        /// Gets or sets the symbol for the channel range.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or sets the colors for the channel range.
        /// </summary>
        public List<Color> Color { set; get; }
    }
}
