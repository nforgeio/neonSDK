// -----------------------------------------------------------------------------
// FILE:	    PopupOptions.cs
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

using OneOf;

namespace Neon.Mapbox.Models
{
    public class PopupOptions
    {
        /// <summary>
        /// If true , a close button will appear in the top right corner of the popup.
        /// </summary>
        [JsonPropertyName("closeButton")] 
        public bool CloseButton { get; set; } = true;

        /// <summary>
        /// If true , the popup will closed when the map is clicked.
        /// </summary>
        [JsonPropertyName("closeOnClick")]
        public bool CloseOnClick { get; set; } = true;

        /// <summary>
        /// If true , the popup will closed when the map moves.
        /// </summary>
        [JsonPropertyName("closeOnMove")]
        public bool CloseOnMove { get; set; } = false;

        /// <summary>
        /// If true , the popup will try to focus the first focusable element inside the popup.
        /// </summary>
        [JsonPropertyName("focusAfterOpen")]
        public bool FocusAfterOpen { get; set; } = false;

        /// <summary>
        /// A string indicating the part of the Popup that should be positioned closest to the coordinate set via Popup#setLngLat. 
        /// Options are 'center' , 'top' , 'bottom' , 'left' , 'right' , 'top-left' , 'top-right' , 'bottom-left' , and 'bottom-right'. 
        /// If unset the anchor will be dynamically set to ensure the popup falls within the map container with a preference for 'bottom'.
        /// </summary>
        [JsonPropertyName("anchor")]
        public string Anchor { get; set; }

        /// <summary>
        /// A pixel offset applied to the popup's location specified as:
        /// • a single number specifying a distance from the popup's location
        /// • a PointLike specifying a constant offset
        /// • an object of Points specifing an offset for each anchor position Negative offsets indicate left and up.
        /// </summary>
        [JsonPropertyName("offset")]
        [JsonConverter(typeof(OneOfConverter))]
        public OneOf<int, Point, Point[]> Offset { get; set; }

        /// <summary>
        /// Space-separated CSS class names to add to popup container.
        /// </summary>
        [JsonPropertyName("className")] 
        public string ClassName { get; set; }

        /// <summary>
        /// A string that sets the CSS property of the popup's maximum width, eg '300px'. 
        /// To ensure the popup resizes to fit its content, set this property to 'none'. 
        /// Available values can be found here: https://developer.mozilla.org/en-US/docs/Web/CSS/max-width
        /// </summary>
        [JsonPropertyName("maxWidth")]
        public string MaxWidth { get; set; } = "240px";
    }
}
