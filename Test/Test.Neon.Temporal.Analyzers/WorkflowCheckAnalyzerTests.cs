// -----------------------------------------------------------------------------
// FILE:	    WorkflowCheckAnalyzerTests.cs
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

using System.Linq;

using FluentAssertions;

using Neon.Roslyn.Xunit;

namespace Test.Neon.Temporal.Analyzers
{
    public class WorkflowCheckAnalyzerTests
    {
        [Fact]
        public void ReportsInvalidActionsDirectlyInsideWorkflowClasses()
        {
            var testCompilation = BuildCompilation(@"
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    [Temporalio.Workflows.Workflow]
    public class TestWorkflow
    {
        public async Task RunAsync()
        {
            await Task.Delay(1);
            var now = DateTime.UtcNow;
        }
    }
}");

            testCompilation.Diagnostics.Count(diagnostic => diagnostic.Id == "NEONTEMP0004").Should().Be(2);
        }

        [Fact]
        public void ReportsInvalidActionsTransitivelyCalledFromWorkflowClasses()
        {
            var testCompilation = BuildCompilation(@"
using System.IO;

namespace TestNamespace
{
    [Temporalio.Workflows.Workflow]
    public class TestWorkflow
    {
        public void Run()
        {
            Helper.Read();
        }
    }

    public static class Helper
    {
        public static string Read()
        {
            return File.ReadAllText(""test.txt"");
        }
    }
}");

            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == "NEONTEMP0001");
        }

        [Fact]
        public void DoesNotReportWorkflowRulesInsideInvokedActivityMethods()
        {
            var testCompilation = BuildCompilation(@"
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    [Temporalio.Workflows.Workflow]
    public class TestWorkflow
    {
        public Task RunAsync()
        {
            return TestActivities.RunAsync();
        }
    }

    public static class TestActivities
    {
        [Temporalio.Activities.Activity]
        public static Task RunAsync()
        {
            var now = DateTime.UtcNow;

            return Task.CompletedTask;
        }
    }
}");

            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id == "NEONTEMP0004");
        }

