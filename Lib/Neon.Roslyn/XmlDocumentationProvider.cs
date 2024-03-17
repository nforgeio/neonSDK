// -----------------------------------------------------------------------------
// FILE:	    XmlDocumentationProvider.cs
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
using System.Xml;
using System.Xml.Linq;

using Microsoft.CodeAnalysis;

using Roslyn.Utilities;

namespace Neon.Roslyn
{

    /// <summary>
    /// Provides XML documentation for symbols in a compilation.
    /// </summary>
    public class XmlDocumentationProvider
    {
        private readonly NonReentrantLock _gate = new();
        private Dictionary<string, Dictionary<string, string>> _docComments = new Dictionary<string, Dictionary<string, string>>();
        private List<string> docFiles = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlDocumentationProvider"/> class.
        /// </summary>
        /// <param name="compilation">The compilation to provide documentation for.</param>
        public XmlDocumentationProvider(Compilation compilation)
        {
            foreach (var extRef in compilation.ExternalReferences)
            {
                AddFile(extRef.Display);
            }
        }

        /// <summary>
        /// Adds a documentation file to the provider.
        /// </summary>
        /// <param name="filePath">The path of the documentation file.</param>
        public void AddFile(string filePath)
        {
            docFiles.Add(GetXmlFilePath(filePath));
        }

        /// <summary>
        /// Adds an XDocument to the provider.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly.</param>
        /// <param name="stream">The stream containing the XDocument.</param>
        public void AddXDocument(string assemblyName, Stream stream)
        {
            if (!_docComments.ContainsKey(assemblyName))
            {
                _docComments[assemblyName] = new Dictionary<string, string>();
            }

            try
            {
                var doc = GetXDocument(stream);

                foreach (var e in doc.Descendants("member"))
                {
                    if (e.Attribute("name") != null)
                        _docComments[assemblyName][e.Attribute("name").Value] = e.ToString();
                }
            }
            catch
            {
                return;
            }
        }

        private XDocument GetXDocument(Stream stream)
        {
            using var xmlReader = XmlReader.Create(stream, s_xmlSettings);
            return XDocument.Load(xmlReader);
        }

        /// <summary>
        /// Gets the documentation for a symbol.
        /// </summary>
        /// <param name="symbol">The symbol to get the documentation for.</param>
        /// <returns>The documentation for the symbol.</returns>
        public string GetDocumentationForSymbol(ISymbol symbol)
        {
            if (symbol == null)
            {
                throw new ArgumentNullException(nameof(symbol));
            }

            var documentationMemberID = symbol.GetDocumentationCommentId();

            if (!_docComments.TryGetValue(symbol.ContainingModule.Name, out var assemblyDocComments))
            {
                var xmlPath = GetXmlFilePath(symbol.ContainingModule.Name);

                var filePath = docFiles.Where(x => x.EndsWith(xmlPath)).FirstOrDefault();

                if (!File.Exists(filePath))
                {
                    return string.Empty;
                }

                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                AddXDocument(symbol.ContainingModule.Name, stream);

                assemblyDocComments = _docComments[symbol.ContainingModule.Name];
            }

            return assemblyDocComments.TryGetValue(documentationMemberID, out var docComment) ? docComment : string.Empty;
        }

        private static readonly XmlReaderSettings s_xmlSettings = new()
        {
            DtdProcessing = DtdProcessing.Prohibit,
        };

        private string GetXmlFilePath(string assemblyPath)
        {
            return assemblyPath.Replace(".dll", ".xml");
        }
    }
}
