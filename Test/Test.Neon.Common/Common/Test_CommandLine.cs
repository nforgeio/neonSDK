//-----------------------------------------------------------------------------
// FILE:        Test_CommandLine.cs
// CONTRIBUTOR: Jeff Lill
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Xunit;

using Xunit;

// $todo(jefflill):
//
// Test response files and CommandLine.Format()

namespace TestCommon
{
    [Trait(TestTrait.Category, TestArea.NeonCommon)]
    public class Test_CommandLine
    {
        [Fact]
        public void Empty()
        {
            var commandLine = new CommandLine(new string[0]);

            Assert.Empty(commandLine.Items);
            Assert.Empty(commandLine.Arguments);
            Assert.True(!commandLine.HasHelpOption);

            commandLine = new CommandLine();

            Assert.Empty(commandLine.Items);
            Assert.Empty(commandLine.Arguments);
            Assert.True(!commandLine.HasHelpOption);
        }

        [Fact]
        public void Basic()
        {
            var commandLine = new CommandLine(new string[] { "one", "two", "three" });

            Assert.Equal<string>(commandLine.Arguments, new string[] { "one", "two", "three" });
            Assert.True(!commandLine.HasHelpOption);
        }

        [Fact]
        public void Help()
        {
            var commandLine = new CommandLine(new string[] { "one", "--help" });

            Assert.True(commandLine.HasHelpOption);
        }

        [Fact]
        public void DashArgs()
        {
            var commandLine = new CommandLine(new string[] { "foo", "-" });

            Assert.Equal("foo", commandLine.Arguments[0]);
            Assert.Equal("-", commandLine.Arguments[1]);

            commandLine = new CommandLine(new string[] { "foo", "--" });

            Assert.Equal("foo", commandLine.Arguments[0]);
            Assert.Equal("--", commandLine.Arguments[1]);

            commandLine = new CommandLine(new string[] { "foo", "bar", "-" });

            Assert.Equal("foo", commandLine.Arguments[0]);
            Assert.Equal("bar", commandLine.Arguments[1]);
            Assert.Equal("-", commandLine.Arguments[2]);

            commandLine = commandLine.Shift(1);

            Assert.Equal(2, commandLine.Arguments.Length);
            Assert.Equal("bar", commandLine.Arguments[0]);
            Assert.Equal("-", commandLine.Arguments[1]);
        }

        [Fact]
        public void OptionsBasic()
        {
            var commandLine = new CommandLine(new string[] { "one", "-a=1", "two", "-b=2" });

            Assert.Equal<string>(new string[] { "one", "two" }, commandLine.Arguments);
            Assert.Equal("1", commandLine.GetOption("-a"));
            Assert.Equal("2", commandLine.GetOption("-b"));
            Assert.Equal("3", commandLine.GetOption("-c", "3"));
            Assert.Null(commandLine.GetOption("-d"));
            Assert.True(commandLine.HasOption("-a"));
            Assert.False(commandLine.HasOption("-d"));

            Assert.False(commandLine.HasHelpOption);
        }

        [Fact]
        public void OptionsCaseSensitive()
        {
            var commandLine = new CommandLine(new string[] { "one", "-a=1", "-A=2" });

            Assert.Equal("1", commandLine.GetOption("-a"));
            Assert.Equal("2", commandLine.GetOption("-A"));
        }

        [Fact]
        public void OptionsDoubleDash()
        {
            var commandLine = new CommandLine(new string[] { "one", "--a=1", "--A=2" });

            Assert.Equal("1", commandLine.GetOption("--a"));
            Assert.Equal("2", commandLine.GetOption("--A"));
        }

        [Fact]
        public void OptionValues()
        {
            var commandLine = new CommandLine(new string[] { "one", "-a=1", "-a=2" });

            Assert.Equal(new string[] { "1", "2" }, commandLine.GetOptionValues("-a"));
        }

