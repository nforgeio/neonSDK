// -----------------------------------------------------------------------------
// FILE:	    Options.cs
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

using Neon.Mapbox.Models;

namespace Neon.Mapbox
{
    public class Options
    {
        public class DefaultStyles
        {
            public const string Streets                 = "mapbox://styles/mapbox/streets-v11";
            public const string Outdoors                = "mapbox://styles/mapbox/outdoors-v11";
            public const string Light                   = "mapbox://styles/mapbox/light-v10";
            public const string Dark                    = "mapbox://styles/mapbox/dark-v10";
            public const string Satellite               = "mapbox://styles/mapbox/satellite-v9";
            public const string SatelliteStreets        = "mapbox://styles/mapbox/satellite-streets-v11";
            public const string NavigationPreviewDay    = "mapbox://styles/mapbox/navigation-preview-day-v4";
            public const string NavigationPreviewNight  = "mapbox://styles/mapbox/navigation-preview-night-v4";
            public const string NavigationGuidanceDay   = "mapbox://styles/mapbox/navigation-guidance-day-v4";
            public const string NavigationGuidanceNight = "mapbox://styles/mapbox/navigation-guidance-night-v4";
        }

        /// <summary>
        /// If specified, map will use this token instead of the one defined in mapboxgl.accessToken.
        /// </summary>
        /// <remarks>
        /// default: null
        /// </remarks>
        [JsonPropertyName("accessToken")]
        [DefaultValue(null)]
        public string AccessToken { get; set; } = null;

        /// <summary>
        /// If true , the gl context will be created with MSAA antialiasing , which can be useful for antialiasing custom layers. This is false by default as
        /// a performance optimization.
        /// </summary>
        [DefaultValue(false)]
        [JsonPropertyName("antialias")]
        public bool AntiAlias { get; set; } = false;

        /// <summary>
        /// If true, an AttributionControl will be added to the map.
        /// </summary>
        [DefaultValue(true)]
        [JsonPropertyName("attributionControl")]
        public bool AttributionControl { get; set; } = true;

        /// <summary>
        /// The initial bearing (rotation) of the map, measured in degrees counter-clockwise from north. If bearing is not specified in the constructor options,
        /// Mapbox GL JS will look for it in the map's style object. If it is not specified in the style, either, it will default to 0 .
        /// </summary>
        [DefaultValue(0)]
        [JsonPropertyName("bearing")]
        public decimal Bearing { get; set; } = 0;

        /// <summary>
        /// The threshold, measured in degrees, that determines when the map's bearing will snap to north. For example, with a bearingSnap of 7, if the user
        /// rotates the map within 7 degrees of north, the map will automatically snap to exact north.
        /// </summary>
        [DefaultValue(7)]
        [JsonPropertyName("bearingSnap")]
        public decimal BearingSnap { get; set; } = 7;

