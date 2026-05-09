//-----------------------------------------------------------------------------
// FILE:	    SyncContextClearCodeFixProvider.cs
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
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace Neon.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SyncContextClearCodeFixProvider))]
    [Shared]
    public sealed class SyncContextClearCodeFixProvider : CodeFixProvider
    {
        private const string Title = "Insert 'await SyncContext.Clear;'";

        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(SyncContextClearAnalyzer.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return;
            }

            foreach (var diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;
                var token          = root.FindToken(diagnosticSpan.Start);

                // Walk up to the enclosing async function node we know how to fix.
                var node = token.Parent;
                while (node != null
                    && !(node is MethodDeclarationSyntax)
                    && !(node is LocalFunctionStatementSyntax)
                    && !(node is AnonymousMethodExpressionSyntax)
                    && !(node is LambdaExpressionSyntax))
                {
                    node = node.Parent;
                }

                if (node == null)
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title:               Title,
                        createChangedDocument: ct => InsertSyncContextClearAsync(context.Document, node, ct),
                        equivalenceKey:      SyncContextClearAnalyzer.DiagnosticId),
                    diagnostic);
            }
        }

        private static async Task<Document> InsertSyncContextClearAsync(Document document, SyntaxNode functionNode, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            if (root == null)
            {
                return document;
            }

            var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
            var endOfLine  = DetectEndOfLine(sourceText);
            var clearStatement = BuildClearStatement(endOfLine);

            SyntaxNode newFunctionNode;

            switch (functionNode)
            {
                case MethodDeclarationSyntax method:
                    newFunctionNode = ReplaceBody(method, method.Body, method.ExpressionBody?.Expression,
                        newBlock => method.WithBody(newBlock).WithExpressionBody(null).WithSemicolonToken(default),
                        clearStatement);
                    break;

                case LocalFunctionStatementSyntax local:
                    newFunctionNode = ReplaceBody(local, local.Body, local.ExpressionBody?.Expression,
                        newBlock => local.WithBody(newBlock).WithExpressionBody(null).WithSemicolonToken(default),
                        clearStatement);
                    break;

                case AnonymousMethodExpressionSyntax anon:
                    newFunctionNode = ReplaceBody(anon, anon.Block, anon.ExpressionBody as ExpressionSyntax,
                        newBlock => anon.WithBlock(newBlock).WithExpressionBody(null),
                        clearStatement);
                    break;

                case ParenthesizedLambdaExpressionSyntax paren:
                    newFunctionNode = ReplaceBody(paren, paren.Block, paren.ExpressionBody,
                        newBlock => paren.WithBlock(newBlock).WithExpressionBody(null),
                        clearStatement);
                    break;

                case SimpleLambdaExpressionSyntax simple:
                    newFunctionNode = ReplaceBody(simple, simple.Block, simple.ExpressionBody,
                        newBlock => simple.WithBlock(newBlock).WithExpressionBody(null),
                        clearStatement);
                    break;

                default:
                    return document;
            }

            var newRoot = root.ReplaceNode(functionNode, newFunctionNode);

            // Add a using directive for Neon.Tasks if the compilation unit doesn't already have one.
            newRoot = EnsureUsing(newRoot);

            return document.WithSyntaxRoot(newRoot);
        }

        private static SyntaxNode ReplaceBody<TNode>(
            TNode                           original,
            BlockSyntax                     existingBlock,
            ExpressionSyntax                existingExpression,
            System.Func<BlockSyntax, TNode> withNewBlock,
            StatementSyntax                 clearStatement)
            where TNode : SyntaxNode
        {
            BlockSyntax newBlock;

            if (existingBlock != null)
            {
                var newStatements = existingBlock.Statements.Insert(0, clearStatement);
                newBlock = existingBlock.WithStatements(newStatements);
            }
            else if (existingExpression != null)
            {
                // Convert expression body to a block. The expression-bodied async function returned
                // a Task / Task<T> / ValueTask, so we must preserve the awaited inner expression.
                // We conservatively wrap as ExpressionStatement (works for Task-returning async voids/methods).
                // For Task<T>-returning methods this would change semantics, but that's a rare edge case
                // an expression-bodied async method that ALSO violates the rule. Author can tweak after.
                var wrapped = SyntaxFactory.ExpressionStatement(existingExpression);
                newBlock = SyntaxFactory.Block(clearStatement, wrapped)
                    .WithAdditionalAnnotations(Formatter.Annotation);
            }
            else
            {
                newBlock = SyntaxFactory.Block(clearStatement)
                    .WithAdditionalAnnotations(Formatter.Annotation);
            }

            return withNewBlock(newBlock).WithAdditionalAnnotations(Formatter.Annotation);
        }

        private static StatementSyntax BuildClearStatement(SyntaxTrivia endOfLine)
        {
            // await Neon.Tasks.SyncContext.Clear;  (Simplifier annotation will reduce to SyncContext.Clear when possible).
            var memberAccess = SyntaxFactory.ParseExpression("global::Neon.Tasks.SyncContext.Clear")
                .WithAdditionalAnnotations(Simplifier.Annotation);

            var awaitExpr = SyntaxFactory.AwaitExpression(memberAccess);

            return SyntaxFactory.ExpressionStatement(awaitExpr)
                .WithTrailingTrivia(endOfLine, endOfLine)
                .WithAdditionalAnnotations(Formatter.Annotation);
        }

        private static SyntaxTrivia DetectEndOfLine(SourceText sourceText)
        {
            if (sourceText != null)
            {
                var text   = sourceText.ToString();
                var lfIdx  = text.IndexOf('\n');
                if (lfIdx >= 0)
                {
                    return lfIdx > 0 && text[lfIdx - 1] == '\r'
                        ? SyntaxFactory.CarriageReturnLineFeed
                        : SyntaxFactory.LineFeed;
                }
            }

            // No newline in the file (empty or single-line). The choice doesn't actually
            // affect the resulting diff since there's no surrounding style to mismatch;
            // default to CRLF for parity with the historical behavior.
            return SyntaxFactory.ElasticCarriageReturnLineFeed;
        }

        private static SyntaxNode EnsureUsing(SyntaxNode root)
        {
            if (!(root is CompilationUnitSyntax compilationUnit))
            {
                return root;
            }

            const string targetNamespace = "Neon.Tasks";

            if (compilationUnit.Usings.Any(u => u.Name?.ToString() == targetNamespace))
            {
                return compilationUnit;
            }

            var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(targetNamespace))
                .WithAdditionalAnnotations(Formatter.Annotation);

            return compilationUnit.AddUsings(usingDirective);
        }
    }
}
