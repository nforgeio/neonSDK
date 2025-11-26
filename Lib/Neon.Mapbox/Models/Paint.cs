// -----------------------------------------------------------------------------
// FILE:	    Paint.cs
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

namespace Neon.Mapbox.Models
{
    [JsonDerivedType(typeof(PaintLine))]
    [JsonDerivedType(typeof(PaintFill))]
    public class Paint
    {
       
    }

    public class PaintFill : Paint
    {
        /// <summary>
        /// Optional color. Defaults to "#000000".
        /// </summary>
        [JsonPropertyName("fill-color")]
        public string FillColor { get; set; } = default!;

        /// <summary>
        /// Optional number between 0 and 1 inclusive. Defaults to 1.
        /// </summary>
        [JsonPropertyName("fill-opacity")]
        public decimal? FillOpacity { get; set; } = default!;
    }

    public class PaintLine : Paint
    {
        /// <summary>
        /// Optional color. Defaults to "#000000". Disabled by line-pattern.
        /// </summary>
        [JsonPropertyName("line-color")]
        public string LineColor { get; set; } = default!;

        /// <summary>
        /// Optional number greater than or equal to 0. Units in pixels. Defaults to 1.
        /// </summary>
        [JsonPropertyName("line-width")]
        public int? LineWidth { get; set; } = default!;

    }
}
