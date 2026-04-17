// -----------------------------------------------------------------------------
// FILE:        Models/ScrollOptions.cs
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
    /// Options for Motion's <c>scroll()</c> function.
    /// </summary>
    public class ScrollOptions
    {
        /// <summary>
        /// Scroll axis to track: <c>"x"</c> for horizontal or <c>"y"</c> for vertical.
        /// Default is <c>"y"</c>.
        /// </summary>
        [JsonPropertyName("axis")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Axis { get; set; }

        /// <summary>
        /// CSS selector for the target element whose position is tracked within the
        /// scrollable container. When <c>null</c>, the whole scrollable area is tracked.
        /// Resolved client-side; not serialized directly.
        /// </summary>
        [JsonIgnore]
        public string Target { get; set; }

        /// <summary>
        /// CSS selector for the scroll container element. When <c>null</c>, the window
        /// is used as the container.
        /// Resolved client-side; not serialized directly.
        /// </summary>
        [JsonIgnore]
        public string Container { get; set; }

        /// <summary>
        /// A two-element array defining the scroll intersection points as
        /// <c>["&lt;target-point&gt; &lt;container-point&gt;", ...]</c>.
        /// Accepts keywords (<c>"start"</c>, <c>"center"</c>, <c>"end"</c>), numbers (0–1),
        /// pixel strings (<c>"100px"</c>), or viewport-relative strings (<c>"50vh"</c>).
        /// Default is <c>["start start", "end end"]</c>.
        /// </summary>
        [JsonPropertyName("offset")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[] Offset { get; set; }

        internal string ToJson() =>
            JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented           = false,
            });
    }
}
