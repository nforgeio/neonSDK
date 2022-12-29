//-----------------------------------------------------------------------------
// FILE:        GitTestHelper.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using LibGit2Sharp;

using Neon.Common;
using Neon.Git;
using Neon.IO;
using Neon.Xunit;

using Xunit;

namespace TestGit
{
    /// <summary>
    /// Git testing helpers.
    /// </summary>
    public static class GitTestHelper
    {
        /// <summary>
        /// Identifies the GitHub repo we'll use for testing the <b>Neon.Git</b> library.
        /// </summary>
        public const string RemoteTestRepo = "neontest/neon-git";

        /// <summary>
        /// Ensures that the current user appears to be a NTONFORGE maintainer.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the current user doesn't appear to be a maintainer.</exception>
        public static void EnsureMaintainer()
        {
            // We're expecting maintainers to have environment variables holding
            // the GitHub credential components.

            if (NeonHelper.IsMaintainer)
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_USERNAME")) ||
                    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_PAT")) ||
                    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_EMAIL")))
                {
                    throw new InvalidOperationException("NEONFORGE maintainers are expected to have these environment variables: GITHUB_USERNAME, GITHUB_PAT, and GITHUB_EMAIL");
                }
            }
        }
    }
}
