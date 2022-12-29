//-----------------------------------------------------------------------------
// FILE:        Test_Git.cs
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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using LibGit2Sharp;

using Neon.Common;
using Neon.Git;
using Neon.IO;
using Neon.Xunit;

using Xunit;

namespace TestGit
{
    [Trait(TestTrait.Category, TestArea.NeonGit)]
    public class Test_Git
    {
        /// <summary>
        /// Identifies the GitHub repo we'll use for testing the <b>Neon.Git</b> library.
        /// </summary>
        private const string RemoteTestRepo = "neontest/neon-git";

        private string saveUsername;
        private string saveAccessToken;
        private string saveEmail;
        private string saveNcUser;

        public Test_Git()
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
        /// Saves and then clears the credential environment variables including the
        /// <b>NC_USER variable which is used by NEONFORGE maintainers to identify
        /// the developer's NEONFORGE.COM Office username which is also used to 
        /// identify the developer's 1Password vault.
        /// </summary>
        private void SaveAndClearEnvCredentials()
        {
            saveUsername    = Environment.GetEnvironmentVariable("GITHUB_USERNAME");
            saveAccessToken = Environment.GetEnvironmentVariable("GITHUB_PAT");
            saveEmail       = Environment.GetEnvironmentVariable("GITHUB_EMAIL");
            saveNcUser      = Environment.GetEnvironmentVariable("NC_USER");

            Environment.SetEnvironmentVariable("GITHUB_USERNAME", null);
            Environment.SetEnvironmentVariable("GITHUB_PAT", null);
            Environment.SetEnvironmentVariable("GITHUB_EMAIL", null);
            Environment.SetEnvironmentVariable("NC_USER", null);
        }

        /// <summary>
        /// Restores the credential environment variables.
        /// </summary>
        private void RestoreEnvCredentials()
        {
            Environment.SetEnvironmentVariable("GITHUB_USERNAME", saveUsername);
            Environment.SetEnvironmentVariable("GITHUB_PAT", saveAccessToken);
            Environment.SetEnvironmentVariable("GITHUB_EMAIL", saveEmail);
            Environment.SetEnvironmentVariable("NC_USER", saveNcUser);
        }

        [MaintainerFact]
        public void Credentials_FromEnvironment()
        {
            // Ensure that [InvalidOperationException] is thrown when one or more
            // credential parts are not present as environment variables and [NC_USER]
            // does not exist, which disables 1Password lookups.
            //
            // Note that we're exercising the internal [GitHubHelper.GitHubCredentials]
            // class instead of calling [] GitHubHelper.CreateGitHubClient()] directly
            // to have more control over the test scenarios.

            try
            {
                // Expecting failure with no credential variables.

                SaveAndClearEnvCredentials();
                Assert.Throws<InvalidOperationException>(() => GitHubHelper.GitHubCredentials.Load());

                // Expecting failure with only: GITHUB_USERNAME

                Environment.SetEnvironmentVariable("GITHUB_USERNAME", saveUsername);
                Assert.Throws<InvalidOperationException>(() => GitHubHelper.GitHubCredentials.Load());

                // Expecting failure with only: GITHUB_USERNAME and GITHUB_PAT

                Environment.SetEnvironmentVariable("GITHUB_PAT", saveAccessToken);
                Assert.Throws<InvalidOperationException>(() => GitHubHelper.GitHubCredentials.Load());

                // Expecting success with all credential veriables.

                Environment.SetEnvironmentVariable("GITHUB_EMAIL", saveEmail);

                var credentials = GitHubHelper.GitHubCredentials.Load();

                Assert.Equal(saveUsername, credentials.Username);
                Assert.Equal(saveAccessToken, credentials.AccessToken);
                Assert.Equal(saveEmail, credentials.Email);

                Assert.Equal(credentials.Username, Environment.GetEnvironmentVariable("GITHUB_USERNAME"));
                Assert.Equal(credentials.AccessToken, Environment.GetEnvironmentVariable("GITHUB_PAT"));
                Assert.Equal(credentials.Email, Environment.GetEnvironmentVariable("GITHUB_EMAIL"));
            }
            finally
            {
                RestoreEnvCredentials();
            }
        }

