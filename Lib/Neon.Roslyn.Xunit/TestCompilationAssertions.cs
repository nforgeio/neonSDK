// -----------------------------------------------------------------------------
// FILE:	    TestCompilationAssertions.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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

using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

using Neon.Common;

namespace Neon.Roslyn.Xunit
{
    /// <summary>
    /// FluentAssertions for <see cref="TestCompilation"/>.
    /// </summary>
    public class TestCompilationAssertions : ReferenceTypeAssertions<TestCompilation, TestCompilationAssertions>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="compilation"></param>
        public TestCompilationAssertions(TestCompilation compilation)
            : base(compilation)
        {

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override string Identifier => "test-compilation";

        /// <summary>
        /// Asserts that the TestCompilation contains the specified source.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="because"></param>
        /// <param name="becauseArgs"></param>
        /// <returns></returns>
        public AndConstraint<TestCompilationAssertions> ContainSource(
            string source, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(!string.IsNullOrEmpty(source))
                .FailWith("Input source cannot be null or empty.")
                .Then
                .Given(() => Subject.HashCodes)
                .ForCondition(hc => hc.Contains(source.GetHashCodeIgnoringWhitespace()))
                .FailWith("Expected compilation to contain {0}, but it was not found.",
                    _ => source);

            return new AndConstraint<TestCompilationAssertions>(this);
        }

        /// <summary>
        /// Asserts that the TestCompilation does not contain the specified source.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="because"></param>
        /// <param name="becauseArgs"></param>
        /// <returns></returns>
        public AndConstraint<TestCompilationAssertions> NotContainSource(
            string source, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(!string.IsNullOrEmpty(source))
                .FailWith("Input source cannot be null or empty.")
                .Then
                .Given(() => Subject.HashCodes)
                .ForCondition(hc => !hc.Contains(source.GetHashCodeIgnoringWhitespace()))
                .FailWith("Expected compilation not to contain {0}, but it was found.",
                    _ => source);

            return new AndConstraint<TestCompilationAssertions>(this);
        }
    }
}
