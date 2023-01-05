//-----------------------------------------------------------------------------
// FILE:	    RemoteRepoApi.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Deployment;

using LibGit2Sharp;
using LibGit2Sharp.Handlers;

using Octokit;

using GitHubBranch     = Octokit.Branch;
using GitHubRepository = Octokit.Repository;
using GitHubSignature  = Octokit.Signature;

using GitBranch     = LibGit2Sharp.Branch;
using GitRepository = LibGit2Sharp.Repository;
using GitSignature  = LibGit2Sharp.Signature;

namespace Neon.Git
{
    /// <summary>
    /// Implements easy-to-use remote GitHub repository related APIs.
    /// </summary>
    public class RemoteRepoApi
    {
        private GitHubRepo  root;

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="root">The root <see cref="GitHubRepo"/>.</param>
        internal RemoteRepoApi(GitHubRepo root)
        {
            Covenant.Requires<ArgumentNullException>(root != null, nameof(root));

            this.root    = root;
            this.Branch  = new RemoteRepoBranchApi(root);
            this.Release = new RemoteRepoReleaseApi(root);
        }

        /// <summary>
        /// Returns the current <see cref="GitHubRepository"/> for the orgin repository
        /// assocated with the parent <see cref="GitHubRepo"/> instance.
        /// </summary>
        /// <returns>The associated <see cref="GitHubRepository"/>.</returns>
        public async Task<GitHubRepository> GetAsync()
        {
            return await root.GitHubApi.Repository.Get(root.RemoteRepoPath.Owner, root.RemoteRepoPath.Name);
        }

        /// <summary>
        /// Returns the friendly GitHub branch related APIs.
        /// </summary>
        public RemoteRepoBranchApi Branch { get; private set; }

        /// <summary>
        /// Returns the friendly GitHub release related APIs.
        /// </summary>
        public RemoteRepoReleaseApi Release { get; private set; }
    }
}
