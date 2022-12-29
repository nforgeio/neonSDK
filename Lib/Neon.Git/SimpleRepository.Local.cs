//-----------------------------------------------------------------------------
// FILE:	    SimpleRepository.Local.cs
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

using GitBranch        = LibGit2Sharp.Branch;
using GitRepository    = LibGit2Sharp.Repository;
using GitSignature     = LibGit2Sharp.Signature;

namespace Neon.Git
{
    public partial class SimpleRepository
    {

        /// <summary>
        /// Clones a remote GitHub repo to a local directory.
        /// </summary>
        /// <param name="branchName">
        /// Optionally specifies the branch to be checked out after the clone operation completes.
        /// This defaults to the remote repos default branch (typically <b>main</b> or <b>master</b>).
        /// </param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the local repository already exists.</exception>
        public async Task CloneAsync(string branchName = null)
        {
            if (LocalRepo != null)
            {
                throw new InvalidOperationException($"Local repository already exists at: {LocalRepoFolder}");
            }

            var remoteUri = $"https://{RemoteRepoPath}";
            var options   = new CloneOptions() { BranchName = branchName };

            GitRepository.Clone(remoteUri, LocalRepoFolder, options);
            LocalRepo = new GitRepository(LocalRepoFolder);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Fetches information from the remote GitHub repository.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public async Task FetchAsync()
        {
            var options = new FetchOptions()
            {
                TagFetchMode        = TagFetchMode.Auto,
                Prune               = true,
                CredentialsProvider = credentialsProvider
            };

            var remote   = LocalRepo.Network.Remotes["origin"];
            var refSpecs = remote.FetchRefSpecs.Select(spec => spec.Specification);

            Commands.Fetch(LocalRepo, remote.Name, refSpecs, options, "fetching");

            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Commits any staged and pending changes to the local git repo.
        /// </summary>
        /// <param name="message">Optionally specifies the commit message.  This defaults to <b>unspecified changes"</b>.</param>
        /// <returns><c>true</c> when changes were comitted, <c>false</c> when there were no pending changes.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the operation fails.</exception>
        public async Task<bool> CommitAsync(string message = null)
        {
            message ??= "unspecified changes";

            if (!LocalRepo.IsDirty())
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
        public async Task<bool> PushAsync()
        {
            // Associate the current local branch with the remote branch with the 
            // same name.  This will cause the remote branch to be created when we
            // push below if the remote branch does not already exist.

            var remote = LocalRepo.Network.Remotes["origin"];

            if (remote == null)
            {
                throw new InvalidOperationException($"Local git repo [{LocalRepoFolder}] has no remote origin.");
            }

            var currentBranch = LocalRepo.CurrentBranch();

            if (currentBranch == null)
            {
                throw new InvalidOperationException($"Local git repo [{LocalRepoFolder}] has no checked-out branch.");
            }

            if (!currentBranch.IsTracking)
            {
                LocalRepo.Branches.Update(currentBranch,
                    branch => branch.Remote = remote.Name,
                    branch => branch.UpstreamBranch = currentBranch.CanonicalName);
            }

            // Push any local commits to the remote branch.

            var options = new PushOptions()
            {
                CredentialsProvider = credentialsProvider
            };

            if (LocalRepo.Commits.Count() == 0)
            {
                return false;
            }

            LocalRepo.Network.Push(currentBranch, options);
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Checks out a local repo branch.
        /// </summary>
        /// <param name="branchName">Specifies the local branch to be checked out.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public async Task CheckoutAsync(string branchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));

            var branch = LocalRepo.Branches[branchName];

            if (branch == null)
            {
                throw new InvalidOperationException($"Branch [{branchName}] does not exist.");
            }

            Commands.Checkout(LocalRepo, branch);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates a local branch from the current repo head commit and pushes it to the remote
        /// GitHub repo as a branch with the same name.
        /// </summary>
        /// <param name="branchName">Specifies the branch to be added.</param>
        /// <returns><c>true</c> if the branch didn't already exist and was created, <c>false</c> otherwise.</returns>
        public async Task<bool> CreateBranchAsync(string branchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));

            var created = false;
            var remote  = LocalRepo.Network.Remotes["origin"];

            if (remote == null)
            {
                throw new InvalidOperationException($"Local git repo [{LocalRepoFolder}] has no remote origin.");
            }

            await FetchAsync();

            // Create the local branch if it doesn't exist.

            var newBranch = LocalRepo.Branches[branchName];

            if (newBranch == null)
            {
                newBranch = LocalRepo.CreateBranch(branchName);
                created   = true;
            }

            // Ensure that the local branch targets a remote with the same name.

            LocalRepo.Branches.Update(newBranch,
                branch => branch.Remote = remote.Name,
                branch => branch.UpstreamBranch = newBranch.CanonicalName);

            // Push any changes.

            if (created)
            {
                var options = new PushOptions()
                {
                    CredentialsProvider = credentialsProvider
                };

                if (LocalRepo.Commits.Count() == 0)
                {
                    return false;
                }

                LocalRepo.Network.Push(newBranch, options);
            }

            return created;
        }

        /// <summary>
        /// <para>
        /// Removes a branch from local repo as well as the remote.
        /// </para>
        /// <note>
        /// <b>master</b> and <b>main</b> branches cannot be removed with this method for safety reasons.
        /// </note>
        /// </summary>
        /// <param name="branchName">Specifies the branch to be removed.</param>
        /// <returns><c>true</c> if the branch existed and was removed, <c>false</c> otherwise.</returns>
        /// <exception cref="InvalidOperationException">Thrown for <b>master</b> and <b>main</b> branches.</exception>
        public async Task<bool> RemoveBranchAsync(string branchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));

            if (branchName.Equals("master", StringComparison.InvariantCultureIgnoreCase) ||
                branchName.Equals("main", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException($"The [{branchName}] branch cannot be removed for safety reasons.");
            }

            var removed = false;
            var remote  = LocalRepo.Network.Remotes["origin"];

            if (remote == null)
            {
                throw new InvalidOperationException($"Local git repo [{LocalRepoFolder}] has no remote origin.");
            }

            await FetchAsync();

            // Remove the local branch if it exists.

            var existingBranch = LocalRepo.Branches[branchName];

            if (existingBranch != null)
            {
                LocalRepo.Branches.Remove(branchName, isRemote: false);

                removed = true;
            }

            // Remove the remote branch if it exists.

            throw new NotImplementedException("$todo(jefflill)");

            return removed;
        }
    }
}
