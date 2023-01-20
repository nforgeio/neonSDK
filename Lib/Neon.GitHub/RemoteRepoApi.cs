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
    /// Implements easy-to-use remote GitHub repository related APIs.
    /// </summary>
    public class RemoteRepoApi
    {
        //---------------------------------------------------------------------
        // Static members

        /// <summary>
        /// Creates a <see cref="GitHubRepo"/> that references and existing remote
        /// GitHub repo.
        /// </summary>
        /// <param name="root">Specifies the root <see cref="GitHubRepo"/>.</param>
        /// <param name="path">Specifies the GitHub path for the repository.</param>
        /// <returns>The <see cref="GitHubRepo"/> instance.</returns>
        internal static async Task<RemoteRepoApi> CreateAsync(GitHubRepo root, RemoteRepoPath path)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(root != null, nameof(root));
            Covenant.Requires<ArgumentNullException>(path != null, nameof(path));

            var repoApi = new RemoteRepoApi();

            repoApi.root    = root;
            repoApi.Path    = path;
            repoApi.Branch  = new RemoteRepoBranchApi(root);
            repoApi.Issue   = new RemoteRepoIssueApi(root);
            repoApi.Release = new RemoteRepoReleaseApi(root);
            repoApi.Tag     = new RemoteRepoTagApi(root);

            return repoApi;
        }

        //---------------------------------------------------------------------
        // Instance members

        private GitHubRepo  root;
        private string      cachedBaseUri;
        private long        cachedId = -1;

        /// <summary>
        /// Internal constructor.
        /// </summary>
        internal RemoteRepoApi()
        {
        }

        /// <summary>
        /// Returns the remote repository's ID.
        /// </summary>
        public long Id
        {
            get
            {
                if (cachedId != -1)
                {
                    return cachedId;
                }

                return cachedId = root.GitHubApi.Repository.Get(Path.Owner, Path.Name).Result.Id;
            }
        }

        /// <summary>
        /// Returns the GitHub repository path.
        /// </summary>
        public RemoteRepoPath Path { get; private set; }

        /// <summary>
        /// <para>
        /// Returns the base URI for the repository on GitHub.
        /// </para>
        /// <note>
        /// This includes the trailing slash.
        /// </note>
        /// </summary>
        public string BaseUri
        {
            get
            {
                if (cachedBaseUri != null)
                {
                    return cachedBaseUri;
                }

                // We need to strip off the last segment of the URI
                // (the "NAME-git" part).

                var uri          = $"https://{root.Remote.Path}";
                var lastSlashPos = uri.LastIndexOf('/');

                Covenant.Assert(lastSlashPos > 0);

                return cachedBaseUri = uri.Substring(0, lastSlashPos + 1);
            }
        }

        /// <summary>
        /// Returns the current <see cref="GitHubRepository"/> for the orgin repository
        /// assocated with the parent <see cref="GitHubRepo"/> instance.
        /// </summary>
        /// <returns>The associated <see cref="GitHubRepository"/>.</returns>
        public async Task<GitHubRepository> GetAsync()
        {
            await SyncContext.Clear;

            return await root.GitHubApi.Repository.Get(Id);
        }

        /// <summary>
        /// Returns the friendly GitHub branch related APIs.
        /// </summary>
        public RemoteRepoBranchApi Branch { get; private set; }

        /// <summary>
        /// Returns the friendly GitHub issue related APIs.
        /// </summary>
        public RemoteRepoIssueApi Issue { get; private set; }

        /// <summary>
        /// Returns the friendly GitHub release related APIs.
        /// </summary>
        public RemoteRepoReleaseApi Release { get; private set; }

        /// <summary>
        /// Returns the friendly GitHub release related APIs.
        /// </summary>
        public RemoteRepoTagApi Tag { get; private set; }

        /// <summary>
        /// Creates a <see cref="GitSignature"/> from the repository's credentials.
        /// </summary>
        /// <returns>The new <see cref="Committer"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the instance is disposed.</exception>
        public Committer CreateComitter()
        {
            root.EnsureNotDisposed();

            return new Committer(root.Credentials.Username, root.Credentials.Email, DateTimeOffset.Now);
        }
    }
}
