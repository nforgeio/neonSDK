// -----------------------------------------------------------------------------
// FILE:        Models/InViewOptions.cs
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

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Neon.Motion.Models
{
    /// <summary>
    /// Options for Motion's <c>inView()</c> function.
    /// </summary>
    public class InViewOptions
    {
        /// <summary>
        /// CSS selector of the element to use as the viewport root. When <c>null</c>
        /// the browser viewport is used.
        /// </summary>
        [JsonPropertyName("root")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Root { get; set; }

        /// <summary>
        /// Margin around the viewport used to expand or contract the detection area.
        /// Uses CSS-style shorthand, e.g. <c>"0px 100px 0px 0px"</c>. Default is <c>"0"</c>.
        /// </summary>
        [JsonPropertyName("margin")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Margin { get; set; }

        /// <summary>
        /// How much of the element must be visible to trigger the callback.
        /// Accepts <c>"some"</c>, <c>"all"</c>, or a number between 0 and 1.
        /// Default is <c>"some"</c>.
        /// </summary>
        [JsonPropertyName("amount")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Amount { get; set; }

        internal string ToJson() =>
            JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented           = false,
            });
    }
}
