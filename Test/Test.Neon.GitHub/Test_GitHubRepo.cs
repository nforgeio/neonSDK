//-----------------------------------------------------------------------------
// FILE:        Test_GitHubRepo.cs
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
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using LibGit2Sharp;

using Neon.Common;
using Neon.Deployment;
using Neon.GitHub;
using Neon.IO;
using Neon.Xunit;

using Octokit;

using Xunit;

namespace TestGitHub
{
    [Trait(TestTrait.Category, TestArea.NeonGit)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public class Test_GitHubRepo
    {
        public Test_GitHubRepo()
        {
            GitHubTestHelper.EnsureMaintainer();
        }

        [MaintainerFact]
        public async Task Clone_Public()
        {
            // Verify that we can clone a public repo to a temporary local folder.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath = tempFolder.Path;

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            Assert.Equal(GitHubTestHelper.RemoteTestRepoPath, repo.Remote.Path.ToString());
                            Assert.Equal("master", repo.Local.CurrentBranch.FriendlyName);
                            Assert.True(File.Exists(Path.Combine(repoPath, ".gitignore")));
                            Assert.Equal(GitHubTestHelper.RemoteRepoName, repo.Name);
                            Assert.Equal(GitHubTestHelper.RemoteRepoName, repo.Remote.Name);

                            // Exercise some other APIs.

                            Assert.Equal("https://github.com/neontest/neon-git.git", repo.Origin.Url);
                            Assert.Equal("https://github.com/neontest/neon-git.git", repo.Origin.PushUrl);
                            Assert.Equal("https://github.com/neontest/", repo.Remote.BaseUri);

                            var validLocalPath = Path.Combine(repo.Local.Folder, "test", "foo.txt");

                            Assert.Equal(validLocalPath, await repo.Local.GetLocalFilePathAsync(@"/test/foo.txt"));
                            Assert.Equal(validLocalPath, await repo.Local.GetLocalFilePathAsync(@"test/foo.txt"));
                            Assert.Equal(validLocalPath, await repo.Local.GetLocalFilePathAsync(@"\test\foo.txt"));
                            Assert.Equal(validLocalPath, await repo.Local.GetLocalFilePathAsync(@"test\foo.txt"));

                            var validRemoteUri = $"{repo.Remote.BaseUri}{repo.Remote.Name}/blob/{repo.Local.CurrentBranch.FriendlyName}/test/foo.txt";

                            Assert.Equal(validRemoteUri, await repo.Local.GetRemoteFileUriAsync(@"/test/foo.txt"));
                            Assert.Equal(validRemoteUri, await repo.Local.GetRemoteFileUriAsync(@"test/foo.txt"));
                            Assert.Equal(validRemoteUri, await repo.Local.GetRemoteFileUriAsync(@"\test\foo.txt"));
                            Assert.Equal(validRemoteUri, await repo.Local.GetRemoteFileUriAsync(@"test\foo.txt"));

                            var validRawRemoteUri = $"https://raw.githubusercontent.com/{repo.Remote.Owner}/{repo.Remote.Name}/{repo.Local.CurrentBranch.FriendlyName}/test/foo.txt";

                            Assert.Equal(validRawRemoteUri, await repo.Local.GetRemoteFileUriAsync(@"/test/foo.txt", raw: true));
                            Assert.Equal(validRawRemoteUri, await repo.Local.GetRemoteFileUriAsync(@"test/foo.txt", raw: true));
                            Assert.Equal(validRawRemoteUri, await repo.Local.GetRemoteFileUriAsync(@"\test\foo.txt", raw: true));
                            Assert.Equal(validRawRemoteUri, await repo.Local.GetRemoteFileUriAsync(@"test\foo.txt", raw: true));
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Clone_Private()
        {
            // Verify that we can clone a private repo to a temporary local folder.

            using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
            {
                var repoPath = tempFolder.Path;

                using (var repo = await GitHubRepo.CloneAsync("github.com/nforgeio/neon-devops", repoPath))
                {
                    Assert.True(File.Exists(Path.Combine(repoPath, "README.md")));

                    await repo.Local.PullAsync();
                }
            }
        }

        [MaintainerFact]
        public async Task Open()
        {
            // Verify that we can open an existing local repo.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    // Verify that we can open an existing local repo.

                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath = tempFolder.Path;

                        // Clone the initial repo and then dispose it.

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            Assert.Equal(GitHubTestHelper.RemoteTestRepoPath, repo.Remote.Path.ToString());
                            Assert.Equal("master", repo.Local.CurrentBranch.FriendlyName);
                            Assert.True(File.Exists(Path.Combine(repoPath, ".gitignore")));
                        }

                        // Verify that we can reopen the existing repo.

                        using (var repo = await GitHubRepo.OpenAsync(repoPath))
                        {
                            Assert.Equal("master", repo.Local.CurrentBranch.FriendlyName);
                            Assert.True(File.Exists(Path.Combine(repoPath, ".gitignore")));
                            Assert.Equal(RemoteRepoPath.Parse(GitHubTestHelper.RemoteTestRepoPath).ToString(), repo.Remote.Path.ToString());
                        }
                    }

                    // Verify that we see an exception when trying to open a local repo
                    // folder that doesn't exist.

                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        await Assert.ThrowsAsync<RepositoryNotFoundException>(async () => await GitHubRepo.OpenAsync(tempFolder.Path));
                    }

                    // Verify that we see an exception when trying to open an empty local repo folder.

                    using (var tempFolder = new TempFolder(prefix: "repo-", create: true))
                    {
                        await Assert.ThrowsAsync<RepositoryNotFoundException>(async () => await GitHubRepo.OpenAsync(tempFolder.Path));
                    }
                });
        }

        [MaintainerFact]
        public async Task Fetch()
        {
            // Verify that we can fetch remote info for a local repo.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath = tempFolder.Path;

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            Assert.True(File.Exists(Path.Combine(repoPath, ".gitignore")));
                            await repo.Local.FetchAsync();
                        }
                    }
                });
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

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder1 = new TempFolder(prefix: "repo1-", create: false))
                    {
                        using (var tempFolder2 = new TempFolder(prefix: "repo2-", create: false))
                        {
                            var repoPath1 = tempFolder1.Path;
                            var repoPath2 = tempFolder2.Path;

                            using (var repo1 = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath1))
                            {
                                using (var repo2 = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath2))
                                {
                                    // Clone the remote repo to two local folders:
                                    // Create a new text file named with GUID to the first repo
                                    // and commit and push the change to the remote:

                                    var testFolder   = GitHubTestHelper.TestFolder;
                                    var testFileName = Path.Combine(testFolder, $"{Guid.NewGuid()}.txt");
                                    var testPath1    = Path.Combine(repoPath1, testFileName);
                                    var testPath2    = Path.Combine(repoPath2, testFileName);

                                    Directory.CreateDirectory(Path.Combine(repoPath1, testFolder));
                                    File.WriteAllText(testPath1, "HELLO WORLD!");
                                    Assert.True(await repo1.Local.CommitAsync("add: test file"));
                                    Assert.True(await repo1.Local.PushAsync());

                                    // Pull the second repo from the remote:

                                    Assert.Equal(MergeStatus.FastForward, await repo2.Local.PullAsync());

                                    // Confirm that the second repo has the new file:

                                    Assert.True(File.Exists(testPath2));
                                    Assert.Equal("HELLO WORLD!", File.ReadAllText(testPath2));

                                    // Remove the file in the second repo and then commit and
                                    // push to the remote:

                                    File.Delete(testPath2);
                                    Assert.True(await repo2.Local.CommitAsync("delete: test file"));
                                    Assert.True(await repo2.Local.PushAsync());

                                    // Go back to the first repo and pull changes from the remote 
                                    // and confirm that the file no longer exists:

                                    Assert.Equal(MergeStatus.FastForward, await repo1.Local.PullAsync());
                                    Assert.False(File.Exists(testPath1));
                                }
                            }
                        }
                        }
                });
        }

        [MaintainerFact]
        public async Task CreateBranch()
        {
            // Verify that we can create a new local branch from master.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath      = tempFolder.Path;
                        var newBranchName = $"testbranch-{Guid.NewGuid()}";
                        var testFilePath  = Path.Combine(repoPath, GitHubTestHelper.TestFolder, $"{Guid.NewGuid()}.txt");

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            // Create the new branch and verify that it's now currently checked out.

                            Assert.True(await repo.Local.CreateBranchAsync(newBranchName, "master"));
                            Assert.True(repo.GitApi.Branches[newBranchName].IsCurrentRepositoryHead);

                            // Verify that the new branch is currently checked out by:
                            //
                            //      1. Creating a test file in the new branch and commiting it.
                            //      2. Check out the master branch
                            //      3. Verify that the test file is not present in master.

                            Directory.CreateDirectory(Path.GetDirectoryName(testFilePath));
                            File.WriteAllText(testFilePath, "HELLO WORLD!");
                            await repo.Local.CommitAsync();

                            await repo.Local.CheckoutAsync("master");
                            Assert.False(File.Exists(testFilePath));
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Remote_GetBranches()
        {
            // Verify that we can list remote branches.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath = tempFolder.Path;

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            var remoteBranches = await repo.Remote.Branch.GetAllAsync();

                            Assert.Contains(remoteBranches, branch => branch.Name == "master");
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task CreateRemoveBranch()
        {
            // Verify that we can create a local branch (from master) and then remove it.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath      = tempFolder.Path;
                        var newBranchName = $"testbranch-{Guid.NewGuid()}";

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            // Create a new local branch and verify.

                            Assert.True(await repo.Local.CreateBranchAsync(newBranchName, "master"));
                            Assert.NotNull(repo.GitApi.Branches[newBranchName]);
                            Assert.Null(await repo.Remote.Branch.FindAsync(newBranchName));

                            // Verify that we see FALSE when trying to create an existing branch.

                            Assert.False(await repo.Local.CreateBranchAsync(newBranchName, "master"));

                            // Remove the local branch and verify.

                            await repo.Local.CheckoutAsync("master");
                            repo.GitApi.Branches.Remove(repo.GitApi.Branches[newBranchName]);
                            Assert.Null(repo.GitApi.Branches[newBranchName]);
                            Assert.Null(await repo.Remote.Branch.FindAsync(newBranchName));
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Branch_Protection()
        {
            // Verify that we can change branch protection by locking and then unlocking a branch.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath      = tempFolder.Path;
                        var newBranchName = $"testbranch-{Guid.NewGuid()}";

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            // Create a new local branch and verify.

                            Assert.True(await repo.Local.CreateBranchAsync(newBranchName, "master"));
                            Assert.NotNull(repo.GitApi.Branches[newBranchName]);
                            Assert.Null(await repo.Remote.Branch.FindAsync(newBranchName));

                            // Push the branch to GitHub and verify.

                            await repo.Local.PushAsync();

                            var newRemoteBranch = await repo.Remote.Branch.FindAsync(newBranchName);

                            Assert.NotNull(newRemoteBranch);

                            // Verify that the new remote branch is not protected (yet).

                            Assert.False(newRemoteBranch.Protected);

                            // Make the branch read-only and block deletions of new remote branch
                            // for everybody (including admins) and verify.

                            var protectionUpdate =
                                new BranchProtectionSettingsUpdate(
                                    new BranchProtectionPushRestrictionsUpdate())
                                    {
                                        EnforceAdmins  = true,
                                        LockBranch     = true,
                                        AllowDeletions = false
                                    };

                            await repo.Remote.Branch.UpdateBranchProtectionAsync(newBranchName, protectionUpdate);

                            newRemoteBranch = await repo.Remote.Branch.FindAsync(newBranchName);

                            var protection = await repo.Remote.Branch.GetBranchProtectionAsync(newBranchName);

                            Assert.NotNull(protection);
                            Assert.NotNull(protection.Restrictions);
                            Assert.True(newRemoteBranch.Protected);
                            Assert.True(protection.EnforceAdmins.Enabled);
                            Assert.True(protection.LockBranch.Enabled);
                            Assert.False(protection.AllowDeletions.Enabled);

                            // Pushing a commit from the local repo to the read-only branch should fail.

                            File.WriteAllText(await repo.Local.GetLocalFilePathAsync("/test.txt"), "HELLO WORLD!");
                            await repo.Local.CommitAsync("This is a test.");
                            await Assert.ThrowsAsync<LibGit2Sharp.LibGit2SharpException>(async () => await repo.Local.PushAsync());

                            // Deleting the branch should fail too.

                            await Assert.ThrowsAsync<ApiValidationException>(async () => await repo.Remote.Branch.RemoveAsync(newBranchName));

                            // Remove all protections and verify.

                            await repo.Remote.Branch.DeleteBranchProtection(newBranchName);

                            newRemoteBranch = await repo.Remote.Branch.FindAsync(newBranchName);

                            Assert.False(newRemoteBranch.Protected);

                            // Branch push should work now.

                            await repo.Local.PushAsync();

                            // Branch deletion should work now too.

                            await repo.Remote.Branch.RemoveAsync(newBranchName);
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Merge()
        {
            // Verify that we can merge changes from one branch into another.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath      = tempFolder.Path;
                        var newBranchName = $"testbranch-{Guid.NewGuid()}";
                        var testFilePath  = Path.Combine(repoPath, GitHubTestHelper.TestFolder, $"{Guid.NewGuid()}.txt");

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            // Create a new local branch and verify.

                            Assert.True(await repo.Local.CreateBranchAsync(newBranchName, "master"));
                            Assert.NotNull(repo.GitApi.Branches[newBranchName]);
                            Assert.Null(await repo.Remote.Branch.FindAsync(newBranchName));

                            // Create a test file in the new branch and commit.

                            Directory.CreateDirectory(Path.GetDirectoryName(testFilePath));
                            File.WriteAllText(testFilePath, "HELLO WORLD!");
                            await repo.Local.CommitAsync();

                            // Switch back to the master branch and merge changes from the other branch
                            // and then verify that we see the new test file.

                            await repo.Local.CheckoutAsync("master");

                            var result = await repo.Local.MergeAsync(newBranchName);

                            Assert.Equal(MergeStatus.FastForward, result.Status);
                            Assert.True(File.Exists(testFilePath));
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Merge_WithConflict()
        {
            // Verify that merge conflicts are detected.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath      = tempFolder.Path;
                        var newBranchName = $"testbranch-{Guid.NewGuid()}";
                        var testFilePath  = Path.Combine(repoPath, GitHubTestHelper.TestFolder, $"{Guid.NewGuid()}.txt");

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            // Create a new local branch and verify.

                            Assert.True(await repo.Local.CreateBranchAsync(newBranchName, "master"));
                            Assert.NotNull(repo.GitApi.Branches[newBranchName]);
                            Assert.Null(await repo.Remote.Branch.FindAsync(newBranchName));

                            // Create a test file in the new branch and commit.

                            Directory.CreateDirectory(Path.GetDirectoryName(testFilePath));
                            File.WriteAllText(testFilePath, "HELLO WORLD!");
                            await repo.Local.CommitAsync();

                            // Switch back to the master branch and change the contents of the
                            // test file to something different and commit.

                            await repo.Local.CheckoutAsync("master");
                            Directory.CreateDirectory(Path.GetDirectoryName(testFilePath));
                            File.WriteAllText(testFilePath, "GOODBYE WORLD!");
                            await repo.Local.CommitAsync();

                            // Try to merge the test branch into master.  This should fail with
                            // a merge conflict exception.  We'll also verify that the original
                            // test file was restored.

                            await Assert.ThrowsAsync<LibGit2SharpException>(async () => await repo.Local.MergeAsync(newBranchName));
                            Assert.Equal("GOODBYE WORLD!", File.ReadAllText(testFilePath));

                            // Try merging again with [throwOnConflict=false] and verify.
                            // We'll also verify that the original test file was restored.

                            var result = await repo.Local.MergeAsync(newBranchName, throwOnConflict: false);

                            Assert.Equal(MergeStatus.Conflicts, result.Status);
                            Assert.Equal("GOODBYE WORLD!", File.ReadAllText(testFilePath));

                            // Edit the test file, attempt the merge and verify that we see
                            // an exception because merge doesn't work when the repo is dirty.

                            File.WriteAllText(testFilePath, "HI WORLD!");
                            await Assert.ThrowsAsync<LibGit2SharpException>(async () => await repo.Local.MergeAsync(newBranchName, throwOnConflict: false));
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Undo()
        {
            // Verify that we can undo uncommited changes to a repo.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath      = tempFolder.Path;
                        var newBranchName = $"testbranch-{Guid.NewGuid()}";
                        var testFilePath  = Path.Combine(repoPath, GitHubTestHelper.TestFolder, $"{Guid.NewGuid()}.txt");

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            // Verify that undo doesn't barf when there are no changes.

                            await repo.Local.UndoAsync();

                            // Create a test file, undo changes, and then verify that the file
                            // no longer exists.

                            Directory.CreateDirectory(Path.GetDirectoryName(testFilePath));
                            File.WriteAllText(testFilePath, "HELLO WORLD!");
                            await repo.Local.UndoAsync();
                            Assert.False(File.Exists(testFilePath));
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Remote_CreateRemoveBranch()
        {
            // Verify that we can create a local branch (from master), push it to
            // the remote, and then remove it from both local and remote.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath      = tempFolder.Path;
                        var newBranchName = $"testbranch-{Guid.NewGuid()}";

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            Assert.Equal("master", repo.Local.CurrentBranch.FriendlyName);

                            // Create a local branch and verify.

                            Assert.True(await repo.Local.CreateBranchAsync(newBranchName, "master"));
                            Assert.NotNull(repo.GitApi.Branches[newBranchName]);
                            Assert.Null(await repo.Remote.Branch.FindAsync(newBranchName));
                            Assert.Equal(newBranchName, repo.Local.CurrentBranch.FriendlyName);

                            // Push to remote and verify.

                            await repo.Local.PushAsync();
                            Assert.NotNull(repo.GitApi.Branches[newBranchName]);
                            Assert.NotNull(await repo.Remote.Branch.FindAsync(newBranchName));

                            // Switch back to master so we'll be able to delete the branch.

                            await repo.Local.CheckoutAsync("master");
                            Assert.Equal("master", repo.Local.CurrentBranch.FriendlyName);

                            // Verify that [GetAsync()] returns an existing branch and throws for a non-existent one.

                            Assert.NotNull(await repo.Remote.Branch.GetAsync(newBranchName));
                            await Assert.ThrowsAsync<Octokit.NotFoundException>(async () => await repo.Remote.Branch.GetAsync($"{Guid.NewGuid()}"));

                            // Remove the new local branch and then verify.

                            await repo.Local.RemoveBranchAsync(newBranchName);
                            Assert.Null(repo.GitApi.Branches[newBranchName]);
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Checkout()
        {
            // Verify that we can checkout an existing remote branch to the
            // local repo with the same name (the default) or to a new branch
            // name.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath       = tempFolder.Path;
                        var newBranchName  = $"testbranch-{Guid.NewGuid()}";
                        var newBranchName2 = $"{newBranchName}-new";

                        // Clone the remote repo, create a new test branch, and push it to GitHub.

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            await repo.Local.CreateBranchAsync(newBranchName, "master");
                            Assert.Equal(newBranchName, repo.Local.CurrentBranch.FriendlyName);
                            Assert.True(repo.GitApi.Branches[newBranchName] != null);
                            Assert.True(repo.GitApi.Branches[newBranchName].IsCurrentRepositoryHead);
                            await repo.Local.PushAsync();
                        }

                        // Delete all repo files, re-clone the remote repo and then verify that
                        // we can checkout the remote with the new branch.
                        //
                        // Then verify that we can switch back and forth between local branches.

                        NeonHelper.DeleteFolderContents(repoPath);
                        Directory.Delete(repoPath);

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath, newBranchName))
                        {
                            Assert.Equal(newBranchName, repo.Local.CurrentBranch.FriendlyName);
                            Assert.True(repo.GitApi.Branches[newBranchName] != null);
                            Assert.True(repo.GitApi.Branches[newBranchName].IsCurrentRepositoryHead);

                            await repo.Local.CheckoutAsync("master");
                            Assert.Equal("master", repo.Local.CurrentBranch.FriendlyName);

                            await repo.Local.CheckoutAsync(newBranchName);
                            Assert.Equal(newBranchName, repo.Local.CurrentBranch.FriendlyName);

                            await repo.Local.CheckoutAsync("master");
                            Assert.Equal("master", repo.Local.CurrentBranch.FriendlyName);
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Open_NeonKube()
        {
            // Verify that we can open the NEONKUBE repo when present
            // at the standard location.

            var repoPath = Environment.GetEnvironmentVariable("NK_ROOT");

            if (string.IsNullOrEmpty(repoPath) || !Directory.Exists(repoPath) || !File.Exists(Path.Combine(repoPath, "neonKUBE.sln")))
            {
                return;
            }

            using (var repo = await GitHubRepo.OpenAsync(repoPath))
            {
                Assert.NotNull(repo.Local.CurrentBranch);
                Assert.NotEmpty(repo.Local.CurrentBranch.FriendlyName);
            }
        }

        [MaintainerFact]
        public async Task ObjectDisposedException()
        {
            // Verify that [ObjectDisposedException] is thrown calling APIs on a disposed [GitHubRepo] instance.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, tempFolder.Path);

                        repo.Dispose();

                        Assert.Throws<ObjectDisposedException>(() => _ = repo.Local.Folder);
                        Assert.Throws<ObjectDisposedException>(() => _ = repo.GitApi);
                        Assert.Throws<ObjectDisposedException>(() => _ = repo.Local.CurrentBranch);
                        Assert.Throws<ObjectDisposedException>(() => _ = repo.Local.CreateSignature());
                        Assert.Throws<ObjectDisposedException>(() => _ = repo.Local.IsDirty);

                        Assert.Throws<ObjectDisposedException>(() => _ = repo.Origin);
                        Assert.Throws<ObjectDisposedException>(() => _ = repo.Remote);

                        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await repo.Local.CheckoutAsync("master"));
                        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await repo.Local.CheckoutOriginAsync("master"));
                        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await repo.Local.CreateBranchAsync("test", "master"));
                        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await repo.Local.CommitAsync());
                        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await repo.Local.FetchAsync());
                        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await repo.Local.MergeAsync("master"));
                        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await repo.Local.PullAsync());
                        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await repo.Local.PushAsync());
                        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await repo.Local.RemoveBranchAsync("master"));
                        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await repo.Local.ListBrancheshAsync());
                        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await repo.Local.BranchExistsAsync("master"));

                        Assert.Throws<ObjectDisposedException>(() => repo.NormalizeBranchName("master"));
                    }
                });
        }

        [MaintainerFact]
        public async Task GetCommits()
        {
            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath = tempFolder.Path;

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            //-------------------------------------------------
                            // Verify that commits returned by GitHub are sorted in decending order by date.

                            var remoteCommits = (await repo.Remote.GetCommitsAsync("master")).ToList();

                            Assert.NotNull(remoteCommits);
                            Assert.NotEmpty(remoteCommits);   // Repos always have at least one commit

                            var orderedRemoteCommits = remoteCommits.OrderByDescending(commit => commit.Commit.Author.Date).ToList();

                            Assert.Equal(remoteCommits.Count, orderedRemoteCommits.Count);

                            for (int i = 0; i < remoteCommits.Count; i++)
                            {
                                Assert.Equal(remoteCommits[i].Sha, orderedRemoteCommits[i].Sha);
                            }

                            //-------------------------------------------------
                            // Verify that commits returned by the local repo are also sorted in decending order by date

                            var localCommits = (await repo.Local.GetCommitsAsync()).ToList();

                            Assert.NotNull(remoteCommits);
                            Assert.NotEmpty(remoteCommits);   // Repos always have at least one commit

                            var orderedLocalCommits = remoteCommits.OrderByDescending(commit => commit.Commit.Author.Date).ToList();

                            Assert.Equal(localCommits.Count, orderedLocalCommits.Count);

                            for (int i = 0; i < remoteCommits.Count; i++)
                            {
                                Assert.Equal(localCommits[i].Sha, orderedLocalCommits[i].Sha);
                            }
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Commit_NotAheadOrBehind()
        {
            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    //-------------------------------------------------
                    // Clone a repo and then verify that the the local repo is not ahead
                    // or behind on commits (since both local and remote should be on the
                    // same commit.

                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath = tempFolder.Path;

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            Assert.False(await repo.Local.IsAheadAsync());
                            Assert.False(await repo.Local.IsBehindAsync());
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Commit_IsAhead()
        {
            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    //-------------------------------------------------
                    // Clone a repo add a file and perform a local commit and then verify
                    // that the the local repo is ahead of the remote.

                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath = tempFolder.Path;

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            var testFolder = Path.Combine(tempFolder.Path, GitHubTestHelper.TestFolder);

                            Directory.CreateDirectory(testFolder);

                            File.WriteAllText(Path.Combine(testFolder, $"{Guid.NewGuid()}.txt"), "HELLO WORLD!");
                            await repo.Local.CommitAsync();

                            Assert.True(await repo.Local.IsAheadAsync());
                            Assert.False(await repo.Local.IsBehindAsync());
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Commit_IsBehind()
        {
            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    //-------------------------------------------------
                    // Clone two repos.  In the first, add a file, commit that and push
                    // to GitHub.  Then verify that the second repos is now behind.

                    using (var tempFolder1 = new TempFolder(prefix: "repo-", create: false))
                    {
                        using (var tempFolder2 = new TempFolder(prefix: "repo-", create: false))
                        {
                            using (var repo1 = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, tempFolder1.Path))
                            {
                                using (var repo2 = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, tempFolder2.Path))
                                {
                                    var testFolder1 = Path.Combine(tempFolder1.Path, GitHubTestHelper.TestFolder);

                                    Directory.CreateDirectory(testFolder1);

                                    File.WriteAllText(Path.Combine(testFolder1, $"{Guid.NewGuid()}.txt"), "HELLO WORLD!");
                                    await repo1.Local.CommitAsync();
                                    await repo1.Local.PushAsync();

                                    Assert.False(await repo2.Local.IsAheadAsync());
                                    Assert.True(await repo2.Local.IsBehindAsync());
                                }
                            }
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task GetRemoteFile()
        {
            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    //-------------------------------------------------
                    // Clone a repo add a file and perform a local commit and then push
                    // and then verify that we can download the file directly from the
                    // remote repo.  Then verify that we get a FALSE result when the remote
                    // file doesn't exist.
                    //
                    // Also verify that we can read a remote file as text.

                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath = tempFolder.Path;

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            var testFolder = Path.Combine(tempFolder.Path, GitHubTestHelper.TestFolder);
                            var fileName   = $"{Guid.NewGuid()}.txt";
                            var filePath   = Path.Combine(testFolder, fileName);

                            Directory.CreateDirectory(testFolder);

                            File.WriteAllText(filePath, "HELLO WORLD!");
                            await repo.Local.CommitAsync();
                            await repo.Local.PushAsync();

                            using (var ms = new MemoryStream())
                            {
                                await repo.Remote.Branch.GetBranchFileAsync("master", $"/{GitHubTestHelper.TestFolder}/{fileName}", ms);

                                ms.Position = 0;

                                using (var reader = new StreamReader(ms))
                                {
                                    Assert.Equal("HELLO WORLD!", reader.ReadToEnd());
                                }

                                await Assert.ThrowsAsync<Octokit.NotFoundException>((async () => await repo.Remote.Branch.GetBranchFileAsync("master", $"/{GitHubTestHelper.TestFolder}/{fileName}.bad", ms)));
                            }

                            Assert.Equal("HELLO WORLD!", await repo.Remote.Branch.GetBranchFileAsTextAsync("master", $"/{GitHubTestHelper.TestFolder}/{fileName}"));

                            // Verify that we detect missing remote branches and files.

                            await Assert.ThrowsAsync<Octokit.NotFoundException>(async () => await repo.Remote.Branch.GetBranchFileAsTextAsync("bad", $"/{GitHubTestHelper.TestFolder}/{fileName}"));
                            await Assert.ThrowsAsync<Octokit.NotFoundException>(async () => await repo.Remote.Branch.GetBranchFileAsTextAsync("master", $"/{GitHubTestHelper.TestFolder}/{fileName}.bad"));
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Local_ListBranches()
        {
            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    //-------------------------------------------------
                    // Clone a repo, add a branch, and then verify that we can list local branches.

                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath = tempFolder.Path;

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            var testFolder = Path.Combine(tempFolder.Path, GitHubTestHelper.TestFolder);
                            var fileName   = $"{Guid.NewGuid()}.txt";
                            var filePath   = Path.Combine(testFolder, fileName);

                            // Create the new local branch.

                            var newBranchName = $"testbranch-{Guid.NewGuid()}";

                            // Verify that the "master" and new branches exist.

                            Assert.True(await repo.Local.CreateBranchAsync(newBranchName, "master"));
                            Assert.Contains(newBranchName, await repo.Local.ListBrancheshAsync());
                            Assert.Contains("master", await repo.Local.ListBrancheshAsync());
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Local_BranchExists()
        {
            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    //-------------------------------------------------
                    // Clone a repo and then verify BranchExistsAsync() works.

                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath = tempFolder.Path;

                        using (var repo = await GitHubRepo.CloneAsync(GitHubTestHelper.RemoteTestRepoPath, repoPath))
                        {
                            var testFolder        = Path.Combine(tempFolder.Path, GitHubTestHelper.TestFolder);
                            var fileName          = $"{Guid.NewGuid()}.txt";
                            var filePath          = Path.Combine(testFolder, fileName);
                            var missingBranchName = $"testbranch-{Guid.NewGuid()}";

                            // Verify that the "master" and new branches exist.

                            Assert.True(await repo.Local.BranchExistsAsync("master"));
                            Assert.False(await repo.Local.BranchExistsAsync(missingBranchName));
                        }
                    }
                });
        }
    }
}
