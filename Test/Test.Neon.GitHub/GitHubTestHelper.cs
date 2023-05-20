//-----------------------------------------------------------------------------
// FILE:        GitHubTestHelper.cs
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
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using LibGit2Sharp;

using Microsoft.Extensions.DependencyInjection;

using Neon.Common;
using Neon.Deployment;
using Neon.GitHub;
using Neon.IO;
using Neon.Xunit;
using Octokit;
using Xunit;

namespace TestGitHub
{
    /// <summary>
    /// Git testing helpers.
    /// </summary>
    public static class GitHubTestHelper
    {
        /// <summary>
        /// Specifies the name of the remote repo used for testing.
        /// </summary>
        public const string RemoteRepoName = "neon-git";

        /// <summary>
        /// Specifies the path for GitHub repo we'll use for testing the <b>Neon.Git</b> library.
        /// </summary>
        public const string RemoteTestRepoPath = $"github.com/neontest/{RemoteRepoName}";

        /// <summary>
        /// Name of the folder used for managing test files.
        /// </summary>
        public const string TestFolder = "test";

        /// <summary>
        /// Ensures that the current user appears to be a NTONFORGE maintainer.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the current user doesn't appear to be a maintainer.</exception>
        public static void EnsureMaintainer()
        {
            // We're expecting maintainers to have environment variables holding
            // the GitHub credential components.

            if (NeonHelper.IsMaintainer)
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_USERNAME")) ||
                    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_PAT")) ||
                    string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_EMAIL")))
                {
                    throw new InvalidOperationException("NEONFORGE maintainers are expected to have these environment variables: GITHUB_USERNAME, GITHUB_PAT, and GITHUB_EMAIL");
                }
            }
        }

        /// <summary>
        /// Clones the test repo, checks out the master branch, and then removes any files in
        /// the repo under the <see cref="TestFolder"/> directory (if it exists) and pushes
        /// the changes to the remote.  This is used to help prevent the accumulation of test files.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public static async Task RemoveTestFilesAsync()
        {
            using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
            {
                var repoPath = tempFolder.Path;

                using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                {
                    var testFolder = Path.Combine(repo.Local.Folder, TestFolder);

                    if (Directory.Exists(testFolder) && Directory.GetFiles(testFolder, "*", SearchOption.AllDirectories).Length > 0)
                    {
                        NeonHelper.DeleteFolderContents(testFolder);
                        Assert.True(await repo.Local.CommitAsync("delete: accumulated test files"));
                        Assert.True(await repo.Local.PushAsync());
                    }
                }
            }
        }

        /// <summary>
        /// Removes any test branches from the GitHub repository.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public static async Task RemoveTestBranchesAsync()
        {
            using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
            {
                var repoPath = tempFolder.Path;

                using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                {
                    foreach (var branch in repo.GitApi.Branches
                        .Select(branch => repo.NormalizeBranchName(branch.FriendlyName))
                        .Where(branchName => branchName.StartsWith("testbranch-"))
                        .ToArray())
                    {
                        try
                        {
                            await repo.Remote.Branch.RemoveAsync(branch);
                        }
                        catch (Octokit.ApiValidationException)
                        {
                            // This can happen when a unit test locked a branch but didn't have the
                            // chance to be unlocked due to a test failure or debugging.
                            //
                            // Ww'll address this by removing all branch protections and trying again.

                            await repo.Remote.Branch.DeleteBranchProtection(branch);
                            await repo.Remote.Branch.RemoveAsync(branch);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes any releases from the GitHub repository.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public static async Task RemoveTestReleasesAsync()
        {
            using (var repo = await GitHubRepo.ConnectAsync(GitHubTestHelper.RemoteTestRepoPath))
            {
                foreach (var release in await repo.Remote.Release.GetAllAsync())
                {
                    await repo.Remote.Release.DeleteAsync(release.Name);
                }
            }
        }

        /// <summary>
        /// Removes any tags from the GitHub repository.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public static async Task RemoveTestTagsAsync()
        {
            using (var repo = await GitHubRepo.ConnectAsync(GitHubTestHelper.RemoteTestRepoPath))
            {
                foreach (var tag in await repo.Remote.Tag.GetAllAsync())
                {
                    await repo.Remote.Tag.RemoveAsync(tag.Name);
                }
            }
        }

        /// <summary>
        /// Closes any open issues.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public static async Task CloseIssuesAsync()
        {
            using (var repo = await GitHubRepo.ConnectAsync(GitHubTestHelper.RemoteTestRepoPath))
            {
                foreach (var issue in await repo.Remote.Issue.GetAllAsync())
                {
                    var update = issue.ToUpdate();

                    update.State = ItemState.Closed;

                    await repo.Remote.Issue.UpdateAsync(issue.Number, update);
                }
            }
        }

        /// <summary>
        /// <para>
        /// Used to run a unit test in a context where <see cref="NeonHelper.ServiceContainer"/>"/> includes
        /// our <see cref="IProfileClient"/> implementation for maintainers that fetches secrets from 
        /// <b>1Password</b> via <b>neon-assistant</b>.  This also saves and restores the ambient maintainer
        /// GITHUB related environment variables.
        /// </para>
        /// <para>
        /// This also ensures that there are no test release branches or test releases before and
        /// after the test is executed.
        /// </para>
        /// </summary>
        /// <param name="action">The test action.</param>
        /// <returns>The tracking <see cref="Task"/>,</returns>
        public static async Task RunTestAsync(Func<Task> action)
        {
            Covenant.Requires<ArgumentNullException>(action != null, nameof(action));

            // We're going to save and restore the ambient service container and then
            // set our [neon-assistant] profile client implementation during the test.

            var savedServices  = NeonHelper.ServiceContainer.Clone();

            try
            {
                NeonHelper.ServiceContainer.AddSingleton<IProfileClient>(new MaintainerProfile());

                await RemoveTestBranchesAsync();
                await RemoveTestReleasesAsync();
                await RemoveTestTagsAsync();
                await CloseIssuesAsync();

                await action();
            }
            finally
            {
                NeonHelper.ServiceContainer = savedServices;

                await RemoveTestBranchesAsync();
                await RemoveTestReleasesAsync();
                await RemoveTestTagsAsync();
                await CloseIssuesAsync();
            }
        }
    }
}
