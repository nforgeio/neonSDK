//-----------------------------------------------------------------------------
// FILE:	    LocalRepoApi.cs
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

namespace Neon.Git
{
    /// <summary>
    /// Implements easy-to-use local git repository related APIs.
    /// </summary>
    public class LocalRepoApi
    {
        private GitHubRepo root;

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="root">The root <see cref="GitHubRepo"/>.</param>
        /// <param name="localRepoFolder">
        /// Specifies the path to the local repository folder.  This will be
        /// <c>null</c> when the root <see cref="GitHubRepo"/> is only connected
        /// to GitHub and there is no local repo.
        /// </param>
        internal LocalRepoApi(GitHubRepo root, string localRepoFolder)
        {
            this.root            = root;
            this.Folder = localRepoFolder;
        }

        /// <summary>
        /// Returns the current branch.
        /// </summary>
        public GitBranch CurrentBranch => root.GitApi.CurrentBranch();

        /// <summary>
        /// Returns the path to the local repository folder.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the instance is disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        public string Folder { get; private set; }

        /// <summary>
        /// Returns <c>true</c> when the local repos has uncommitted changes.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the instance is disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        public bool IsDirty => root.GitApi.IsDirty();

        /// <summary>
        /// Creates a <see cref="GitSignature"/> from the repository's credentials.
        /// </summary>
        /// <returns>The new <see cref="GitSignature"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the instance is disposed.</exception>
        public GitSignature CreateSignature()
        {
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            return new GitSignature(root.Credentials.Username, root.Credentials.Email, DateTimeOffset.Now);
        }

        /// <summary>
        /// Returns a <see cref="PushOptions"/> instance initialized with the credentials provider.
        /// </summary>
        /// <returns>The new <see cref="PushOptions"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the instance is disposed.</exception>
        public PushOptions CreatePushOptions()
        {
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            return new PushOptions()
            {
                CredentialsProvider = root.CredentialsProvider
            };
        }

