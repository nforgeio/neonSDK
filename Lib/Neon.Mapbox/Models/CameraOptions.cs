// -----------------------------------------------------------------------------
// FILE:	    CameraOptions.cs
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

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Neon.Mapbox.Models
{
    public class CameraOptions
    {
        [DefaultValue(null)]
        [JsonPropertyName("around")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public LngLat Around { get; set; }

        [DefaultValue(null)]
        [JsonPropertyName("bearing")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double? Bearing { get; set; }

        [DefaultValue(null)]
        [JsonPropertyName("center")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public LngLat Center { get; set; }

        [DefaultValue(null)]
        [JsonPropertyName("padding")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public PaddingOptions Padding { get; set; }

        [DefaultValue(null)]
        [JsonPropertyName("pitch")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double? Pitch { get; set; }

        [DefaultValue(null)]
        [JsonPropertyName("zoom")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public double? Zoom { get; set; }
    }
}
