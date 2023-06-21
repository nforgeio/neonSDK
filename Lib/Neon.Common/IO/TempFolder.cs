//-----------------------------------------------------------------------------
// FILE:        TempFolder.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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

using Neon.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Neon.IO
{
    /// <summary>
    /// Manages a temporary file system folder to be used for the duration of a unit test.
    /// </summary>
    public sealed class TempFolder : IDisposable
    {
        //---------------------------------------------------------------------
        // Static members

        /// <summary>
        /// Optionally specifies the root directory where the temporary folders will
        /// be created.  This defaults to <see cref="System.IO.Path.GetTempPath()"/>
        /// when this is <c>null</c> or empty.
        /// </summary>
        public static string Root { get; set; }

        //---------------------------------------------------------------------
        // Instance members

        /// <summary>
        /// Creates a temporary folder.
        /// </summary>
        /// <param name="folder">Optionally overrides <see cref="Root"/> as the parent folder for this instance.</param>
        /// <param name="prefix">Optionally specifies a prefix to be added to the temporary directory name.</param>
        /// <param name="create">Optionally controls whether the temporary folder should actuall be created.  This defaults to <c>true</c>.</param>
        public TempFolder(string folder = null, string prefix = null, bool create = true)
        {
            prefix ??= string.Empty;

            if (string.IsNullOrEmpty(folder))
            {
                folder = Root;
            }

            if (string.IsNullOrEmpty(folder))
            {
                Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{prefix}{Guid.NewGuid()}");
            }
            else
            {
                Directory.CreateDirectory(folder);

                Path = System.IO.Path.Combine(folder, Guid.NewGuid().ToString());
            }

            if (create)
            {
                Directory.CreateDirectory(Path);
            }
        }

        /// <summary>
        /// Returns the fully qualifed path to the temporary folder.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Deletes the temporary folder and all of its contents.
        /// </summary>
        public void Dispose()
        {
            if (Path != null && Directory.Exists(Path))
            {
                try
                {
                    NeonHelper.DeleteFolderContents(Path);
                    Directory.Delete(Path, recursive: true);

                    Path = null;
                }
                catch (IOException)
                {
                    // We're going to ignore any I/O errors.
                }
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Path;
        }
    }
}
