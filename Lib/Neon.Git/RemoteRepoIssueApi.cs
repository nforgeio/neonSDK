//-----------------------------------------------------------------------------
// FILE:	    RemoteRepoIssueApi.cs
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

namespace Neon.Git
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
            this.root = root;
        }

        /// <summary>
        /// Creates a new repo issue.
        /// </summary>
        /// <param name="newIssue">Specifies the new issue.</param>
        /// <returns>The new <see cref="Issue"/>.</returns>
        public async Task<Issue> CreateAsync(NewIssue newIssue)
        {
            Covenant.Requires<ArgumentNullException>(newIssue != null, nameof(newIssue));

            return await root.GitHubApi.Issue.Create(root.Remote.Id, newIssue);
        }

        /// <summary>
        /// Updates an existing issue.
        /// </summary>
        /// <param name="number">Specifies the issue number.</param>
        /// <param name="issueUpdate">Specifies the issue update.</param>
        /// <returns>The updated <see cref="Issue"/>.</returns>
        public async Task<Issue> UpdateAsync(int number, IssueUpdate issueUpdate)
        {
            Covenant.Requires<ArgumentException>(number > 0, nameof(number));
            Covenant.Requires<ArgumentNullException>(issueUpdate != null, nameof(issueUpdate));

            return await root.GitHubApi.Issue.Update(root.Remote.Id, number, issueUpdate);
        }

        /// <summary>
        /// Returns an issue by number.
        /// </summary>
        /// <param name="number">Specifies the issue number.</param>
        /// <returns>The <see cref="Issue"/>.</returns>
        /// <exception cref="Octokit.NotFoundException">Thrown when the issue doesn't exist</exception>
        public async Task<Issue> GetAsync(int number)
        {
            Covenant.Requires<ArgumentException>(number > 0, nameof(number));

            return await root.GitHubApi.Issue.Get(root.Remote.Id, number);
        }

        // $todo(Jefflill): There's many more APIs to wrap here.
    }
}