        [Fact]
        public void OptionDefinition()
        {
            // Test matching the first option definition.

            var commandLine = new CommandLine(new string[] { "one", "-a=1", "-b=2" });

            commandLine.DefineOption("-a", "--advanced");

            Assert.Equal("1", commandLine.GetOption("-a"));
            Assert.Equal("1", commandLine.GetOption("--advanced"));
            Assert.Equal("2", commandLine.GetOption("-b"));
            Assert.True(commandLine.HasOption("-a"));
            Assert.True(commandLine.HasOption("--advanced"));
            Assert.True(commandLine.HasOption("-b"));

            // Test matching the second option definition.

            commandLine = new CommandLine(new string[] { "one", "--advanced=1", "-b=2" });

            commandLine.DefineOption("-a", "--advanced");

            Assert.Equal("1", commandLine.GetOption("-a"));
            Assert.Equal("1", commandLine.GetOption("--advanced"));
            Assert.Equal("2", commandLine.GetOption("-b"));
            Assert.True(commandLine.HasOption("-a"));
            Assert.True(commandLine.HasOption("--advanced"));
            Assert.True(commandLine.HasOption("-b"));

            // Test default values.

            commandLine = new CommandLine(new string[] { "one", "--advanced", "-b=2" });

            commandLine.DefineOption("-a", "--advanced").Default = "TEST";

            Assert.Equal("TEST", commandLine.GetOption("-a"));
            Assert.Equal("TEST", commandLine.GetOption("--advanced"));
            Assert.Equal("OVERRIDE", commandLine.GetOption("-a", "OVERRIDE"));
            Assert.Equal("OVERRIDE", commandLine.GetOption("--advanced", "OVERRIDE"));
            Assert.Equal("2", commandLine.GetOption("-b"));
            Assert.True(commandLine.HasOption("-a"));
            Assert.True(commandLine.HasOption("--advanced"));
            Assert.True(commandLine.HasOption("-b"));

            // Test obtaining multiple values.

            commandLine = new CommandLine(new string[] { "one", "--advanced=1", "-b=2", "-a=3", "-a=4", "--advanced=5", "-b=6" });

            commandLine.DefineOption("-a", "--advanced");

            Assert.Equal(new string[] { "2", "6" }, commandLine.GetOptionValues("-b"));
            Assert.Equal(new string[] { "1", "3", "4", "5" }, commandLine.GetOptionValues("-a"));
            Assert.Equal(new string[] { "1", "3", "4", "5" }, commandLine.GetOptionValues("--advanced"));
        }

        [Fact]
        public void OptionFlags()
        {
            var commandLine = new CommandLine(new string[] { "-a" });

            Assert.True(commandLine.GetFlag("-a"));
            Assert.False(commandLine.GetFlag("-b"));

            commandLine = new CommandLine(new string[] { "-q" });

            commandLine.DefineOption("-q", "--quiet");
            Assert.True(commandLine.GetFlag("-q"));
            Assert.True(commandLine.GetFlag("--quiet"));
            Assert.False(commandLine.GetFlag("-b"));
        }

        [Fact]
        public void QuotedOptions()
        {
            // Verify that quoted option values work.

            var commandLine = new CommandLine(new string[] { "--v1=\"hello world!\"", "--v2=\"\"", "--v3='<>| '" });

            Assert.Equal("hello world!", commandLine.GetOption("--v1"));
            Assert.Equal("", commandLine.GetOption("--v2"));
            Assert.Equal("<>| ", commandLine.GetOption("--v3"));

            // Verify that quoted options including single and double quotes work.

            commandLine = new CommandLine(new string[] { "--v1=\"'hello world!'\"", "--v2='\"hello world!\"'" });

            Assert.Equal("'hello world!'", commandLine.GetOption("--v1"));
            Assert.Equal("\"hello world!\"", commandLine.GetOption("--v2"));

            // Verify that quoted options with special characters work.

            commandLine = new CommandLine(new string[] { "--v1=' \t<>:|&^'", "--v2=' \t<>:|&^'" });

            Assert.Equal(" \t<>:|&^", commandLine.GetOption("--v1"));
            Assert.Equal(" \t<>:|&^", commandLine.GetOption("--v2"));
        }

