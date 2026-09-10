//-----------------------------------------------------------------------------
// FILE:	    WorkflowCheckAnalyzer.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:	Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Neon.Temporal.Analyzers
{
    /// <summary>
    /// Checks workflow call graphs for known invalid deterministic workflow actions.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class WorkflowCheckAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// Workflow I/O diagnostic ID.
        /// </summary>
        public const string WorkflowIoDiagnosticId = "NEONTEMP0001";

        /// <summary>
        /// External mutable state diagnostic ID.
        /// </summary>
        public const string ExternalMutableStateDiagnosticId = "NEONTEMP0002";

        /// <summary>
        /// Threading diagnostic ID.
        /// </summary>
        public const string ThreadingDiagnosticId = "NEONTEMP0003";

        /// <summary>
        /// System clock or timer diagnostic ID.
        /// </summary>
        public const string ClockOrTimerDiagnosticId = "NEONTEMP0004";

        /// <summary>
        /// Random or nondeterministic identifier diagnostic ID.
        /// </summary>
        public const string RandomDiagnosticId = "NEONTEMP0005";

        /// <summary>
        /// Task scheduler diagnostic ID.
        /// </summary>
        public const string TaskSchedulerDiagnosticId = "NEONTEMP0006";

        /// <summary>
        /// Task coordination diagnostic ID.
        /// </summary>
        public const string TaskCoordinationDiagnosticId = "NEONTEMP0007";

        /// <summary>
        /// Blocking wait diagnostic ID.
        /// </summary>
        public const string BlockingWaitDiagnosticId = "NEONTEMP0008";

        /// <summary>
        /// Activity async call cancellation token diagnostic ID.
        /// </summary>
        public const string ActivityCancellationTokenDiagnosticId = "NEONTEMP0009";

        private const string WorkflowAttributeMetadataName = "Temporalio.Workflows.WorkflowAttribute";
        private const string ActivityAttributeMetadataName = "Temporalio.Activities.ActivityAttribute";
        private const string AdditionalInvalidMethodsOption = "neon_temporal_workflow_check_invalid_methods";
        private const string AdditionalInvalidPropertiesOption = "neon_temporal_workflow_check_invalid_properties";
        private const string AdditionalInvalidTypesOption = "neon_temporal_workflow_check_invalid_types";
        private const string AllowedSymbolsOption = "neon_temporal_workflow_check_allowed_symbols";

        private static readonly DiagnosticDescriptor WorkflowIoRule = CreateRule(
            WorkflowIoDiagnosticId,
            "Temporal workflow must not perform I/O",
            "Temporal workflows must be deterministic. Move network, disk, and stdio operations to activities.");

        private static readonly DiagnosticDescriptor ExternalMutableStateRule = CreateRule(
            ExternalMutableStateDiagnosticId,
            "Temporal workflow must not access external mutable state",
            "Temporal workflows must not access or mutate external state directly. Move external state interaction to activities.");

        private static readonly DiagnosticDescriptor ThreadingRule = CreateRule(
            ThreadingDiagnosticId,
            "Temporal workflow must not use threading primitives",
            "Temporal workflows run on a deterministic scheduler. Do not create threads, use thread-pool scheduling, or use non-Temporal synchronization primitives.");

        private static readonly DiagnosticDescriptor ClockOrTimerRule = CreateRule(
            ClockOrTimerDiagnosticId,
            "Temporal workflow must not use system clock or .NET timers",
            "Temporal workflows must use deterministic Temporal clock and timer APIs instead of the system clock or .NET timers.");

        private static readonly DiagnosticDescriptor RandomRule = CreateRule(
            RandomDiagnosticId,
            "Temporal workflow must not use nondeterministic random values",
            "Temporal workflows must use deterministic Temporal random APIs instead of random or nondeterministic identifier APIs.");

        private static readonly DiagnosticDescriptor TaskSchedulerRule = CreateRule(
            TaskSchedulerDiagnosticId,
            "Temporal workflow must not use the default task scheduler",
            "Temporal workflows require the deterministic current scheduler. Avoid APIs that implicitly use TaskScheduler.Default or break the current context.");

        private static readonly DiagnosticDescriptor TaskCoordinationRule = CreateRule(
            TaskCoordinationDiagnosticId,
            "Temporal workflow should use Temporal task coordination APIs",
            "Temporal workflows should use Temporal workflow task coordination wrappers instead of .NET task coordination APIs.");

        private static readonly DiagnosticDescriptor BlockingWaitRule = CreateRule(
            BlockingWaitDiagnosticId,
            "Temporal workflow must not block on tasks",
            "Temporal workflows must use asynchronous deterministic workflow APIs instead of blocking waits.");

        private static readonly DiagnosticDescriptor ActivityCancellationTokenRule = new DiagnosticDescriptor(
            id:                 ActivityCancellationTokenDiagnosticId,
            title:              "Temporal activity async calls should provide a CancellationToken",
            messageFormat:      "Temporal activity async call '{0}' does not provide a CancellationToken",
            category:           "Usage",
            defaultSeverity:    DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:        "Temporal activity methods should pass a CancellationToken to async calls so activity cancellation can be observed promptly.");

        private static readonly ImmutableArray<DiagnosticDescriptor> Rules = ImmutableArray.Create(
            WorkflowIoRule,
            ExternalMutableStateRule,
            ThreadingRule,
            ClockOrTimerRule,
            RandomRule,
            TaskSchedulerRule,
            TaskCoordinationRule,
            BlockingWaitRule,
            ActivityCancellationTokenRule);

        private static readonly ImmutableDictionary<InvalidActionRule, DiagnosticDescriptor> RulesByInvalidAction =
            ImmutableDictionary.CreateRange(new[]
            {
                new KeyValuePair<InvalidActionRule, DiagnosticDescriptor>(InvalidActionRule.WorkflowIo, WorkflowIoRule),
                new KeyValuePair<InvalidActionRule, DiagnosticDescriptor>(InvalidActionRule.ExternalMutableState, ExternalMutableStateRule),
                new KeyValuePair<InvalidActionRule, DiagnosticDescriptor>(InvalidActionRule.Threading, ThreadingRule),
                new KeyValuePair<InvalidActionRule, DiagnosticDescriptor>(InvalidActionRule.ClockOrTimer, ClockOrTimerRule),
                new KeyValuePair<InvalidActionRule, DiagnosticDescriptor>(InvalidActionRule.Random, RandomRule),
                new KeyValuePair<InvalidActionRule, DiagnosticDescriptor>(InvalidActionRule.TaskScheduler, TaskSchedulerRule),
                new KeyValuePair<InvalidActionRule, DiagnosticDescriptor>(InvalidActionRule.TaskCoordination, TaskCoordinationRule),
                new KeyValuePair<InvalidActionRule, DiagnosticDescriptor>(InvalidActionRule.BlockingWait, BlockingWaitRule)
            });

        private static DiagnosticDescriptor CreateRule(string id, string title, string description)
        {
            return new DiagnosticDescriptor(
            id:                 id,
            title:              title,
            messageFormat:      "Temporal workflow transitively uses '{0}', which is not deterministic in workflows: {1}",
            category:           "Usage",
            defaultSeverity:    DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:        description,
            customTags:         WellKnownDiagnosticTags.CompilationEnd);
        }

        private static readonly ImmutableArray<InvalidActionSpec> DefaultInvalidMethods = ImmutableArray.Create(
            InvalidActionSpec.Method("System.Threading.Tasks.Task.Run", InvalidActionRule.TaskScheduler, "Use Temporalio.Workflows.Workflow.RunTaskAsync instead."),
            InvalidActionSpec.Method("System.Threading.Tasks.Task.Delay", InvalidActionRule.ClockOrTimer, "Use Temporalio.Workflows.Workflow.DelayAsync instead."),
            InvalidActionSpec.Method("System.Threading.Tasks.Task.Wait", InvalidActionRule.BlockingWait, "Use workflow async APIs instead of blocking waits."),
            InvalidActionSpec.Method("System.Threading.Tasks.Task.WaitAll", InvalidActionRule.BlockingWait, "Use Temporalio.Workflows.Workflow.WhenAllAsync instead."),
            InvalidActionSpec.Method("System.Threading.Tasks.Task.WaitAny", InvalidActionRule.BlockingWait, "Use Temporalio.Workflows.Workflow.WhenAnyAsync instead."),
            InvalidActionSpec.Method("System.Threading.Tasks.Task.WaitAsync", InvalidActionRule.ClockOrTimer, "Use Temporal workflow wait/delay APIs instead of .NET timers."),
            InvalidActionSpec.Method("System.Threading.Tasks.Task.WhenAny", InvalidActionRule.TaskCoordination, "Use Temporalio.Workflows.Workflow.WhenAnyAsync instead."),
            InvalidActionSpec.Method("System.Threading.Tasks.Task.WhenAll", InvalidActionRule.TaskCoordination, "Use Temporalio.Workflows.Workflow.WhenAllAsync instead."),
            InvalidActionSpec.Method("System.Threading.Tasks.Task.ConfigureAwait", InvalidActionRule.TaskScheduler, "Use ConfigureAwait(true) if ConfigureAwait is required."),
            InvalidActionSpec.Method("System.Threading.Tasks.ValueTask.ConfigureAwait", InvalidActionRule.TaskScheduler, "Use ConfigureAwait(true) if ConfigureAwait is required."),
            InvalidActionSpec.Method("System.Threading.Tasks.TaskFactory.StartNew", InvalidActionRule.TaskScheduler, "Pass TaskScheduler.Current explicitly or use Temporalio.Workflows.Workflow.RunTaskAsync."),
            InvalidActionSpec.Method("System.Threading.Tasks.TaskFactory.ContinueWhenAll", InvalidActionRule.TaskScheduler, "Use Temporal workflow task APIs instead."),
            InvalidActionSpec.Method("System.Threading.Tasks.TaskFactory.ContinueWhenAny", InvalidActionRule.TaskScheduler, "Use Temporal workflow task APIs instead."),
            InvalidActionSpec.Method("System.Threading.Thread.Sleep", InvalidActionRule.ClockOrTimer, "Use Temporalio.Workflows.Workflow.DelayAsync instead."),
            InvalidActionSpec.Method("System.Threading.Thread.Start", InvalidActionRule.Threading, "Do not start threads from workflows."),
            InvalidActionSpec.Method("System.Threading.ThreadPool.QueueUserWorkItem", InvalidActionRule.Threading, "Do not schedule thread-pool work from workflows."),
            InvalidActionSpec.Method("System.Threading.ThreadPool.UnsafeQueueUserWorkItem", InvalidActionRule.Threading, "Do not schedule thread-pool work from workflows."),
            InvalidActionSpec.Method("System.Threading.CancellationTokenSource.CancelAsync", InvalidActionRule.TaskCoordination, "Use CancellationTokenSource.Cancel instead."),
            InvalidActionSpec.Method("System.Guid.NewGuid", InvalidActionRule.Random, "Use deterministic workflow APIs for identifiers."),
            InvalidActionSpec.Method("System.IO.File", InvalidActionRule.WorkflowIo, "Do not perform disk I/O from workflows."),
            InvalidActionSpec.Method("System.IO.Directory", InvalidActionRule.WorkflowIo, "Do not perform disk I/O from workflows."),
            InvalidActionSpec.Method("System.IO.FileInfo", InvalidActionRule.WorkflowIo, "Do not perform disk I/O from workflows."),
            InvalidActionSpec.Method("System.IO.DirectoryInfo", InvalidActionRule.WorkflowIo, "Do not perform disk I/O from workflows."),
            InvalidActionSpec.Method("System.Console", InvalidActionRule.WorkflowIo, "Do not perform stdio from workflows."),
            InvalidActionSpec.Method("System.Net.Http.HttpClient", InvalidActionRule.WorkflowIo, "Move network I/O to an activity."));

        private static readonly ImmutableArray<InvalidActionSpec> DefaultInvalidProperties = ImmutableArray.Create(
            InvalidActionSpec.Property("System.DateTime.Now", InvalidActionRule.ClockOrTimer, "Use Temporalio.Workflows.Workflow.UtcNow instead."),
            InvalidActionSpec.Property("System.DateTime.UtcNow", InvalidActionRule.ClockOrTimer, "Use Temporalio.Workflows.Workflow.UtcNow instead."),
            InvalidActionSpec.Property("System.DateTime.Today", InvalidActionRule.ClockOrTimer, "Use Temporalio.Workflows.Workflow.UtcNow instead."),
            InvalidActionSpec.Property("System.DateTimeOffset.Now", InvalidActionRule.ClockOrTimer, "Use Temporalio.Workflows.Workflow.UtcNow instead."),
            InvalidActionSpec.Property("System.DateTimeOffset.UtcNow", InvalidActionRule.ClockOrTimer, "Use Temporalio.Workflows.Workflow.UtcNow instead."),
            InvalidActionSpec.Property("System.Random.Shared", InvalidActionRule.Random, "Use Temporalio.Workflows.Workflow.Random instead."));

        private static readonly ImmutableArray<InvalidActionSpec> DefaultInvalidTypes = ImmutableArray.Create(
            InvalidActionSpec.Type("System.Random", InvalidActionRule.Random, "Use Temporalio.Workflows.Workflow.Random instead."),
            InvalidActionSpec.Type("System.Threading.Thread", InvalidActionRule.Threading, "Do not create threads from workflows."),
            InvalidActionSpec.Type("System.Threading.CancellationTokenSource", InvalidActionRule.ClockOrTimer, "Use non-timeout cancellation token sources in workflows."),
            InvalidActionSpec.Type("System.Threading.Semaphore", InvalidActionRule.Threading, "Use Temporalio.Workflows.Semaphore instead."),
            InvalidActionSpec.Type("System.Threading.SemaphoreSlim", InvalidActionRule.Threading, "Use Temporalio.Workflows.Semaphore instead."),
            InvalidActionSpec.Type("System.Threading.Mutex", InvalidActionRule.Threading, "Use Temporalio.Workflows.Mutex instead."),
            InvalidActionSpec.Type("System.IO.FileStream", InvalidActionRule.WorkflowIo, "Move disk I/O to an activity."),
            InvalidActionSpec.Type("System.IO.StreamReader", InvalidActionRule.WorkflowIo, "Move disk I/O to an activity."),
            InvalidActionSpec.Type("System.IO.StreamWriter", InvalidActionRule.WorkflowIo, "Move disk I/O to an activity."),
            InvalidActionSpec.Type("System.Net.Http.HttpClient", InvalidActionRule.WorkflowIo, "Move network I/O to an activity."));

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Rules;

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext context)
        {
            var workflowAttributeType = context.Compilation.GetTypeByMetadataName(WorkflowAttributeMetadataName);
            var activityAttributeType = context.Compilation.GetTypeByMetadataName(ActivityAttributeMetadataName);
            if (workflowAttributeType == null && activityAttributeType == null)
            {
                return;
            }

            var graph = workflowAttributeType == null ? null : new WorkflowCallGraph();

            context.RegisterOperationAction(c => AnalyzeInvocation(c, graph, activityAttributeType), OperationKind.Invocation);

            if (workflowAttributeType != null)
            {
                context.RegisterOperationAction(c => AnalyzeObjectCreation(c, graph), OperationKind.ObjectCreation);
                context.RegisterOperationAction(c => AnalyzePropertyReference(c, graph), OperationKind.PropertyReference);
                context.RegisterCompilationEndAction(c => ReportInvalidActions(c, graph, workflowAttributeType, activityAttributeType));
            }
        }

        private static void AnalyzeInvocation(OperationAnalysisContext context, WorkflowCallGraph graph, INamedTypeSymbol activityAttributeType)
        {
            var invocation = (IInvocationOperation)context.Operation;
            var containingMethod = context.ContainingSymbol as IMethodSymbol;
            if (containingMethod == null)
            {
                return;
            }

            if (graph != null)
            {
                graph.AddInvocation(containingMethod, invocation.TargetMethod);

                var options = WorkflowCheckOptions.From(context.Options.AnalyzerConfigOptionsProvider.GlobalOptions);
                var spec = options.MatchMethod(invocation.TargetMethod);
                if (spec != null && !IsAllowedConfigureAwait(invocation) && !IsAllowedTaskFactoryStartNew(invocation))
                {
                    graph.AddInvalidAction(containingMethod, spec, invocation.Syntax.GetLocation());
                }
            }

            if (activityAttributeType != null
                && IsActivityMethod(containingMethod, activityAttributeType)
                && IsAsyncCall(invocation.TargetMethod)
                && HasCancellationTokenOverload(context.Compilation, containingMethod, invocation.TargetMethod)
                && !HasCancellationTokenArgument(invocation))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ActivityCancellationTokenRule,
                    invocation.Syntax.GetLocation(),
                    GetSymbolKey(invocation.TargetMethod)));
            }
        }

        private static void AnalyzeObjectCreation(OperationAnalysisContext context, WorkflowCallGraph graph)
        {
            var objectCreation = (IObjectCreationOperation)context.Operation;
            var containingMethod = context.ContainingSymbol as IMethodSymbol;
            if (containingMethod == null || objectCreation.Type == null)
            {
                return;
            }

            var options = WorkflowCheckOptions.From(context.Options.AnalyzerConfigOptionsProvider.GlobalOptions);
            var spec = options.MatchType(objectCreation.Type);
            if (spec != null && !IsAllowedCancellationTokenSource(objectCreation))
            {
                graph.AddInvalidAction(containingMethod, spec, objectCreation.Syntax.GetLocation());
            }
        }

        private static void AnalyzePropertyReference(OperationAnalysisContext context, WorkflowCallGraph graph)
        {
            var propertyReference = (IPropertyReferenceOperation)context.Operation;
            var containingMethod = context.ContainingSymbol as IMethodSymbol;
            if (containingMethod == null)
            {
                return;
            }

            var options = WorkflowCheckOptions.From(context.Options.AnalyzerConfigOptionsProvider.GlobalOptions);
            var spec = options.MatchProperty(propertyReference.Property);
            if (spec != null)
            {
                graph.AddInvalidAction(containingMethod, spec, propertyReference.Syntax.GetLocation());
            }
        }

        private static void ReportInvalidActions(
            CompilationAnalysisContext context,
            WorkflowCallGraph          graph,
            INamedTypeSymbol workflowAttributeType,
            INamedTypeSymbol activityAttributeType)
        {
            foreach (var invalidAction in graph.GetInvalidActionsReachableFromWorkflow(workflowAttributeType, activityAttributeType))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    RulesByInvalidAction[invalidAction.Spec.Rule],
                    invalidAction.Location,
                    invalidAction.Spec.SymbolKey,
                    invalidAction.Spec.Message));
            }
        }

        private static bool IsAllowedConfigureAwait(IInvocationOperation invocation)
        {
            if (invocation.TargetMethod.Name != "ConfigureAwait")
            {
                return false;
            }

            var firstArgument = invocation.Arguments.FirstOrDefault();
            return firstArgument != null
                && firstArgument.Value.ConstantValue.HasValue
                && firstArgument.Value.ConstantValue.Value is bool continueOnCapturedContext
                && continueOnCapturedContext;
        }

        private static bool IsAllowedTaskFactoryStartNew(IInvocationOperation invocation)
        {
            if (invocation.TargetMethod.Name != "StartNew"
                || GetTypeKey(invocation.TargetMethod.ContainingType) != "System.Threading.Tasks.TaskFactory")
            {
                return false;
            }

            return invocation.Arguments.Any(
                argument => GetTypeKey(argument.Parameter?.Type) == "System.Threading.Tasks.TaskScheduler"
                    && IsTaskSchedulerCurrent(argument.Value));
        }

        private static bool IsTaskSchedulerCurrent(IOperation operation)
        {
            if (!(operation is IPropertyReferenceOperation propertyReference))
            {
                return false;
            }

            return GetSymbolKey(propertyReference.Property) == "System.Threading.Tasks.TaskScheduler.Current";
        }

        private static bool IsAllowedCancellationTokenSource(IObjectCreationOperation objectCreation)
        {
            return GetTypeKey(objectCreation.Type) == "System.Threading.CancellationTokenSource"
                && objectCreation.Arguments.Length == 0;
        }

        private static bool IsActivityMethod(IMethodSymbol method, INamedTypeSymbol activityAttributeType)
        {
            return method
                .GetAttributes()
                .Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, activityAttributeType));
        }

        private static bool IsAsyncCall(IMethodSymbol method)
        {
            var returnType = GetTypeKey(method.ReturnType);

            return returnType == "System.Threading.Tasks.Task"
                || returnType == "System.Threading.Tasks.ValueTask";
        }

        private static bool HasCancellationTokenArgument(IInvocationOperation invocation)
        {
            return invocation.Arguments.Any(argument => GetTypeKey(argument.Value.Type) == "System.Threading.CancellationToken");
        }

        private static bool HasCancellationTokenOverload(
            Compilation compilation,
            IMethodSymbol containingMethod,
            IMethodSymbol invokedMethod)
        {
            var methodDefinition = (invokedMethod.ReducedFrom ?? invokedMethod).OriginalDefinition;

            return methodDefinition.ContainingType
                .GetMembers(methodDefinition.Name)
                .OfType<IMethodSymbol>()
                .Select(method => method.OriginalDefinition)
                .Any(candidate =>
                    compilation.IsSymbolAccessibleWithin(candidate, containingMethod.ContainingType)
                    && HasCompatibleCancellationTokenSignature(methodDefinition, candidate));
        }

        private static bool HasCompatibleCancellationTokenSignature(IMethodSymbol invokedMethod, IMethodSymbol candidate)
        {
            if (invokedMethod.Arity != candidate.Arity
                || invokedMethod.IsStatic != candidate.IsStatic
                || !IsAsyncCall(candidate))
            {
                return false;
            }

            var candidateParameters = candidate.Parameters
                .Where(parameter => GetTypeKey(parameter.Type) != "System.Threading.CancellationToken")
                .ToImmutableArray();

            if (candidateParameters.Length == candidate.Parameters.Length
                || candidateParameters.Length != invokedMethod.Parameters.Length)
            {
                return false;
            }

            for (var i = 0; i < invokedMethod.Parameters.Length; i++)
            {
                var invokedParameter   = invokedMethod.Parameters[i];
                var candidateParameter = candidateParameters[i];

                if (invokedParameter.RefKind != candidateParameter.RefKind
                    || invokedParameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                        != candidateParameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                {
                    return false;
                }
            }

            return true;
        }

        private static string GetSymbolKey(ISymbol symbol)
        {
            if (symbol == null)
            {
                return string.Empty;
            }

            switch (symbol)
            {
                case IMethodSymbol method:
                    return $"{GetTypeKey(method.ContainingType)}.{method.Name}";

                case IPropertySymbol property:
                    return $"{GetTypeKey(property.ContainingType)}.{property.Name}";

                case INamedTypeSymbol type:
                    return GetTypeKey(type);

                default:
                    return symbol.ToDisplayString();
            }
        }

        private static string GetTypeKey(ITypeSymbol type)
        {
            if (type == null)
            {
                return string.Empty;
            }

            var typeKey = type.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                .Replace("global::", string.Empty);

            var genericStart = typeKey.IndexOf('<');
            if (genericStart >= 0)
            {
                typeKey = typeKey.Substring(0, genericStart);
            }

            return typeKey;
        }

        private sealed class WorkflowCheckOptions
        {
            private readonly ImmutableArray<InvalidActionSpec> invalidMethods;
            private readonly ImmutableArray<InvalidActionSpec> invalidProperties;
            private readonly ImmutableArray<InvalidActionSpec> invalidTypes;
            private readonly ImmutableHashSet<string> allowedSymbols;

            private WorkflowCheckOptions(
                ImmutableArray<InvalidActionSpec> invalidMethods,
                ImmutableArray<InvalidActionSpec> invalidProperties,
                ImmutableArray<InvalidActionSpec> invalidTypes,
                ImmutableHashSet<string>          allowedSymbols)
            {
                this.invalidMethods     = invalidMethods;
                this.invalidProperties  = invalidProperties;
                this.invalidTypes       = invalidTypes;
                this.allowedSymbols     = allowedSymbols;
            }

            internal static WorkflowCheckOptions From(AnalyzerConfigOptions options)
            {
                return new WorkflowCheckOptions(
                    invalidMethods:    DefaultInvalidMethods.AddRange(ReadSpecs(options, AdditionalInvalidMethodsOption, InvalidActionKind.Method)),
                    invalidProperties: DefaultInvalidProperties.AddRange(ReadSpecs(options, AdditionalInvalidPropertiesOption, InvalidActionKind.Property)),
                    invalidTypes:      DefaultInvalidTypes.AddRange(ReadSpecs(options, AdditionalInvalidTypesOption, InvalidActionKind.Type)),
                    allowedSymbols:    ReadSymbols(options, AllowedSymbolsOption));
            }

            internal InvalidActionSpec MatchMethod(IMethodSymbol method)
            {
                return Match(GetSymbolKey(method), invalidMethods);
            }

            internal InvalidActionSpec MatchProperty(IPropertySymbol property)
            {
                return Match(GetSymbolKey(property), invalidProperties);
            }

            internal InvalidActionSpec MatchType(ITypeSymbol type)
            {
                return Match(GetTypeKey(type), invalidTypes);
            }

            private InvalidActionSpec Match(string symbolKey, ImmutableArray<InvalidActionSpec> specs)
            {
                if (allowedSymbols.Contains(symbolKey))
                {
                    return null;
                }

                return specs.FirstOrDefault(spec => spec.Matches(symbolKey));
            }

            private static ImmutableArray<InvalidActionSpec> ReadSpecs(
                AnalyzerConfigOptions options,
                string                optionName,
                InvalidActionKind     kind)
            {
                if (!options.TryGetValue(optionName, out var optionValue) || string.IsNullOrWhiteSpace(optionValue))
                {
                    return ImmutableArray<InvalidActionSpec>.Empty;
                }

                return optionValue
                    .Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => InvalidActionSpec.Parse(value, kind))
                    .Where(spec => spec != null)
                    .ToImmutableArray();
            }

            private static ImmutableHashSet<string> ReadSymbols(AnalyzerConfigOptions options, string optionName)
            {
                if (!options.TryGetValue(optionName, out var optionValue) || string.IsNullOrWhiteSpace(optionValue))
                {
                    return ImmutableHashSet<string>.Empty;
                }

                return optionValue
                    .Split(new[] { ';', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => value.Trim())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .ToImmutableHashSet();
            }
        }

        private sealed class WorkflowCallGraph
        {
            private readonly object syncLock = new object();
            private readonly Dictionary<IMethodSymbol, MethodRecord> records =
                new Dictionary<IMethodSymbol, MethodRecord>(SymbolEqualityComparer.Default);

            internal void AddInvocation(IMethodSymbol containingMethod, IMethodSymbol targetMethod)
            {
                if (targetMethod == null)
                {
                    return;
                }

                lock (syncLock)
                {
                    GetRecord(containingMethod).Callees.Add(targetMethod.OriginalDefinition);
                    GetRecord(targetMethod.OriginalDefinition);
                }
            }

            internal void AddInvalidAction(IMethodSymbol containingMethod, InvalidActionSpec spec, Location location)
            {
                lock (syncLock)
                {
                    GetRecord(containingMethod).InvalidActions.Add(new InvalidAction(spec, location));
                }
            }

            internal IEnumerable<InvalidAction> GetInvalidActionsReachableFromWorkflow(
                INamedTypeSymbol workflowAttributeType,
                INamedTypeSymbol activityAttributeType)
            {
                var snapshot = Snapshot();
                var workflowMethods = snapshot.Keys
                    .Where(method => IsWorkflowMethod(method, workflowAttributeType, activityAttributeType))
                    .ToArray();
                var visited = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
                var stack = new Stack<IMethodSymbol>(workflowMethods);

                while (stack.Count > 0)
                {
                    var method = stack.Pop().OriginalDefinition;
                    if (!visited.Add(method))
                    {
                        continue;
                    }

                    if (!snapshot.TryGetValue(method, out var record))
                    {
                        continue;
                    }

                    foreach (var invalidAction in record.InvalidActions)
                    {
                        yield return invalidAction;
                    }

                    foreach (var callee in record.Callees)
                    {
                        if (!IsActivityMethod(callee, activityAttributeType))
                    {
                        stack.Push(callee.OriginalDefinition);
                    }
                }
            }
            }

            private Dictionary<IMethodSymbol, MethodRecord> Snapshot()
            {
                lock (syncLock)
                {
                    var snapshot = new Dictionary<IMethodSymbol, MethodRecord>(SymbolEqualityComparer.Default);

                    foreach (var pair in records)
                    {
                        snapshot.Add(pair.Key, pair.Value.Clone());
                    }

                    return snapshot;
                }
            }

            private MethodRecord GetRecord(IMethodSymbol method)
            {
                method = method.OriginalDefinition;

                if (!records.TryGetValue(method, out var record))
                {
                    record = new MethodRecord();
                    records.Add(method, record);
                }

                return record;
            }

            private static bool IsWorkflowMethod(
                IMethodSymbol method,
                INamedTypeSymbol workflowAttributeType,
                INamedTypeSymbol activityAttributeType)
            {
                var containingType = method.ContainingType;

                return containingType != null
                    && !IsActivityMethod(method, activityAttributeType)
                    && containingType.GetAttributes().Any(
                        attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, workflowAttributeType));
            }
        }

        private sealed class MethodRecord
        {
            internal HashSet<IMethodSymbol> Callees { get; } = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

            internal List<InvalidAction> InvalidActions { get; } = new List<InvalidAction>();

            internal MethodRecord Clone()
            {
                var clone = new MethodRecord();
                clone.Callees.UnionWith(Callees);
                clone.InvalidActions.AddRange(InvalidActions);

                return clone;
            }
        }

        private sealed class InvalidAction
        {
            internal InvalidAction(InvalidActionSpec spec, Location location)
            {
                Spec     = spec;
                Location = location;
            }

            internal InvalidActionSpec Spec { get; }

            internal Location Location { get; }
        }

        private sealed class InvalidActionSpec
        {
            private InvalidActionSpec(InvalidActionKind kind, string symbolKey, string message)
                : this(kind, symbolKey, InvalidActionRule.ExternalMutableState, message)
            {
            }

            private InvalidActionSpec(InvalidActionKind kind, string symbolKey, InvalidActionRule rule, string message)
            {
                Kind      = kind;
                SymbolKey = symbolKey;
                Rule      = rule;
                Message   = message;
            }

            internal InvalidActionKind Kind { get; }

            internal string SymbolKey { get; }

            internal InvalidActionRule Rule { get; }

            internal string Message { get; }

            internal static InvalidActionSpec Method(string symbolKey, InvalidActionRule rule, string message) => new InvalidActionSpec(InvalidActionKind.Method, symbolKey, rule, message);

            internal static InvalidActionSpec Property(string symbolKey, InvalidActionRule rule, string message) => new InvalidActionSpec(InvalidActionKind.Property, symbolKey, rule, message);

            internal static InvalidActionSpec Type(string symbolKey, InvalidActionRule rule, string message) => new InvalidActionSpec(InvalidActionKind.Type, symbolKey, rule, message);

            internal static InvalidActionSpec Parse(string value, InvalidActionKind kind)
            {
                var parts = value.Split(new[] { '|' }, 2);
                var symbolKey = parts[0].Trim();
                if (string.IsNullOrWhiteSpace(symbolKey))
                {
                    return null;
                }

                var message = parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1])
                    ? parts[1].Trim()
                    : "This action is configured as invalid for workflows.";

                return new InvalidActionSpec(kind, symbolKey, message);
            }

            internal bool Matches(string symbolKey)
            {
                return symbolKey == SymbolKey
                    || symbolKey.StartsWith(SymbolKey + ".", StringComparison.Ordinal);
            }
        }

        private enum InvalidActionKind
        {
            Method,
            Property,
            Type
        }

        private enum InvalidActionRule
        {
            WorkflowIo,
            ExternalMutableState,
            Threading,
            ClockOrTimer,
            Random,
            TaskScheduler,
            TaskCoordination,
            BlockingWait
        }
    }
}
