// -----------------------------------------------------------------------------
// FILE:	    Test_StringBuilderExtensions.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Xunit;

using Xunit;

namespace TestCommon.Extensions
{
    [Trait(TestTrait.Category, TestArea.NeonCommon)]
    public class Test_StringBuilderExtensions
    {
        [Fact]
        public void AppendLineLinux()
        {
            var sb       = new StringBuilder();
            var expected = "line:0\nline:1\n";

            sb.AppendLineLinux("line:0");
            sb.AppendLineLinux("line:1");
            Assert.Equal(expected, sb.ToString());
        }

        [Fact]
        public void AppendWithSeperator()
        {
            var sb       = new StringBuilder();
            var expected = "a b c";

            sb.AppendWithSeparator("a");
            sb.AppendWithSeparator("b");
            sb.AppendWithSeparator("c");
            Assert.Equal(expected, sb.ToString());

            sb.Clear();
            expected = "a, b, c";

            sb.AppendWithSeparator("a", ", ");
            sb.AppendWithSeparator("b", ", ");
            sb.AppendWithSeparator("c", ", ");
            Assert.Equal(expected, sb.ToString());
        }

        [Fact]
        public void ToStringWithoutLastNewLine()
        {
            var sb       = new StringBuilder("");
            var expected = "";

            Assert.Equal(expected, sb.ToStringWithoutLastNewLine());

            sb = new StringBuilder("\n");

            expected = "";
            Assert.Equal(expected, sb.ToStringWithoutLastNewLine());

            sb = new StringBuilder("\r\n");

            expected = "";
            Assert.Equal(expected, sb.ToStringWithoutLastNewLine());

            sb = new StringBuilder("HELLO WORLD!\n");

            expected = "HELLO WORLD!";
            Assert.Equal(expected, sb.ToStringWithoutLastNewLine());

            sb = new StringBuilder("HELLO WORLD!\r\n");

            expected = "HELLO WORLD!";
            Assert.Equal(expected, sb.ToStringWithoutLastNewLine());
        }
    }
}
