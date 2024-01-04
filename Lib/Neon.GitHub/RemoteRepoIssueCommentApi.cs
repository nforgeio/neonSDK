//-----------------------------------------------------------------------------
// FILE:        RemoteRepoIssueCommentApi.cs
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

using GitHubBranch = Octokit.Branch;
using GitHubRepository = Octokit.Repository;
using GitHubSignature = Octokit.Signature;

using GitBranch = LibGit2Sharp.Branch;
using GitRepository = LibGit2Sharp.Repository;
using GitSignature = LibGit2Sharp.Signature;
using YamlDotNet.Core.Events;

namespace Neon.GitHub
{
    /// <summary>
    /// Implements the friendly GitHub repository issue comment related APIs. 
    /// </summary>
    public class RemoteRepoIssueCommentApi
    {
        private GitHubRepo root;

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="root">The root <see cref="GitHubRepo"/>.</param>
        internal RemoteRepoIssueCommentApi(GitHubRepo root)
        {
            Covenant.Requires<ArgumentNullException>(root != null, nameof(root));

            this.root = root;
        }

        /// <summary>
        /// Gets a specific repository comment by ID.
        /// </summary>
        /// <param name="id">Specifies the comment ID.</param>
        /// <returns>The <see cref="IssueComment"/>.</returns>
        public async Task<IssueComment> GetAsync(int id)
        {
            return await root.GitHubApi.Issue.Comment.Get(root.Remote.Id, id);
        }

        /// <summary>
        /// Gets all comments for the respository.
        /// </summary>
        /// <returns>The <see cref="IssueComment"/> instances</returns>
        public async Task<IEnumerable<IssueComment>> GetAllForRepository()
        {
            return await root.GitHubApi.Issue.Comment.GetAllForRepository(root.Remote.Id);
        }

        /// <summary>
        /// Gets all comments from a specific issue.
        /// </summary>
        /// <param name="number">Specifies the issue number.</param>
        /// <returns></returns>
        /// <returns>The <see cref="IssueComment"/> instances</returns>
        public async Task<IEnumerable<IssueComment>> GetAllForIssue(int number)
        {
            return await root.GitHubApi.Issue.Comment.GetAllForIssue(root.Remote.Owner, root.Remote.Name, number);
        }

        /// <summary>
        /// Adds a comment to an issue.
        /// </summary>
        /// <param name="number">Specifies the issue number.</param>
        /// <param name="newComment">Specifies the comment text.</param>
        /// <returns></returns>
        public async Task<IssueComment> CreateAsync(int number, string newComment)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(newComment), nameof(newComment));

            return await root.GitHubApi.Issue.Comment.Create(root.Remote.Id, number, newComment);
        }

        /// <summary>
        /// Removes a comment from the repository.
        /// </summary>
        /// <param name="id">Specifies the comment ID.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public async Task DeleteAsync(int id)
        {
            await root.GitHubApi.Issue.Comment.Delete(root.Remote.Id, id);
        }

        /// <summary>
        /// Updates an issue comment.
        /// </summary>
        /// <param name="id">Specifies the comment ID.</param>
        /// <param name="commentUpdate">Specifies the updated comment text.</param>
        /// <returns>The updated <see cref="IssueComment"/>.</returns>
        public async Task<IssueComment> UpdateAsync(int id, string commentUpdate)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(commentUpdate), nameof(commentUpdate));

            return await root.GitHubApi.Issue.Comment.Update(root.Remote.Id, id, commentUpdate);
        }
    }
}
