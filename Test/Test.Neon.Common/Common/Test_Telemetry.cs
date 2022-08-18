//-----------------------------------------------------------------------------
// FILE:	    Test_Telemetry.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright (c) 2005-2022 by neonFORGE LLC.  All rights reserved.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Diagnostics;
using Neon.Xunit;

using Xunit;

namespace TestCommon
{
    public class Test_Telemetry
    {
        [Fact]
        public void TelemetryAttribute()
        {
            //-----------------------------------------------------------------
            // Non-error cases:

            var attribute = new TelemetryAttribute("test", "value");

            Assert.Equal("test", attribute.Key);
            Assert.Equal("value", attribute.Value);

            attribute = new TelemetryAttribute("test-1234", 123);

            Assert.Equal("test-1234", attribute.Key);
            Assert.Equal(123, attribute.Value);

            attribute = new TelemetryAttribute("test.value", true);

            Assert.Equal("test.value", attribute.Key);
            Assert.Equal(true, attribute.Value);

            attribute = new TelemetryAttribute("TEST_value", new Uri("http://test.com"));

            Assert.Equal("TEST_value", attribute.Key);
            Assert.Equal(new Uri("http://test.com"), attribute.Value);

            attribute = new TelemetryAttribute(new string('a', 255));

            Assert.Equal(new string('a', 255), attribute.Key);

            //-----------------------------------------------------------------
            // Error cases:

            Assert.Throws<ArgumentNullException>(() => new TelemetryAttribute().Validate());        // Key is null
            Assert.Throws<ArgumentNullException>(() => new TelemetryAttribute(null));               // Key is null
            Assert.Throws<ArgumentNullException>(() => new TelemetryAttribute(""));                 // Key is empty
            Assert.Throws<ArgumentException>(() => new TelemetryAttribute(new string('a', 256)));   // Key is longer than 255 chars
            Assert.Throws<ArgumentException>(() => new TelemetryAttribute("\"hello\""));            // Key includes invalid chars
            Assert.Throws<ArgumentException>(() => new TelemetryAttribute("ö"));                    // Key includes non-ASCII character
        }
    }
}
