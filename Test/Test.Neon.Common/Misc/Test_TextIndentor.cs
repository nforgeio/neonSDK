// -----------------------------------------------------------------------------
// FILE:	    Test_TextIndentor.cs
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Xunit;

using Xunit;

namespace TestCommon
{
    [Trait(TestTrait.Category, TestArea.NeonCommon)]
    public class Test_TextIndentor
    {
        [Fact]
        public void StringBuilder()
        {
            var sb = new StringBuilder();

            //---------------------------------------------
            // Test defaults.

            var indentor = new TextIndentor(sb);
            var expected =
@"
line:0
    line:1
        line:2
    line:3
line:4
";

            sb.Clear();
            indentor.AppendLine();
            indentor.AppendLine("line:0");
            indentor.Indent();
            indentor.AppendLine("line:1");
            indentor.Indent();
            indentor.AppendLine("line:2");
            indentor.UnIndent();
            indentor.AppendLine("line:3");
            indentor.UnIndent();
            indentor.AppendLine("line:4");

            Assert.Equal(expected, sb.ToString());

            //---------------------------------------------
            // Test different indent spaces.

            indentor = new TextIndentor(sb, indentWidth: 1);
            expected =
@"
line:0
 line:1
  line:2
 line:3
line:4
";

            sb.Clear();
            indentor.AppendLine();
            indentor.AppendLine("line:0");
            indentor.Indent();
            indentor.AppendLine("line:1");
            indentor.Indent();
            indentor.AppendLine("line:2");
            indentor.UnIndent();
            indentor.AppendLine("line:3");
            indentor.UnIndent();
            indentor.AppendLine("line:4");

            Assert.Equal(expected, sb.ToString());

            //---------------------------------------------
            // Test with an initial indentation level.

            indentor = new TextIndentor(sb, indentLevel: 1);
            expected =
@"
    line:0
        line:1
            line:2
        line:3
    line:4
";

            sb.Clear();
            indentor.AppendLine();
            indentor.AppendLine("line:0");
            indentor.Indent();
            indentor.AppendLine("line:1");
            indentor.Indent();
            indentor.AppendLine("line:2");
            indentor.UnIndent();
            indentor.AppendLine("line:3");
            indentor.UnIndent();
            indentor.AppendLine("line:4");

            Assert.Equal(expected, sb.ToString());

            //---------------------------------------------
            // Test with TABs.

            indentor = new TextIndentor(sb, indentWidth: 1, indentChar: '\t');
            expected =
$@"
line:0
{(char)NeonHelper.TAB}line:1
{(char)NeonHelper.TAB}{(char)NeonHelper.TAB}line:2
{(char)NeonHelper.TAB}line:3
line:4
";

            sb.Clear();
            indentor.AppendLine();
            indentor.AppendLine("line:0");
            indentor.Indent();
            indentor.AppendLine("line:1");
            indentor.Indent();
            indentor.AppendLine("line:2");
            indentor.UnIndent();
            indentor.AppendLine("line:3");
            indentor.UnIndent();
            indentor.AppendLine("line:4");

            Assert.Equal(expected, sb.ToString());

            //---------------------------------------------
            // Verify indentation behavior.

            indentor = new TextIndentor(sb, indentWidth: 4);
            expected =
$@"

    HELLO WORLD!
";

            sb.Clear();
            indentor.AppendLine();
            indentor.Indent();
            indentor.AppendLine();
            indentor.Append("HELLO");
            indentor.Append(" WORLD!");
            indentor.AppendLine();

            Assert.Equal(expected, sb.ToString());

            //---------------------------------------------
            // Verify Reset().

            indentor = new TextIndentor(sb, indentWidth: 4);
            expected =
$@"
    line: 0
        line: 1
            line: 2
line: 3
";

            sb.Clear();
            indentor.AppendLine();
            indentor.Indent();
            indentor.AppendLine("line: 0");
            indentor.Indent();
            indentor.AppendLine("line: 1");
            indentor.Indent();
            indentor.AppendLine("line: 2");
            indentor.Reset();
            indentor.AppendLine("line: 3");

            Assert.Equal(expected, sb.ToString());
        }

