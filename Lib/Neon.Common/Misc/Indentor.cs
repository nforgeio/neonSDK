// -----------------------------------------------------------------------------
// FILE:	    Indentor.cs
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
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;

namespace Neon.Common
{
    /// <summary>
    /// Implements a simple scheme for managing line indentation when writing
    /// to a <see cref="StringBuilder"/> or <see cref="TextWriter"/>.
    /// </summary>
    public class Indentor
    {
        private StringBuilder   sb;
        private TextWriter      writer;
        private string          indentSpaces;
        private int             indent;

        /// <summary>
        /// Constructs and instance that outputs to a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="builder">Specifies the target <see cref="StringBuilder"/>.</param>
        /// <param name="indentSpaces">Optionally specifies the number of spaces to indent for each indentation level.  This defaults to <b>4</b>.</param>
        /// <param name="indent">Optionally specifies the initial indent level.  This defaults to <b>0</b>.</param>
        public Indentor(StringBuilder builder, int indentSpaces = 4, int indent = 0)
        {
            Covenant.Requires<ArgumentNullException>(builder != null, nameof(builder));
            Covenant.Requires<ArgumentException>(indentSpaces > 0, nameof(indentSpaces));
            Covenant.Requires<ArgumentException>(indent >= 0, nameof(indent));

            this.sb           = builder;
            this.indentSpaces = new string(' ', indentSpaces);
            this.indent       = indent;
        }

        /// <summary>
        /// Constructs and instance that outputs to a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">Specifies the target <see cref="TextWriter"/>.</param>
        /// <param name="indentSpaces">Optionally specifies the number of spaces to indent for each indentation level.  This defaults to <b>4</b>.</param>
        /// <param name="indent">Optionally specifies the initial indent level.  This defaults to <b>0</b>.</param>
        public Indentor(TextWriter writer, int indentSpaces = 4, int indent = 0)
        {
            Covenant.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Covenant.Requires<ArgumentException>(indentSpaces > 0, nameof(indentSpaces));
            Covenant.Requires<ArgumentException>(indent >= 0, nameof(indent));

            this.writer       = writer;
            this.indentSpaces = new string(' ', indentSpaces);
            this.indent       = indent;
        }

        /// <summary>
        /// Writes a line of text to the target prefixed by the indentation.
        /// </summary>
        /// <param name="text">Optionally specifies the text line.</param>
        public void WriteLine(string text = null)
        {
            if (sb != null)
            {
                for (int i = 0; i < indent; i++)
                {
                    sb.Append(indentSpaces);
                }

                sb.AppendLine(text);
            }
            else
            {
                for (int i = 0; i < indent; i++)
                {
                    writer.Write(indentSpaces);
                }

                writer.WriteLine(text);
            }
        }

        /// <summary>
        /// Increments the indentation level.
        /// </summary>
        public void Indent()
        {
            indent++;
        }

        /// <summary>
        /// Decrements the indentation level.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the indentation level is already at zero.</exception>
        public void UnIndent()
        {
            if (indent <= 0)
            {
                throw new InvalidOperationException("Indentation level is already at zero.");
            }

            indent--;
        }
    }
}
