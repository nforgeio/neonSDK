// -----------------------------------------------------------------------------
// FILE:	    Map.cs
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

namespace Neon.ECharts.Options.Series.Map
{
    /// <summary>
    /// Represents a map series in ECharts.
    /// </summary>
    public record Map : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "map";

        /// <summary>
        /// Initializes a new instance of the <see cref="Map"/> class.
        /// </summary>
        public Map() : base(_Type) { }

        /// <summary>
        /// Gets or sets the type of the map.
        /// </summary>
        public string MapType { set; get; }

        /// <summary>
        /// Gets or sets the label settings for the map series.
        /// </summary>
        public Label Label { set; get; }

        /// <summary>
        /// Gets or sets the name map for the map series.
        /// </summary>
        public object NameMap { set; get; }

        /// <summary>
        /// Gets or sets the name of the map.
        /// </summary>
        [JsonPropertyName("map")]
        public string MapName { get; set; }

        /// <summary>
        /// Gets or sets the roam settings for the map series.
        /// </summary>
        public object Roam { set; get; }

        /// <summary>
        /// Gets or sets the emphasis settings for the map series.
        /// </summary>
        public Emphasis Emphasis { set; get; }

        /// <summary>
        /// Gets or sets the selected mode for the map series.
        /// </summary>
        public object SelectedMode { set; get; }

        /// <summary>
        /// Gets or sets the zoom level for the map series.
        /// </summary>
        public double? Zoom { set; get; }

        /// <summary>
        /// Gets or sets the layout center for the map series.
        /// </summary>
        public object[] LayoutCenter { set; get; }

        /// <summary>
        /// Gets or sets the layout size for the map series.
        /// </summary>
        public object LayoutSize { set; get; }
    }
}
