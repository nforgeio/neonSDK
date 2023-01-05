//-----------------------------------------------------------------------------
// FILE:	    EasyRepoTagApi.cs
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

using LibGit2Sharp;
using LibGit2Sharp.Handlers;

using Octokit;

using GitHubBranch        = Octokit.Branch;
using GitHubRepository    = Octokit.Repository;
using GitHubSignature     = Octokit.Signature;
using GitHubRepositoryTag = Octokit.RepositoryTag;

using GitBranch     = LibGit2Sharp.Branch;
using GitRepository = LibGit2Sharp.Repository;
using GitSignature  = LibGit2Sharp.Signature;

namespace Neon.Git
{
    /// <summary>
    /// Implements friendly GitHub repository tag related APIs.
    /// </summary>
    public class EasyRepoTagApi
    {
        private GitHubRepo  repo;

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="repo">The parent <see cref="GitHubRepo"/>.</param>
        internal EasyRepoTagApi(GitHubRepo repo)
        {
            this.repo = repo;
        }

        /// <summary>
        /// Returns all tags from the GitHub origin repository.
        /// </summary>
        /// <returns>The list of tags.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        public async Task<IReadOnlyList<GitHubRepositoryTag>> GetAsync()
        {
            repo.EnsureNotDisposed();
            repo.EnsureLocalRepo();

            return await repo.GitHubServer.Repository.GetAllTags(repo.OriginRepoPath.Owner, repo.OriginRepoPath.Name);
        }

        /// <summary>
        /// Returns a specific GitHub origin repository tag, if it exists.
        /// </summary>
        /// <param name="tageName">Specifies the origin repository tag name.</param>
        /// <returns>The <see cref="GitHubRepositoryTag"/> or <c>null</c> when the tag doesn't exist.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        public async Task<GitHubRepositoryTag> GetAsync(string tageName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(tageName), nameof(tageName));
            repo.EnsureNotDisposed();
            repo.EnsureLocalRepo();

            return (await GetAsync()).FirstOrDefault(tag => tag.Name.Equals(tageName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Removes an origin repository tag, if it exists.
        /// </summary>
        /// <param name="tagName">Specifies the origin repository tag name.</param>
        /// <returns><c>true</c> if the tag existed and was removed, <c>false</c> otherwise.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        public async Task<bool> RemoveAsync(string tagName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(tagName), nameof(tagName));
            repo.EnsureNotDisposed();
            repo.EnsureLocalRepo();

            var tag = await GetAsync(tagName);

            if (tag == null)
            {
                return false;
            }

            // OctoKit doesn't have a nice tag removal method, so we'll need to use
            // the REST API.  This is a bit tricky:
            //
            //      https://stackoverflow.com/questions/7247414/delete-a-tag-with-github-v3-api

            var uri = $"/repos/{repo.OriginRepoPath.Owner}/{repo.OriginRepoPath.Name}/git/refs/tags/{tagName}";

            NetHelper.EnsureSuccess(await repo.GitHubServer.Connection.Delete(new Uri(uri)));

            return true;
        }
    }
}
