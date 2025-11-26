// -----------------------------------------------------------------------------
// FILE:	    LngLatBundle.cs
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
    /// <summary>
    /// A LngLatBounds object represents a geographical bounding box, defined by its southwest and northeast points in longitude and latitude.
    /// If no arguments are provided to the constructor, a null bounding box is created.
    /// Note that any Mapbox GL method that accepts a LngLatBounds object as an argument or option can also accept an Array of two LngLatLike constructs and will perform an implicit conversion.This flexible type is documented as LngLatBoundsLike.
    /// </summary>
    public class LngLatBounds
    {
        /// <summary>
        /// The southwest corner of the bounding box.
        /// </summary>
        [JsonPropertyName("sw")]
        public LngLat Southwest { get; set; }

        /// <summary>
        /// The northeast corner of the bounding box.
        /// </summary>
        [JsonPropertyName("ne")]
        public LngLat Northeast { get; set; }

        public LngLatBounds(LngLat southwest, LngLat northeast)
        {
            Southwest = southwest;
            Northeast = northeast;
        }
    }
}
