// -----------------------------------------------------------------------------
// FILE:	    DataZoom.cs
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

using System.Drawing;
using System.Text.Json.Serialization;

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents a data zoom component.
    /// </summary>
    public record DataZoom
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataZoom"/> class with the specified type.
        /// </summary>
        /// <param name="type">The type of the data zoom.</param>
        public DataZoom(string type)
        {
            Type = type;
        }

        /// <summary>
        /// Gets or sets the ID of the data zoom.
        /// </summary>
        public string Id { set; get; }

        /// <summary>
        /// Gets the type of the data zoom.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets or sets the index of the X axis.
        /// </summary>
        public object XAxisIndex { set; get; }

        // Rest of the properties...
    }
    public record DataZoomInside : DataZoom
    {
        public DataZoomInside() : base("inside") { }

        public bool? Disabled { set; get; }

        public object ZoomOnMouseWheel { set; get; }

        public object MoveOnMouseMove { set; get; }

        public object MoveOnMouseWheel { set; get; }

        public bool? PreventDefaultMouseMove { set; get; }
    }
    public record DataZoomSlider : DataZoom
    {
        public DataZoomSlider() : base("slider") { }

        [JsonConverter(typeof(ColorConverter))]
        public Color? BackgroundColor { get; }

        public DataBackground DataBackground { set; get; }

        [JsonConverter(typeof(ColorConverter))]
        public Color? FillerColor { set; get; }

        [JsonConverter(typeof(ColorConverter))]
        public Color? BorderColor { set; get; }

        public string HandleIcon { set; get; }

        public object HandleSize { set; get; }

        public HandleStyle HandleStyle { set; get; }

        public int? LabelPrecision { set; get; }

        public object LabelFormatter { set; get; }

        public bool? ShowDetail { set; get; }

        public string ShowDataShadow { set; get; }

        public TextStyle TextStyle { set; get; }

        public int? Zlevel { set; get; }

        public int? Z { set; get; }

        public object Left { set; get; }

        public object Top { set; get; }

        public object Right { set; get; }

        public object Bottom { set; get; }
    }
    public record DataBackground
    {
        public LineStyle LineStyle { set; get; }

        public AreaStyle AreaStyle { set; get; }
    }
}
