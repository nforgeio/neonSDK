//-----------------------------------------------------------------------------
// FILE:        RemoteRepoIssueApi.cs
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
using Neon.Cryptography;
using Neon.Deployment;
using Neon.IO;
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
    /// Implements the friendly GitHub repository issue related APIs.
    /// </summary>
    public class RemoteRepoIssueApi
    {
        private GitHubRepo root;

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="root">The root <see cref="GitHubRepo"/>.</param>
        internal RemoteRepoIssueApi(GitHubRepo root)
        {
            Covenant.Requires<ArgumentNullException>(root != null, nameof(root));

            this.root    = root;
            this.Comment = new RemoteRepoIssueCommentApi(root);
        }

        /// <summary>
        /// Returns the <see cref="RemoteRepoIssueCommentApi"/> which can be used to
        /// manage issue comments.
        /// </summary>
        public RemoteRepoIssueCommentApi Comment { get; private set; }

        /// <summary>
        /// Creates a new repo issue.
        /// </summary>
        /// <param name="newIssue">Specifies the new issue.</param>
        /// <returns>The new <see cref="Issue"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
        public async Task<Issue> CreateAsync(NewIssue newIssue)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(newIssue != null, nameof(newIssue));
            root.EnsureNotDisposed();

            return await root.GitHubApi.Issue.Create(root.Remote.Id, newIssue);
        }

        /// <summary>
        /// Updates an existing issue.
        /// </summary>
        /// <param name="number">Specifies the issue number.</param>
        /// <param name="issueUpdate">Specifies the issue update.</param>
        /// <returns>The updated <see cref="Issue"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
        public async Task<Issue> UpdateAsync(int number, IssueUpdate issueUpdate)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentException>(number > 0, nameof(number));
            Covenant.Requires<ArgumentNullException>(issueUpdate != null, nameof(issueUpdate));
            root.EnsureNotDisposed();

            return await root.GitHubApi.Issue.Update(root.Remote.Id, number, issueUpdate);
        }

        /// <summary>
        /// Returns an issue by number.
        /// </summary>
        /// <param name="number">Specifies the issue number.</param>
        /// <returns>The <see cref="Issue"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="Octokit.NotFoundException">Thrown when the issue doesn't exist</exception>
        public async Task<Issue> GetAsync(int number)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentException>(number > 0, nameof(number));
            root.EnsureNotDisposed();

            return await root.GitHubApi.Issue.Get(root.Remote.Id, number);
        }

        /// <summary>
        /// Returns all repository issues, potentially filtered.
        /// </summary>
        /// <param name="options">Optionally specifies result pagination ootions.</param>
        /// <returns>The issues.</returns>
        public async Task<IEnumerable<Issue>> GetAllAsync(ApiOptions options = null)
        {
            await SyncContext.Clear;
            root.EnsureNotDisposed();

            if (options == null)
            {
                return await root.GitHubApi.Issue.GetAllForRepository(root.Remote.Id);
            }
            else
            {
                return await root.GitHubApi.Issue.GetAllForRepository(root.Remote.Id, options);
            }
        }

        /// <summary>
        /// Returns repository issues that satisfy a filter.
        /// </summary>
        /// <param name="request">Used to filter issues by assignee, mileston, etc.</param>
        /// <param name="options">Optionally secifies result pagination ootions.</param>
        /// <returns>The issues.</returns>
        public async Task<IEnumerable<Issue>> GetAllAsync(RepositoryIssueRequest request, ApiOptions options = null)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(request != null, nameof(request));
            root.EnsureNotDisposed();

            if (options == null)
            {
                if (request != null)
                {
                    return await root.GitHubApi.Issue.GetAllForRepository(root.Remote.Id, request);
                }
                else
                {
                    return await root.GitHubApi.Issue.GetAllForRepository(root.Remote.Id);
                }
            }
            else
            {
                if (request != null)
                {
                    return await root.GitHubApi.Issue.GetAllForRepository(root.Remote.Id, request, options);
                }
                else
                {
                    return await root.GitHubApi.Issue.GetAllForRepository(root.Remote.Id, options);
                }
            }
        }

        /// <summary>
        /// Searches the repository for issues satisfying a search request, returning
        /// pages of results.
        /// </summary>
        /// <param name="request">Specifies the search parameters.</param>
        /// <returns>The <see cref="SearchIssuesResult"/></returns>
        public async Task<SearchIssuesResult> SearchAsync(SearchIssuesRequest request)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(request != null, nameof(request));
            root.EnsureNotDisposed();

            request.Repos = new RepositoryCollection();
            request.Repos.Add(root.Remote.Owner, root.Remote.Name);

            return await root.GitHubApi.Search.SearchIssues(request);
        }

        /// <summary>
        /// Searches the repository for issues satisfying a search request, returning
        /// all results (not paged).
        /// </summary>
        /// <param name="request">Specifies the search parameters.</param>
        /// <returns>The <see cref="SearchIssuesResult"/></returns>
        public async Task<IEnumerable<Issue>> SearchAllAsync(SearchIssuesRequest request)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(request != null, nameof(request));
            root.EnsureNotDisposed();

            var issues = new List<Issue>();

            request.Repos = new RepositoryCollection()
            {
                { root.Remote.Owner, root.Remote.Name }
            };

            while (true)
            {
                var page = await root.GitHubApi.Search.SearchIssues(request);

                if (page.Items.Count == 0)
                {
                    break;
                }

                foreach (var issue in page.Items)
                {
                    issues.Add(issue);
                }

                request.Page++;
            }

            return issues;
        }
    }
}
