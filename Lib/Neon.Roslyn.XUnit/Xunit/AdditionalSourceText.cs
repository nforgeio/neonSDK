// -----------------------------------------------------------------------------
// FILE:	    AdditionalText.cs
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.IO;


using System.Diagnostics;
using Roslyn.Utilities;

namespace Neon.Roslyn.Xunit
{
    public class AdditionalSourceText : AdditionalText
    {
        private readonly SourceText text;

        public AdditionalSourceText(string path, SourceText? text)
        {
            Path = path;
            this.text = text;
        }

        public AdditionalSourceText(string text = "", Encoding encoding = null, string path = "dummy", SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1)
            : this(path, SourceText.From(text, encoding, checksumAlgorithm: checksumAlgorithm))
        {
        }

        public override string Path { get; }

        public override SourceText GetText(CancellationToken cancellationToken = default) => text;

        public static AdditionalSourceText FromFile(string path)
        {
            return new AdditionalSourceText(File.ReadAllText(path));
        }

        public static AdditionalSourceText FromString(string text)
        {
            return new AdditionalSourceText(text);
        }
    }
}
