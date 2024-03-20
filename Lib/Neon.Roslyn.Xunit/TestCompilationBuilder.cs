// -----------------------------------------------------------------------------
// FILE:	    TestCompilationBuilder.cs
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
using System.Linq;
using System.Reflection;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Neon.Common;

namespace Neon.Roslyn.Xunit
{
    /// <summary>
    /// Builder for creating <see cref="TestCompilation"/> instances.
    /// </summary>
    public class TestCompilationBuilder
    {
        internal bool Executable { get; set; }
        internal List<Assembly> Assemblies { get; set; } = new List<Assembly>();
        internal List<string> AssemblyPaths { get; set; } = new List<string>();

        internal List<string> Sources { get; set; } = new List<string>();
        internal List<string> AdditionalFilePaths { get; set; } = new List<string>();
        internal List<string> AdditionalFiles { get; set; } = new List<string>();
        internal List<ISourceGenerator> Generators { get; set; } = new List<ISourceGenerator>();

        internal CompilationOptionsProvider CompilationOptionsProvider { get; set; }

        internal Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="executable"></param>
        public TestCompilationBuilder(bool executable = false)
        {
            this.Executable = executable;
        }

        /// <summary>
        /// Builds the <see cref="TestCompilation"/> instance.
        /// </summary>
        /// <returns></returns>
        public TestCompilation Build()
        {
            CompilationOptionsProvider = new CompilationOptionsProvider();
            CompilationOptionsProvider.SetOptions(new CompilationOptions()
            {
                Options = Options
            });

            foreach (var path in AssemblyPaths)
            {
                Assemblies.Add(Assembly.LoadFile(path));
            }

            var references = new List<MetadataReference>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }

            foreach (var assembly in Assemblies)
            {
                if (!assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
                {
                    references.Add(MetadataReference.CreateFromFile(assembly.Location));
                }
            }

            var compilation = CSharpCompilation.Create(
                assemblyName: "Foo",
                syntaxTrees:  Sources.Select(s => CSharpSyntaxTree.ParseText(s)),
                references:   references,
                options: new  CSharpCompilationOptions(
                    outputKind: Executable ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary)
                );

            var additionalFiles = new List<AdditionalSourceText>();


            additionalFiles.AddRange(AdditionalFilePaths?.Select(x => AdditionalSourceText.FromFile(x)));
            additionalFiles.AddRange(AdditionalFiles?.Select(x => AdditionalSourceText.FromString(x)));

            var driver = CSharpGeneratorDriver.Create(
                generators: Generators,
                optionsProvider: CompilationOptionsProvider,
                additionalTexts: additionalFiles);

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);

            var sources = new Dictionary<string, string>();

            foreach (var s in outputCompilation.SyntaxTrees)
            {
                if (!string.IsNullOrWhiteSpace(s.ToString())
                    && !string.IsNullOrEmpty(s.FilePath))
                {
                    sources.Add(
                        key:   s.FilePath.Split("\\", StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty,
                        value: s.ToString());
                }
            }

            return new TestCompilation()
            {
                Compilation = outputCompilation,
                Diagnostics = generateDiagnostics.ToList(),
                HashCodes   = outputCompilation.SyntaxTrees
                .Where(s => !string.IsNullOrWhiteSpace(s.ToString()))
                .Select(s => s.ToString().GetHashCodeIgnoringWhitespace(ignoreCase: false))
                .ToList(),
                Sources = sources
            };
        }
    }
}
