// -----------------------------------------------------------------------------
// FILE:	    MarkLine.cs
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

using Neon.ECharts.Options.Enum;

namespace Neon.ECharts.Options.Series
{
    public record MarkLine
    {
        public bool Animation { set; get; }
        
        public bool? Silent { set; get; }

        public object Symbol { set; get; }

        public List<object> Data { set; get; }
    }
    public record MarkLineData
    {
        public string Name { set; get; }

        public object XAxis { set; get; }

        public object YAxis { set; get; }

        public object X { set; get; }

        public object Y { set; get; }

        public string Symbol { set; get; }

        public Sampling? Type { set; get; }

        public MarkLineDataLabel Label { set; get; }

        public string ValueDim { set; get; }

        public object SymbolSize { set; get; }

        public Emphasis Emphasis { set; get; }
    }

    public record MarkLineDataLabel
    {
        public bool? Show { set; get; }

        public Location? Position { set; get; }

        public object Formatter { set; get; }
    }
}
