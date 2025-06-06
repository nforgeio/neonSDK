//-----------------------------------------------------------------------------
// FILE:        HostFolders.cs
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Neon.Common;

using Renci.SshNet;

namespace Neon.SSH
{
    /// <summary>
    /// Enumerates the paths of important directories on cluster 
    /// host servers.
    /// </summary>
    /// <remarks>
    /// <note>
    /// Although these constants are referenced by C# code, Linux scripts 
    /// are likely to hardcode these strings.  You should do a search and
    /// replace whenever you change any of these values.
    /// </note>
    /// <note>
    /// Changing any of these will likely break [NeonCLIENT] interactions
    /// with existing clusters that use the previous folder path.  Be
    /// ver sure you know what you're doing when you make changes.
    /// </note>
    /// </remarks>
    public static class HostFolders
    {
        /// <summary>
        /// Path to a user's <b>neon</b> home directory.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The directory path.</returns>
        public static string NeonHome(string username) => $"/home/{username}/.neon";

        /// <summary>
        /// Path to the user download directory.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The directory path.</returns>
        public static string Download(string username) => $"{NeonHome(username)}/download";

        /// <summary>
        /// The user folder where cluster tools can upload, unpack, and then
        /// execute <see cref="CommandBundle"/>s as well as store temporary
        /// command output files.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The directory path.</returns>
        public static string Exec(string username) => $"{NeonHome(username)}/exec";

        /// <summary>
        /// Path to a user archive directory.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The directory path.</returns>
        public static string Home(string username) => $"/home/{username}";

        /// <summary>
        /// Root folder on the local tmpfs (shared memory) folder where 
        /// cluster will persist misc temporary files.
        /// </summary>
        public const string Tmpfs = "/dev/shm/neonssh";

        /// <summary>
        /// Path to a user upload directory.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The directory path.</returns>
        public static string Upload(string username) => $"{NeonHome(username)}/upload";
    }
}
