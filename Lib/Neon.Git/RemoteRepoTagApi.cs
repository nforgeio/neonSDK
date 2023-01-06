//-----------------------------------------------------------------------------
// FILE:	    RemoteRepoTagApi.cs
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
    public class RemoteRepoTagApi
    {
        private GitHubRepo  root;

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="root">The root <see cref="GitHubRepo"/>.</param>
        internal RemoteRepoTagApi(GitHubRepo root)
        {
            this.root = root;
        }

        /// <summary>
        /// Returns all tags from the GitHub origin repository.
        /// </summary>
        /// <returns>The list of tags.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        public async Task<IReadOnlyList<GitHubRepositoryTag>> GetAllAsync()
        {
            root.EnsureNotDisposed();

            return await root.GitHubApi.Repository.GetAllTags(root.RemoteRepoPath.Owner, root.RemoteRepoPath.Name);
        }

        /// <summary>
        /// Returns a specific GitHub origin repository tag, if it exists.
        /// </summary>
        /// <param name="tagName">Specifies the origin repository tag name.</param>
        /// <returns>The <see cref="GitHubRepositoryTag"/> or <c>null</c> when the tag doesn't exist.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        public async Task<GitHubRepositoryTag> GetAsync(string tagName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(tagName), nameof(tagName));
            root.EnsureNotDisposed();

            // $todo(jefflill):
            //
            // It's unfortunate to have to list all of these objects just to obtain a
            // specific one.  We should come back and refactor this to use the low-level
            // API.

            return (await GetAllAsync()).FirstOrDefault(tag => tag.Name.Equals(tagName, StringComparison.InvariantCultureIgnoreCase));
        }

        // $todo(jefflill):
        //
        // The tag create methods don't work.  Create functionality distinct from
        // publishing releases isn't really important right now, so I'm going to
        // comment-out the create methods.

#if TODO
        /// <summary>
        /// Creates a repository tag from the tip commit of a branch.
        /// </summary>
        /// <param name="tagName">Specifies the new tag name.</param>
        /// <param name="branchName">Specifies the name of the branch who's to commit will be tagged.</param>
        /// <param name="message">Optionally specifies a creation message.</param>
        /// <exception cref="InvalidOperationException">Thrown if the branch does not exist.</exception>
        public async Task CreateFromBranchAsync(string tagName, string branchName, string message = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(tagName), nameof(tagName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            root.EnsureNotDisposed();

            message ??= string.Empty;

            var branch = await root.RemoteRepository.Branch.GetAsync(branchName);

            if (branch == null)
            {
                throw new InvalidOperationException($"Branch [{branchName}] does not exist.");
            }

            var newTag = new NewTag()
            {
                Tag     = tagName,
                Object  = branch.Commit.Sha,
                Type    = TaggedType.Commit,
                Message = message,
                Tagger  = root.RemoteRepository.CreateComitter()
            };

            var tag = await root.GitHubApi.Git.Tag.Create(root.RemoteRepoPath.Owner, root.RemoteRepoPath.Name, newTag);

            // GitHub might not create the tag synchronously, so we'll wait for
            // it to appear.

            await root.WaitForGitHubAsync(
                async () =>
                {
                    var tag = await GetAsync(tagName);

                    var all = await GetAllAsync();

                    return tag != null;
                });
        }

        /// <summary>
        /// Creates a repository tag from a specifc commit SHA.
        /// </summary>
        /// <param name="tagName">Specifies the new tag name.</param>
        /// <param name="commitSha">The commit SHA (long or short form).</param>
        /// <param name="message">Optionally specifies a creation message.</param>
        public async Task CreateFromCommitAsync(string tagName, string commitSha, string message = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(tagName), nameof(tagName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(commitSha), nameof(commitSha));
            root.EnsureNotDisposed();

            message ??= string.Empty;

            var newTag = new NewTag()
            {
                Tag     = tagName,
                Object  = commitSha,
                Type    = TaggedType.Commit,
                Message = message,
                Tagger  = root.RemoteRepository.CreateComitter()
            };

            await root.GitHubApi.Git.Tag.Create(root.RemoteRepoPath.Owner, root.RemoteRepoPath.Name, newTag);

            // GitHub might not create the tag synchronously, so we'll wait for
            // it to appear.

            await root.WaitForGitHubAsync(
                async () =>
                {
                    var tag = await GetAsync(tagName);

                    return tag != null;
                });
        }
#endif

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
            root.EnsureNotDisposed();

            var tag = await GetAsync(tagName);

            if (tag == null)
            {
                return false;
            }

            // OctoKit doesn't have a nice tag removal method, so we'll need to use
            // the REST API.  This is a bit tricky:
            //
            //      https://stackoverflow.com/questions/7247414/delete-a-tag-with-github-v3-api

            await root.HttpClient.DeleteSafeAsync($"/repos/{root.RemoteRepoPath.Owner}/{root.RemoteRepoPath.Name}/git/refs/tags/{tagName}");

            // GitHub might not delete the tag synchronously, so we'll wait for
            // it to disappear.

            await root.WaitForGitHubAsync(
                async () =>
                {
                    var tag = await GetAsync(tagName);

                    return tag == null;
                });

            return true;
        }
    }
}
