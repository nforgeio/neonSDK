//-----------------------------------------------------------------------------
// FILE:	    GitHubExtensions.cs
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
using System.ComponentModel.DataAnnotations;

namespace Neon.Git
{
    /// <summary>
    /// Implements GitHub client extensions.
    /// </summary>
    public static class GitHubExtensions
    {
        /// <summary>
        /// Returns the GitHub username for the current user.  This is obtained from the <b>GIHUB_USERNAME</b>
        /// environment variable which is intialized by <see cref="GitHubHelper.CreateGitHubClient(string, string, string, string)"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the username could not be located.</exception>
        private static string Username
        {
            get
            {
                var username = Environment.GetEnvironmentVariable("GITHUB_USERNAME");

                if (string.IsNullOrEmpty(username))
                {
                    throw new InvalidOperationException("Could not locate your GitHub username.  Add this as the GITHUB_USERNAME environment variable and/or to your GITHUB_PAT[email] 1Password secret.");
                }

                return username;
            }
        }

        /// <summary>
        /// Returns the GitHub PAT for the current user.  This is obtained from the <b>GITHuB_PAT</b>
        /// environment variable which is intialized by <see cref="GitHubHelper.CreateGitHubClient(string, string, string, string)"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the access token could not be located.</exception>
        private static string AccessToken
        {
            get
            {
                var accessToken = Environment.GetEnvironmentVariable("GITHUB_PAT");

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new InvalidOperationException("Could not locate your GitHub access token.  Add this as the GITHUB_PAT environment variable and/or to your GITHUB_PAT[password] 1Password secret.");
                }

                return accessToken;
            }
        }

        /// <summary>
        /// Returns the GitHub email for the current user.  This is obtained from the <b>GIHUB_EMAIL</b>
        /// environment variable which is intialized by <see cref="GitHubHelper.CreateGitHubClient(string, string, string, string)"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the email address could not be located.</exception>
        private static string Email
        {
            get
            {
                var email = Environment.GetEnvironmentVariable("GITHUB_EMAIL");

                if (string.IsNullOrEmpty(email))
                {
                    throw new InvalidOperationException("Could not locate your GitHub email address.  Add this as the GITHUB_EMAIL environment variable and/or to your GITHUB_PAT[email] 1Password secret.");
                }

                return email;
            }
        }

        /// <summary>
        /// Returns a git credentials provider for <see cref="Username"/> and <see cref="AccessToken"/>.
        /// </summary>
        private static CredentialsHandler CredentialsProvider
        {
            get
            {
                return new CredentialsHandler(
                    (url, usernameFromUrl, types) =>
                        new UsernamePasswordCredentials()
                        {
                            Username = Username,
                            Password = AccessToken
                        });
            }
        }

        /// <summary>
        /// Clones a remote GitHub repo to a local directory.
        /// </summary>
        /// <param name="githubClient">Specifies the GitHub client.</param>
        /// <param name="remoteRepo">Specifies the remote (GitHub) repository path, like: <b>[SERVER/]OWNER/REPO</b></param>
        /// <param name="localRepoPath">Specifies the local repo directory.</param>
        /// <param name="branchName">
        /// Optionally specifies the branch to be checked out after the clone operation completes.
        /// This defaults to the remote repos default branch (typically <b>main</b> or <b>master</b>).
        /// </param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public static async Task CloneAsync(this GitHubClient githubClient, string remoteRepo, string localRepoPath, string branchName = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(remoteRepo), nameof(remoteRepo));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(localRepoPath), nameof(localRepoPath));
            Covenant.Requires<ArgumentException>(Directory.Exists(localRepoPath), $"Local git repo does not exist at: {localRepoPath}");

            GitHubRepoPath.Parse(remoteRepo);     // Validate the repo path

            var remoteUri = $"https://github.com/{remoteRepo}";
            var options = new CloneOptions() { BranchName = branchName };

            GitRepository.Clone(remoteUri, localRepoPath, options);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Fetches information from the remote GitHub repository.
        /// </summary>
        /// <param name="githubClient">Specifies the GitHub client.</param>
        /// <param name="localRepoPath">Specifies the local repo directory.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public static async Task FetchAsync(this GitHubClient githubClient, string localRepoPath)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(localRepoPath), nameof(localRepoPath));
            Covenant.Requires<ArgumentException>(Directory.Exists(localRepoPath), $"Local git repo does not exist at: {localRepoPath}");

