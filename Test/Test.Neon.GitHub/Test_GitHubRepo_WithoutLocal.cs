//-----------------------------------------------------------------------------
// FILE:        Test_GitHubRepo_WithoutLocal.cs
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
using Neon.GitHub;
using Neon.IO;
using Neon.Xunit;

using Xunit;

namespace TestGitHub
{
    [Trait(TestTrait.Category, TestArea.NeonGit)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public class Test_GitHubRepo_WithoutLocal
    {
        public Test_GitHubRepo_WithoutLocal()
        {
            GitHubTestHelper.EnsureMaintainer();
        }

        [MaintainerFact]
        public async Task Connect()
        {
            // Verify that we can connect to a GitHub account without a local git repo.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitHubTestHelper.RemoteTestRepoPath))
                    {
                        // These shouldn't throw any exceptions.

                        _ = repo.Remote;
                    }
                });
        }

        [MaintainerFact]
        public async Task NoLocalRepositoryException()
        {
            // Verify that [NoLocalRepositoryException] are thrown when we attempt local
            // rep[ository options on repos that are not associated with a local repo.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitHubTestHelper.RemoteTestRepoPath))
                    {
                        Assert.Throws<NoLocalRepositoryException>(() => _ = repo.Local.IsDirty);
                        Assert.Throws<NoLocalRepositoryException>(() => _ = repo.Local.CurrentBranch);
                        Assert.Throws<NoLocalRepositoryException>(() => _ = repo.Local.CreateSignature());
                        Assert.Throws<NoLocalRepositoryException>(() => _ = repo.Local.CreatePushOptions());
                        Assert.Throws<NoLocalRepositoryException>(() => _ = repo.Local.Folder);
                        Assert.Throws<NoLocalRepositoryException>(() => _ = repo.GitApi);

                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.Local.CheckoutAsync("master"));
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.Local.CheckoutOriginAsync("master"));
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.Local.CreateBranchAsync("test", "master"));
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.Local.CommitAsync());
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.Local.FetchAsync());
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.Local.MergeAsync("master"));
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.Local.PullAsync());
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.Local.PushAsync());
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.Local.RemoveBranchAsync("master"));
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.Local.UndoAsync());
                    }
                });
        }
    }
}
