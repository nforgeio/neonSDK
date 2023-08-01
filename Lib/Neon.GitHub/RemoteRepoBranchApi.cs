//-----------------------------------------------------------------------------
// FILE:        RemoteRepoBranchApi.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
            Covenant.Requires<ArgumentNullException>(root != null, nameof(root));

            this.root = root;
        }

        /// <summary>
        /// Returns all branches from the GitHub origin repository.
        /// </summary>
        /// <returns>The list of branches.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
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
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
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
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
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
        /// Determines whether a branch exists in the origin repo.
        /// </summary>
        /// <param name="branchName">Specifies the origin repository branch name.</param>
        /// <returns><c>true</c> when the branch exists.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
        public async Task<bool> ExistsAsync(string branchName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            root.EnsureNotDisposed();

            return await FindAsync(branchName) != null;
        }

        /// <summary>
        /// Removes an origin branch, if it exists.
        /// </summary>
        /// <param name="branchName">Specifies the origin repository branch name.</param>
        /// <returns><c>true</c> if the branch existed and was removed, <c>false</c> otherwise.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
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

            NetHelper.EnsureSuccess(await root.GitHubApi.Connection.Delete(new Uri(root.Remote.GetApiUri($"/repos/{root.Remote.Path.Owner}/{root.Remote.Path.Name}/git/refs/heads/{branchName}"))));

            return true;
        }

        /// <summary>
        /// <para>
        /// Converts a relative local repository file path like "/my-folder/test.txt" 
        /// or "my-folder/test.txt" to the remote GitHub URI for the file within the 
        /// the currently checked out branch.
        /// </para>
        /// <note>
        /// The local or remote file doesn't need to actually exist.
        /// </note>
        /// </summary>
        /// <param name="branchName">Specifies the origin repository branch name.</param>
        /// <param name="relativePath">
        /// Specifies the path to the file relative to the local repository root folder.
        /// This may include a leading slash (which is assumed when not present) and both 
        /// forward and backslashes are allowed as path separators.
        /// </param>
        /// <param name="raw">
        /// Optionally returns the link to the raw file bytes as opposed to the URL
        /// for the GitHub HTML page for the file.
        /// </param>
        /// <returns>The GitHub URI for the file from the current branch.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <remarks>
        /// <note>
        /// This method <b>does not</b> ensure that the target file actually exists in the repo.
        /// </note>
        /// </remarks>
        public async Task<string> GetRemoteFileUriAsync(string branchName, string relativePath, bool raw = false)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(relativePath), nameof(relativePath));
            root.EnsureNotDisposed();

            relativePath = relativePath.Replace('\\', '/');

            if (relativePath.StartsWith('/'))
            {
                relativePath = relativePath.Substring(1);
            }

            if (raw)
            {
                return new Uri($"https://raw.githubusercontent.com/{root.Remote.Path.Owner}/{root.Remote.Path.Name}/{branchName}/{relativePath}").ToString();
            }
            else
            {
                return new Uri($"{root.Remote.BaseUri}{root.Remote.Path.Name}/blob/{branchName}/{relativePath}").ToString();
            }
        }

        /// <summary>
        /// Copies the contents of a file in a specific repo branch to a stream.
        /// </summary>
        /// <param name="branchName">Specifies the origin repository branch name.</param>
        /// <param name="relativePath">
        /// Specifies the path to the file relative to the remote repository root folder.
        /// This may include a leading slash (which is assumed when not present) and both 
        /// forward and backslashes are allowed as path separators.
        /// </param>
        /// <param name="output">Specifies the stream where the remote file contents will be copied.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="Octokit.NotFoundException">Thrown if the branch or file file doesn't exist.</exception>
        public async Task GetBranchFileAsync(string branchName, string relativePath, Stream output)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(relativePath), nameof(relativePath));
            Covenant.Requires<ArgumentNullException>(output != null, nameof(output));
            root.EnsureNotDisposed();

            // This ensures that the branch actually exists.

            await root.GitHubApi.Repository.Branch.Get(root.Remote.Id, branchName);

            // Fetch the file, returning FALSE when the file doesn't exist.

            var fileUri  = await GetRemoteFileUriAsync(branchName, relativePath, raw: true);
            var response = await root.HttpClient.GetAsync(fileUri);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new Octokit.NotFoundException($"Remote file not found: {relativePath}", HttpStatusCode.NotFound);
                }

                response.EnsureSuccessStatusCodeEx();
            }

            await response.Content.CopyToAsync(output);
        }

        /// <summary>
        /// Retieves the contents file in a specific repo branch as text.
        /// </summary>
        /// <param name="branchName">Specifies the origin repository branch name.</param>
        /// <param name="relativePath">
        /// Specifies the path to the file relative to the remote repository root folder.
        /// This may include a leading slash (which is assumed when not present) and both 
        /// forward and backslashes are allowed as path separators.
        /// </param>
        /// <returns>The file text.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="Octokit.NotFoundException">Thrown if the branch or the file doesn't exist.</exception>
        public async Task<string> GetBranchFileAsTextAsync(string branchName, string relativePath)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(relativePath), nameof(relativePath));
            root.EnsureNotDisposed();

            // This ensures that the branch actually exists.

            await root.GitHubApi.Repository.Branch.Get(root.Remote.Id, branchName);

            // Fetch the file, returning FALSE when the file doesn't exist.

            var fileUri  = await GetRemoteFileUriAsync(branchName, relativePath, raw: true);
            var response = await root.HttpClient.GetAsync(fileUri);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new Octokit.NotFoundException($"Remote file not found: {relativePath}", HttpStatusCode.NotFound);
                }

                response.EnsureSuccessStatusCodeEx();
            }

            using (var ms = new MemoryStream())
            {
                await response.Content.CopyToAsync(ms);

                ms.Position = 0;

                using (var reader = new StreamReader(ms))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Returns the branch protections settings for a specific GitHub origin repository branch.
        /// </summary>
        /// <param name="branchName">Specifies the origin repository branch name.</param>
        /// <returns>The requested <see cref="BranchProtectionSettings"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="Octokit.NotFoundException">Thrown when the branch does not exist.</exception>
        /// <exception cref="Octokit.NotFoundException">Thrown when the branch is not currently protected.</exception>
        public async Task<BranchProtectionSettings> GetBranchProtectionAsync(string branchName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            root.EnsureNotDisposed();

            return await root.GitHubApi.Repository.Branch.GetBranchProtection(root.Remote.Id, branchName);
        }

        /// <summary>
        /// Updates the branch protections settings for a specific GitHub origin repository branch.
        /// </summary>
        /// <param name="branchName">Specifies the origin repository branch name.</param>
        /// <param name="update">Specifies the new protection settings update for the branch.</param>
        /// <returns>The updated <see cref="BranchProtectionSettings"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="Octokit.NotFoundException">Thrown when the branch does not exist.</exception>
        public async Task<BranchProtectionSettings> UpdateBranchProtectionAsync(string branchName, BranchProtectionSettingsUpdate update)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            Covenant.Requires<ArgumentNullException>(update != null, nameof(update));
            root.EnsureNotDisposed();

            return await root.GitHubApi.Repository.Branch.UpdateBranchProtection(root.Remote.Id, branchName, update);
        }

        /// <summary>
        /// Removes all protection for for a specific GitHub origin repository branch.
        /// </summary>
        /// <param name="branchName">>Specifies the origin repository branch name.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public async Task RemoveBranchProtection(string branchName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            root.EnsureNotDisposed();

            await root.GitHubApi.Repository.Branch.DeleteBranchProtection(root.Remote.Id, branchName);
        }
    }
}