        [Fact]
        public void GetArguments()
        {
            var commandLine = new CommandLine(new string[] { "one", "two", "three" });

            Assert.Equal(new string[] { "one", "two", "three" }, commandLine.GetArguments());
            Assert.Equal(new string[] { "two", "three" }, commandLine.GetArguments(1));
            Assert.Equal(new string[] { "three" }, commandLine.GetArguments(2));
            Assert.Equal(new string[] { }, commandLine.GetArguments(3));

            commandLine = new CommandLine(new string[] { });

            Assert.Equal(new string[] { }, commandLine.GetArguments(3));
        }

        [Fact]
        public void StartsWithArgs()
        {
            var commandLine = new CommandLine(new string[] { "one", "two", "three" });

            Assert.True(commandLine.StartsWithArgs("one"));
            Assert.True(commandLine.StartsWithArgs("one", "two"));
            Assert.True(commandLine.StartsWithArgs("one", "two", "three"));
            Assert.True(commandLine.StartsWithArgs("one", "two", "three"));

            Assert.False(commandLine.StartsWithArgs());
            Assert.False(commandLine.StartsWithArgs("one", "two", "three", "four"));
            Assert.False(commandLine.StartsWithArgs("x"));
            Assert.False(commandLine.StartsWithArgs("one", "x"));
            Assert.False(commandLine.StartsWithArgs("one", "two", "three", "x"));

            Assert.Throws<ArgumentException>(() => commandLine.StartsWithArgs("one", null));

            commandLine = new CommandLine(new string[] { "--test", "one", "two", "three" });

            Assert.True(commandLine.StartsWithArgs("one"));
            Assert.True(commandLine.StartsWithArgs("one", "two"));
            Assert.True(commandLine.StartsWithArgs("one", "two", "three"));
            Assert.True(commandLine.StartsWithArgs("one", "two", "three"));

            Assert.False(commandLine.StartsWithArgs());
            Assert.False(commandLine.StartsWithArgs("one", "two", "three", "four"));
            Assert.False(commandLine.StartsWithArgs("x"));
            Assert.False(commandLine.StartsWithArgs("one", "x"));
            Assert.False(commandLine.StartsWithArgs("one", "two", "three", "x"));

            Assert.Throws<ArgumentException>(() => commandLine.StartsWithArgs("one", null));
        }

