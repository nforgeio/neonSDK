// -----------------------------------------------------------------------------
// FILE:	    TestCompilationBuilderExtensions.cs
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

using Microsoft.CodeAnalysis;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Neon.Roslyn.Xunit
{
    /// <summary>
    /// Extensions for building <see cref="TestCompilation"/> instances.
    /// </summary>
    public static class TestCompilationBuilderExtensions
    {
        /// <summary>
        /// Adds an assembly to the compilation.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddAssembly(this TestCompilationBuilder compilation, Assembly assembly)
        {
            compilation.Assemblies.Add(assembly);

            return compilation;
        }

        /// <summary>
        /// Adds an assembly to the compilation.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddAssembly(this TestCompilationBuilder compilation, string path)
        {
            compilation.AssemblyPaths.Add(path);

            return compilation;
        }

        /// <summary>
        /// Adds assemblies to the compilation.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddAssemblies(this TestCompilationBuilder compilation, params Assembly[] assemblies)
        {
            compilation.Assemblies.AddRange(assemblies);

            return compilation;
        }

        /// <summary>
        /// Adds assemblies to the compilation.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddAssemblies(this TestCompilationBuilder compilation, params string[] paths)
        {
            compilation.AssemblyPaths.AddRange(paths);

            return compilation;
        }

        /// <summary>
        /// Adds source text to the compilation.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddSource(this TestCompilationBuilder compilation, string source)
        {
            compilation.Sources.Add(source);
            return compilation;
        }

        /// <summary>
        /// Adds source file to the compilation.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddSourceFile(this TestCompilationBuilder compilation, string path)
        {
            compilation.Sources.Add(File.ReadAllText(path));
            return compilation;
        }

        /// <summary>
        /// Adds source texts to the compilation.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="sources"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddSources(this TestCompilationBuilder compilation, params string[] sources)
        {
            compilation.Sources.AddRange(sources);
            return compilation;
        }

        /// <summary>
        /// Adds source files to the compilation.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddSourceFiles(this TestCompilationBuilder compilation, params string[] paths)
        {
            compilation.Sources.AddRange(paths.Select(File.ReadAllText));
            return compilation;
        }

        /// <summary>
        /// Adds Additional files to the compilation by file path.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddAdditionalFilePath(this TestCompilationBuilder compilation, string filePath)
        {
            compilation.AdditionalFilePaths.Add(filePath);
            return compilation;
        }

        /// <summary>
        /// Adds Additional files to the compilation by file paths.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddAdditionalFilePaths(this TestCompilationBuilder compilation, params string[] filePaths)
        {
            compilation.AdditionalFilePaths.AddRange(filePaths);
            return compilation;
        }

        /// <summary>
        /// Adds Additional files to the compilation.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="fileText"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddAdditionalFile(this TestCompilationBuilder compilation, string fileText)
        {
            compilation.AdditionalFiles.Add(fileText);
            return compilation;
        }

        /// <summary>
        /// Adds Additional files to the compilation.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="fileTexts"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddAdditionalFiles(this TestCompilationBuilder compilation, params string[] fileTexts)
        {
            compilation.AdditionalFiles.AddRange(fileTexts);
            return compilation;
        }

        /// <summary>
        /// Adds an <see cref="ISourceGenerator"/> to the compilation.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="sourceGenerator"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddSourceGenerator(this TestCompilationBuilder compilation, ISourceGenerator sourceGenerator)
        {
            compilation.Generators.Add(sourceGenerator);
            return compilation;
        }

        /// <summary>
        /// Adds an <see cref="ISourceGenerator"/> to the compilation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="compilation"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddSourceGenerator<T>(this TestCompilationBuilder compilation)
            where T : ISourceGenerator, new()
        {
            compilation.Generators.Add(new T());
            return compilation;
        }

        /// <summary>
        /// Adds a build option to the compilation.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddOption(this TestCompilationBuilder compilation, string key, string value)
        {
            compilation.Options.Add(key, value);
            return compilation;
        }

        /// <summary>
        /// Adds a build option to the compilation.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TestCompilationBuilder AddOption(this TestCompilationBuilder compilation, string key, object value)
        {
            compilation.Options.Add(key, value.ToString());
            return compilation;
        }

        /// <summary>
        /// Adds a collection of build options to the compilation.
        /// </summary>
        /// <param name="compilation">The compilation.</param>
        /// <param name="options">A dictionary of options.</param>
        /// <returns></returns>
        public static TestCompilationBuilder AddOptions(this TestCompilationBuilder compilation, Dictionary<string, string> options)
        {
            foreach (var opt in options)
            {
                compilation.Options.Add(opt.Key, opt.Value);
            }

            return compilation;
        }
    }
}