        /// <summary>
        /// Fetches information from the associated GitHub origin repository.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task FetchAsync()
        {
            await SyncContext.Clear;
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            var options = new FetchOptions()
            {
                TagFetchMode        = TagFetchMode.Auto,
                Prune               = true,
                CredentialsProvider = root.CredentialsProvider
            };

            var refSpecs = root.Origin.FetchRefSpecs.Select(spec => spec.Specification);

            Commands.Fetch(root.GitApi, root.Origin.Name, refSpecs, options, "fetching");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Commits any staged and pending changes to the local git repository.
        /// </summary>
        /// <param name="message">Optionally specifies the commit message.  This defaults to <b>unspecified changes"</b>.</param>
        /// <returns><c>true</c> when changes were comitted, <c>false</c> when there were no pending changes.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<bool> CommitAsync(string message = null)
        {
            await SyncContext.Clear;
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            message ??= "unspecified changes";

            if (!IsDirty)
            {
                return false;
            }

            Commands.Stage(root.GitApi, "*");

            var signature = CreateSignature();

            root.GitApi.Commit(message, signature, signature);

            return await Task.FromResult(true);
        }

        /// <summary>
        /// <para>
        /// Fetches and pulls the changes from GitHub into the current checked-out branch within a local git repository.
        /// </para>
        /// <note>
        /// The pull operation will be aborted and rolled back for merge conflicts.  Check the result status
        /// to understand what happened.
        /// </note>
        /// </summary>
        /// <returns>The <see cref="MergeStatus"/> for the operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<MergeStatus> PullAsync()
        {
            await SyncContext.Clear;
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            var options = new PullOptions()
            {
                MergeOptions = new MergeOptions()
                {
                    FailOnConflict = true
                }
            };

            await FetchAsync();

            return await Task.FromResult(Commands.Pull(root.GitApi, CreateSignature(), options).Status);
        }


        /// <summary>
        /// Pushes any pending local commits from the checked out branch to GitHub, creating the
        /// branch on GitHub and associating the local branch when the branch doesn't already exist
        /// on GitHub.  Any GitHub origin repository branch created will have the same name as the 
        /// local branch.
        /// </summary>
        /// <returns><c>true</c> when commits were pushed, <c>false</c> when there were no pending commits.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<bool> PushAsync()
        {
            await SyncContext.Clear;
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            // Associate the current local branch with the origin branch having the 
            // same name.  This will cause the origin branch to be created when we
            // push below if the origin branch does not already exist.

            var currentBranch = root.GitApi.CurrentBranch();

            if (currentBranch == null)
            {
                throw new LibGit2SharpException($"Local git repository [{Folder}] has no checked-out branch.");
            }

            if (!currentBranch.IsTracking)
            {
                root.GitApi.Branches.Update(currentBranch,
                    updater => updater.Remote = root.Origin.Name,
                    updater => updater.UpstreamBranch = currentBranch.CanonicalName);
            }

            // Push any local commits to the origin branch.

            if (root.GitApi.Commits.Count() == 0)
            {
                return false;
            }

            root.GitApi.Network.Push(currentBranch, CreatePushOptions());

            await root.WaitForGitHubAsync(
                async () =>
                {
                    var serverBranchUpdate = await root.Remote.Branch.GetAsync(currentBranch.FriendlyName);

                    return serverBranchUpdate.Commit.Sha == currentBranch.Tip.Sha;
                });

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Checks out a local repository branch.
        /// </summary>
        /// <param name="branchName">Specifies the local branch to be checked out.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task CheckoutAsync(string branchName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            var branch = root.GitApi.Branches[branchName];

            if (branch == null)
            {
                throw new LibGit2SharpException($"Branch [{branchName}] does not exist.");
            }

            Commands.Checkout(root.GitApi, branch);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates a new local branch from the tip of a source branch if the new branch
        /// doesn't already exist and then checks out the new branch.
        /// </summary>
        /// <param name="branchName">Identifies the branch to being created.</param>
        /// <param name="sourceBranchName">Identifies the source branch.</param>
        /// <returns><c>true</c> if the branch didn't already exist and was created, <c>false</c> otherwise.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<bool> CreateBranchAsync(string branchName, string sourceBranchName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(sourceBranchName), nameof(sourceBranchName));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            var newBranch = root.GitApi.Branches[branchName];

            if (newBranch != null)
            {
                await CheckoutAsync(branchName);

                return false;
            }

            var sourceBranch = root.GitApi.Branches[sourceBranchName];

            if (sourceBranch == null)
            {
                throw new LibGit2SharpException($"Source branch [{sourceBranchName}] does not exist.");
            }

            root.GitApi.CreateBranch(branchName, sourceBranch.Tip);
            await CheckoutAsync(branchName);

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Creates a local branch from a named GitHub repository origin branch and then checks 
        /// out the branch.  By default, the local branch will have the same name as the origin, 
        /// but this can be customized.
        /// </summary>
        /// <param name="originBranchName">Specifies the GitHub origin repository branch name.</param>
        /// <param name="branchName">Optionally specifies the local branch name.  This defaults to <paramref name="originBranchName"/>.</param>
        /// <returns><c>true</c> if the local branch didn't already exist and was created from the GitHib origin repository, <c>false</c> otherwise.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<bool> CheckoutOriginAsync(string originBranchName, string branchName = null)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(originBranchName), nameof(originBranchName));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            branchName ??= originBranchName;

            var created = root.GitApi.Branches[branchName] == null;

            if (created)
            {
                root.GitApi.CreateBranch(branchName, $"{root.Origin.Name}/{originBranchName}");
            }

            await CheckoutAsync(branchName);

            return created;
        }

        /// <summary>
        /// Removes a branch from local repository as well as the from the GitHub origin repository, if they exist.
        /// </summary>
        /// <param name="branchName">Specifies the branch to be removed.</param>
        /// <returns><c>true</c> if the branch existed and was removed, <c>false</c> otherwise.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task RemoveBranchAsync(string branchName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            // Remove the origin branch.

            root.GitApi.Network.Push(root.Origin, $"+:refs/heads/{branchName}", CreatePushOptions());

            // Remove the local branch.

            root.GitApi.Branches.Remove(branchName);

            await Task.CompletedTask;
        }