        [Fact]
        public void Split()
        {
            //-------------------------
            var commandLine = new CommandLine(new string[] { "one", "--x", "two", "three"});
            var split = commandLine.Split();

            Assert.Equal(split.Left.Items, new string[] { "one", "--x", "two", "three"}, new CollectionComparer<string>());
            Assert.Null(split.Right);

            //-------------------------
            commandLine = new CommandLine(new string[] { "one", "--x", "two", "three", "--", "four", "five" });
            split = commandLine.Split();

            Assert.Equal(split.Left.Items, new string[] { "one", "--x", "two", "three" }, new CollectionComparer<string>());
            Assert.Equal(split.Right.Items, new string[] { "four", "five" }, new CollectionComparer<string>());

            //-------------------------
            commandLine = new CommandLine(new string[] { "one", "--x", "two", "three", "splitter", "four", "five" });
            split = commandLine.Split("splitter");

            Assert.Equal(split.Left.Items, new string[] { "one", "--x", "two", "three" }, new CollectionComparer<string>());
            Assert.Equal(split.Right.Items, new string[] { "four", "five" }, new CollectionComparer<string>());

            //-------------------------
            commandLine = new CommandLine(new string[] { "one", "--x", "two", "three", "splitter", "four", "five" });
            split = commandLine.Split("splitter", addSplitterToRight: true);

            Assert.Equal(split.Left.Items, new string[] { "one", "--x", "two", "three" }, new CollectionComparer<string>());
            Assert.Equal(split.Right.Items, new string[] { "splitter", "four", "five" }, new CollectionComparer<string>());

            //-------------------------
            commandLine = new CommandLine(new string[] { "--" });
            split = commandLine.Split();

            Assert.Empty(split.Left.Items);
            Assert.Empty(split.Right.Items);

            //-------------------------
            commandLine = new CommandLine(new string[] { "left", "--left0", "--left1=1", "--left2", "2", "--", "right", "--right0", "--right1=1", "--right2", "2" });
            split = commandLine.Split();

            Assert.Equal(split.Left.Items, new string[] { "left", "--left0", "--left1=1", "--left2", "2" }, new CollectionComparer<string>());
            Assert.Equal(split.Right.Items, new string[] { "right", "--right0", "--right1=1", "--right2", "2" }, new CollectionComparer<string>());

            //-------------------------
            commandLine = new CommandLine(new string[] { "left", "--left0", "--left1=1", "--left2", "2", "--", "right", "--right0", "--right1=1", "--right2", "2" });
            split = commandLine.Split(addSplitterToRight: true);

            Assert.Equal(split.Left.Items, new string[] { "left", "--left0", "--left1=1", "--left2", "2" }, new CollectionComparer<string>());
            Assert.Equal(split.Right.Items, new string[] { "--", "right", "--right0", "--right1=1", "--right2", "2" }, new CollectionComparer<string>());

            //-------------------------
            commandLine = new CommandLine(new string[] { "left", "--left0", "--left1=1", "--left2", "2", "XX", "right", "--right0", "--right1=1", "--right2", "2" });
            split = commandLine.Split("XX");

            Assert.Equal(split.Left.Items, new string[] { "left", "--left0", "--left1=1", "--left2", "2" }, new CollectionComparer<string>());
            Assert.Equal(split.Right.Items, new string[] { "right", "--right0", "--right1=1", "--right2", "2" }, new CollectionComparer<string>());

            //-------------------------
            commandLine = new CommandLine(new string[] { "cmd", "left", "--left0", "--left1=1", "--left2", "2", "--", "right", "--right0", "--right1=1", "--right2", "2" });
            commandLine = commandLine.Shift(1);
            split = commandLine.Split("--");

            Assert.Equal(split.Left.Items, new string[] { "left", "--left0", "--left1=1", "--left2", "2" }, new CollectionComparer<string>());
            Assert.Equal(split.Right.Items, new string[] { "right", "--right0", "--right1=1", "--right2", "2" }, new CollectionComparer<string>());
        }

        [Fact]
        public void Option_OptionalValue()
        {
            // Verify that an option can optionally have a value.

            var commandLine = new CommandLine(new string[] { "--option" });

            Assert.True(commandLine.HasOption("--option"));
            Assert.Equal(string.Empty, commandLine.GetOption("--option"));

            commandLine = new CommandLine(new string[] { "--option=value" });

            Assert.True(commandLine.HasOption("--option"));
            Assert.Equal("value", commandLine.GetOption("--option"));
        }

        [Fact]
        public void Preprocess()
        {
            // Verify that references like ${env:VARIABLE} are converted correctly when 
            // preprocessing a command line (using [PreprocessReader]).

            try
            {
                Environment.SetEnvironmentVariable("TEST_MYENVVAR1", "env.value1");
                Environment.SetEnvironmentVariable("TEST_MYENVVAR2", "env.value2");

                var variables = new Dictionary<string, string>()
                {
                    { "TEST_MYVAR1", "value1" },
                    { "TEST_MYVAR2", "value2" }
                };

                var commandLine = new CommandLine(new string[] { "${env:TEST_MYENVVAR1}", "${env:TEST_MYENVVAR2}", "--test1", "--test2=${env:TEST_MYENVVAR2}", "${TEST_MYVAR1}", "${TEST_MYVAR2}" });
                var processed   = commandLine.Preprocess(variables);

                Assert.Equal(6, processed.Items.Count());
                Assert.Equal(4, processed.Arguments.Count());
                Assert.Equal(2, processed.Options.Count());

                Assert.Equal("env.value1", processed.Arguments[0]);
                Assert.Equal("env.value2", processed.Arguments[1]);
                Assert.Equal("value1", processed.Arguments[2]);
                Assert.Equal("value2", processed.Arguments[3]);

                Assert.True(processed.HasOption("--test1"));
                Assert.Equal("env.value2", processed.GetOption("--test2"));
            }
            finally
            {
                Environment.SetEnvironmentVariable("TEST_MYENVVAR1", null);
                Environment.SetEnvironmentVariable("TEST_MYENVVAR2", null);
            }
        }

