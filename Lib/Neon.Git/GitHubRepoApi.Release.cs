//-----------------------------------------------------------------------------
// FILE:	    GitHubRepoApi.Release.cs
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
    public partial class GitHubRepoApi
    {
        /// <summary>
        /// Creates a GitHub release.
        /// </summary>
        /// <param name="tagName">Specifies the release name.</param>
        /// <param name="releaseName">Optionally specifies the release name (defaults to <paramref name="tagName"/>).</param>
        /// <param name="body">Optionally specifies the markdown formatted release notes.</param>
        /// <param name="draft">Optionally indicates that the release won't be published immediately.</param>
        /// <param name="prerelease">Optionally indicates that the release is not production ready.</param>
        /// <returns>The new release.</returns>
        public async Task<Release> CreateRelease(string tagName, string releaseName = null, string body = null, bool draft = false, bool prerelease = false)
        {
            repo.EnsureNotDisposed();

            releaseName ??= tagName;

            var release = new NewRelease(tagName)
            {
                Name       = releaseName,
                Draft      = draft,
                Prerelease = prerelease,
                Body       = body
            };

            var newRelease = await repo.GitHubServer.Repository.Release.Create(repo.OriginRepoPath.Owner, repo.OriginRepoPath.Name, release);

            // GitHub doesn't appear to create releases synchronously, so we're going
            // to wait for the new release to show up.

            await repo.WaitForGitHubAsync(
                async () =>
                {
                    return await repo.OriginRepoApi.GetReleaseAsync(releaseName) != null;
                });

            return newRelease;
        }

        /// <summary>
        /// Returns all releases from the GitHub origin repository.
        /// </summary>
        /// <returns>The list of releases.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<IReadOnlyList<Release>> GetReleasesAsync()
        {
            repo.EnsureNotDisposed();

            return await repo.GitHubServer.Repository.Release.GetAll(repo.OriginRepoPath.Owner, repo.OriginRepoPath.Name);
        }

        /// <summary>
        /// Returns a specific GitHub origin repository release.
        /// </summary>
        /// <param name="releaseName">Specifies the origin repository release name.</param>
        /// <returns>The <see cref="Octokit.Release"/> or <c>null</c> when the release doesn't exist.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<Octokit.Release> GetReleaseAsync(string releaseName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(releaseName), nameof(releaseName));
            repo.EnsureNotDisposed();

            return (await GetReleasesAsync()).FirstOrDefault(release => release.Name.Equals(releaseName, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Removes a GitHub release if it exists,
        /// </summary>
        /// <param name="releaseName">Specifies the release name.</param>
        /// <returns><c>true</c> when the release existed and was removed, <c>false</c> otherwise.</returns>
        public async Task<bool> RemoveReleaseAsync(string releaseName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(releaseName), nameof(releaseName));
            repo.EnsureNotDisposed();

            var release = await GetReleaseAsync(releaseName);

            if (release == null)
            {
                return false;
            }

            await repo.GitHubServer.Repository.Release.Delete(repo.OriginRepoPath.Owner, repo.OriginRepoPath.Name, release.Id);

            return true;
        }
    }
}
