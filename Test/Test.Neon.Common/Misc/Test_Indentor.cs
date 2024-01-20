// -----------------------------------------------------------------------------
// FILE:	    Test_Indentor.cs
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
    public class Test_Indentor
    {
        [Fact]
        public void StringBuilder()
        {
            var sb = new StringBuilder();

            //---------------------------------------------
            // Test defaults.

            var indentor = new Indentor(sb);
            var expected =
@"line:0
    line:1
        line:2
    line:3
line:4
";

            sb.Clear();
            indentor.WriteLine("line:0");
            indentor.Indent();
            indentor.WriteLine("line:1");
            indentor.Indent();
            indentor.WriteLine("line:2");
            indentor.UnIndent();
            indentor.WriteLine("line:3");
            indentor.UnIndent();
            indentor.WriteLine("line:4");

            Assert.Equal(expected, sb.ToString());

            //---------------------------------------------
            // Test different indent spaces.

            indentor = new Indentor(sb, indentSpaces: 1);
            expected =
@"line:0
 line:1
  line:2
 line:3
line:4
";

            sb.Clear();
            indentor.WriteLine("line:0");
            indentor.Indent();
            indentor.WriteLine("line:1");
            indentor.Indent();
            indentor.WriteLine("line:2");
            indentor.UnIndent();
            indentor.WriteLine("line:3");
            indentor.UnIndent();
            indentor.WriteLine("line:4");

            Assert.Equal(expected, sb.ToString());

            //---------------------------------------------
            // Test with an initial indentation level.

            indentor = new Indentor(sb, indent: 1);
            expected =
@"    line:0
        line:1
            line:2
        line:3
    line:4
";

            sb.Clear();
            indentor.WriteLine("line:0");
            indentor.Indent();
            indentor.WriteLine("line:1");
            indentor.Indent();
            indentor.WriteLine("line:2");
            indentor.UnIndent();
            indentor.WriteLine("line:3");
            indentor.UnIndent();
            indentor.WriteLine("line:4");

            Assert.Equal(expected, sb.ToString());
        }

        [Fact]
        public void TextWriter()
        {
            //---------------------------------------------
            // Test defaults.

            var writer   = new StringWriter();
            var indentor = new Indentor(writer);
            var expected =
@"line:0
    line:1
        line:2
    line:3
line:4
";


            indentor.WriteLine("line:0");
            indentor.Indent();
            indentor.WriteLine("line:1");
            indentor.Indent();
            indentor.WriteLine("line:2");
            indentor.UnIndent();
            indentor.WriteLine("line:3");
            indentor.UnIndent();
            indentor.WriteLine("line:4");

            Assert.Equal(expected, writer.ToString());

            //---------------------------------------------
            // Test different indent spaces.

            writer   = new StringWriter();
            indentor = new Indentor(writer, indentSpaces: 1);
            expected =
@"line:0
 line:1
  line:2
 line:3
line:4
";

            indentor.WriteLine("line:0");
            indentor.Indent();
            indentor.WriteLine("line:1");
            indentor.Indent();
            indentor.WriteLine("line:2");
            indentor.UnIndent();
            indentor.WriteLine("line:3");
            indentor.UnIndent();
            indentor.WriteLine("line:4");

            Assert.Equal(expected, writer.ToString());

            //---------------------------------------------
            // Test with an initial indentation level.

            writer   = new StringWriter();
            indentor = new Indentor(writer, indent: 1);
            expected =
@"    line:0
        line:1
            line:2
        line:3
    line:4
";

            indentor.WriteLine("line:0");
            indentor.Indent();
            indentor.WriteLine("line:1");
            indentor.Indent();
            indentor.WriteLine("line:2");
            indentor.UnIndent();
            indentor.WriteLine("line:3");
            indentor.UnIndent();
            indentor.WriteLine("line:4");

            Assert.Equal(expected, writer.ToString());
        }

        [Fact]
        public void Errors()
        {
            Assert.Throws<ArgumentNullException>(() => new Indentor((StringBuilder)null));
            Assert.Throws<ArgumentException>(() => new Indentor(new StringBuilder(), indentSpaces: 0));
            Assert.Throws<ArgumentException>(() => new Indentor(new StringBuilder(), indent: -1));

            Assert.Throws<ArgumentNullException>(() => new Indentor((TextWriter)null));
            Assert.Throws<ArgumentException>(() => new Indentor(new StringWriter(), indentSpaces: 0));
            Assert.Throws<ArgumentException>(() => new Indentor(new StringWriter(), indent: -1));

            var indentor = new Indentor(new StringBuilder());

            indentor.Indent();
            indentor.Indent();

            indentor.UnIndent();
            indentor.UnIndent();

            Assert.Throws<InvalidOperationException>(() => indentor.UnIndent());
        }
    }
}
