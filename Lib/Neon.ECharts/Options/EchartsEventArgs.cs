// -----------------------------------------------------------------------------
// FILE:	    EChartsEventArgs.cs
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
using System.Text;
using System.Text.Json.Serialization;

using Neon.ECharts.Options.Enum;

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents the event arguments for ECharts events.
    /// </summary>
    public record EchartsEventArgs
    {
        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        public EventType? EventType { get; set; }

        /// <summary>
        /// Gets or sets the component type.
        /// </summary>
        public string ComponentType { get; set; }

        /// <summary>
        /// Gets or sets the series type.
        /// </summary>
        public string SeriesType { get; set; }

        /// <summary>
        /// Gets or sets the series index.
        /// </summary>
        public int SeriesIndex { get; set; }

        /// <summary>
        /// Gets or sets the series name.
        /// </summary>
        public string SeriesName { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the data index.
        /// </summary>
        public int DataIndex { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets the data type.
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        [JsonConverter(typeof(ColorConverter))]
        public Color? Color { get; set; }

        /// <summary>
        /// Gets or sets the additional information.
        /// </summary>
        public object Info { get; set; }

        /// <summary>
        /// Gets or sets the data URL.
        /// </summary>
        public string DataUrl { get; set; }

        /// <summary>
        /// Gets or sets the event batch.
        /// </summary>
        public IEnumerable<EchartsEventBatch> Batch { get; set; }

        /// <summary>
        /// Gets or sets the areas.
        /// </summary>
        public Area[] Areas { get; set; }

        /// <summary>
        /// Returns a string representation of the <see cref="EchartsEventArgs"/> object.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString()
        {
            StringBuilder sb = new();
            foreach (var property in GetType().GetProperties())
            {
                sb.Append($"{property.Name}:{property.GetValue(this)}; ");
            }
            return sb.ToString();
        }
    }
}
