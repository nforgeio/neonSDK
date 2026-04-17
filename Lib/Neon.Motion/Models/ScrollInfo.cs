// -----------------------------------------------------------------------------
// FILE:        Models/ScrollInfo.cs
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

namespace Neon.Motion.Models
{
    /// <summary>
    /// Detailed scroll information passed to scroll callbacks.
    /// </summary>
    public class ScrollInfo
    {
        /// <summary>Horizontal axis scroll data.</summary>
        [JsonPropertyName("x")]
        public ScrollAxisInfo X { get; set; }

        /// <summary>Vertical axis scroll data.</summary>
        [JsonPropertyName("y")]
        public ScrollAxisInfo Y { get; set; }
    }

    /// <summary>
    /// Scroll data for a single axis.
    /// </summary>
    public class ScrollAxisInfo
    {
        /// <summary>Current scroll position in pixels.</summary>
        [JsonPropertyName("current")]
        public double Current { get; set; }

        /// <summary>Resolved scroll offset in pixels.</summary>
        [JsonPropertyName("offset")]
        public double Offset { get; set; }

        /// <summary>Scroll progress from 0 to 1.</summary>
        [JsonPropertyName("progress")]
        public double Progress { get; set; }

        /// <summary>Total scrollable length in pixels.</summary>
        [JsonPropertyName("scrollLength")]
        public double ScrollLength { get; set; }

        /// <summary>Current scroll velocity in pixels per second.</summary>
        [JsonPropertyName("velocity")]
        public double Velocity { get; set; }
    }
}
