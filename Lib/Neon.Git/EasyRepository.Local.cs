//-----------------------------------------------------------------------------
// FILE:	    EasyRepository.Local.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
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
    public partial class EasyRepository
    {
        /// <summary>
        /// Returns the current branch.
        /// </summary>
        public GitBranch CurrentBranch => LocalRepo.CurrentBranch();

        /// <summary>
        /// Fetches information from the remote GitHub repository.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task FetchAsync()
        {
            var options = new FetchOptions()
            {
                TagFetchMode        = TagFetchMode.Auto,
                Prune               = true,
                CredentialsProvider = credentialsProvider
            };

            var refSpecs = Origin.FetchRefSpecs.Select(spec => spec.Specification);

            Commands.Fetch(LocalRepo, Origin.Name, refSpecs, options, "fetching");

            await Task.CompletedTask;
        }

        /// <summary>
        /// Commits any staged and pending changes to the local git repo.
        /// </summary>
        /// <param name="message">Optionally specifies the commit message.  This defaults to <b>unspecified changes"</b>.</param>
        /// <returns><c>true</c> when changes were comitted, <c>false</c> when there were no pending changes.</returns>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<bool> CommitAsync(string message = null)
        {
            message ??= "unspecified changes";

            if (!IsDirty)
            {
                return false;
            }

            Commands.Stage(LocalRepo, "*");

            var signature = CreateSignature();

            LocalRepo.Commit(message, signature, signature);

            return await Task.FromResult(true);
        }

        /// <summary>
        /// <para>
        /// Fetches and pulls the changes from GitHub into the current checked-out branch within a local git repo.
        /// </para>
        /// <note>
        /// The pull operation will be aborted and rolled back for merge conflicts.  Check the result status
        /// to understand what happened.
        /// </note>
        /// </summary>
        /// <returns>The <see cref="MergeStatus"/> for the operation.</returns>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<MergeStatus> PullAsync()
        {
            var options = new PullOptions()
            {
                MergeOptions = new MergeOptions()
                {
                    FailOnConflict = true
                }
            };

            await FetchAsync();

            return await Task.FromResult(Commands.Pull(LocalRepo, CreateSignature(), options).Status);
        }


        /// <summary>
        /// Pushes any pending local commits from the checked out branch to GitHub, creating the
        /// branch on GitHub and associating the local branch when the branch doesn't already exist
        /// on GitHub.  Any remote branch created will have the same name as the local branch.
        /// </summary>
        /// <returns><c>true</c> when commits were pushed, <c>false</c> when there were no pending commits.</returns>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<bool> PushAsync()
        {
            // Associate the current local branch with the remote branch with the 
            // same name.  This will cause the remote branch to be created when we
            // push below if the remote branch does not already exist.

            var currentBranch = LocalRepo.CurrentBranch();

            if (currentBranch == null)
            {
                throw new LibGit2SharpException($"Local git repo [{LocalRepoFolder}] has no checked-out branch.");
            }

            if (!currentBranch.IsTracking)
            {
                LocalRepo.Branches.Update(currentBranch,
                    updater => updater.Remote = Origin.Name,
                    updater => updater.UpstreamBranch = currentBranch.CanonicalName);
            }

            // Push any local commits to the remote branch.

            if (LocalRepo.Commits.Count() == 0)
            {
                return false;
            }

            LocalRepo.Network.Push(currentBranch, CreatePushOptions());

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Checks out a local repo branch.
        /// </summary>
        /// <param name="branchName">Specifies the local branch to be checked out.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task CheckoutAsync(string branchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));

            var branch = LocalRepo.Branches[branchName];

            if (branch == null)
            {
                throw new LibGit2SharpException($"Branch [{branchName}] does not exist.");
            }

            Commands.Checkout(LocalRepo, branch);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates a new local branch from the tip of a source branch if the new branch
        /// doesn't already exist and then checks out the new branch.
        /// </summary>
        /// <param name="branchName">Identifies the branch to being created.</param>
        /// <param name="sourceBranchName">Identifies the source branch.</param>
        /// <returns><c>true</c> if the branch didn't already exist and was created, <c>false</c> otherwise.</returns>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<bool> CreateBranchAsync(string branchName, string sourceBranchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(sourceBranchName), nameof(sourceBranchName));

            var newBranch = LocalRepo.Branches[branchName];

            if (newBranch != null)
            {
                await CheckoutAsync(branchName);

                return false;
            }

            var sourceBranch = LocalRepo.Branches[sourceBranchName];

            if (sourceBranch == null)
            {
                throw new LibGit2SharpException($"Source branch [{sourceBranchName}] does not exist.");
            }
             
            LocalRepo.CreateBranch(branchName, sourceBranch.Tip);
            await CheckoutAsync(branchName);

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Creates a local branch from a named remote branch and then checks out the branch.
        /// By default, the local branch will have the same name as the remote, but this can 
        /// be customized.
        /// </summary>
        /// <param name="remoteBranchName">Specifies the remote branch name.</param>
        /// <param name="branchName">Optionally specifies the local branch name.  This defaults to <paramref name="remoteBranchName"/>.</param>
        /// <returns><c>true</c> if the local branch didn't already exist and was created from the remote, <c>false</c> otherwise.</returns>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<bool> CheckoutRemoteAsync(string remoteBranchName, string branchName = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(remoteBranchName), nameof(remoteBranchName));

            branchName ??= remoteBranchName;

            var created = LocalRepo.Branches[branchName] == null;

            if (created)
            {
                LocalRepo.CreateBranch(branchName, $"{Origin.Name}/{remoteBranchName}");
            }

            await CheckoutAsync(branchName);

            return created;
        }

        /// <summary>
        /// Removes a branch from local repo as well as the from the remote, if they exist.
        /// </summary>
        /// <param name="branchName">Specifies the branch to be removed.</param>
        /// <returns><c>true</c> if the branch existed and was removed, <c>false</c> otherwise.</returns>
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task RemoveBranchAsync(string branchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));

            // Remove the remote branch.

            LocalRepo.Network.Push(Origin, $"+:refs/heads/{branchName}", CreatePushOptions());

            // Remove the local branch.

            LocalRepo.Branches.Remove(branchName);

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
        /// <exception cref="LibGit2SharpException">Thrown if the operation fails.</exception>
        public async Task<MergeResult> MergeAsync(string branchName, bool throwOnConflict = true)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));

            var branch = LocalRepo.Branches[branchName];

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

            var result = LocalRepo.Merge(branch, CreateSignature());

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
        /// Reverts any uncommitted changes in the current local repo branch.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public async Task UndoAsync()
        {
            LocalRepo.CheckoutPaths(CurrentBranch.Tip.Sha, new string[] { "*" }, new CheckoutOptions() { CheckoutModifiers = CheckoutModifiers.Force });
            LocalRepo.RemoveUntrackedFiles();

            await Task.CompletedTask;
        }
    }
}
