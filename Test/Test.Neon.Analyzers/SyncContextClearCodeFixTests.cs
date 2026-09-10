// -----------------------------------------------------------------------------
// FILE:        SyncContextClearCodeFixTests.cs
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
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Neon.Analyzers;
using Neon.Tasks;

using Xunit;

namespace Test.Neon.Analyzers
{
    /// <summary>
    /// Verifies that inserting SyncContext.Clear preserves expression body results.
    /// </summary>
    public class SyncContextClearCodeFixTests
    {
        /// <summary>
        /// Applies the registered fix and verifies that the result compiles and preserves the body.
        /// </summary>
        /// <param name="member">
        /// A member containing one async function requiring the fix.
        /// </param>
        /// <param name="expectedStatement">
        /// The statement expected after the inserted synchronization context clear.
        /// </param>
        [Theory]
        [InlineData("void M() { Func<Task<bool>> f = async () => await Task.FromResult<object>(null) != null; }", "return await Task.FromResult<object>(null) != null;")]
        [InlineData("void M() { Func<int, Task<bool>> f = async value => await Task.FromResult(value) != 0; }", "return await Task.FromResult(value) != 0;")]
        [InlineData("void M() { Func<Task<int>> f = async () => await Task.FromResult(42); }", "return await Task.FromResult(42);")]
        [InlineData("void M() { Func<ValueTask<int>> f = async () => await Task.FromResult(42); }", "return await Task.FromResult(42);")]
        [InlineData("void M() { Func<Task<int>> f = async () => 42; }", "return 42;")]
        [InlineData("void M() { Func<Task> f = async () => await Task.Delay(1); }", "await Task.Delay(1);")]
        [InlineData("void M() { Func<int, Task> f = async value => await Task.Delay(value); }", "await Task.Delay(value);")]
        [InlineData("void M() { Func<ValueTask> f = async () => await Task.Delay(1); }", "await Task.Delay(1);")]
        [InlineData("void M() { Action f = async () => await Task.Delay(1); }", "await Task.Delay(1);")]
        [InlineData("void M() { Action f = async () => Task.FromResult(42); }", "Task.FromResult(42);")]
        [InlineData("async Task<int> M() => await Task.FromResult(42);", "return await Task.FromResult(42);")]
        [InlineData("async ValueTask<int> M() => await Task.FromResult(42);", "return await Task.FromResult(42);")]
        [InlineData("async Task M() => await Task.Delay(1);", "await Task.Delay(1);")]
        [InlineData("void M() { async Task<int> Local() => await Task.FromResult(42); }", "return await Task.FromResult(42);")]
        [InlineData("void M() { async Task Local() => await Task.Delay(1); }", "await Task.Delay(1);")]
        [InlineData("void M() { Func<Task<int>> f = async () => { return await Task.FromResult(42); }; }", "return await Task.FromResult(42);")]
        [InlineData("void M() { Func<Task<int>> f = async delegate { return await Task.FromResult(42); }; }", "return await Task.FromResult(42);")]
        public async Task PreserveExpressionBodyAsync(string member, string expectedStatement)
        {
            await SyncContext.Clear;

            string source = @"
using System;
using System.Threading.Tasks;

namespace Neon.Tasks
{
    public static class SyncContext
    {
        public static Task Clear => Task.CompletedTask;
    }
}

class Example
{
    " + member + @"
}";

            using var workspace = new AdhocWorkspace();

            string[] platformAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);
            IEnumerable<MetadataReference> references = platformAssemblies
                .Where(path => new[] { "System.Private.CoreLib.dll", "System.Runtime.dll", "System.Threading.Tasks.dll" }.Contains(Path.GetFileName(path)))
                .Select(path => MetadataReference.CreateFromFile(path));

            Project project = workspace.AddProject("CodeFixTest", LanguageNames.CSharp)
                .WithParseOptions(new CSharpParseOptions(LanguageVersion.Latest))
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddMetadataReferences(references);
            Document document = project.AddDocument("Example.cs", source);
            Compilation compilation = await document.Project.GetCompilationAsync();

            Assert.Empty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

            ImmutableArray<DiagnosticAnalyzer> analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(new SyncContextClearAnalyzer());
            Diagnostic diagnostic = Assert.Single(await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync());
            var actions = new List<CodeAction>();
            var provider = new SyncContextClearCodeFixProvider();

            await provider.RegisterCodeFixesAsync(new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None));

            CodeAction fix = Assert.Single(actions);
            ImmutableArray<CodeActionOperation> operations = await fix.GetOperationsAsync(CancellationToken.None);
            ApplyChangesOperation changes = Assert.Single(operations.OfType<ApplyChangesOperation>());
            Document fixedDocument = changes.ChangedSolution.GetDocument(document.Id);
            Compilation fixedCompilation = await fixedDocument.Project.GetCompilationAsync();

            Assert.Empty(fixedCompilation.GetDiagnostics().Where(item => item.Severity == DiagnosticSeverity.Error));
            Assert.Empty(await fixedCompilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync());

            SyntaxNode fixedRoot = await fixedDocument.GetSyntaxRootAsync();
            BlockSyntax fixedBody = Assert.Single(fixedRoot.DescendantNodes().OfType<BlockSyntax>()
                .Where(block => block.Statements.FirstOrDefault()?.ToString() == "await SyncContext.Clear;"));

            Assert.Equal(2, fixedBody.Statements.Count);
            Assert.Equal(expectedStatement, fixedBody.Statements[1].NormalizeWhitespace().ToFullString());
        }
    }
}