        /// <summary>
        /// The initial bounds of the map. If bounds is specified, it overrides center and zoom constructor options.
        /// </summary>
        [DefaultValue(null)]
        [JsonPropertyName("bounds")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<LngLat> Bounds { get; set; } = null;


        /// <summary>
        /// If true , the "box zoom" interaction is enabled (see BoxZoomHandler ).
        /// </summary>
        [DefaultValue(true)]
        [JsonPropertyName("true")]
        public bool BoxZoom { get; set; } = true;

        /// <summary>
        /// The initial geographical centerpoint of the map. If center is not specified in the constructor options, Mapbox GL JS will look for it
        /// in the map's style object. If it is not specified in the style, either, it will default to [0, 0] Note: Mapbox GL uses longitude,
        /// latitude coordinate order (as opposed to latitude, longitude) to match GeoJSON.
        /// </summary>
        [DefaultValue(null)]
        [JsonPropertyName("center")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public LngLat Center { get; set; } = null;

        /// <summary>
        /// The max number of pixels a user can shift the mouse pointer during a click for it to be considered a valid click (as opposed to a mouse drag).
        /// </summary>
        [DefaultValue(3)]
        [JsonPropertyName("clickTolerance")]
        public int ClickTolerance { get; set; } = 3;

        /// <summary>
        /// If true , Resource Timing API information will be collected for requests made by GeoJSON and Vector Tile web workers (this information is
        /// normally inaccessible from the main Javascript thread). Information will be returned in a resourceTiming property of relevant data events.
        /// </summary>
        [DefaultValue(false)]
        [JsonPropertyName("collectResourceTiming")]
        public bool CollectResourceTiming { get; set; } = false;

        /// <summary>
        /// The HTML element in which Mapbox GL JS will render the map, or the element's string id . The specified element must have no children.
        /// </summary>
        [JsonPropertyName("container")]
        [DefaultValue(null)]
        public string Container { get; set; }

        /// <summary>
        /// If true , scroll zoom will require pressing the ctrl or ⌘ key while scrolling to zoom map, and touch pan will require using two
        /// fingers while panning to move the map. Touch pitch will require three fingers to activate if enabled.
        /// </summary>
        [DefaultValue(null)]
        [JsonPropertyName("cooperativeGestures")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool? CooperativeGestures { get; set; }

        /// <summary>
        /// If true , symbols from multiple sources can collide with each other during collision detection. If false , collision detection is
        /// run separately for the symbols in each source.
        /// </summary>
        [DefaultValue(true)]
        [JsonPropertyName("crossSourceCollisions")]
        public bool CrossSourceCollisions { get; set; } = true;

        /// <summary>
        /// Strings to show in an AttributionControl. Only applicable if <see cref="AttributionControl"/> is true.
        /// </summary>
        [DefaultValue(true)]
        [JsonPropertyName("customAttribution")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<string> CustomAttribution { get; set; } = null;

        /// <summary>
        /// If true , the "double click to zoom" interaction is enabled (see DoubleClickZoomHandler ).
        /// </summary>
        [DefaultValue(true)]
        [JsonPropertyName("doubleClickZoom")]
        public bool DoubleClickZoom { get; set; } = true;

        /// <summary>
        /// If true , the "drag to rotate" interaction is enabled (see DragRotateHandler ).
        /// </summary>
        [DefaultValue(true)]
        [JsonPropertyName("dragRotate")]
        public bool DragRotate { get; set; } = true;

        /// <summary>
        /// Controls the duration of the fade-in/fade-out animation for label collisions, in milliseconds. This setting affects all symbol layers.
        /// This setting does not affect the duration of runtime styling transitions or raster tile cross-fading.
        /// </summary>
        [DefaultValue(300)]
        [JsonPropertyName("fadeDuration")]
        public int FadeDuration { get; set; } = 300;

        /// <summary>
        /// If true , map creation will fail if the performance of Mapbox GL JS would be dramatically worse than expected
        /// (a software renderer would be used).
        /// </summary>
        [DefaultValue(false)]
        [JsonPropertyName("failIfMajorPerformanceCaveat")]
        public bool FailIfMajorPerformanceCaveat { get; set; } = false;

        /// <summary>
        /// If false , no mouse, touch, or keyboard listeners will be attached to the map, so
        /// it will not respond to interaction.
        /// </summary>
        [DefaultValue(false)]
        [JsonPropertyName("interactive")]
        public bool Interactive { get; set; } = true;

        /// <summary>
        /// The initial zoom level of the map. If zoom is not specified in the constructor options, Mapbox GL JS will look for it in the map's style object. 
        /// If it is not specified in the style, either, it will default to 0.
        /// </summary>
        [JsonPropertyName("zoom")]
        [DefaultValue(null)]
        public int? Zoom { get; set; }

        /// <summary>
        /// The map's Mapbox style. This must be an a JSON object conforming to the schema described in the Mapbox Style Specification , or a URL to such JSON.
        /// To load a style from the Mapbox API, you can use a URL of the form mapbox://styles/:owner/:style, 
        /// where :owner is your Mapbox account name and :style is the style ID. Or you can use one of the following the predefined Mapbox styles:
        /// mapbox://styles/mapbox/streets-v11
        /// mapbox://styles/mapbox/outdoors-v11
        /// mapbox://styles/mapbox/light-v10
        /// mapbox://styles/mapbox/dark-v10
        /// mapbox://styles/mapbox/satellite-v9
        /// mapbox://styles/mapbox/satellite-streets-v11
        /// mapbox://styles/mapbox/navigation-preview-day-v4
        /// mapbox://styles/mapbox/navigation-preview-night-v4
        /// mapbox://styles/mapbox/navigation-guidance-day-v4
        /// mapbox://styles/mapbox/navigation-guidance-night-v4
        /// Tilesets hosted with Mapbox can be style-optimized if you append ?optimize=true to the end of your style URL, 
        /// like mapbox://styles/mapbox/streets-v11?optimize=true. Learn more about style-optimized vector tiles in our API documentation.
        /// </summary>
        [JsonPropertyName("style")]
        [DefaultValue(null)]
        public string Style { get; set; } = DefaultStyles.Streets;

        /// <summary>
        /// The minimum zoom level of the map (0-24).
        /// </summary>
        /// <remarks>
        /// default: 0
        /// </remarks>
        [JsonPropertyName("minZoom")]
        [DefaultValue(null)]
        public int MinZoom { get; set; }

        /// <summary>
        /// The maximum zoom level of the map (0-24).
        /// </summary>
        /// <remarks>
        /// default: 22
        /// </remarks>
        [JsonPropertyName("maxZoom")]
        [DefaultValue(null)]
        public int MaxZoom { get; set; }
    }
}
