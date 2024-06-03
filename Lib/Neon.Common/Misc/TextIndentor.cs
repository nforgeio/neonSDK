// -----------------------------------------------------------------------------
// FILE:	    TextIndentor.cs
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
    public class TextIndentor
    {
        private StringBuilder   sb;
        private TextWriter      writer;
        private string          indentString;
        private int             indentLevel;
        private bool            emptyLine = true;

        /// <summary>
        /// Constructs and instance that outputs to a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="builder">Specifies the target <see cref="StringBuilder"/>.</param>
        /// <param name="indentWidth">Optionally specifies the number of spaces to indent for each indentation level.  This defaults to <b>4</b>.</param>
        /// <param name="indentChar">Optionally specifies the indent character (defaults to a space).</param>
        /// <param name="indentLevel">Optionally specifies the initial indent level.  This defaults to <b>0</b>.</param>
        public TextIndentor(StringBuilder builder, int indentWidth = 4, char indentChar = ' ', int indentLevel = 0)
        {
            Covenant.Requires<ArgumentNullException>(builder != null, nameof(builder));
            Covenant.Requires<ArgumentException>(indentWidth > 0, nameof(indentWidth));
            Covenant.Requires<ArgumentException>(indentLevel >= 0, nameof(indentLevel));

            this.sb           = builder;
            this.indentString = new string(indentChar, indentWidth);
            this.indentLevel  = indentLevel;
        }

        /// <summary>
        /// Constructs and instance that outputs to a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">Specifies the target <see cref="TextWriter"/>.</param>
        /// <param name="indentWidth">Optionally specifies the number of spaces to indent for each indentation level.  This defaults to <b>4</b>.</param>
        /// <param name="indentChar">Optionally specifies the indent character (defaults to a space).</param>
        /// <param name="indentLevel">Optionally specifies the initial indent level.  This defaults to <b>0</b>.</param>
        public TextIndentor(TextWriter writer, int indentWidth = 4, char indentChar = ' ', int indentLevel = 0)
        {
            Covenant.Requires<ArgumentNullException>(writer != null, nameof(writer));
            Covenant.Requires<ArgumentException>(indentWidth > 0, nameof(indentWidth));
            Covenant.Requires<ArgumentException>(indentLevel >= 0, nameof(indentLevel));

            this.writer       = writer;
            this.indentString = new string(indentChar, indentWidth);
            this.indentLevel  = indentLevel;
        }

        /// <summary>
        /// Used internally to append text to the the output.
        /// </summary>
        /// <param name="text">Specifies the text to be appended.</param>
        private void InternalAppend(string text)
        {
            if (sb != null)
            {
                sb.Append(text);
            }
            else
            {
                writer.Write(text);
            }
        }

        /// <summary>
        /// Appends the indentation for the the current level when the
        /// text passed is not <c>null</c> or empty and the current
        /// line is empty.
        /// </summary>
        /// <param name="text">Specifies the text that will (eventually) be written ort <c>null</c>.</param>
        private void InternalAppendIndent(string text)
        {
            if (!string.IsNullOrEmpty(text) && emptyLine)
            {
                for (int i = 0; i < indentLevel; i++)
                {
                    InternalAppend(indentString);
                }

                emptyLine = false;
            }
        }

        /// <summary>
        /// Appends text to the the output, writing the current indent if this
        /// is the first text being written to the current line.
        /// </summary>
        /// <param name="text">Specifies the text to be written.</param>
        public void Append(string text)
        {
            Covenant.Requires<ArgumentNullException>(text != null, nameof(text));

            InternalAppendIndent(text);
            InternalAppend(text ?? string.Empty);
        }

        /// <summary>
        /// Writes a line of text to the target prefixed by the indentation.
        /// </summary>
        /// <param name="text">Optionally specifies the text to be written.</param>
        public void AppendLine(string text = null)
        {
            InternalAppendIndent(text);
            InternalAppend(text);
            InternalAppend(Environment.NewLine);

            emptyLine = true;
        }

        /// <summary>
        /// Resets the current indentation level zero.
        /// </summary>
        public void Reset()
        {
            indentLevel = 0;
        }

        /// <summary>
        /// Increments the indentation level.
        /// </summary>
        public void Indent()
        {
            indentLevel++;
        }

        /// <summary>
        /// Decrements the indentation level.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the indentation level is already at zero.</exception>
        public void UnIndent()
        {
            if (indentLevel <= 0)
            {
                throw new InvalidOperationException("Indentation level is already at zero.");
            }

            indentLevel--;
        }
    }
}
