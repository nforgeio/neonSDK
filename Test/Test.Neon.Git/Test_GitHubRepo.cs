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
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public class Test_GitHubRepo
    {
        public Test_GitHubRepo()
        {
            GitTestHelper.EnsureMaintainer();
        }

        [MaintainerFact]
        public async Task Clone()
        {
            // Verify that we can clone the repo to a temporary local folder.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath = tempFolder.Path;

                        using (var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, tempFolder.Path))
                        {
                            Assert.Equal(GitTestHelper.RemoteTestRepo, repo.Remote.Path.ToString());
                            Assert.Equal("master", repo.Local.CurrentBranch.FriendlyName);
                            Assert.True(File.Exists(Path.Combine(repoPath, ".gitignore")));

                            // Exercise some other APIs.

                            Assert.Equal("https://github.com/neontest/neon-git", repo.Origin.Url);
                            Assert.Equal("https://github.com/neontest/neon-git", repo.Origin.PushUrl);
                            Assert.Equal("https://github.com/neontest/", repo.Remote.BaseUri);

                            var validLocalPath = Path.Combine(repo.Local.Folder, "test", "foo.txt");

                            Assert.Equal(validLocalPath, await repo.Local.GetLocalFilePathAsync(@"/test/foo.txt"));
                            Assert.Equal(validLocalPath, await repo.Local.GetLocalFilePathAsync(@"test/foo.txt"));
                            Assert.Equal(validLocalPath, await repo.Local.GetLocalFilePathAsync(@"\test\foo.txt"));
                            Assert.Equal(validLocalPath, await repo.Local.GetLocalFilePathAsync(@"test\foo.txt"));

                            var validRemoteUri = $"{repo.Remote.BaseUri}test/foo.txt";

                            Assert.Equal(validRemoteUri, await repo.Local.GetRemoteFileUriAsync(@"/test/foo.txt"));
                            Assert.Equal(validRemoteUri, await repo.Local.GetRemoteFileUriAsync(@"test/foo.txt"));
                            Assert.Equal(validRemoteUri, await repo.Local.GetRemoteFileUriAsync(@"\test\foo.txt"));
                            Assert.Equal(validRemoteUri, await repo.Local.GetRemoteFileUriAsync(@"test\foo.txt"));
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Open()
        {
            // Verify that we can open an existing local repo.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    // Verify that we can open an existing local repo.

                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath = tempFolder.Path;

                        // Clone the initial repo and then dispose it.

                        using (var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, tempFolder.Path))
                        {
                            Assert.Equal(GitTestHelper.RemoteTestRepo, repo.Remote.Path.ToString());
                            Assert.Equal("master", repo.Local.CurrentBranch.FriendlyName);
                            Assert.True(File.Exists(Path.Combine(repoPath, ".gitignore")));
                        }

                        // Verify that we can reopen the existing repo.

                        using (var repo = await GitHubRepo.OpenAsync(tempFolder.Path))
                        {
                            Assert.Equal("master", repo.Local.CurrentBranch.FriendlyName);
                            Assert.True(File.Exists(Path.Combine(repoPath, ".gitignore")));
                            Assert.Equal(RemoteRepoPath.Parse(GitTestHelper.RemoteTestRepo).ToString(), repo.Remote.Path.ToString());
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
            // Verify that we can fetch remote info for a local repo without trouble.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath = tempFolder.Path;

                        using (var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, tempFolder.Path))
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

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder1 = new TempFolder(prefix: "repo1-", create: false))
                    {
                        using (var tempFolder2 = new TempFolder(prefix: "repo2-", create: false))
                        {
                            var repoPath1 = tempFolder1.Path;
                            var repoPath2 = tempFolder2.Path;

                            using (var repo1 = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, repoPath1))
                            {
                                using (var repo2 = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, repoPath2))
                                {
                                    // Clone the remote repo to two local folders:
                                    // Create a new text file named with GUID to the first repo
                                    // and commit and push the change to the remote:

                                    var testFolder   = GitTestHelper.TestFolder;
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

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath      = tempFolder.Path;
                        var newBranchName = $"testbranch-{Guid.NewGuid()}";
                        var testFilePath  = Path.Combine(repoPath, GitTestHelper.TestFolder, $"{Guid.NewGuid()}.txt");

                        using (var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, repoPath))
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

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath = tempFolder.Path;

                        using (var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, repoPath))
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

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath      = tempFolder.Path;
                        var newBranchName = $"testbranch-{Guid.NewGuid()}";

                        using (var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, repoPath))
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
        public async Task Merge()
        {
            // Verify that we can merge changes from one branch into another.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath      = tempFolder.Path;
                        var newBranchName = $"testbranch-{Guid.NewGuid()}";
                        var testFilePath  = Path.Combine(repoPath, GitTestHelper.TestFolder, $"{Guid.NewGuid()}.txt");

                        using (var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, repoPath))
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

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath      = tempFolder.Path;
                        var newBranchName = $"testbranch-{Guid.NewGuid()}";
                        var testFilePath  = Path.Combine(repoPath, GitTestHelper.TestFolder, $"{Guid.NewGuid()}.txt");

                        using (var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, repoPath))
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

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath      = tempFolder.Path;
                        var newBranchName = $"testbranch-{Guid.NewGuid()}";
                        var testFilePath  = Path.Combine(repoPath, GitTestHelper.TestFolder, $"{Guid.NewGuid()}.txt");

                        using (var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, repoPath))
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

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath      = tempFolder.Path;
                        var newBranchName = $"testbranch-{Guid.NewGuid()}";

                        using (var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, repoPath))
                        {
                            Assert.Equal("master", repo.Local.CurrentBranch.FriendlyName);

                            // Create a local branch only and verify.

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

                            // Verify that [GetAsync()] returns an existing branch and throws for a non-existant one.

                            Assert.NotNull(await repo.Remote.Branch.GetAsync(newBranchName));
                            await Assert.ThrowsAsync<Octokit.NotFoundException>(async () => await repo.Remote.Branch.GetAsync($"{Guid.NewGuid()}"));

                            // Remove the new branch and verify.

                            await repo.Local.RemoveBranchAsync(newBranchName);

                            Assert.Null(repo.GitApi.Branches[newBranchName]);
                            Assert.Null(await repo.Remote.Branch.FindAsync(newBranchName));
                            Assert.Equal("master", repo.Local.CurrentBranch.FriendlyName);
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Remote_Checkout()
        {
            // Verify that we can checkout an existing remote branch to the
            // local repo with the same name (the default) or to a new branch
            // name.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repoPath       = tempFolder.Path;
                        var newBranchName  = $"testbranch-{Guid.NewGuid()}";
                        var newBranchName2 = $"{newBranchName}-new";

                        // Clone the remote repo, create a new test branch, and push it to GitHub.

                        using (var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, repoPath))
                        {
                            await repo.Local.CreateBranchAsync(newBranchName, "master");
                            Assert.True(repo.GitApi.Branches[newBranchName] != null);
                            Assert.True(repo.GitApi.Branches[newBranchName].IsCurrentRepositoryHead);
                            await repo.Local.PushAsync();
                        }

                        // Delete all repo files, re-clone the remote repo and then verify that
                        // we can checkout the remote with the new branch.

                        NeonHelper.DeleteFolderContents(repoPath);
                        Directory.Delete(repoPath);

                        using (var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, repoPath, newBranchName))
                        {
                            Assert.Equal(newBranchName, repo.Local.CurrentBranch.FriendlyName);
                            Assert.True(repo.GitApi.Branches[newBranchName] != null);
                            Assert.True(repo.GitApi.Branches[newBranchName].IsCurrentRepositoryHead);
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task ObjectDisposedException()
        {
            // Verify that [ObjectDisposedException] is thrown calling APIs on a disposed [GitHubRepo] instance.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var tempFolder = new TempFolder(prefix: "repo-", create: false))
                    {
                        var repo = await GitHubRepo.CloneAsync(GitTestHelper.RemoteTestRepo, tempFolder.Path);

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
                        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await repo.Local.UndoAsync());

                        Assert.Throws<ObjectDisposedException>(() => repo.NormalizeBranchName("master"));
                    }
                });
        }
    }
}