        /// <summary>
        /// <para>
        /// Merges another local branch into the current branch.
        /// </para>
        /// <note>
        /// The checked out branch must not included an non-committed changes.
        /// </note>
        /// </summary>
        /// <param name="branchName">Identifies the branch to be merged into the current branch.</param>
        /// <param name="throwOnConflict">Optionally specifies that the method should not throw an exception for conflicts.</param>
        /// <returns>
        /// A <see cref="MergeResult"/> for successful merges or when the merged failed and 
        /// <paramref name="throwOnConflict"/> is <c>false</c>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<MergeResult> MergeAsync(string branchName, bool throwOnConflict = true)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            var branch = root.GitApi.Branches[branchName];

            if (branch == null)
            {
                throw new LibGit2SharpException($"Branch [{branchName}] does not exist.");
            }

            if (IsDirty)
            {
                throw new LibGit2SharpException($"Target branch [{CurrentBranch.FriendlyName}] has uncommited changes.");
            }

            var mergeOptions = new MergeOptions()
            {
                FailOnConflict = true
            };

            var result = root.GitApi.Merge(branch, CreateSignature());

            if (result.Status == MergeStatus.Conflicts)
            {
                await UndoAsync();

                if (throwOnConflict)
                {
                    throw new LibGit2SharpException($"Merge conflict: changes were rolled back.");
                }
            }
            
            return await Task.FromResult(result);
        }

        /// <summary>
        /// Reverts any uncommitted changes in the current local repository branch.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task UndoAsync()
        {
            await SyncContext.Clear;
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            root.GitApi.CheckoutPaths(CurrentBranch.Tip.Sha, new string[] { "*" }, new CheckoutOptions() { CheckoutModifiers = CheckoutModifiers.Force });
            root.GitApi.RemoveUntrackedFiles();

            await Task.CompletedTask;
        }

        /// <summary>
        /// <para>
        /// Converts a relative local repository file path like "/my-folder/test.txt" 
        /// or "my-folder/test.txt into the actual local file system path for the file.
        /// </para>
        /// <note>
        /// The local file doesn't need to actually exist.
        /// </note>
        /// </summary>
        /// <param name="relativePath">
        /// Specifies the path to the file relative to the local repository root folder.
        /// This may include a leading slash and both forward and backslashes are allowed
        /// as path separators.
        /// </param>
        /// <returns>The fully qualified file system path to the specified repo file.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        public async Task<string> GetLocalFilePathAsync(string relativePath)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(relativePath), nameof(relativePath));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            if (NeonHelper.IsWindows)
            {
                relativePath = relativePath.Replace('/', '\\');

                if (relativePath.StartsWith('\\'))
                {
                    relativePath = relativePath.Substring(1);
                }
            }
            else
            {
                relativePath = relativePath.Replace('\\', '/');

                if (relativePath.StartsWith('/'))
                {
                    relativePath = relativePath.Substring(1);
                }
            }

            return await Task.FromResult(Path.GetFullPath(Path.Combine(root.Local.Folder, relativePath)));
        }

        /// <summary>
        /// <para>
        /// Converts a relative local repository file path like "/my-folder/test.txt" 
        /// or "my-folder/test.txt to the remote GitHub URI for the file within the 
        /// the currently checked out branch.
        /// </para>
        /// <note>
        /// The local or remote file doesn't need to actually exist.
        /// </note>
        /// </summary>
        /// <param name="relativePath">
        /// Specifies the path to the file relative to the local repository root folder.
        /// This may include a leading slash (which is assumed when not present) and both 
        /// forward and backslashes are allowed as path separators.
        /// </param>
        /// <returns>The GitHub URI for the file from the current branch.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        public async Task<string> GetRemoteFileUriAsync(string relativePath)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(relativePath), nameof(relativePath));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            relativePath = relativePath.Replace('\\', '/');

            if (relativePath.StartsWith('/'))
            {
                relativePath = relativePath.Substring(1);
            }

            var uri = new Uri($"{root.Remote.BaseUri}{relativePath}");

            return await Task.FromResult(uri.ToString());
        }
    }
}