        [Fact]
        public void AsString()
        {
            // Verify: Commandline.ToString()

            Assert.Equal("test", new CommandLine("test").ToString());
            Assert.Equal("test p1", new CommandLine("test", "p1").ToString());
            Assert.Equal("test p1 p2", new CommandLine("test", "p1", "p2").ToString());
            Assert.Equal("test p1 p2 p3", new CommandLine("test", "p1", "p2", "p3").ToString());
            Assert.Equal("test p1=foo", new CommandLine("test", "p1=foo").ToString());
            Assert.Equal("test \"p1=foo bar\"", new CommandLine("test", "p1=foo bar").ToString());
            
            //-----------------------------------------------------------------
            // Argument quoting

            // Test quoting of special characters.

            Assert.Equal("'\"hello world!\"'", new CommandLine(new string[] { "\"hello world!\"" }).ToString());
            Assert.Equal("\"'hello world!'\"", new CommandLine(new string[] { "'hello world!'" }).ToString());
            Assert.Equal("\" \t<>:|&^\"", new CommandLine(new string[] { " \t<>:|&^" }).ToString());

            // Verify that this fails when an argument includes both single and double quotes.

            Assert.Throws<InvalidOperationException>(() => new CommandLine(new string[] { "\"'" }).ToString());

            //-----------------------------------------------------------------
            // Option quoting

            Assert.Equal("--option=\"hello world!\"", new CommandLine(new string[] { "--option=hello world!" }).ToString());
            Assert.Equal("--option='\"hello world!\"'", new CommandLine(new string[] { "--option=\"hello world!\"" }).ToString());
            Assert.Equal("--option=\"'hello world!'\"", new CommandLine(new string[] { "--option='hello world!'" }).ToString());
            Assert.Equal("--option=\" \t<>:|&^\"", new CommandLine(new string[] { "--option= \t<>:|&^" }).ToString());

            // Verify that this fails when an argument includes both single and double quotes.

            Assert.Throws<InvalidOperationException>(() => new CommandLine(new string[] { "--option=\"'" }).ToString());
        }

        [Fact]
        public void AsFormatted_WithoutBars_WithoutContinuation()
        {
            var lineContinuation = string.Empty;

            // Verify: Commandline.ToFormatted() with bars

            Assert.Equal($"test{NeonHelper.LineEnding}", new CommandLine().ToFormatted("test"));

            Assert.Equal(
$@"test{lineContinuation}
    p1
",
                new CommandLine("p1").ToFormatted("test"));

            Assert.Equal(
$@"test{lineContinuation}
    p1{lineContinuation}
    p2
",
                new CommandLine("p1", "p2").ToFormatted("test"));
        }

        [Fact]
        public void AsFormatted_WithoutBars_WithContinuation()
        {
            var lineContinuation = NeonHelper.IsWindows ? " ^" : " \\";

            // Verify: Commandline.ToFormatted() with bars

            Assert.Equal($"test{NeonHelper.LineEnding}", new CommandLine().ToFormatted("test", withLineContinuation: true));

            Assert.Equal(
$@"test{lineContinuation}
    p1
",
                new CommandLine("p1").ToFormatted("test", withLineContinuation: true));

            Assert.Equal(
$@"test{lineContinuation}
    p1{lineContinuation}
    p2
",
                new CommandLine("p1", "p2").ToFormatted("test", withLineContinuation: true));
        }