        [MaintainerFact]
        public void Credentials_FromSecrets()
        {
            // Ensure that we can retrieve credentials as secrets when the
            // environment variables are not present.

            try
            {
                // Expecting failure with no credential variables.

                SaveAndClearEnvCredentials();
                Environment.SetEnvironmentVariable("NC_USER", saveNcUser);

                var credentials = GitHubHelper.GitHubCredentials.Load();

                Assert.NotEmpty(credentials.Username);
                Assert.NotEmpty(credentials.AccessToken);
                Assert.NotEmpty(credentials.Email);

                Assert.Equal(credentials.Username, Environment.GetEnvironmentVariable("GITHUB_USERNAME"));
                Assert.Equal(credentials.AccessToken, Environment.GetEnvironmentVariable("GITHUB_PAT"));
                Assert.Equal(credentials.Email, Environment.GetEnvironmentVariable("GITHUB_EMAIL"));
            }
            finally
            {
                RestoreEnvCredentials();
            }
        }

        [MaintainerFact]
        public void Credentials_FromMixed()
        {
            // Ensure that we can retrieve credentials using a mix of
            // environment variables and secrets.

            try
            {
                // Expecting failure with no credential variables.

                const string testUsername = "TEST_USERNAME_122737373";

                SaveAndClearEnvCredentials();
                Environment.SetEnvironmentVariable("NC_USER", saveNcUser);
                Environment.SetEnvironmentVariable("GITHUB_USERNAME", testUsername);

                var credentials = GitHubHelper.GitHubCredentials.Load();

                Assert.Equal(testUsername, credentials.Username);
                Assert.NotEmpty(credentials.AccessToken);
                Assert.NotEmpty(credentials.Email);

                Assert.Equal(credentials.Username, Environment.GetEnvironmentVariable("GITHUB_USERNAME"));
                Assert.Equal(credentials.AccessToken, Environment.GetEnvironmentVariable("GITHUB_PAT"));
                Assert.Equal(credentials.Email, Environment.GetEnvironmentVariable("GITHUB_EMAIL"));
            }
            finally
            {
                RestoreEnvCredentials();
            }
        }

        [MaintainerFact]
        public void Credentials_Explicit()
        {
            // Ensure that we can can explicitly specify the credentials.

            try
            {
                // Expecting failure with no credential variables.

                const string testUsername = "TEST_USERNAME_122737373";

                SaveAndClearEnvCredentials();

                var credentials = GitHubHelper.GitHubCredentials.Load(
                    username:    testUsername,
                    accessToken: "1234567890",
                    email:       "sally@test.com");

                Assert.Equal(testUsername, credentials.Username);
                Assert.Equal("1234567890", credentials.AccessToken);
                Assert.Equal("sally@test.com", credentials.Email);

                Assert.Equal(credentials.Username, Environment.GetEnvironmentVariable("GITHUB_USERNAME"));
                Assert.Equal(credentials.AccessToken, Environment.GetEnvironmentVariable("GITHUB_PAT"));
                Assert.Equal(credentials.Email, Environment.GetEnvironmentVariable("GITHUB_EMAIL"));
            }
            finally
            {
                RestoreEnvCredentials();
            }
        }

        [MaintainerFact]
        public async Task Clone()
        {
            // Verify that we can clone the repo to a temporary local folder.

            using (var tempFolder = new TempFolder())
            {
                var repoPath = tempFolder.Path;
                var github   = GitHubHelper.CreateGitHubClient();

                await github.CloneAsync(RemoteTestRepo, repoPath, "master");
                Assert.True(File.Exists(Path.Combine(repoPath, ".gitignore")));
            }
        }

        [MaintainerFact]
        public async Task Fetch()
        {
            // Verify that we can fetch remote info for a local repo
            // without crashing.

            using (var tempFolder = new TempFolder())
            {
                var repoPath = tempFolder.Path;
                var github   = GitHubHelper.CreateGitHubClient();

                await github.CloneAsync(RemoteTestRepo, repoPath, "master");
                Assert.True(File.Exists(Path.Combine(repoPath, ".gitignore")));
                await github.FetchAsync(repoPath);
            }
        }

