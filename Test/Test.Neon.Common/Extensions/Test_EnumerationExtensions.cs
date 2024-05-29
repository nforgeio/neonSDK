//-----------------------------------------------------------------------------
// FILE:        Test_EnumerationExtensions.cs
// CONTRIBUTOR: Marcus Bowyer
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

namespace TestCommon
{
    [Trait(TestTrait.Category, TestArea.NeonCommon)]
    public class Test_EnumerationExtensions
    {
        [Fact]
        public void TakeUpTo()
        {
            var items0 = new List<int>();
            var items5 = new List<int>() { 0,1,2,3,4 };

            Assert.Empty(items0.TakeUpTo(0));
            Assert.Empty(items0.TakeUpTo(10));

            Assert.Empty(items5.TakeUpTo(0));

            var taken = items5.TakeUpTo(1);

            Assert.Single(taken);
            Assert.Equal(0, taken.ElementAt(0));

            taken = items5.TakeUpTo(2);

            Assert.Equal(2, taken.Count());
            Assert.Equal(0, taken.ElementAt(0));
            Assert.Equal(1, taken.ElementAt(1));

            taken = items5.TakeUpTo(5);

            Assert.Equal(5, taken.Count());
            Assert.Equal(0, taken.ElementAt(0));
            Assert.Equal(1, taken.ElementAt(1));
            Assert.Equal(2, taken.ElementAt(2));
            Assert.Equal(3, taken.ElementAt(3));
            Assert.Equal(4, taken.ElementAt(4));

            taken = items5.TakeUpTo(10);

            Assert.Equal(5, taken.Count());
            Assert.Equal(0, taken.ElementAt(0));
            Assert.Equal(1, taken.ElementAt(1));
            Assert.Equal(2, taken.ElementAt(2));
            Assert.Equal(3, taken.ElementAt(3));
            Assert.Equal(4, taken.ElementAt(4));
        }
    }
}
