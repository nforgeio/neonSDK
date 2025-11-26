// -----------------------------------------------------------------------------
// FILE:	    Feature.cs
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
    /// Feature identifier. Feature objects returned from Map#queryRenderedFeatures or event handlers can be used as feature identifiers.
    /// </summary>
    public class Feature
    {
        /// <summary>
        /// Unique id of the feature.
        /// </summary>
        [JsonPropertyName("id")]
        public object Id { get; set; }

        /// <summary>
        /// string	The id of the vector or GeoJSON source for the feature.
        /// </summary>
        [JsonPropertyName("source")] 
        public string Source { get; set; }

        /// <summary>
        /// (optional) For vector tile sources, sourceLayer is required.
        /// </summary>
        [JsonPropertyName("sourceLayer")] 
        public string SourceLayer { get; set; }
    }
}
