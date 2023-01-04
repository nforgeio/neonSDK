//-----------------------------------------------------------------------------
// FILE:	    GitHubRepo.Local.cs
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
    public partial class GitHubRepo
    {
        /// <summary>
        /// Returns the current branch.
        /// </summary>
        public GitBranch CurrentBranch => LocalRepository.CurrentBranch();

        /// <summary>
        /// Fetches information from the associated GitHub origin repository.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task FetchAsync()
        {
            EnsureNotDisposed();
            EnsureLocalRepo();

            var options = new FetchOptions()
            {
                TagFetchMode        = TagFetchMode.Auto,
                Prune               = true,
                CredentialsProvider = credentialsProvider
            };

            var refSpecs = OriginRemote.FetchRefSpecs.Select(spec => spec.Specification);

            Commands.Fetch(LocalRepository, OriginRemote.Name, refSpecs, options, "fetching");

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
            EnsureNotDisposed();
            EnsureLocalRepo();

            message ??= "unspecified changes";

            if (!IsDirty)
            {
                return false;
            }

            Commands.Stage(LocalRepository, "*");

            var signature = CreateSignature();

            LocalRepository.Commit(message, signature, signature);

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
            EnsureNotDisposed();
            EnsureLocalRepo();

            var options = new PullOptions()
            {
                MergeOptions = new MergeOptions()
                {
                    FailOnConflict = true
                }
            };

            await FetchAsync();

            return await Task.FromResult(Commands.Pull(LocalRepository, CreateSignature(), options).Status);
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
            EnsureNotDisposed();
            EnsureLocalRepo();

            // Associate the current local branch with the origin branch having the 
            // same name.  This will cause the origin branch to be created when we
            // push below if the origin branch does not already exist.

            var currentBranch = LocalRepository.CurrentBranch();

            if (currentBranch == null)
            {
                throw new LibGit2SharpException($"Local git repository [{LocalRepoFolder}] has no checked-out branch.");
            }

            if (!currentBranch.IsTracking)
            {
                LocalRepository.Branches.Update(currentBranch,
                    updater => updater.Remote = OriginRemote.Name,
                    updater => updater.UpstreamBranch = currentBranch.CanonicalName);
            }

            // Push any local commits to the origin branch.

            if (LocalRepository.Commits.Count() == 0)
            {
                return false;
            }

            LocalRepository.Network.Push(currentBranch, CreatePushOptions());

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
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            EnsureNotDisposed();
            EnsureLocalRepo();

            var branch = LocalRepository.Branches[branchName];

            if (branch == null)
            {
                throw new LibGit2SharpException($"Branch [{branchName}] does not exist.");
            }

            Commands.Checkout(LocalRepository, branch);
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
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(sourceBranchName), nameof(sourceBranchName));
            EnsureNotDisposed();
            EnsureLocalRepo();

            var newBranch = LocalRepository.Branches[branchName];

            if (newBranch != null)
            {
                await CheckoutAsync(branchName);

                return false;
            }

            var sourceBranch = LocalRepository.Branches[sourceBranchName];

            if (sourceBranch == null)
            {
                throw new LibGit2SharpException($"Source branch [{sourceBranchName}] does not exist.");
            }
             
            LocalRepository.CreateBranch(branchName, sourceBranch.Tip);
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
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(originBranchName), nameof(originBranchName));
            EnsureNotDisposed();
            EnsureLocalRepo();

            branchName ??= originBranchName;

            var created = LocalRepository.Branches[branchName] == null;

            if (created)
            {
                LocalRepository.CreateBranch(branchName, $"{OriginRemote.Name}/{originBranchName}");
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
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            EnsureNotDisposed();
            EnsureLocalRepo();

            // Remove the origin branch.

            LocalRepository.Network.Push(OriginRemote, $"+:refs/heads/{branchName}", CreatePushOptions());

            // Remove the local branch.

            LocalRepository.Branches.Remove(branchName);

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
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            EnsureNotDisposed();
            EnsureLocalRepo();

            var branch = LocalRepository.Branches[branchName];

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

            var result = LocalRepository.Merge(branch, CreateSignature());

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
            EnsureNotDisposed();
            EnsureLocalRepo();

            LocalRepository.CheckoutPaths(CurrentBranch.Tip.Sha, new string[] { "*" }, new CheckoutOptions() { CheckoutModifiers = CheckoutModifiers.Force });
            LocalRepository.RemoveUntrackedFiles();

            await Task.CompletedTask;
        }
    }
}
