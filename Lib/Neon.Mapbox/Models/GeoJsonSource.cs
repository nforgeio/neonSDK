// -----------------------------------------------------------------------------
// FILE:	    GeoJsonSource.cs
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
using System.ComponentModel;
using System.Text.Json.Serialization;

using NetTopologySuite.Features;

namespace Neon.Mapbox.Models
{
    /// <summary>
    /// Sources state which data the map should display. Specify the type of source with the "type" property, which must be one of vector, 
    /// raster, raster-dem, geojson, image, video. Adding a source isn't enough to make data appear on the map because sources don't contain 
    /// styling details like color or width. Layers refer to a source and give it a visual representation. This makes it possible to 
    /// style the same source in different ways, like differentiating between types of roads in a highways layer.
    /// </summary>
    public class GeoJsonSource : Source
    {
        public GeoJsonSource()
            : base()
        {
            Type = "geojson";
        }

        [JsonPropertyName("attribution")]
        public string Attribution { get; set; }

        [JsonPropertyName("buffer")]
        [DefaultValue(null)]
        public int? Buffer { get; set; }

        [JsonPropertyName("cluster")]
        [DefaultValue(null)]
        public bool? Cluster { get; set; }

        [JsonPropertyName("clusterMaxZoom")]
        [DefaultValue(null)]
        public int? ClusterMaxZoom { get; set; }

        [JsonPropertyName("clusterMinPoints")]
        [DefaultValue(null)]
        public int? ClusterMinPoints { get; set; }

        [JsonPropertyName("clusterProperties")]
        [DefaultValue(null)]
        public Dictionary<string, object> Attributes { get; set; }

        [JsonPropertyName("clusterRadius")]
        [DefaultValue(null)]
        public int? ClusterRadius { get; set; }

        [JsonPropertyName("data")]
        [DefaultValue(null)]
        public FeatureCollection Data { get; set; }

        [JsonPropertyName("dynamic")]
        [DefaultValue(null)]
        public bool? Dynamic { get; set; }

        [JsonPropertyName("generateId")]
        [DefaultValue(null)]
        public bool? GenerateId { get; set; }

        [JsonPropertyName("lineMetrics")]
        [DefaultValue(null)]
        public bool? LineMetrics { get; set; }

        [JsonPropertyName("maxzoom")]
        [DefaultValue(null)]
        public int? MaxZoom { get; set; }

        [JsonPropertyName("minzoom")]
        [DefaultValue(null)]
        public int? MinZoom { get; set; }

        [JsonPropertyName("tolerance")]
        [DefaultValue(null)]
        public double? Tolerance { get; set; }
    }
}
