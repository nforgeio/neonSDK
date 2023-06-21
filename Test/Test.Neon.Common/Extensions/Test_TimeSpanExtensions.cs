//-----------------------------------------------------------------------------
// FILE:        Test_TimeSpanExtensions.cs
// CONTRIBUTOR: Marcus Bowyer
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Neon.Common;
using Neon.Collections;
using Neon.Net;
using Neon.Retry;
using Neon.Xunit;

using Xunit;

namespace TestCommon.Extensions
{
    [Trait(TestTrait.Category, TestArea.NeonCommon)]
    public class Test_TimeSpanExtensions
    {
        [Fact]
        public void AdjustToFitDateRange()
        {
            // No adjustment required.

            var timespan = TimeSpan.FromDays(365);

            Assert.Equal(timespan, timespan.AdjustToFitDateRange(new DateTime(2023, 1, 23)));

            // Adjust for MAX date.

            Assert.Equal(TimeSpan.FromDays(1), timespan.AdjustToFitDateRange(DateTime.MaxValue - TimeSpan.FromDays(1)));

            // Adjust for MIN date.

            timespan = -timespan;

            Assert.Equal(-TimeSpan.FromDays(1), timespan.AdjustToFitDateRange(DateTime.MinValue + TimeSpan.FromDays(1)));
        }

        [Fact]
        public void RoundToSeconds()
        {
            Assert.Equal(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(1.5).RoundToSeconds());
            Assert.Equal(TimeSpan.Zero, TimeSpan.FromSeconds(0).RoundToSeconds());
            Assert.Equal(TimeSpan.FromSeconds(-2), TimeSpan.FromSeconds(-1.5).RoundToSeconds());
        }
    }
}
