// -----------------------------------------------------------------------------
// FILE:	    AxisPointerLink.cs
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
    /// Represents a link between axis pointers in ECharts options.
    /// </summary>
    public class AxisPointerLink
    {
        /// <summary>
        /// Gets or sets the X-axis pointer.
        /// </summary>
        public object XAxis { set; get; }

        /// <summary>
        /// Gets or sets the ID of the X-axis.
        /// </summary>
        public object XAxisId { set; get; }

        /// <summary>
        /// Gets or sets the name of the X-axis.
        /// </summary>
        public object XAxisName { set; get; }

        /// <summary>
        /// Gets or sets the index of the X-axis.
        /// </summary>
        public object XAxisIndex { set; get; }

        /// <summary>
        /// Gets or sets the Y-axis pointer.
        /// </summary>
        public object YAxis { set; get; }

        /// <summary>
        /// Gets or sets the ID of the Y-axis.
        /// </summary>
        public object YAxisId { set; get; }

        /// <summary>
        /// Gets or sets the name of the Y-axis.
        /// </summary>
        public object YAxisName { set; get; }

        /// <summary>
        /// Gets or sets the index of the Y-axis.
        /// </summary>
        public object YAxisIndex { set; get; }

        /// <summary>
        /// Gets or sets the radius axis pointer.
        /// </summary>
        public object RadiusAxis { set; get; }

        /// <summary>
        /// Gets or sets the ID of the radius axis.
        /// </summary>
        public object RadiusAxisId { set; get; }

        /// <summary>
        /// Gets or sets the name of the radius axis.
        /// </summary>
        public object RadiusAxisName { set; get; }

        /// <summary>
        /// Gets or sets the index of the radius axis.
        /// </summary>
        public object RadiusAxisIndex { set; get; }

        /// <summary>
        /// Gets or sets the angle axis pointer.
        /// </summary>
        public object AngleAxis { set; get; }

        /// <summary>
        /// Gets or sets the ID of the angle axis.
        /// </summary>
        public object AngleAxisId { set; get; }

        /// <summary>
        /// Gets or sets the name of the angle axis.
        /// </summary>
        public object AngleAxisName { set; get; }

        /// <summary>
        /// Gets or sets the index of the angle axis.
        /// </summary>
        public object AngleAxisIndex { set; get; }

        /// <summary>
        /// Gets or sets the single axis pointer.
        /// </summary>
        public object SingleAxis { set; get; }

        /// <summary>
        /// Gets or sets the ID of the single axis.
        /// </summary>
        public object SingleAxisId { set; get; }

        /// <summary>
        /// Gets or sets the name of the single axis.
        /// </summary>
        public object SingleAxisName { set; get; }

        /// <summary>
        /// Gets or sets the index of the single axis.
        /// </summary>
        public object SingleAxisIndex { set; get; }
    }
}
