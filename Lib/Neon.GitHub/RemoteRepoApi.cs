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
using Neon.Collections;

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
        private string      cachedApiBaseUri;
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
        /// Returns the remote repo owner.
        /// </summary>
        public string Owner => Path.Owner;

        /// <summary>
        /// Returns the remote repo name.
        /// </summary>
        public string Name => Path.Name;

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

                // We need to strip off the last segment of the URI (the "NAME-git" part).

                var uri          = $"https://{root.Remote.Path}";
                var lastSlashPos = uri.LastIndexOf('/');

                Covenant.Assert(lastSlashPos > 0);

                return cachedBaseUri = uri.Substring(0, lastSlashPos + 1);
            }
        }

        /// <summary>
        /// Returns a URI for the GitHub API server by combining the GitHub API endpoint
        /// with the relative path passed.
        /// </summary>
        /// <param name="path">
        /// Specifies the relative path.  This may or may not include a leading forward
        /// slash (/).
        /// </param>
        /// <returns>The API URI.</returns>
        public string GetApiUri(string path)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path), nameof(path));

            if (cachedApiBaseUri == null)
            {
                // $hack(jefflill):
                //
                // This code assumes that the server hostname should always start with "api.",
                // which will be true for GitHub but perhaps not for privately hosted enterprise
                // GitHub deployments.  We're not going to worry about that right now.

                var serverPart = root.Remote.Path.Server;

                if (!serverPart.StartsWith("api.", StringComparison.CurrentCultureIgnoreCase))
                {
                    serverPart = $"api.{serverPart}";
                }

                cachedApiBaseUri = $"https://{serverPart}";
            }

            if (path.StartsWith('/'))
            {
                return cachedApiBaseUri + path;
            }
            else
            {
                return cachedApiBaseUri + '/' + path;
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

        /// <summary>
        /// Returns the commits for a named branch in decending order by date/time..
        /// </summary>
        /// <param name="branch">Specifies the branch name.</param>
        /// <param name="since">Optionally specifies that only commits <b>before</b> the date/time are to be returned.</param>
        /// <param name="until">Optionally specifies that only commits <b>after</b> the date/time are to be returned.</param>
        /// <returns>The commits in decending order by date/time.</returns>
        public async Task<IEnumerable<GitHubCommit>> GetCommitsAsync(string branch, DateTimeOffset? since = null, DateTimeOffset? until = null)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branch), nameof(branch));

            return await root.GitHubApi.Repository.Commit.GetAll(root.Remote.Id,
                new CommitRequest()
                {
                    Sha   = branch,
                    Since = since,
                    Until = until
                });
        }

        /// <summary>
        /// Merges changes to the current forked repo from the upstream source repo.
        /// </summary>
        /// <param name="branch">Identifies the local branch to be merged.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public async Task MergeUpstreamAsync(string branch)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branch), nameof(branch));

            var uri     = root.Remote.GetApiUri("/merge-upstream");
            var content = new StringContent($"{{\"branch\":\"{branch}\"}}", Encoding.UTF8);
            var headers = new ArgDictionary() { { "Accept", GitHubRepo.AcceptMediaType } };

            await root.HttpClient.PostSafeAsync(uri, content, headers);
        }
    }
}
