// -----------------------------------------------------------------------------
// FILE:	    ISeries.cs
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

using System.Text.Json.Serialization;

namespace Neon.ECharts.Options.Series
{
    /// <summary>
    /// Represents a series in ECharts options.
    /// </summary>
    [JsonConverter(typeof(SeriesConverter))]
    public interface ISeries
    {
        /// <summary>
        /// Gets or sets the type of the series.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the series.
        /// </summary>
        public string Name { get; set; }
    }
}
