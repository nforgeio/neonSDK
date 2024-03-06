// -----------------------------------------------------------------------------
// FILE:	    AdditionalSourceText.cs
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

using System.IO;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Neon.Roslyn.Xunit
{
    /// <summary>
    /// Represents an additional source text to be used in a test.
    /// </summary>
    public class AdditionalSourceText : AdditionalText
    {
        private readonly SourceText text;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="text"></param>
        public AdditionalSourceText(string path, SourceText? text)
        {
            Path = path;
            this.text = text;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="encoding"></param>
        /// <param name="path"></param>
        /// <param name="checksumAlgorithm"></param>
        public AdditionalSourceText(string text = "", Encoding encoding = null, string path = "dummy", SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1)
            : this(path, SourceText.From(text, encoding, checksumAlgorithm: checksumAlgorithm))
        {
        }

        /// <summary>
        /// The path to the source text.
        /// </summary>
        public override string Path { get; }

        /// <summary>
        /// Returns the source text.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override SourceText GetText(CancellationToken cancellationToken = default) => text;

        /// <summary>
        /// Helper method to get the source text from a file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static AdditionalSourceText FromFile(string path)
        {
            return new AdditionalSourceText(File.ReadAllText(path));
        }

        /// <summary>
        /// Helper method to get the source text from a string.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static AdditionalSourceText FromString(string text)
        {
            return new AdditionalSourceText(text);
        }
    }
}
