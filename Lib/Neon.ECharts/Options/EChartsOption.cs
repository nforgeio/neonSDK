// -----------------------------------------------------------------------------
// FILE:	    EChartsOption.cs
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
using System.Drawing;
using System.Text.Json.Serialization;

using Neon.ECharts.Options.Series;

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents the configuration options for an ECharts chart.
    /// </summary>
    /// <typeparam name="T">The type of the data used in the chart.</typeparam>
    public record EChartsOption<T>
    {
        /// <summary>
        /// Gets or sets the base options for the chart.
        /// </summary>
        public EChartsBaseOption<T> BaseOption { get; set; }

        /// <summary>
        /// Gets or sets the list of additional options for the chart.
        /// </summary>
        public List<EChartsOption<T>> Options { get; set; }

        /// <summary>
        /// Gets or sets the title of the chart.
        /// </summary>
        public Title Title { set; get; }

        /// <summary>
        /// Gets or sets the label settings for the chart.
        /// </summary>
        public Label Label { get; set; }

        /// <summary>
        /// Gets or sets the layout of the labels in the chart.
        /// </summary>
        public LabelLayout LabelLayout { set; get; }

        /// <summary>
        /// Gets or sets the scale limit settings for the chart.
        /// </summary>
        public ScaleLimit ScaleLimit { get; set; }

        /// <summary>
        /// Gets or sets the tooltip settings for the chart.
        /// </summary>
        public Tooltip Tooltip { set; get; }

        /// <summary>
        /// Gets or sets the X-axis settings for the chart.
        /// </summary>
        public List<XAxis> XAxis { set; get; }

        /// <summary>
        /// Gets or sets the Y-axis settings for the chart.
        /// </summary>
        public List<YAxis> YAxis { set; get; }

        /// <summary>
        /// Gets or sets the 3D X-axis settings for the chart.
        /// </summary>
        public List<XAxis> XAxis3D { set; get; }

        /// <summary>
        /// Gets or sets the 3D Y-axis settings for the chart.
        /// </summary>
        public List<YAxis> YAxis3D { set; get; }

        /// <summary>
        /// Gets or sets the 3D Z-axis settings for the chart.
        /// </summary>
        public List<ZAxis> ZAxis3D { set; get; }

        /// <summary>
        /// Gets or sets the series data for the chart.
        /// </summary>
        public List<ISeries> Series { set; get; }

        /// <summary>
        /// Gets or sets the legend settings for the chart.
        /// </summary>
        public Legend Legend { set; get; }

        /// <summary>
        /// Gets or sets the toolbox settings for the chart.
        /// </summary>
        public Toolbox Toolbox { set; get; }

        /// <summary>
        /// Gets or sets the dataset for the chart.
        /// </summary>
        public DataSet DataSet { get; set; }

        /// <summary>
        /// Gets or sets the grid settings for the chart.
        /// </summary>
        public List<Grid> Grid { set; get; }

        /// <summary>
        /// Gets or sets the 3D grid settings for the chart.
        /// </summary>
        public List<Grid3D> Grid3D { set; get; }

        /// <summary>
        /// Gets or sets the data zoom settings for the chart.
        /// </summary>
        public List<object> DataZoom { set; get; }

        /// <summary>
        /// Gets or sets the color settings for the chart.
        /// </summary>
        public IEnumerable<Color> Color { set; get; }

        /// <summary>
        /// Gets or sets the background color of the chart.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? BackgroundColor { set; get; }

        /// <summary>
        /// Gets or sets the text style settings for the chart.
        /// </summary>
        public TextStyle TextStyle { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether animation is enabled for the chart.
        /// </summary>
        public bool? Animation { set; get; }

        /// <summary>
        /// Gets or sets the animation threshold for the chart.
        /// </summary>
        public int? AnimationThreshold { set; get; }

        /// <summary>
        /// Gets or sets the duration of the animation for the chart.
        /// </summary>
        public object AnimationDuration { set; get; }

        /// <summary>
        /// Gets or sets the easing function for the animation of the chart.
        /// </summary>
        public string AnimationEasing { set; get; }

        /// <summary>
        /// Gets or sets the delay before the animation starts for the chart.
        /// </summary>
        public object AnimationDelay { set; get; }

        /// <summary>
        /// Gets or sets the duration of the update animation for the chart.
        /// </summary>
        public object AnimationDurationUpdate { set; get; }

        /// <summary>
        /// Gets or sets the easing function for the update animation of the chart.
        /// </summary>
        public string AnimationEasingUpdate { set; get; }

        /// <summary>
        /// Gets or sets the delay before the update animation starts for the chart.
        /// </summary>
        public object AnimationDelayUpdate { set; get; }

        /// <summary>
        /// Gets or sets the blend mode for the chart.
        /// </summary>
        public string BlendMode { set; get; }

        /// <summary>
        /// Gets or sets the hover layer threshold for the chart.
        /// </summary>
        public int? HoverLayerThreshold { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to use UTC time for the chart.
        /// </summary>
        public bool? UseUTC { set; get; }

        /// <summary>
        /// Gets or sets the axis pointer settings for the chart.
        /// </summary>
        public AxisPointer AxisPointer { set; get; }

        /// <summary>
        /// Gets or sets the visual map settings for the chart.
        /// </summary>
        public object VisualMap { set; get; }

        /// <summary>
        /// Gets or sets the graphic settings for the chart.
        /// </summary>
        public List<object> Graphic { set; get; }

        /// <summary>
        /// Gets or sets the polar settings for the chart.
        /// </summary>
        public Polar Polar { set; get; }

        /// <summary>
        /// Gets or sets the radius axis settings for the chart.
        /// </summary>
        public RadiusAxis RadiusAxis { set; get; }

        /// <summary>
        /// Gets or sets the angle axis settings for the chart.
        /// </summary>
        public AngleAxis AngleAxis { set; get; }

        /// <summary>
        /// Gets or sets the radar settings for the chart.
        /// </summary>
        public Radar Radar { set; get; }

        /// <summary>
        /// Gets or sets the brush settings for the chart.
        /// </summary>
        public Brush Brush { set; get; }

        /// <summary>
        /// Gets or sets the BMap settings for the chart.
        /// </summary>
        public BMap Bmap { get; set; }

        /// <summary>
        /// Returns a string representation of the EChartsOption.
        /// </summary>
        /// <returns>A string representation of the EChartsOption.</returns>
        public override string ToString()
        {
            return EChartsOptionSerializer.Default.Serialize(this);
        }
    }
}
