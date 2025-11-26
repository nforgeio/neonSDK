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

namespace Neon.ECharts.Options
{
    /// <summary>
    /// Represents the features available in the ECharts options.
    /// </summary>
    public record Feature
    {
        /// <summary>
        /// Gets or sets the SaveAsImage feature.
        /// </summary>
        public SaveAsImage SaveAsImage { set; get; }

        /// <summary>
        /// Gets or sets the Restore feature.
        /// </summary>
        public Restore Restore { set; get; }

        /// <summary>
        /// Gets or sets the DataZoom feature.
        /// </summary>
        public FeatureDataZoom DataZoom { set; get; }

        /// <summary>
        /// Gets or sets the DataView feature.
        /// </summary>
        public DataView DataView { set; get; }

        /// <summary>
        /// Gets or sets the MagicType feature.
        /// </summary>
        public MagicType MagicType { set; get; }

        /// <summary>
        /// Gets or sets the Brush feature.
        /// </summary>
        public FeatureBrush Brush { set; get; }
    }
}
