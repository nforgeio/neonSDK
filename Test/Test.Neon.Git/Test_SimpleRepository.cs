//-----------------------------------------------------------------------------
// FILE:        Test_SimpleRepository.cs
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
    public class Test_SimpleRepository
    {
        [MaintainerFact]
        public async Task Clone()
        {
            // Verify that we can clone the repo to a temporary local folder.

            using (var tempFolder = new TempFolder())
            {
                var repoPath = tempFolder.Path;

                using (var repo = new SimpleRepository(GitTestHelper.RemoteTestRepo, tempFolder.Path))
                {
                    // Ensure that the [GitRepository] property is NULL because the temp
                    // folder is empty initially.

                    Assert.Null(repo.LocalRepo);

                    // Verify that we can clone a remote repo.

                    await repo.CloneAsync("master");
                    Assert.True(File.Exists(Path.Combine(repoPath, ".gitignore")));

                    // Verify that we can't clone over an existing repo.

                    await Assert.ThrowsAsync<InvalidOperationException>(async () => await repo.CloneAsync("master"));
                }
            }
        }

        [MaintainerFact]
        public async Task Fetch()
        {
            // Verify that we can fetch remote info for a local repo without trouble.

            using (var tempFolder = new TempFolder())
            {
                var repoPath = tempFolder.Path;

                using (var repo = new SimpleRepository(GitTestHelper.RemoteTestRepo, tempFolder.Path))
                {
                    await repo.CloneAsync();
                    Assert.True(File.Exists(Path.Combine(repoPath, ".gitignore")));
                    await repo.FetchAsync();
                }
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

                    using (var repo1 = new SimpleRepository(GitTestHelper.RemoteTestRepo, repoPath1))
                    {
                        using (var repo2 = new SimpleRepository(GitTestHelper.RemoteTestRepo, repoPath2))
                        {
                            // Clone the remote repo to two local folders:

                            await repo1.CloneAsync("master");
                            await repo2.CloneAsync("master");

                            // Create a new text file named with GUID to the first repo
                            // and commit and push the change to the remote:

                            var testFolder   = "test";
                            var testFileName = Path.Combine(testFolder, $"{Guid.NewGuid()}.txt");
                            var testPath1    = Path.Combine(repoPath1, testFileName);
                            var testPath2    = Path.Combine(repoPath2, testFileName);

                            Directory.CreateDirectory(Path.Combine(repoPath1, testFolder));
                            File.WriteAllText(testPath1, "HELLO WORLD!");
                            Assert.True(await repo1.CommitAsync("add: test file"));
                            Assert.True(await repo1.PushAsync());

                            // Pull the second repo from the remote:

                            Assert.Equal(MergeStatus.FastForward, await repo2.PullAsync());

                            // Confirm that the second repo has the new file:

                            Assert.True(File.Exists(testPath2));
                            Assert.Equal("HELLO WORLD!", File.ReadAllText(testPath2));

                            // Remove the file in the second repo and then commit and
                            // push to the remote:

                            File.Delete(testPath2);
                            Assert.True(await repo2.CommitAsync("delete: test file"));
                            Assert.True(await repo2.PushAsync());

                            // Go back to the first repo and pull changes from the remote 
                            // and confirm that the file no longer exists:

                            Assert.Equal(MergeStatus.FastForward, await repo1.PullAsync());
                            Assert.False(File.Exists(testPath1));

                            // It's possible for test files to accumulate when past unit tests were
                            // aborted or failed.  We're going to remove any test files in the first
                            // repo and then commit/push any changes to GitHub to address this.

                            var testFolder1 = Path.Combine(repoPath1, testFolder);

                            if (Directory.Exists(testFolder1) && Directory.GetFiles(testFolder1, "*", SearchOption.AllDirectories).Length > 0)
                            {
                                NeonHelper.DeleteFolderContents(testFolder1);
                                Assert.True(await repo1.CommitAsync("delete: accumulated test files"));
                                Assert.True(await repo1.PushAsync());
                            }
                        }
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

                using (var repo = new SimpleRepository(GitTestHelper.RemoteTestRepo, repoPath))
                {
                    //await github.CreateLocalBranchAsync(repoPath, newBranchName);
                    //await github.RemoveBranchAsync(repoPath, newBranchName);
                }
            }
        }
    }
}
