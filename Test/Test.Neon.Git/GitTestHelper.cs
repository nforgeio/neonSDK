//-----------------------------------------------------------------------------
// FILE:        GitTestHelper.cs
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
using Neon.Git;
using Neon.IO;
using Neon.Xunit;

using Xunit;

namespace TestGit
{
    /// <summary>
    /// Git testing helpers.
    /// </summary>
    public static class GitTestHelper
    {
        /// <summary>
        /// Identifies the GitHub repo we'll use for testing the <b>Neon.Git</b> library.
        /// </summary>
        public const string RemoteTestRepo = "github.com/neontest/neon-git";

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

                using (var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, repoPath))
                {
                    var testFolder = Path.Combine(repo.LocalRepoFolder, TestFolder);

                    if (Directory.Exists(testFolder) && Directory.GetFiles(testFolder, "*", SearchOption.AllDirectories).Length > 0)
                    {
                        NeonHelper.DeleteFolderContents(testFolder);
                        Assert.True(await repo.CommitAsync("delete: accumulated test files"));
                        Assert.True(await repo.PushAsync());
                    }
                }
            }
        }

        /// <summary>
        /// Clones the test repo, checks out the master branch, and then removes any test branches
        /// (whose names  start with "testbranch-") from the local repo and pushes the changes to the
        /// remote to help prevent the accumulation of branches.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public static async Task RemoveTestBranchesAsync()
        {
            using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
            {
                var repoPath = tempFolder.Path;

                using (var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, repoPath))
                {
                    // We need to check out the remote test branches first.

                    foreach (var branch in repo.Branches
                        .Where(branch => branch.FriendlyName.StartsWith("origin/testbranch-"))
                        .ToArray())
                    {
                        await repo.CheckoutRemoteAsync(repo.NormalizeRemoteBranchName(branch.FriendlyName));
                    }

                    // Now remove the test branches.

                    await repo.CheckoutAsync("master");

                    foreach (var branch in repo.Branches
                        .Select(branch => repo.NormalizeRemoteBranchName(branch.FriendlyName))
                        .Where(branchName => branchName.StartsWith("testbranch-"))
                        .ToArray())
                    {
                        await repo.RemoveBranchAsync(branch);
                    }
                }
            }
        }

        /// <summary>
        /// Used to run a unit test in a context where <see cref="NeonHelper.ServiceContainer"/>"/> includes
        /// our <see cref="IProfileClient"/> implementation for maintainers that fetches secrets from 
        /// <b>1Password</b> via <b>neon-assistant</b>.
        /// </summary>
        /// <param name="action">The test action.</param>
        /// <returns>The tracking <see cref="Task"/>,</returns>
        public static async Task RunWithProfileClientAsync(Func<Task> action)
        {
            Covenant.Requires<ArgumentNullException>(action != null, nameof(action));

            // We're going to save and restore the ambient service container and then
            // set our [neon-assistant] profile client implementation during the test.

            var savedServices = NeonHelper.ServiceContainer.Clone();

            try
            {
                NeonHelper.ServiceContainer.AddSingleton<IProfileClient>(new MaintainerProfileClient());

                await action();
            }
            finally
            {
                NeonHelper.ServiceContainer = savedServices;
            }
        }
    }
}
