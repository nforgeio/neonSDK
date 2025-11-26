// -----------------------------------------------------------------------------
// FILE:	    LabelLayout.cs
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

using Neon.ECharts.Options.Enum;

namespace Neon.ECharts.Options.Series
{
    public record LabelLayout
    {
        public string Text { get; set; }
        public Rect LabelRect { get; set; }
        public Align2 Align { get; set; }
        public VerticalAlign VerticalAlign { get; set; }
        public Rect Rect { get; set; }
        public object FontSize { get; set; }
        public bool? HideOverlap { get; set; }
        public MoveOverlap? MoveOverlap { get; set; }
    }
}
