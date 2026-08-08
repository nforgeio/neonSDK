// -----------------------------------------------------------------------------
// FILE:	    WorkflowCheckSuppressorTests.cs
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

using System.Collections.Immutable;
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Neon.Roslyn.Xunit;

namespace Test.Neon.Temporal.Analyzers
{
    public class WorkflowCheckSuppressorTests
    {
        [Fact]
        public void SuppressesConfiguredDiagnosticsInsideWorkflowClasses()
        {
            var source = @"
using System;

namespace Temporalio.Workflows
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class WorkflowAttribute : Attribute
    {
    }
}

namespace TestNamespace
{
    [Temporalio.Workflows.Workflow]
    public class TestWorkflow
    {
        public void Run()
        {
        }
    }
}";

            var testCompilation = new TestCompilationBuilder()
                .AddDiagnosticAnalyzer(new WorkflowDiagnosticsAnalyzer())
                .AddDiagnosticAnalyzer(new global::Neon.Temporal.Analyzers.WorkflowCheckSuppressor())
                .AddSource(source)
                .Build();

            foreach (string diagnosticId in WorkflowDiagnosticsAnalyzer.SupportedDiagnosticIds)
            {
                testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id == diagnosticId);
            }
        }

        [Fact]
        public void DoesNotSuppressConfiguredDiagnosticsOutsideWorkflowClasses()
        {
            var source = @"
using System;

namespace Temporalio.Workflows
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class WorkflowAttribute : Attribute
    {
    }
}

namespace TestNamespace
{
    public class TestWorkflow
    {
        public void Run()
        {
        }
    }
}";

            var testCompilation = new TestCompilationBuilder()
                .AddDiagnosticAnalyzer(new WorkflowDiagnosticsAnalyzer())
                .AddDiagnosticAnalyzer(new global::Neon.Temporal.Analyzers.WorkflowCheckSuppressor())
                .AddSource(source)
                .Build();

            foreach (string diagnosticId in WorkflowDiagnosticsAnalyzer.SupportedDiagnosticIds)
            {
                testCompilation.Diagnostics.Should().ContainSingle(
                    diagnostic => diagnostic.Id == diagnosticId && !diagnostic.IsSuppressed);
            }
        }

        [Fact]
        public void SuppressesSyncContextClearInsideWorkflowClasses()
        {
            var testCompilation = new TestCompilationBuilder()
                .AddDiagnosticAnalyzer(new global::Neon.Analyzers.SyncContextClearAnalyzer())
                .AddDiagnosticAnalyzer(new global::Neon.Temporal.Analyzers.WorkflowCheckSuppressor())
                .AddSource(SyncContextClearSource(workflowAttribute: "[Temporalio.Workflows.Workflow]"))
                .Build();

            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id == "NEON0001");
        }

        [Fact]
        public void DoesNotSuppressSyncContextClearOutsideWorkflowClasses()
        {
            var testCompilation = new TestCompilationBuilder()
                .AddDiagnosticAnalyzer(new global::Neon.Analyzers.SyncContextClearAnalyzer())
                .AddDiagnosticAnalyzer(new global::Neon.Temporal.Analyzers.WorkflowCheckSuppressor())
                .AddSource(SyncContextClearSource(workflowAttribute: string.Empty))
                .Build();

            testCompilation.Diagnostics.Should().ContainSingle(
                diagnostic => diagnostic.Id == "NEON0001" && !diagnostic.IsSuppressed);
        }

        /// <summary>
        /// Returns a workflow-like class whose <c>async</c> method omits <c>await SyncContext.Clear;</c>,
        /// along with the minimal <c>Neon.Tasks.SyncContext</c> and Temporal attribute stubs the
        /// analyzers need to resolve.
        /// </summary>
        /// <param name="workflowAttribute">The attribute to apply to the class, or empty for none.</param>
        private static string SyncContextClearSource(string workflowAttribute)
        {
            return @"
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Neon.Tasks
{
    public struct SyncContext : INotifyCompletion
    {
        public static SyncContext Clear { get; } = default;

        public bool IsCompleted => true;
        public void OnCompleted(Action continuation) { }
        public SyncContext GetAwaiter() => this;
        public void GetResult() { }
    }
}

namespace Temporalio.Workflows
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class WorkflowAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class WorkflowRunAttribute : Attribute
    {
    }
}

namespace TestNamespace
{
    " + workflowAttribute + @"
    public class TestWorkflow
    {
        [Temporalio.Workflows.WorkflowRun]
        public async Task<string> RunAsync()
        {
            await Task.Yield();

            return ""done"";
        }
    }
}";
        }

        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        private sealed class WorkflowDiagnosticsAnalyzer : DiagnosticAnalyzer
        {
            internal static readonly ImmutableArray<string> SupportedDiagnosticIds = ImmutableArray.Create(
                "CA1024",
                "CA1822",
                "CA2007",
                "CA2008",
                "CA5394",
                "CS1998",
                "VSTHRD105",
                "NEON0001");

            private static readonly ImmutableArray<DiagnosticDescriptor> Rules = SupportedDiagnosticIds
                .Select(id => new DiagnosticDescriptor(
                    id:                 id,
                    title:              id,
                    messageFormat:      id,
                    category:           "Usage",
                    defaultSeverity:    DiagnosticSeverity.Warning,
                    isEnabledByDefault: true))
                .ToImmutableArray();

            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

            public override void Initialize(AnalysisContext context)
            {
                context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
                context.EnableConcurrentExecution();
                context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
            }

            private static void AnalyzeNamedType(SymbolAnalysisContext context)
            {
                if (context.Symbol.Name != "TestWorkflow")
                {
                    return;
                }

                foreach (var rule in Rules)
                {
                    context.ReportDiagnostic(Diagnostic.Create(rule, context.Symbol.Locations[0]));
                }
            }
        }
    }
}
