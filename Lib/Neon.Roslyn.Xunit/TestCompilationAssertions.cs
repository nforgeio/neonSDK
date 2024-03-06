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

using System.Linq;

using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

using Microsoft.CodeAnalysis;

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

        /// <summary>
        /// Asserts that a <see cref="Diagnostic"/> with the given Id is present in the TestCompilation.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="because"></param>
        /// <param name="becauseArgs"></param>
        /// <returns></returns>
        public AndConstraint<TestCompilationAssertions> HaveDiagnostic(
            string id, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(!string.IsNullOrEmpty(id))
                .FailWith("Input id cannot be null or empty.")
                .Then
                .Given(() => Subject.Diagnostics)
                .ForCondition(d => d.Any(d => d.Id == id))
                .FailWith("Expected compilation to have diagnostic with Id: {0}, but it was not found.",
                    _ => id);

            return new AndConstraint<TestCompilationAssertions>(this);
        }

        /// <summary>
        /// Asserts that a <see cref="Diagnostic"/> with the given Ids are present in the TestCompilation.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="because"></param>
        /// <param name="becauseArgs"></param>
        /// <returns></returns>
        public AndConstraint<TestCompilationAssertions> HaveDiagnostics(
            string[] ids, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject.Diagnostics)
                .ForCondition(d => ids.All(id => d.Any(d => d.Id == id)))
                .FailWith("Expected compilation to have diagnostic Ids: [{0}], but [{1}] were present.",
                    _ => string.Join(",", ids), diags => string.Join(",", diags.Select(d => d.Id)));

            return new AndConstraint<TestCompilationAssertions>(this);
        }

        /// <summary>
        /// Assedrts that a diagnostic is present in the TestCompilation.
        /// </summary>
        /// <param name="diagnostic"></param>
        /// <param name="because"></param>
        /// <param name="becauseArgs"></param>
        /// <returns></returns>
        public AndConstraint<TestCompilationAssertions> HaveDiagnostic(
            Diagnostic diagnostic, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .ForCondition(diagnostic != null)
                .FailWith("Input diagnostic cannot be null.")
                .Then
                .Given(() => Subject.Diagnostics)
                .ForCondition(d => d.Any(d => d.Location.Equals(diagnostic.Location)
                    && d.Properties.SequenceEqual(diagnostic.Properties)
                    && d.AdditionalLocations.SequenceEqual(diagnostic.AdditionalLocations)
                    && d.WarningLevel                  == diagnostic.WarningLevel
                    && d.DefaultSeverity               == diagnostic.DefaultSeverity
                    && d.Id                            == diagnostic.Id
                    && d.IsSuppressed                  == diagnostic.IsSuppressed
                    && d.IsWarningAsError              == diagnostic.IsWarningAsError
                    && d.Severity                      == diagnostic.Severity
                    && d.Descriptor.Id                 == diagnostic.Descriptor.Id
                    && d.Descriptor.Category           == diagnostic.Descriptor.Category
                    && d.Descriptor.CustomTags         == diagnostic.Descriptor.CustomTags
                    && d.Descriptor.DefaultSeverity    == diagnostic.Descriptor.DefaultSeverity
                    && d.Descriptor.Description        == diagnostic.Descriptor.Description
                    && d.Descriptor.HelpLinkUri        == diagnostic.Descriptor.HelpLinkUri
                    && d.Descriptor.IsEnabledByDefault == diagnostic.Descriptor.IsEnabledByDefault
                    && d.Descriptor.MessageFormat      == diagnostic.Descriptor.MessageFormat
                    && d.Descriptor.Title              == diagnostic.Descriptor.Title))
                .FailWith("Expected compilation to have diagnostic with Id: {0}, but it was not found.",
                    _ => diagnostic.Id);

            return new AndConstraint<TestCompilationAssertions>(this);
        }
    }
}