        [Fact]
        public void AsFormatted_WithBars_WithContinuation()
        {
            var lineContinuation = NeonHelper.IsWindows ? " ^" : " \\";
            var expected         = string.Empty;
            var bar              = new string('-', 80);

            // Verify: Commandline.ToFormatted() with bars

            expected =
$@"{bar}
test
{bar}
";
            Assert.Equal(expected, new CommandLine().ToFormatted("test", withBars: true, withLineContinuation: true));

            expected =
$@"{bar}
test{lineContinuation}
    p1
{bar}
";
            Assert.Equal(expected, new CommandLine("p1").ToFormatted("test", withBars: true, withLineContinuation: true));

            expected =
$@"{bar}
test{lineContinuation}
    p1{lineContinuation}
    p2
{bar}
";
            Assert.Equal(expected, new CommandLine("p1", "p2").ToFormatted("test", withBars: true, withLineContinuation: true));

            // Verify that shifting the command line still formats the original.

            expected =
$@"{bar}
test{lineContinuation}
    p1
{bar}
";
            Assert.Equal(expected, new CommandLine("p1").Shift(1).ToFormatted("test", withBars: true, withLineContinuation: true));
        }

        [Fact]
        public void AsFormatted_WithBars_WithoutContinuation()
        {
            var lineContinuation = string.Empty;
            var expected         = string.Empty;
            var bar              = new string('-', 80);

            // Verify: Commandline.ToFormatted() with bars

            expected =
$@"{bar}
test
{bar}
";
            Assert.Equal(expected, new CommandLine().ToFormatted("test", withBars: true));

            expected =
$@"{bar}
test{lineContinuation}
    p1
{bar}
";
            Assert.Equal(expected, new CommandLine("p1").ToFormatted("test", withBars: true));

            expected =
$@"{bar}
test{lineContinuation}
    p1{lineContinuation}
    p2
{bar}
";
            Assert.Equal(expected, new CommandLine("p1", "p2").ToFormatted("test", withBars: true));

            // Verify that shifting the command line still formats the original.

            expected =
$@"{bar}
test{lineContinuation}
    p1
{bar}
";
            Assert.Equal(expected, new CommandLine("p1").Shift(1).ToFormatted("test", withBars: true));
        }

        [Fact]
        public void AsFormatted_WithSpecialChars()
        {
            var lineContinuation = string.Empty;
            var expected         = string.Empty;
            var bar              = new string('-', 80);

            //-----------------------------------------------------------------
            // Verify arguments with special characters.

            expected =
$@"{bar}
test{lineContinuation}
    ""hello world!""
{bar}
";
            Assert.Equal(expected, new CommandLine("hello world!").ToFormatted("test", withBars: true));

            expected =
$@"{bar}
test{lineContinuation}
    "" <>:|&^""
{bar}
";
            Assert.Equal(expected, new CommandLine(" <>:|&^").ToFormatted("test", withBars: true));

            // Ensure that we see an InvalidOperationException when the arg contains single and double quotes.

            Assert.Throws<InvalidOperationException>(() => new CommandLine("'\"").ToFormatted("test", withBars: true));

            //-----------------------------------------------------------------
            // Verify options with special characters

            expected =
$@"{bar}
test{lineContinuation}
    --option=""hello world!""
{bar}
";
            Assert.Equal(expected, new CommandLine("--option=hello world!").ToFormatted("test", withBars: true));

            expected =
$@"{bar}
test{lineContinuation}
    --option="" <>:|&^""
{bar}
";
            Assert.Equal(expected, new CommandLine("--option= <>:|&^").ToFormatted("test", withBars: true));

            // Ensure that we see an InvalidOperationException when the arg contains single and double quotes.

            Assert.Throws<InvalidOperationException>(() => new CommandLine("--option='\"").ToFormatted("test", withBars: true));
        }
    }
}
