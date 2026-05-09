//-----------------------------------------------------------------------------
// FILE:	    SyncContextClearAnalyzer.cs
// CONTRIBUTOR: Marcus Bowyer
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

using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Neon.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SyncContextClearAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NEON0001";

        private const string SyncContextMetadataName = "Neon.Tasks.SyncContext";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id:                 DiagnosticId,
            title:              "Async method must start with 'await SyncContext.Clear;'",
            messageFormat:      "Async method '{0}' must call 'await SyncContext.Clear;' as its first statement",
            category:           "Usage",
            defaultSeverity:    DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description:        "Every async method, local function, and lambda should detach from the current SynchronizationContext by awaiting SyncContext.Clear as its very first statement.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(OnCompilationStart);
        }

        private static void OnCompilationStart(CompilationStartAnalysisContext ctx)
        {
            var syncContextType = ctx.Compilation.GetTypeByMetadataName(SyncContextMetadataName);
            if (syncContextType == null)
            {
                return;
            }

            var clearSymbol = syncContextType
                .GetMembers("Clear")
                .OfType<IPropertySymbol>()
                .FirstOrDefault(p => p.IsStatic);

            if (clearSymbol == null)
            {
                return;
            }

            ctx.RegisterSyntaxNodeAction(
                c => Analyze(c, clearSymbol),
                SyntaxKind.MethodDeclaration,
                SyntaxKind.LocalFunctionStatement,
                SyntaxKind.AnonymousMethodExpression,
                SyntaxKind.SimpleLambdaExpression,
                SyntaxKind.ParenthesizedLambdaExpression);
        }

        private static void Analyze(SyntaxNodeAnalysisContext ctx, IPropertySymbol clearSymbol)
        {
            SyntaxTokenList modifiers;
            BlockSyntax     body;
            SyntaxNode      exprBody;
            Location        reportLocation;
            string          displayName;

            switch (ctx.Node)
            {
                case MethodDeclarationSyntax method:

                    modifiers      = method.Modifiers;
                    body           = method.Body;
                    exprBody       = method.ExpressionBody;
                    reportLocation = method.Identifier.GetLocation();
                    displayName    = method.Identifier.ValueText;
                    break;

                case LocalFunctionStatementSyntax local:

                    modifiers      = local.Modifiers;
                    body           = local.Body;
                    exprBody       = local.ExpressionBody;
                    reportLocation = local.Identifier.GetLocation();
                    displayName    = local.Identifier.ValueText;
                    break;

                case AnonymousMethodExpressionSyntax anon:

                    modifiers      = anon.Modifiers;
                    body           = anon.Block;
                    exprBody       = anon.ExpressionBody;
                    reportLocation = anon.AsyncKeyword.GetLocation();
                    displayName    = "<anonymous method>";
                    break;

                case SimpleLambdaExpressionSyntax simple:

                    modifiers      = simple.Modifiers;
                    body           = simple.Block;
                    exprBody       = simple.ExpressionBody;
                    reportLocation = simple.AsyncKeyword.GetLocation();
                    displayName    = "<lambda>";
                    break;

                case ParenthesizedLambdaExpressionSyntax paren:

                    modifiers      = paren.Modifiers;
                    body           = paren.Block;
                    exprBody       = paren.ExpressionBody;
                    reportLocation = paren.AsyncKeyword.GetLocation();
                    displayName    = "<lambda>";
                    break;

                default:
                    return;
            }

            if (!modifiers.Any(SyntaxKind.AsyncKeyword))
            {
                return;
            }

            // Expression-bodied async function — single expression cannot satisfy the rule.
            if (body == null && exprBody != null)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Rule, reportLocation, displayName));
                return;
            }

            // No body at all (abstract/partial/unimplemented) — nothing to enforce.
            if (body == null || body.Statements.Count == 0)
            {
                return;
            }

            if (!IsSyncContextClearAwait(body.Statements[0], ctx.SemanticModel, clearSymbol))
            {
                ctx.ReportDiagnostic(Diagnostic.Create(Rule, reportLocation, displayName));
            }
        }

        private static bool IsSyncContextClearAwait(StatementSyntax statement, SemanticModel model, IPropertySymbol clearSymbol)
        {
            if (!(statement is ExpressionStatementSyntax exprStmt))
            {
                return false;
            }

            if (!(exprStmt.Expression is AwaitExpressionSyntax awaitExpr))
            {
                return false;
            }

            var symbol = model.GetSymbolInfo(awaitExpr.Expression).Symbol;

            return symbol != null && SymbolEqualityComparer.Default.Equals(symbol, clearSymbol);
        }
    }
}
