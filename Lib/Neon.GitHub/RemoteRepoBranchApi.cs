//-----------------------------------------------------------------------------
// FILE:	    RemoteRepoBranchApi.cs
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
using Neon.Net;
using Neon.Tasks;

using LibGit2Sharp;
using LibGit2Sharp.Handlers;

using Octokit;

using GitHubBranch     = Octokit.Branch;
using GitHubRepository = Octokit.Repository;
using GitHubSignature  = Octokit.Signature;

using GitBranch     = LibGit2Sharp.Branch;
using GitRepository = LibGit2Sharp.Repository;
using GitSignature  = LibGit2Sharp.Signature;

namespace Neon.GitHub
{
    /// <summary>
    /// Implements friendly GitHub repository branch related APIs.
    /// </summary>
    public class RemoteRepoBranchApi
    {
        private GitHubRepo  root;

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="root">The root <see cref="GitHubRepo"/>.</param>
        internal RemoteRepoBranchApi(GitHubRepo root)
        {
            this.root = root;
        }

        /// <summary>
        /// Returns all branches from the GitHub origin repository.
        /// </summary>
        /// <returns>The list of branches.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        public async Task<IReadOnlyList<GitHubBranch>> GetAllAsync()
        {
            await SyncContext.Clear;
            root.EnsureNotDisposed();

            return await root.GitHubApi.Repository.Branch.GetAll(root.Remote.Id);
        }

        /// <summary>
        /// Returns a specific GitHub origin repository branch.
        /// </summary>
        /// <param name="branchName">Specifies the origin repository branch name.</param>
        /// <returns>The requested <see cref="GitHubBranch"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="Octokit.NotFoundException">Thrown when the branch does not exist.</exception>
        public async Task<GitHubBranch> GetAsync(string branchName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            root.EnsureNotDisposed();

            return await root.GitHubApi.Repository.Branch.Get(root.Remote.Id, branchName);
        }

        /// <summary>
        /// Searches for a specific GitHub origin repository branch.
        /// </summary>
        /// <param name="branchName">Specifies the origin repository branch name.</param>
        /// <returns>The requested <see cref="GitHubBranch"/> or <c>null</c> when it doesn't exist.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="Octokit.NotFoundException">Thrown when the branch does not exist.</exception>
        public async Task<GitHubBranch> FindAsync(string branchName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            root.EnsureNotDisposed();

            try
            {
                return await root.GitHubApi.Repository.Branch.Get(root.Remote.Id, branchName);
            }
            catch (Octokit.NotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Removes an origin branch, if it exists.
        /// </summary>
        /// <param name="branchName">Specifies the origin repository branch name.</param>
        /// <returns><c>true</c> if the branch existed and was removed, <c>false</c> otherwise.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        public async Task<bool> RemoveAsync(string branchName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            root.EnsureNotDisposed();

            var branch = await FindAsync(branchName);

            if (branch == null)
            {
                return false;
            }

            // OctoKit doesn't have a nice branch removal method, so we'll need to use
            // the REST API.  This is a bit tricky:
            //
            //      https://github.com/orgs/community/discussions/24603

            var uri = $"/repos/{root.Remote.Path.Owner}/{root.Remote.Path.Name}/git/heads/{branchName}";

            NetHelper.EnsureSuccess(await root.GitHubApi.Connection.Delete(new Uri(uri)));

            return true;
        }
    }
}
