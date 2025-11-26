// -----------------------------------------------------------------------------
// FILE:	    Pie.cs
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

namespace Neon.ECharts.Options.Series.Pie
{
    /// <summary>
    /// Represents a pie series in ECharts.
    /// </summary>
    public record Pie : SeriesBase
    {
        /// <summary>
        /// The type of the series.
        /// </summary>
        public const string _Type = "pie";

        /// <summary>
        /// Initializes a new instance of the <see cref="Pie"/> class.
        /// </summary>
        public Pie() : base(_Type) { }

        /// <summary>
        /// Gets or sets the radius of the pie.
        /// </summary>
        public object Radius { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether to avoid label overlap.
        /// </summary>
        public bool? AvoidLabelOverlap { set; get; }

        /// <summary>
        /// Gets or sets the label settings for the pie.
        /// </summary>
        public Label Label { set; get; }

        /// <summary>
        /// Gets or sets the label line settings for the pie.
        /// </summary>
        public LabelLine LabelLine { set; get; }

        /// <summary>
        /// Gets or sets the center of the pie.
        /// </summary>
        public object Center { set; get; }

        /// <summary>
        /// Gets or sets the emphasis settings for the pie.
        /// </summary>
        public Emphasis Emphasis { set; get; }

        /// <summary>
        /// Gets or sets the selected mode of the pie.
        /// </summary>
        public object SelectedMode { set; get; }
    }
}