            using (var repo = new GitRepository(localRepoPath))
            {
                var options = new FetchOptions()
                {
                    TagFetchMode        = TagFetchMode.Auto,
                    Prune               = true,
                    CredentialsProvider = CredentialsProvider
                };

                var remote   = repo.Network.Remotes["origin"];
                var refSpecs = remote.FetchRefSpecs.Select(spec => spec.Specification);

                Commands.Fetch(repo, remote.Name, refSpecs, options, "fetching");

                await Task.CompletedTask;
            }
        }

        /// <summary>
        /// Returns the remote branches for a GitHub repo.
        /// </summary>
        /// <param name="githubClient">Specifies the GitHub client.</param>
        /// <param name="remoteRepo">Specifies the remote (GitHub) repository path, like: <b>[SERVER/]OWNER/REPO</b></param>
        /// <returns>The list of branches.</returns>
        public static async Task<IReadOnlyList<GitHubBranch>> ListRemoteBanchesAsync(this GitHubClient githubClient, string remoteRepo)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(remoteRepo), nameof(remoteRepo));

            var repoPath = GitHubRepoPath.Parse(remoteRepo);

            return await githubClient.Repository.Branch.GetAll(repoPath.Owner, repoPath.Repo);
        }

        /// <summary>
        /// Commits any staged and pending changes to the local git repo.
        /// </summary>
        /// <param name="githubClient">Specifies the GitHub client.</param>
        /// <param name="localRepoPath">Path to the root of the local Git repo.</param>
        /// <param name="message">Optionally specifies the commit message.  This defaults to <b>unspecified changes"</b>.</param>
        /// <returns><c>true</c> when changes were comitted, <c>false</c> when there were no pending changes.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the operation fails.</exception>
        public static async Task<bool> CommitAsync(this GitHubClient githubClient, string localRepoPath, string message = "unspecified changes")
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(localRepoPath), nameof(localRepoPath));
            Covenant.Requires<ArgumentException>(Directory.Exists(localRepoPath), $"Local git repo does not exist at: {localRepoPath}");
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(message), nameof(message));

            using (var localRepo = new GitRepository(localRepoPath))
            {
                if (!localRepo.IsDirty())
                {
                    return false;
                }

                Commands.Stage(localRepo, "*");

                var signature = new GitSignature(Username, Email, DateTimeOffset.Now);

                localRepo.Commit(message, signature, signature);
            }

            return await Task.FromResult(true);
        }

        /// <summary>
        /// <para>
        /// Fetches and pulls the changes from GitHub into the current checked-out branch within a local git repo.
        /// </para>
        /// <note>
        /// The pull operation will be aborted and rolled back for merge conflicts.
        /// Check the result status to understand what happened.
        /// </note>
        /// </summary>
        /// <param name="githubClient">Specifies the GitHub client.</param>
        /// <param name="localRepoPath">Specifies the path to the root of the local Git repo.</param>
        /// <returns>The <see cref="MergeStatus"/> for the operation.</returns>
        public static async Task<MergeStatus> PullAsync(this GitHubClient githubClient, string localRepoPath)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(localRepoPath), nameof(localRepoPath));
            Covenant.Requires<ArgumentException>(Directory.Exists(localRepoPath), $"Local git repo does not exist at: {localRepoPath}");

            using (var localRepo = new GitRepository(localRepoPath))
            {
                var options = new PullOptions()
                {
                    MergeOptions = new MergeOptions()
                    {
                        FailOnConflict = true
                    }
                };

                await githubClient.FetchAsync(localRepoPath);

                return await Task.FromResult(Commands.Pull(localRepo, new GitSignature(Username, Email, DateTimeOffset.Now), options).Status);
            }
        }

        /// <summary>
        /// Pushes any pending local commits from the checked out branch to GitHub, creating the
        /// branch on GitHub and associating the local branch when the branch doesn't already exist
        /// on GitHub.  Any remote branch created will have the same name as the local branch.
        /// </summary>
        /// <param name="githubClient">The GitHub client.</param>
        /// <param name="remoteRepo">Specifies the remote (GitHub) repository path, like: <b>[SERVER/]OWNER/REPO</b></param>
        /// <param name="localRepoPath">Path to the root of the local Git repo.</param>
        /// <returns><c>true</c> when commits were pushed, <c>false</c> when there were no pending commits.</returns>
        public static async Task<bool> PushAsync(this GitHubClient githubClient, string remoteRepo, string localRepoPath)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(remoteRepo), nameof(remoteRepo));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(localRepoPath), nameof(localRepoPath));
            Covenant.Requires<ArgumentException>(Directory.Exists(localRepoPath), $"Local git repo does not exist at: {localRepoPath}");

            GitHubRepoPath.Parse(remoteRepo);   // Validates the remote repo

            using (var localRepo = new GitRepository(localRepoPath))
            {
                // Associate the current local branch with the remote branch with the 
                // same name.  This will cause the remote branch to be created when we
                // push below if the remote branch does not already exist.

                var remote = localRepo.Network.Remotes["origin"];

                if (remote == null)
                {
                    throw new InvalidOperationException($"Local git repo [{localRepoPath}] has no remote origin.");
                }

                var currentBranch = localRepo.CurrentBranch();

                if (currentBranch == null)
                {
                    throw new InvalidOperationException($"Local git repo [{localRepoPath}] has no checked-out branch.");
                }

                if (!currentBranch.IsTracking)
                {
                    localRepo.Branches.Update(currentBranch,
                        branch => branch.Remote = remote.Name,
                        branch => branch.UpstreamBranch = currentBranch.CanonicalName);
                }

                // Push any local commits to the remote branch.

                var options = new PushOptions()
                {
                    CredentialsProvider = CredentialsProvider
                };

                if (localRepo.Commits.Count() == 0)
                {
                    return false;
                }

                localRepo.Network.Push(currentBranch, options);
            }

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Checks out a local repo branch.
        /// </summary>
        /// <param name="githubClient">Specifies the GitHub client.</param>
        /// <param name="localRepoPath">Specifies the local repo directory.</param>
        /// <param name="branchName">Specifies the local branch to be checked out.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public static async Task CheckoutAsync(this GitHubClient githubClient, string localRepoPath, string branchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(localRepoPath), nameof(localRepoPath));
            Covenant.Requires<ArgumentException>(Directory.Exists(localRepoPath), $"Local git repo does not exist at: {localRepoPath}");
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));

            using (var repo = new GitRepository(localRepoPath))
            {
                var branch = repo.Branches[branchName];

                if (branch == null)
                {
                    throw new InvalidOperationException($"Branch [{branchName}] does not exist.");
                }

                Commands.Checkout(repo, branch);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Creates a local branch from the current repo head commit and pushes it to the remote
        /// GitHub repo as a branch with the same name.
        /// </summary>
        /// <param name="githubClient">Specifies the GitHub client.</param>
        /// <param name="localRepoPath">Specifies the local repo directory.</param>
        /// <param name="branchName">Specifies the branch to be added.</param>
        /// <returns><c>true</c> if the branch didn't already exist and was created, <c>false</c> otherwise.</returns>
        public static async Task<bool> CreateBranchAsync(this GitHubClient githubClient, string localRepoPath, string branchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(localRepoPath), nameof(localRepoPath));
            Covenant.Requires<ArgumentException>(Directory.Exists(localRepoPath), $"Local git repo does not exist at: {localRepoPath}");
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));

            var created = false;

            using (var localRepo = new GitRepository(localRepoPath))
            {
                var remote = localRepo.Network.Remotes["origin"];

                if (remote == null)
                {
                    throw new InvalidOperationException($"Local git repo [{localRepoPath}] has no remote origin.");
                }

                // Fetch so we'll see all of the 

                await githubClient.FetchAsync(localRepoPath);

                // Create the local branch if it doesn't exist.

                var newBranch = localRepo.Branches[branchName];

                if (newBranch == null)
                {
                    newBranch = localRepo.CreateBranch(branchName);
                    created = true;
                }

                // Ensure that the local branch targets a remote with the same name.

                localRepo.Branches.Update(newBranch,
                    branch => branch.Remote = remote.Name,
                    branch => branch.UpstreamBranch = newBranch.CanonicalName);

                // Push any changes.

                if (created)
                {
                    var options = new PushOptions()
                    {
                        CredentialsProvider = CredentialsProvider
                    };

                    if (localRepo.Commits.Count() == 0)
                    {
                        return false;
                    }

                    localRepo.Network.Push(newBranch, options);
                }
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
        /// <param name="githubClient">Specifies the GitHub client.</param>
        /// <param name="localRepoPath">Specifies the local repo directory.</param>
        /// <param name="branchName">Specifies the branch to be removed.</param>
        /// <returns><c>true</c> if the branch existed and was removed, <c>false</c> otherwise.</returns>
        /// <exception cref="InvalidOperationException">Thrown for <b>master</b> and <b>main</b> branches.</exception>
        public static async Task<bool> RemoveBranchAsync(this GitHubClient githubClient, string localRepoPath, string branchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(localRepoPath), nameof(localRepoPath));
            Covenant.Requires<ArgumentException>(Directory.Exists(localRepoPath), $"Local git repo does not exist at: {localRepoPath}");
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));

            if (branchName.Equals("master", StringComparison.InvariantCultureIgnoreCase) ||
                branchName.Equals("main", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new InvalidOperationException($"The [{branchName}] branch cannot be removed for safety reasons.");
            }

            var removed = false;

            using (var localRepo = new GitRepository(localRepoPath))
            {
                var remote = localRepo.Network.Remotes["origin"];

                if (remote == null)
                {
                    throw new InvalidOperationException($"Local git repo [{localRepoPath}] has no remote origin.");
                }

                await githubClient.FetchAsync(localRepoPath);

                // Remove the local branch if it exists.

                var existingBranch = localRepo.Branches[branchName];

                if (existingBranch != null)
                {
                    localRepo.Branches.Remove(branchName, isRemote: false);

                    removed = true;
                }

                // Remove the remote branch if it exists.

                throw new NotImplementedException("$todo(jefflill)");
            }

            return removed;
        }
    }
}
