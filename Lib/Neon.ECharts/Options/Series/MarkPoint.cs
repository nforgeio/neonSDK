// -----------------------------------------------------------------------------
// FILE:	    MarkPoint.cs
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
    public class MarkPoint
    {

        public List<MarkPointData> Data { set; get; }

        public Label Label { set; get; }

        public Tooltip Tooltip { set; get; }
    }
    public class MarkPointData
    {
        public string Name { set; get; }

        public MarkPointDataType? Type { set; get; }

        public int? Value { set; get; }

        public int? XAxis { set; get; }

        public int? YAxis { set; get; }

        public string ValueDim { set; get; }
    }
}
