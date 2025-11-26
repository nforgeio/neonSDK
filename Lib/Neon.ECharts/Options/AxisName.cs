// -----------------------------------------------------------------------------
// FILE:	    AxisName.cs
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

using Neon.ECharts.Options.Enum;

namespace Neon.ECharts.Options
{
    public class AxisName
    {
        /// <summary>
        /// Whether to display the indicator's name.
        /// </summary>
        public bool Show { set; get; }

        /// <summary>
        /// The text color.
        /// </summary>
        public Color? Color { set; get; }

        /// <summary>
        /// The font style.
        /// </summary>
        public FontStyle? FontStyle { set; get; }

        /// <summary>
        /// The font weight.
        /// </summary>
        public FontWeight? FontWeight { set; get; }

        /// <summary>
        /// Line height of the text fragment.
        /// </summary>
        public int LineHeight { set; get; }

        /// <summary>
        /// The background color of the text fragment.
        /// </summary>
        public Color? BackgroundColor { set; get; }

    }
}