        [MaintainerFact]
        public async Task CommitPushPull()
        {
            // Here's what we're going to do:
            //
            //       1. Clone the remote repo to two local folders
            //       2. Create a new text file named with GUID to the first repo
            //       3. Commit the change and push to the remote
            //       4. Pull the second repo from the remote
            //       5. Confirm that the second repo has the new file
            //       6. Remove the file in the second repo
            //       7. Commit and push the second repo to the remote
            //       8. Go back to the first repo and pull changes from the remote
            //       9. Confirm that the new file no longer exists
            //      10. Delete both local repo folders

            using (var tempFolder1 = new TempFolder(prefix: "repo1-"))
            {
                using (var tempFolder2 = new TempFolder(prefix: "repo2-"))
                {
                    var repoPath1 = tempFolder1.Path;
                    var repoPath2 = tempFolder2.Path;
                    var github    = GitHubHelper.CreateGitHubClient();

                    // Clone the remote repo to two local folders:

                    await github.CloneAsync(RemoteTestRepo, repoPath1, "master");
                    await github.CloneAsync(RemoteTestRepo, repoPath2, "master");

                    // Create a new text file named with GUID to the first repo
                    // and commit and push the change to the remote:

                    var testFolder   = "test";
                    var testFileName = Path.Combine(testFolder, $"{Guid.NewGuid()}.txt");
                    var testPath1    = Path.Combine(repoPath1, testFileName);
                    var testPath2    = Path.Combine(repoPath2, testFileName);

                    Directory.CreateDirectory(Path.Combine(repoPath1, testFolder));
                    File.WriteAllText(testPath1, "HELLO WORLD!");
                    Assert.True(await github.CommitAsync(repoPath1, "add: test file"));
                    Assert.True(await github.PushAsync(RemoteTestRepo, repoPath1));

                    // Pull the second repo from the remote:

                    Assert.Equal(MergeStatus.FastForward, await github.PullAsync(repoPath2));

                    // Confirm that the second repo has the new file:

                    Assert.True(File.Exists(testPath2));
                    Assert.Equal("HELLO WORLD!", File.ReadAllText(testPath2));

                    // Remove the file in the second repo and then commit and
                    // push to the remote:

                    File.Delete(testPath2);
                    Assert.True(await github.CommitAsync(repoPath2, "delete: test file"));
                    Assert.True(await github.PushAsync(RemoteTestRepo, repoPath2));

                    // Go back to the first repo and pull changes from the remote 
                    // and confirm that the file no longer exists:

                    Assert.Equal(MergeStatus.FastForward, await github.PullAsync(repoPath1));
                    Assert.False(File.Exists(testPath1));

                    // It's possible for test files to accumulate when past unit tests were
                    // aborted or failed.  We're going to remove any test files in the first
                    // repo and then commit/push any changes to GitHub to address this.

                    var testFolder1 = Path.Combine(repoPath1, testFolder);

                    if (Directory.Exists(testFolder1) && Directory.GetFiles(testFolder1, "*", SearchOption.AllDirectories).Length > 0)
                    {
                        NeonHelper.DeleteFolderContents(testFolder1);
                        Assert.True(await github.CommitAsync(repoPath1, "delete: accumulated test files"));
                        Assert.True(await github.PushAsync(RemoteTestRepo, repoPath2));
                    }
                }
            }
            }

        [MaintainerFact]
        public async Task CreateRemoveBranch()
        {
            // Verify that we can create a local branch, push it to GitHub,
            // and then remove both the local and remote branches.

            using (var tempFolder = new TempFolder(prefix: "repo-"))
            {
                var repoPath      = tempFolder.Path;
                var newBranchName = $"testbranch-{Guid.NewGuid()}";
                var github        = GitHubHelper.CreateGitHubClient();

                //await github.CreateLocalBranchAsync(repoPath, newBranchName);

                //await github.RemoveBranchAsync(repoPath, newBranchName);
            }
        }
    }
}
