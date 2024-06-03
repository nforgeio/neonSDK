//-----------------------------------------------------------------------------
// FILE:        IStaticDirectoryExtensions.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ICSharpCode.SharpZipLib.Zip;

using Neon.Common;
using Neon.IO;

namespace Neon.Common
{
    /// <summary>
    /// Enumerates the ZIP options for <see cref="IStaticDirectoryExtensions.ZipAsync(IStaticDirectory, Stream, string, SearchOption, StaticZipOptions, ZipPreprocessor)"/> and
    /// <see cref="IStaticDirectoryExtensions.ZipAsync(IStaticDirectory, string, string, SearchOption, StaticZipOptions, ZipPreprocessor)"/>.  These may be bitwise ORed togther
    /// in various combinations.
    /// </summary>
    [Flags]
    public enum StaticZipOptions
    {
        /// <summary>
        /// No special options required.  This is the default.
        /// </summary>
        None = 0x00000000,

        /// <summary>
        /// <para>
        /// Convert any Windows CRLF line endings into Linux compatiable LF endings.
        /// </para>
        /// <note>
        /// Any line endings are converted <b>before</b> the preprocessor is called, when a
        /// preprocessor is specified.
        /// </note>
        /// </summary>
        LinuxLineEndings = 0x00000001
    }

    /// <summary>
    /// Specifies a preprocessor that can be used to preprocess files before they
    /// are added to a ZIP archive.
    /// </summary>
    /// <param name="path">
    /// Specifies the path of the file being preprocessed relative to the folder being zipped.
    /// Your delegate implementation may decide whether and/or how to preprocess the file based
    /// on this path, often using file name or extension.
    /// </param>
    /// <param name="input">Specifies a stream holding the file data being preprocessed.</param>
    /// <returns>As the processed stream.</returns>
    public delegate Task<Stream> ZipPreprocessor(string path, Stream input);

    /// <summary>
    /// Extension methods for <see cref="IStaticDirectory"/>.
    /// </summary>
    public static class IStaticDirectoryExtensions
    {
        /// <summary>
        /// Creates a ZIP file, including the selected files from the static directory.
        /// </summary>
        /// <param name="directory">The static directory instance.</param>
        /// <param name="zipPath">Path to the output ZIP file.</param>
        /// <param name="searchPattern">
        /// Optionally specifies a file name pattern using standard file system wildcards
        /// like <b>[*]</b> and <b>[?]</b>.  This defaults to including all files.
        /// </param>
        /// <param name="searchOptions">Optionally perform a recursive search.  This defaults to 
        /// <see cref="SearchOption.TopDirectoryOnly"/>.
        /// </param>
        /// <param name="zipOptions">
        /// Additional options that control things like whether the files are zipped within
        /// the parent directory or whether the files are assumed to contain UTF-8 text and
        /// that Windows style CRLF line endings are to be converted to Linux compatible LF 
        /// endings.  You can combine options by bitwise ORing them.  This defaults to
        /// <see cref="StaticZipOptions.None"/>.
        /// </param>
        /// <param name="preprocessor">
        /// Optionally specifies a preprocessor that can modify files before they are
        /// added to the ZIP archive.
        /// </param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <remarks>
        /// <note>
        /// The current implementation loads the files into memory so this isn't really suitable
        /// for zipping very large files.
        /// </note>
        /// </remarks>
        public static async Task ZipAsync(
            this IStaticDirectory   directory, 
            string                  zipPath, 
            string                  searchPattern = null, 
            SearchOption            searchOptions = SearchOption.TopDirectoryOnly,
            StaticZipOptions        zipOptions    = StaticZipOptions.None,
            ZipPreprocessor         preprocessor  = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(zipPath), nameof(zipPath));

            using (var stream = new FileStream(zipPath, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                await ZipAsync(directory, stream, searchPattern, searchOptions, zipOptions, preprocessor);
            }
        }

        /// <summary>
        /// Writes a ZIP file to a stream, including the selected files from the static directory.
        /// </summary>
        /// <param name="directory">The static directory instance.</param>
        /// <param name="zipStream">The output stream.</param>
        /// <param name="searchPattern">
        /// Optionally specifies a file name pattern using standard file system wildcards
        /// like <b>[*]</b> and <b>[?]</b>.  This defaults to including all files.
        /// </param>
        /// <param name="searchOptions">Optionally perform a recursive search.  This defaults to 
        /// <see cref="SearchOption.TopDirectoryOnly"/>.
        /// </param>
        /// <param name="zipOptions">
        /// Additional options that control things like whether the files are zipped within
        /// the parent directory or whether the files are assumed to contain UTF-8 text and
        /// that Windows style CRLF line endings are to be converted to Linux compatible LF 
        /// endings.  You can combine options by bitwise ORing them.  This defaults to
        /// <see cref="StaticZipOptions.None"/>.
        /// </param>
        /// <param name="preprocessor">
        /// Optionally specifies a preprocessor that can modify files before they are
        /// added to the ZIP archive.
        /// </param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <remarks>
        /// <note>
        /// The current implementation loads the files into memory so this isn't really suitable
        /// for zipping very large files.
        /// </note>
        /// </remarks>
        public static async Task ZipAsync(
            this IStaticDirectory   directory, 
            Stream                  zipStream, 
            string                  searchPattern = null,
            SearchOption            searchOptions = SearchOption.TopDirectoryOnly,
            StaticZipOptions        zipOptions    = StaticZipOptions.None,
            ZipPreprocessor         preprocessor  = null)
        {
            Covenant.Requires<ArgumentNullException>(zipStream != null, nameof(zipStream));

            using (var zip = ZipFile.Create(zipStream))
            {
                zip.BeginUpdate();

                foreach (var file in directory.GetFiles(searchPattern, searchOptions))
                {
                    var relativePath = file.Path.Substring(directory.Path.Length + 1);

                    if ((zipOptions | StaticZipOptions.LinuxLineEndings) != 0)
                    {
                        var text = file.ReadAllText(Encoding.UTF8);

                        text = NeonHelper.ToLinuxLineEndings(text);

                        if (preprocessor != null)
                        {
                            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                            {
                                using (var output = await preprocessor(relativePath, input))
                                {
                                    Covenant.Assert(output != null, "Preprocessor returned a NULL stream.");

                                    output.Position = 0;
                                    zip.Add(new StaticBytesDataSource(output.ReadToEnd()), relativePath);
                                }
                            }
                        }
                        else
                        {
                            zip.Add(new StaticBytesDataSource(Encoding.UTF8.GetBytes(text)), relativePath);
                        }
                    }
                    else
                    {
                        if (preprocessor != null)
                        {
                            using (var input = File.OpenRead(file.Path) )
                            {
                                using (var output = await preprocessor(relativePath, input))
                                {
                                    Covenant.Assert(output != null, "Preprocessor returned a NULL stream.");

                                    output.Position = 0;
                                    zip.Add(new StaticBytesDataSource(output.ReadToEnd()), relativePath);
                                }
                            }
                        }
                        else
                        {
                            zip.Add(new StaticBytesDataSource(file.ReadAllBytes()), relativePath);
                        }
                    }
                }

                zip.CommitUpdate();
            }
        }
    }
}