        [Fact]
        public void TextWriter()
        {
            //---------------------------------------------
            // Test defaults.

            var writer   = new StringWriter();
            var indentor = new TextIndentor(writer);
            var expected =
@"
line:0
    line:1
        line:2
    line:3
line:4
";

            indentor.AppendLine();
            indentor.AppendLine("line:0");
            indentor.Indent();
            indentor.AppendLine("line:1");
            indentor.Indent();
            indentor.AppendLine("line:2");
            indentor.UnIndent();
            indentor.AppendLine("line:3");
            indentor.UnIndent();
            indentor.AppendLine("line:4");

            Assert.Equal(expected, writer.ToString());

            //---------------------------------------------
            // Test different indent spaces.

            writer   = new StringWriter();
            indentor = new TextIndentor(writer, indentWidth: 1);
            expected =
@"
line:0
 line:1
  line:2
 line:3
line:4
";

            indentor.AppendLine();
            indentor.AppendLine("line:0");
            indentor.Indent();
            indentor.AppendLine("line:1");
            indentor.Indent();
            indentor.AppendLine("line:2");
            indentor.UnIndent();
            indentor.AppendLine("line:3");
            indentor.UnIndent();
            indentor.AppendLine("line:4");

            Assert.Equal(expected, writer.ToString());

            //---------------------------------------------
            // Test with an initial indentation level.

            writer   = new StringWriter();
            indentor = new TextIndentor(writer, indentLevel: 1);
            expected =
@"
    line:0
        line:1
            line:2
        line:3
    line:4
";

            indentor.AppendLine();
            indentor.AppendLine("line:0");
            indentor.Indent();
            indentor.AppendLine("line:1");
            indentor.Indent();
            indentor.AppendLine("line:2");
            indentor.UnIndent();
            indentor.AppendLine("line:3");
            indentor.UnIndent();
            indentor.AppendLine("line:4");

            Assert.Equal(expected, writer.ToString());

            //---------------------------------------------
            // Test with TABs.

            writer   = new StringWriter();
            indentor = new TextIndentor(writer, indentWidth: 1, indentChar: '\t');
            expected =
$@"
line:0
{(char)NeonHelper.TAB}line:1
{(char)NeonHelper.TAB}{(char)NeonHelper.TAB}line:2
{(char)NeonHelper.TAB}line:3
line:4
";

            indentor.AppendLine();
            indentor.AppendLine("line:0");
            indentor.Indent();
            indentor.AppendLine("line:1");
            indentor.Indent();
            indentor.AppendLine("line:2");
            indentor.UnIndent();
            indentor.AppendLine("line:3");
            indentor.UnIndent();
            indentor.AppendLine("line:4");

            Assert.Equal(expected, writer.ToString());

            //---------------------------------------------
            // Verify indentation behavior.

            writer   = new StringWriter();
            indentor = new TextIndentor(writer, indentWidth: 4);
            expected =
$@"

    HELLO WORLD!
";

            indentor.AppendLine();
            indentor.Indent();
            indentor.AppendLine();
            indentor.Append("HELLO");
            indentor.Append(" WORLD!");
            indentor.AppendLine();

            Assert.Equal(expected, writer.ToString());

            //---------------------------------------------
            // Verify Reset().

            writer   = new StringWriter();
            indentor = new TextIndentor(writer, indentWidth: 4);
            expected =
$@"
    line: 0
        line: 1
            line: 2
line: 3
";

            indentor.AppendLine();
            indentor.Indent();
            indentor.AppendLine("line: 0");
            indentor.Indent();
            indentor.AppendLine("line: 1");
            indentor.Indent();
            indentor.AppendLine("line: 2");
            indentor.Reset();
            indentor.AppendLine("line: 3");

            Assert.Equal(expected, writer.ToString());
        }

        [Fact]
        public void Errors()
        {
            Assert.Throws<ArgumentNullException>(() => new TextIndentor((StringBuilder)null));
            Assert.Throws<ArgumentException>(() => new TextIndentor(new StringBuilder(), indentWidth: 0));
            Assert.Throws<ArgumentException>(() => new TextIndentor(new StringBuilder(), indentLevel: -1));

            Assert.Throws<ArgumentNullException>(() => new TextIndentor((TextWriter)null));
            Assert.Throws<ArgumentException>(() => new TextIndentor(new StringWriter(), indentWidth: 0));
            Assert.Throws<ArgumentException>(() => new TextIndentor(new StringWriter(), indentLevel: -1));

            var indentor = new TextIndentor(new StringBuilder());

            indentor.Indent();
            indentor.Indent();

            indentor.UnIndent();
            indentor.UnIndent();

            Assert.Throws<InvalidOperationException>(() => indentor.UnIndent());    // Indent underflow
        }
    }
}
