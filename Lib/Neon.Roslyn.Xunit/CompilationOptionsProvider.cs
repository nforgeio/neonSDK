// -----------------------------------------------------------------------------
// FILE:	    CompilationOptionsProvider.cs
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
using Microsoft.CodeAnalysis.Diagnostics;

namespace Neon.Roslyn.Xunit
{
    /// <summary>
    /// The compilation options provider.
    /// </summary>
    public class CompilationOptionsProvider : AnalyzerConfigOptionsProvider
    {
        private AnalyzerConfigOptions _options;

        /// <summary>
        /// The global options.
        /// </summary>
        public override AnalyzerConfigOptions GlobalOptions => _options;

        /// <summary>
        /// Sets the options.
        /// </summary>
        /// <param name="options"></param>
        public void SetOptions(AnalyzerConfigOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
        {
            return GlobalOptions;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="textFile"></param>
        /// <returns></returns>
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
        {
            return GlobalOptions;
        }
    }
}
