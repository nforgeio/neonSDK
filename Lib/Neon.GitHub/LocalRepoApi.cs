//-----------------------------------------------------------------------------
// FILE:        LocalRepoApi.cs
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
using GitCommit     = LibGit2Sharp.Commit;

namespace Neon.GitHub
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
            Covenant.Requires<ArgumentNullException>(root != null, nameof(root));

            this.root   = root;
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
        /// <exception cref="LibGit2SharpException">Thrown when the operation fails.</exception>
        public PushOptions CreatePushOptions()
        {
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            return new PushOptions()
            {
                CredentialsProvider = root.CredentialsProvider,
                OnPushStatusError   = errors => throw new LibGit2SharpException(errors.Message)
            };
        }

        /// <summary>
        /// Fetches information from the associated GitHub origin repository.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
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
        /// <param name="autoStage">Optionally disable automatic staging of new and changed files.  This defaults to <c>true</c>.</param>
        /// <returns><c>true</c> when changes were committed, <c>false</c> when there were no pending changes.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<bool> CommitAsync(string message = null, bool autoStage = true)
        {
            await SyncContext.Clear;
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            message ??= "unspecified changes";

            if (!IsDirty)
            {
                return false;
            }

            if (autoStage)
            {
                var statusOptions = new StatusOptions
                {
                    DetectRenamesInIndex   = false,
                    DetectRenamesInWorkDir = false
                };

                var changes = root.GitApi.RetrieveStatus(statusOptions)
                    .Select(change => change.FilePath);

                Commands.Stage(root.GitApi, "*");
            }

            var signature = CreateSignature();

            try
            {
                root.GitApi.Commit(message, signature, signature);
            }
            catch (EmptyCommitException)
            {
                // $todo(jefflill):
                // $hack(jefflill):
                //
                // I'm not entirely sure why we're seeing these exceptions when creating
                // the [neonversion.go] source file while initializing a new [neon-kubernetes]
                // branch via our [neon-kube] tool.
                //
                // In this case, the local repo branch reports being dirty and it the source
                // file does end up being committed and pushed to GitHub.
                //
                // We're going to ignore this for the time being, but should have a look
                // again sometime in the future.
            }

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
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<MergeStatus> PullAsync()
        {
            await SyncContext.Clear;
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            var options = new PullOptions()
            {
                FetchOptions = new FetchOptions()
                {
                    CredentialsProvider = root.CredentialsProvider
                },

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
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
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
                currentBranch = root.GitApi.Branches.Update(currentBranch,
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
                    // It may take some time for the new branch to be created
                    // on GitHub, so we're going to ignore [NotFoundException].

                    try
                    {
                        var serverBranchUpdate = await root.Remote.Branch.GetAsync(currentBranch.FriendlyName);

                        return serverBranchUpdate.Commit.Sha == currentBranch.Tip.Sha;
                    }
                    catch (Octokit.NotFoundException)
                    {
                        return false;
                    }
                });

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Checks out a local repository branch.
        /// </summary>
        /// <param name="branchName">Specifies the local branch to be checked out.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task CheckoutAsync(string branchName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            // Try the local branch first and if that doesn't exist, try the remote branch.

            var branch = root.GitApi.Branches[branchName];

            if (branch != null)
            {
                // The branch is already local so we can check it out immediately.

                Commands.Checkout(root.GitApi, branch);
            }
            else
            {
                var remoteBranch = root.GitApi.Branches[$"origin/{branchName}"];

                if (remoteBranch == null)
                {
                    throw new LibGit2SharpException($"Branch [{branchName}] does not exist locally or remote.");
                }

                // Create local branch with the specified name and then configure it
                // to track the remote branch and then check out the local branch.

                branch = root.GitApi.CreateBranch(branchName);
                branch = root.GitApi.Branches.Update(branch, branch => branch.TrackedBranch = remoteBranch.CanonicalName);

                Commands.Checkout(root.GitApi, branch);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates a new local branch from the tip of a source branch if the new branch
        /// doesn't already exist and then checks out the new branch.
        /// </summary>
        /// <param name="branchName">Identifies the branch to being created.</param>
        /// <param name="sourceBranchName">Identifies the source branch.</param>
        /// <returns><c>true</c> if the branch didn't already exist and was created, <c>false</c> otherwise.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
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
        /// <returns><c>true</c> if the local branch didn't already exist and was created from the GitHub origin repository, <c>false</c> otherwise.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
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
        /// Removes a branch from the local repository if it exists.
        /// </summary>
        /// <param name="branchName">Specifies the branch to be removed.</param>
        /// <returns><c>true</c> if the branch existed and was removed, <c>false</c> otherwise.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task RemoveBranchAsync(string branchName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

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
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
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
        /// Reverts any uncommitted changes in the current local repository branch,
        /// including untracked files.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
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
        /// <exception cref="ObjectDisposedException">Thrown when the <see cref="GitHubRepo"/> has been disposed.</exception>
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
        public async Task<string> GetRemoteFileUriAsync(string relativePath, bool raw = false)
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

            if (raw)
            {
                return new Uri($"https://raw.githubusercontent.com/{root.Remote.Path.Owner}/{root.Remote.Path.Name}/{CurrentBranch.FriendlyName}/{relativePath}").ToString();
            }
            else
            {
                return new Uri($"{root.Remote.BaseUri}{root.Remote.Path.Name}/blob/{CurrentBranch.FriendlyName}/{relativePath}").ToString();
            }
        }

        /// <summary>
        /// Returns the local commits in decending order by commit date/time.
        /// </summary>
        /// <returns>The local commits in decending order by commit date/time.</returns>
        public async Task<IEnumerable<LibGit2Sharp.Commit>> GetCommitsAsync()
        {
            return await Task.FromResult(root.GitApi.Commits.ToList());
        }

        /// <summary>
        /// Determines whether the local repo is behind the remote banch.
        /// </summary>
        /// <returns><c>true</c> when the local repo is behind the remote branch.</returns>
        public async Task<bool> IsBehindAsync()
        {
            await SyncContext.Clear;

            var localCommits  = await GetCommitsAsync();
            var localTipSha   = localCommits.First().Sha;
            var remoteCommits = await root.Remote.GetCommitsAsync(CurrentBranch.FriendlyName);
            var remoteTipSha  = remoteCommits.First().Sha;

            if (localTipSha == remoteTipSha)
            {
                return false;   // Local and remote branches are at the same commit
            }

            // We're behind when the local branch doesn't include the tip commit
            // of the remote branch.

            return !localCommits.Any(localCommit => localCommit.Sha == remoteTipSha);
        }

        /// <summary>
        /// Determines whether the local repo is ahead of the remote banch.
        /// </summary>
        /// <returns><c>true</c> when the local repo is ahead of the remote branch.</returns>
        public async Task<bool> IsAheadAsync()
        {
            await SyncContext.Clear;

            var localCommits  = await GetCommitsAsync();
            var localTipSha   = localCommits.First().Sha;
            var remoteCommits = await root.Remote.GetCommitsAsync(CurrentBranch.FriendlyName);
            var remoteTipSha  = remoteCommits.First().Sha;

            if (localTipSha == remoteTipSha)
            {
                return false;   // Local and remote branches are at the same commit
            }

            // We're ahead when the local branch tip commit is not present
            // in the remote branch.

            return !remoteCommits.Any(remoteCommit => remoteCommit.Sha == localTipSha);
        }

        /// <summary>
        /// Enumerates the friendly names of the local branches.
        /// </summary>
        /// <returns>The friendly names.</returns>
        public async Task<IEnumerable<string>> ListBrancheshAsync()
        {
            await SyncContext.Clear;
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            return root.GitApi.Branches.Select(branch => branch.FriendlyName);
        }

        /// <summary>
        /// Determines whether a specific branch exists in the local repo.
        /// </summary>
        /// <param name="branchName">Specfies the friendly name of the target branch.</param>
        /// <returns><c>true</c> when the branch exists.</returns>
        public async Task<bool> BranchExistsAsync(string branchName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            return (await ListBrancheshAsync()).Any(branch => branch == branchName);
        }

        /// <summary>
        /// Attempts to retrieve a branch by name.
        /// </summary>
        /// <param name="branchName">Specifies the friendly name of the local branch.</param>
        /// <returns>The <see cref="GitBranch"/> if it exists, <c>null</c> otherwise.</returns>
        public async Task<GitBranch> FindBranchAsync(string branchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));

            return await Task.FromResult(root.GitApi.Branches.SingleOrDefault(branch => branch.FriendlyName == branchName));
        }

        /// <summary>
        /// Returns the named branch.
        /// </summary>
        /// <param name="branchName">Specifies the friendly name of the local branch.</param>s
        /// <returns>The <see cref="GitBranch"/>.</returns>
        /// <exception cref="LibGit2Sharp.NotFoundException">Thrown if the branch doesn't exist.</exception>
        public async Task<GitBranch> GetBranchAsync(string branchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));

            var branch = await FindBranchAsync(branchName);

            if (branch == null)
            {
                throw new LibGit2Sharp.NotFoundException($"Branch does not exist: {branchName}");
            }

            return branch;
        }

        /// <summary>
        /// Returns the commits for a local branch.
        /// </summary>
        /// <param name="branchName">Specifies the friendly name of the local branch.</param>
        /// <returns>The commits.</returns>
        /// <exception cref="LibGit2Sharp.NotFoundException">Thrown if the branch doesn't exist.</exception>
        public async Task<IEnumerable<GitCommit>> GetBranchCommitsAsync(string branchName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            var branch = root.GitApi.Branches.SingleOrDefault(branch => branch.FriendlyName == branchName);

            if (branch == null)
            {
                throw new LibGit2Sharp.NotFoundException($"Branch does not exist: {branchName}");
            }

            return branch.Commits.ToArray();
        }

        /// <summary>
        /// Returns the status of a working file by comparing it against any staged file
        /// and the current commit.
        /// </summary>
        /// <param name="path">Path to the working file.</param>
        /// <returns>The <see cref="FileStatus"/>.</returns>
        public async Task<FileStatus> RetrieveStatusAsync(string path)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path), nameof(path));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            return await Task.FromResult(root.GitApi.RetrieveStatus(path));
        }

        /// <summary>
        /// Returns the status of a working file by comparing it against any staged file
        /// and the current commit.
        /// </summary>
        /// <param name="options">Optionally specifies status retrieval options.</param>
        /// <returns>The <see cref="RepositoryStatus"/>.</returns>
        public async Task<RepositoryStatus> RetrieveStatusAsync(StatusOptions options = null)
        {
            await SyncContext.Clear;
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            return await Task.FromResult(root.GitApi.RetrieveStatus(options ?? new StatusOptions()));
        }

        /// <summary>
        /// Stages a file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public async Task StageAsync(string path)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path), nameof(path));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            Commands.Stage(root.GitApi, path);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Unstages a file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public async Task UnstageAsync(string path)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path), nameof(path));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            Commands.Unstage(root.GitApi, path);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Cherry-picks commits from a target branch and adds them to the current branch.
        /// </summary>
        /// <param name="sourceBranchName">Specifies the friendly name of the local source branch.</param>
        /// <param name="commits">
        /// Specifies the commits to be cherry-picked from the source branch and added to the target.
        /// The commits will be applied in the order they appear here.
        /// </param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="LibGit2SharpException">
        /// Thrown when the source branch doesn't include any of the commits, the target
        /// branch already includes one or more, or when there was a conflict cherry-picking
        /// a commit.
        /// </exception>
        /// <remarks>
        /// <para>
        /// Before applying the commits from the source branch, this method verifies that
        /// the source branch actually includes all of the specifies commits and the current
        /// branch includes none of these commits.
        /// </para>
        /// <para>
        /// Cherry-picking will stop if one of the operations fails due to a conflict.
        /// </para>
        /// </remarks>
        public async Task CherryPickAsync(string sourceBranchName, IEnumerable<GitCommit> commits)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(sourceBranchName), nameof(sourceBranchName));
            Covenant.Requires<ArgumentNullException>(commits != null, nameof(commits));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            var sourceBranch      = await GetBranchAsync(sourceBranchName);
            var idToPickCommit    = commits.ToDictionary(commit => commit.Sha);
            var idToSourceCommit  = sourceBranch.Commits.ToDictionary(commit => commit.Sha);
            var idToCurrentCommit = CurrentBranch.Commits.ToDictionary(commit => commit.Sha);

            // Ensure that all of the requested commits exist in the source branch.

            var sbError = new StringBuilder();

            foreach (var commit in commits)
            {
                if (!idToSourceCommit.ContainsKey(commit.Sha))
                {
                    sbError.AppendWithSeparator(commit.Sha);
                }
            }

            if (sbError.Length > 0)
            {
                throw new LibGit2SharpException($"Source branch [{sourceBranchName}] does not include these commits: {sbError}");
            }

            // Ensure that none of the commits already exist in the current branch.

            sbError.Clear();

            foreach (var commit in commits)
            {
                if (idToCurrentCommit.ContainsKey(commit.Sha))
                {
                    sbError.AppendWithSeparator(commit.Sha);
                }
            }

            if (sbError.Length > 0)
            {
                throw new LibGit2SharpException($"Current branch [{CurrentBranch.FriendlyName}] already includes these commits: {sbError}");
            }

            // Cherry-pick the commits from the source branch.

            var signature = CreateSignature();

            foreach (var commit in commits)
            {
                // Merge commits have two parents and we need to configure the cherry-pick
                // mainline option for these.  We're going to hardcode the first branch there.

                var options = new CherryPickOptions()
                {
                    FileConflictStrategy = CheckoutFileConflictStrategy.Theirs,
                    CommitOnSuccess      = true,
                };

                if (commit.Parents.Count() > 1)
                {
                    options.Mainline = 1;
                }

                try
                {
                    var result = root.GitApi.CherryPick(commit, signature, options);

                    if (result.Status == CherryPickStatus.Conflicts)
                    {
                        throw new LibGit2SharpException($"Conflict cherry-picking: {commit.Sha}: {commit.Message}");
                    }
                }
                catch (EmptyCommitException)
                {
                    // We're going to ignore these.
                }
            }
        }

        /// <summary>
        /// Resets the current repo branch to match the HEAD commit using the <paramref name="resetMode"/>.
        /// </summary>
        /// <param name="resetMode">Specifies the reset mode</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <remarks>
        /// <para>
        /// This method is used to undo uncommitted changes to the local repository.
        /// This may impact the repo's working directory and staging index, depending
        /// on the <paramref name="resetMode"/> passed.
        /// </para>
        /// <list type="table">
        /// <item>
        ///     <term><see cref="ResetMode.Hard"/></term>
        ///     <description>
        ///     <para>
        ///     Resets the working directory and staging index to match the current
        ///     branch's HEAD commit.  This effectively undoes all repo changes with
        ///     any pending changes being lost.
        ///     </para>
        ///     <note>
        ///     Untracked files in the working directory are not impacted.
        ///     </note>
        ///     </description>
        /// </item>
        /// <item>
        ///     <term><see cref="ResetMode.Mixed"/></term>
        ///     <description>
        ///     Unstages any changes by moving them back to the working directory
        ///     with any other changes to the working directory being left alone.
        ///     </description>
        /// </item>
        /// <item>
        ///     <term><see cref="ResetMode.Soft"/></term>
        ///     <description>
        ///     This mode doesn't make sense when working with the HEAD commit
        ///     and does nothing.
        ///     </description>
        /// </item>
        /// </list>
        /// <para>
        /// See this for more details: <a href="https://www.atlassian.com/git/tutorials/undoing-changes/git-reset"/>
        /// </para>
        /// </remarks>
        public async Task ResetAsync(ResetMode resetMode)
        {
            await SyncContext.Clear;
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            if (resetMode == ResetMode.Soft)
            {
                return;
            }

            root.GitApi.Reset(resetMode);
        }

        /// <summary>
        /// Changes current branch HEAD commit to the specified commit and then applies
        /// any changes from this commit as specified by <paramref name="resetMode"/>.
        /// </summary>
        /// <param name="resetMode">Specifies the reset mode</param>
        /// <param name="commit">Specifies the target commit.</param>
        /// <param name="options">Optionally specifies checkout options.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <remarks>
        /// <para>
        /// This method is used to undo uncommitted changes to the local repository.
        /// This may impact the repo's working directory and staging index, depending
        /// on the <paramref name="resetMode"/> passed.
        /// </para>
        /// <list type="table">
        /// <item>
        ///     <term><see cref="ResetMode.Hard"/></term>
        ///     <description>
        ///     <para>
        ///     Moves the current branch HEAD to the specified commit and resets
        ///     the working directory and staging index to match this commit.
        ///     This effectively undoes changes to staged and tracked files.
        ///     </para>
        ///     <note>
        ///     Untracked files in the working directory are not impacted.
        ///     </note>
        ///     </description>
        /// </item>
        /// <item>
        ///     <term><see cref="ResetMode.Mixed"/></term>
        ///     <description>
        ///     Moves the current branch HEAD to the specified commit, applying any
        ///     staged changes back to the working directory.  No other modifications
        ///     will be made to the working directory.
        ///     </description>
        /// </item>
        /// <item>
        ///     <term><see cref="ResetMode.Soft"/></term>
        ///     <description>
        ///     Moves the current branch HEAD to the specified commit.  The working
        ///     directory and any staged files will remain unchanged.
        ///     </description>
        /// </item>
        /// </list>
        /// <para>
        /// See this for more details: <a href="https://www.atlassian.com/git/tutorials/undoing-changes/git-reset"/>
        /// </para>
        /// </remarks>
        public async Task ResetAsync(ResetMode resetMode, GitCommit commit, CheckoutOptions options = null)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(commit != null, nameof(commit));
            root.EnsureNotDisposed();
            root.EnsureLocalRepo();

            root.GitApi.Reset(resetMode, commit, options ?? new CheckoutOptions());
        }
    }
}
