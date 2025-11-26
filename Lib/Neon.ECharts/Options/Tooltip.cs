// -----------------------------------------------------------------------------
// FILE:	    Tooltip.cs
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

using Neon.ECharts.Options.Enum;

namespace Neon.ECharts.Options
{
    public class Tooltip
    {
        public static Tooltip Default = new Tooltip();

        /// <summary>
        /// Whether to show the tooltip component.
        /// Including tooltip floating layer and <see cref="AxisPointer"/>.
        /// </summary>
        public bool? Show { set; get; }

        /// <summary>
        /// Whether to show the tooltip floating layer, whose default value is true.
        /// It should be configurated to be false, if you only need tooltip to trigger
        /// the event or show the axisPointer without content.
        /// </summary>
        public bool? ShowContent { set; get; }

        /// <summary>
        /// Gets or sets the trigger type for displaying the tooltip.
        /// </summary>
        public TooltipTrigger? Trigger { set; get; }

        /// <summary>
        /// Gets or sets the trigger condition for displaying the tooltip.
        /// </summary>
        public string TriggerOn { get; set; }

        /// <summary>
        /// Gets or sets the configuration for the axis pointer of the tooltip.
        /// </summary>
        public TooltipAxisPointer AxisPointer { set; get; }

        /// <summary>
        /// Gets or sets the position of the tooltip.
        /// </summary>
        public object Position { set; get; }

        /// <summary>
        /// Gets or sets the formatter function for the tooltip.
        /// </summary>
        public Formatter Formatter { set; get; }

        /// <summary>
        /// Gets or sets the background color of the tooltip.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? BackgroundColor { set; get; }

        /// <summary>
        /// Gets or sets the border width of the tooltip.
        /// </summary>
        public int? BorderWidth { set; get; }

        /// <summary>
        /// Gets or sets the border color of the tooltip.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? BorderColor { set; get; }

        /// <summary>
        /// Gets or sets the padding of the tooltip.
        /// </summary>
        public object Padding { set; get; }

        /// <summary>
        /// Gets or sets the text style of the tooltip.
        /// </summary>
        public TextStyle TextStyle { set; get; }

        /// <summary>
        /// Gets or sets the value formatter function for the tooltip.
        /// </summary>
        public JFunc ValueFormatter { get; set; }

        /// <summary>
        /// Whether to confine tooltip content in the view rect of chart instance.
        /// </summary>
        public bool Confine { get; set; }
    }
}
