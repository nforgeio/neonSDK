//-----------------------------------------------------------------------------
// FILE:        RemoteRepoPath.cs
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;

namespace Neon.GitHub
{
    /// <summary>
    /// <para>
    /// Abstracts GitHub origin repository paths formatted like: <b>[SERVER/]OWNER/REPO</b>.
    /// </para>
    /// <para>
    /// Examples: <b>github.com/owner/repo</b> or <b>owner/repo</b> (where <b>github.com</b> 
    /// is the implied server).
    /// </para>
    /// </summary>
    public class RemoteRepoPath
    {
        //---------------------------------------------------------------------
        // Static members

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="v1">Specifies the first value.</param>
        /// <param name="v2">Specifies the second value.</param>
        /// <returns><c>true</c> when the valuea are the same.</returns>
        public static bool operator ==(RemoteRepoPath v1, RemoteRepoPath v2)
        {
            if ((v1 is null) != (v2 is null))
            {
                return false;
            }

            if (v1 is not null)
            {
                return v1.Equals(v2);
            }
            else
            {
                return v2.Equals(v1);
            }
        }

        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="v1">Specifies the first value.</param>
        /// <param name="v2">Specifies the second value.</param>
        /// <returns><c>true</c> when the valuea are different.</returns>
        public static bool operator !=(RemoteRepoPath v1, RemoteRepoPath v2)
        {
            return !(v1 == v2);
        }

        /// <summary>
        /// Parses a GitHub repository path.
        /// </summary>
        /// <param name="path">The path, like: <b>[SERVER/]OWNER/REPO</b></param>
        /// <returns>The parsed <see cref="RemoteRepoPath"/>.</returns>
        /// <exception cref="FormatException">Thrown when the input is invalid.</exception>
        /// <remarks>
        /// <note>
        /// <b>github.com</b> will be assumed when no server is specified.
        /// </note>
        /// </remarks>
        public static RemoteRepoPath Parse(string path)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path), nameof(path));

            var parts = path.Split('/');

            foreach (var part in parts)
            {
                if (part.Length == 0 || part.Contains(' '))
                {
                    throw new FormatException($"Invalid GitHub repository path: {path}");
                }
            }

            var repoPath = new RemoteRepoPath();

            switch (parts.Length)
            {
                case 2:

                    repoPath.Server = "github.com";
                    repoPath.Owner  = parts[0];
                    repoPath.Name   = parts[1];
                    break;

                case 3:

                    repoPath.Server = parts[0];
                    repoPath.Owner  = parts[1];
                    repoPath.Name   = parts[2];
                    break;

                default:

                    throw new FormatException($"Invalid GitHub repository path: {path}");
            }

            // Check for blank parts.

            if (repoPath.Server == string.Empty ||
                repoPath.Owner == string.Empty ||
                repoPath.Name == string.Empty)
            {
                throw new FormatException($"Invalid GitHub repository path: {path}");
            }

            return repoPath;
        }

        //---------------------------------------------------------------------
        // Instance members

        /// <summary>
        /// Static constructor.
        /// </summary>
        private RemoteRepoPath()
        {
        }

        /// <summary>
        /// Returns the <b>server</b> part of the path.
        /// </summary>
        public string Server { get; private set; }

        /// <summary>
        /// Returns the <b>owner</b> part of the path.
        /// </summary>
        public string Owner { get; private set; }

        /// <summary>
        /// Returns the name of the repository.
        /// </summary>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Server}/{Owner}/{Name}";
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj is RemoteRepoPath other)
            {
                return this.ToString() == other.ToString();
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