        [Fact]
        public void DoesNotTreatActivityMethodsOnWorkflowClassesAsWorkflowRoots()
        {
            var testCompilation = BuildCompilation(@"
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    [Temporalio.Workflows.Workflow]
    public class TestWorkflow
    {
        public Task RunAsync()
        {
            return Task.CompletedTask;
        }

        [Temporalio.Activities.Activity]
        public Task RunActivityAsync()
        {
            var now = DateTime.UtcNow;

            return Task.CompletedTask;
        }
    }
}");

            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id == "NEONTEMP0004");
        }

        [Fact]
        public void ReportsTaskAndThreadingInvalidActions()
        {
            var testCompilation = BuildCompilation(@"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestNamespace
{
    [Temporalio.Workflows.Workflow]
    public class TestWorkflow
    {
        public async Task RunAsync()
        {
            await Task.Run(() => { });
            await Task.FromResult(1).ConfigureAwait(false);
            Task.Factory.StartNew(() => { });
            Thread.Sleep(1);
            var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var semaphore = new SemaphoreSlim(1);
        }
    }
}");

            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == "NEONTEMP0003");
            testCompilation.Diagnostics.Count(diagnostic => diagnostic.Id == "NEONTEMP0004").Should().Be(2);
            testCompilation.Diagnostics.Count(diagnostic => diagnostic.Id == "NEONTEMP0006").Should().Be(3);
        }

        [Fact]
        public void ReportsRandomTaskCoordinationAndBlockingWaitRules()
        {
            var testCompilation = BuildCompilation(@"
using System;
using System.Threading.Tasks;

namespace TestNamespace
{
    [Temporalio.Workflows.Workflow]
    public class TestWorkflow
    {
        public void Run(Task task1, Task task2)
        {
            var random = new Random();
            var id = Guid.NewGuid();
            Task.WhenAny(task1, task2);
            task1.Wait();
        }
    }
}");

            testCompilation.Diagnostics.Count(diagnostic => diagnostic.Id == "NEONTEMP0005").Should().Be(2);
            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == "NEONTEMP0007");
            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == "NEONTEMP0008");
        }

        [Fact]
        public void DoesNotReportInvalidActionsWhenUnreachableFromWorkflowClasses()
        {
            var testCompilation = BuildCompilation(@"
using System;

namespace TestNamespace
{
    [Temporalio.Workflows.Workflow]
    public class TestWorkflow
    {
        public void Run()
        {
        }
    }

    public class Helper
    {
        public DateTime Read()
        {
            return DateTime.Now;
        }
    }
}");

            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id.StartsWith("NEONTEMP"));
        }

        [Fact]
        public void AllowsConfiguredSymbols()
        {
            var testCompilation = BuildCompilation(@"
using System;

namespace TestNamespace
{
    [Temporalio.Workflows.Workflow]
    public class TestWorkflow
    {
        public DateTime Run()
        {
            return DateTime.UtcNow;
        }
    }
}",
                builder => builder.AddOption("neon_temporal_workflow_check_allowed_symbols", "System.DateTime.UtcNow"));

            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id.StartsWith("NEONTEMP"));
        }

        [Fact]
        public void ReportsConfiguredInvalidMethods()
        {
            var testCompilation = BuildCompilation(@"
namespace TestNamespace
{
    [Temporalio.Workflows.Workflow]
    public class TestWorkflow
    {
        public void Run()
        {
            External.MutableState();
        }
    }

    public static class External
    {
        public static void MutableState()
        {
        }
    }
}",
                builder => builder.AddOption(
                    "neon_temporal_workflow_check_invalid_methods",
                    "TestNamespace.External.MutableState|Move mutable state access to an activity."));

            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == "NEONTEMP0002");
        }

        [Fact]
        public void AllowsSafeConfigureAwaitAndTaskFactoryStartNewForms()
        {
            var testCompilation = BuildCompilation(@"
using System.Threading;
using System.Threading.Tasks;

namespace TestNamespace
{
    [Temporalio.Workflows.Workflow]
    public class TestWorkflow
    {
        public async Task RunAsync()
        {
            await Task.FromResult(1).ConfigureAwait(true);
            Task.Factory.StartNew(() => { }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Current);
        }
    }
}");

            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id.StartsWith("NEONTEMP"));
        }

        [Fact]
        public void ReportsActivityAsyncCallsWithoutCancellationToken()
        {
            var testCompilation = BuildCompilation(@"
using System.Threading;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestActivities
    {
        [Temporalio.Activities.Activity]
        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await ServiceAsync();
            await ServiceAsync(cancellationToken);
        }

        private static Task ServiceAsync()
        {
            return Task.CompletedTask;
        }

        private static Task ServiceAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}");

            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == "NEONTEMP0009");
        }

        [Fact]
        public void DoesNotReportActivityAsyncCallsWithoutCancellationTokenOverload()
        {
            var testCompilation = BuildCompilation(@"
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestActivities
    {
        [Temporalio.Activities.Activity]
        public async Task RunAsync()
        {
            await ServiceAsync();
        }

        private static Task ServiceAsync()
        {
            return Task.CompletedTask;
        }
    }
}");

            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id == "NEONTEMP0009");
        }

        [Fact]
        public void DoesNotReportActivityAsyncCallsWithOnlyIncompatibleCancellationTokenOverload()
        {
            var testCompilation = BuildCompilation(@"
using System.Threading;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestActivities
    {
        [Temporalio.Activities.Activity]
        public async Task RunAsync()
        {
            await ServiceAsync();
        }

        private static Task ServiceAsync()
        {
            return Task.CompletedTask;
        }

        private static Task ServiceAsync(int value, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}");

            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id == "NEONTEMP0009");
        }

        [Fact]
        public void DoesNotReportActivityAsyncCallsWithUnusableCancellationTokenOverloads()
        {
            var testCompilation = BuildCompilation(@"
using System.Threading;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestActivities
    {
        [Temporalio.Activities.Activity]
        public async Task RunAsync()
        {
            await Service.RunAsync();
            await Service.LookupAsync();
        }
    }

    public static class Service
    {
        public static Task RunAsync()
        {
            return Task.CompletedTask;
        }

        private static Task RunAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public static Task LookupAsync()
        {
            return Task.CompletedTask;
        }

        public static int LookupAsync(CancellationToken cancellationToken)
        {
            return 0;
        }
    }
}");

            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id == "NEONTEMP0009");
        }

        [Fact]
        public void ReportsActivityExtensionAsyncCallsWithCancellationTokenOverload()
        {
            var testCompilation = BuildCompilation(@"
using System.Threading;
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestActivities
    {
        [Temporalio.Activities.Activity]
        public async Task RunAsync()
        {
            await ""value"".ServiceAsync();
        }
    }

    public static class ServiceExtensions
    {
        public static Task ServiceAsync(this string value)
        {
            return Task.CompletedTask;
        }

        public static Task ServiceAsync(this string value, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}");

            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == "NEONTEMP0009");
        }

        [Fact]
        public void DoesNotReportActivityCancellationTokenRuleOutsideActivityMethods()
        {
            var testCompilation = BuildCompilation(@"
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestActivities
    {
        public async Task RunAsync()
        {
            await ServiceAsync();
        }

        private static Task ServiceAsync()
        {
            return Task.CompletedTask;
        }
    }
}");

            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id == "NEONTEMP0009");
        }

        [Fact]
        public void ReportsActivityCancellationTokenRuleWithoutWorkflowAttributeInCompilation()
        {
            var source = @"
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Temporalio.Activities
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ActivityAttribute : Attribute
    {
    }
}

namespace TestNamespace
{
    public class TestActivities
    {
        [Temporalio.Activities.Activity]
        public async Task RunAsync()
        {
            await ServiceAsync();
        }

        private static Task ServiceAsync()
        {
            return Task.CompletedTask;
        }

        private static Task ServiceAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}";

            var testCompilation = new TestCompilationBuilder()
                .AddDiagnosticAnalyzer(new global::Neon.Temporal.Analyzers.WorkflowCheckAnalyzer())
                .AddSource(source)
                .Build();

            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == "NEONTEMP0009");
        }

        private static TestCompilation BuildCompilation(string source, System.Action<TestCompilationBuilder> configure = null)
        {
            var temporalStub = @"
using System;

namespace Temporalio.Workflows
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class WorkflowAttribute : Attribute
    {
    }

    public static class Workflow
    {
        public static DateTime UtcNow => DateTime.UtcNow;
    }
}

namespace Temporalio.Activities
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ActivityAttribute : Attribute
    {
    }
}";

            var builder = new TestCompilationBuilder()
                .AddDiagnosticAnalyzer(new global::Neon.Temporal.Analyzers.WorkflowCheckAnalyzer())
                .AddSource(temporalStub)
                .AddSource(source);

            configure?.Invoke(builder);

            return builder.Build();
        }
    }
}
