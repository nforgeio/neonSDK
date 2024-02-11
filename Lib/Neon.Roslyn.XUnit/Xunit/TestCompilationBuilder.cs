// -----------------------------------------------------------------------------
// FILE:	    Compilation.cs
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Neon.Roslyn.Xunit
{
    public class TestCompilationBuilder
    {
        public bool Executable { get; set; }
        internal List<Assembly> Assemblies { get; set; } = new List<Assembly>();

        internal List<string> Sources { get; set; } = new List<string>();
        internal List<string> AdditionalFilePaths { get; set; } = new List<string>();
        internal List<string> AdditionalFiles { get; set; } = new List<string>();
        internal List<ISourceGenerator> Generators { get; set; } = new List<ISourceGenerator>();

        internal CompilationOptionsProvider CompilationOptionsProvider { get; set; }

        public Compilation Compilation { get; set; }
        public List<Diagnostic> Diagnostics { get; set; }
        public TestCompilationBuilder(bool executable = false)
        {

        }

        public TestCompilation Build()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(string.Join(Environment.NewLine, Sources));

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

            var compilation = CSharpCompilation.Create("Foo",
                                new SyntaxTree[] { syntaxTree },
                                references,
                                new CSharpCompilationOptions(Executable ? OutputKind.ConsoleApplication : OutputKind.DynamicallyLinkedLibrary));


            var additionalFiles = new List<AdditionalSourceText>();


            additionalFiles.AddRange(AdditionalFilePaths?.Select(x => AdditionalSourceText.FromFile(x)));
            additionalFiles.AddRange(AdditionalFiles?.Select(x => AdditionalSourceText.FromString(x)));

            var driver = CSharpGeneratorDriver.Create(
                generators: Generators,
                optionsProvider: CompilationOptionsProvider,
                additionalTexts: additionalFiles);

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generateDiagnostics);

            return new TestCompilation()
            {
                Compilation = outputCompilation,
                Diagnostics = generateDiagnostics.ToList()
            };
        }
    }
}
