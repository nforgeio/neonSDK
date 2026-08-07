//-----------------------------------------------------------------------------
// FILE:	    WorkflowCheckSuppressor.cs
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Neon.Temporal.Analyzers
{
    /// <summary>
    /// Suppresses diagnostics that conflict with deterministic Temporal workflow patterns.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class WorkflowCheckSuppressor : DiagnosticSuppressor
    {
        private const string WorkflowAttributeMetadataName = "Temporalio.Workflows.WorkflowAttribute";

        private static readonly SuppressionDescriptor CA1024Suppression = CreateSuppression("SPTEMP0001", "CA1024");
        private static readonly SuppressionDescriptor CA1822Suppression = CreateSuppression("SPTEMP0002", "CA1822");
        private static readonly SuppressionDescriptor CA2007Suppression = CreateSuppression("SPTEMP0003", "CA2007");
        private static readonly SuppressionDescriptor CA2008Suppression = CreateSuppression("SPTEMP0004", "CA2008");
        private static readonly SuppressionDescriptor CA5394Suppression = CreateSuppression("SPTEMP0005", "CA5394");
        private static readonly SuppressionDescriptor CS1998Suppression = CreateSuppression("SPTEMP0006", "CS1998");
        private static readonly SuppressionDescriptor VSTHRD105Suppression = CreateSuppression("SPTEMP0007", "VSTHRD105");

        private static readonly ImmutableDictionary<string, SuppressionDescriptor> SuppressionsByDiagnosticId =
            ImmutableDictionary.CreateRange(new[]
            {
                new KeyValuePair<string, SuppressionDescriptor>("CA1024", CA1024Suppression),
                new KeyValuePair<string, SuppressionDescriptor>("CA1822", CA1822Suppression),
                new KeyValuePair<string, SuppressionDescriptor>("CA2007", CA2007Suppression),
                new KeyValuePair<string, SuppressionDescriptor>("CA2008", CA2008Suppression),
                new KeyValuePair<string, SuppressionDescriptor>("CA5394", CA5394Suppression),
                new KeyValuePair<string, SuppressionDescriptor>("CS1998", CS1998Suppression),
                new KeyValuePair<string, SuppressionDescriptor>("VSTHRD105", VSTHRD105Suppression)
            });

        /// <inheritdoc/>
        public override ImmutableArray<SuppressionDescriptor> SupportedSuppressions =>
            SuppressionsByDiagnosticId.Values.ToImmutableArray();

        /// <inheritdoc/>
        public override void ReportSuppressions(SuppressionAnalysisContext context)
        {
            var workflowAttributeType = context.Compilation.GetTypeByMetadataName(WorkflowAttributeMetadataName);
            if (workflowAttributeType == null)
            {
                return;
            }

            foreach (var diagnostic in context.ReportedDiagnostics)
            {
                if (diagnostic.IsSuppressed
                    || !SuppressionsByDiagnosticId.TryGetValue(diagnostic.Id, out var suppression))
                {
                    continue;
                }

                var tree = diagnostic.Location.SourceTree;
                if (tree == null)
                {
                    continue;
                }

                var root = tree.GetRoot(context.CancellationToken);
                var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

                if (IsInWorkflowClass(node, context.GetSemanticModel(tree), workflowAttributeType, context.CancellationToken))
                {
                    context.ReportSuppression(Suppression.Create(suppression, diagnostic));
                }
            }
        }

        private static SuppressionDescriptor CreateSuppression(string id, string suppressedDiagnosticId)
        {
            return new SuppressionDescriptor(
                id:                     id,
                suppressedDiagnosticId: suppressedDiagnosticId,
                justification:          "Temporal workflow classes intentionally use patterns required by deterministic workflow execution.");
        }

        private static bool IsInWorkflowClass(
            SyntaxNode                    node,
            SemanticModel                 semanticModel,
            INamedTypeSymbol              workflowAttributeType,
            System.Threading.CancellationToken cancellationToken)
        {
            var classDeclaration = node
                .AncestorsAndSelf()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault();

            if (classDeclaration == null)
            {
                return false;
            }

            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken);
            if (classSymbol == null)
            {
                return false;
            }

            return classSymbol
                .GetAttributes()
                .Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, workflowAttributeType));
        }
    }
}
