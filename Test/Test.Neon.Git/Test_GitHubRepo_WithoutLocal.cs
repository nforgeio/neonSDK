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
using Neon.Git;
using Neon.IO;
using Neon.Xunit;

using Xunit;

namespace TestGit
{
    [Trait(TestTrait.Category, TestArea.NeonGit)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public class Test_GitHubRepo_WithoutLocal
    {
        public Test_GitHubRepo_WithoutLocal()
        {
            GitTestHelper.EnsureMaintainer();
        }

        [MaintainerFact]
        public async Task Connect()
        {
            // Verify that we can connect to a GitHub account without a local git repo.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitTestHelper.RemoteTestRepo))
                    {
                        // These shouldn't throw any exceptions.

                        _ = repo.OriginRepoApi;
                        _ = repo.OriginRepoPath;
                        _ = repo.OriginRepository;
                    }
                });
        }

        [MaintainerFact]
        public async Task NoLocalRepositoryException()
        {
            // Verify that [NoLocalRepositoryException] are thrown when we attempt local
            // rep[ository options on repos that are not associated with a local repo.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitTestHelper.RemoteTestRepo))
                    {
                        Assert.Throws<NoLocalRepositoryException>(() => _ = repo.Branches);
                        Assert.Throws<NoLocalRepositoryException>(() => _ = repo.IsDirty);
                        Assert.Throws<NoLocalRepositoryException>(() => _ = repo.CurrentBranch);
                        Assert.Throws<NoLocalRepositoryException>(() => _ = repo.CreateSignature());
                        Assert.Throws<NoLocalRepositoryException>(() => _ = repo.CreatePushOptions());
                        Assert.Throws<NoLocalRepositoryException>(() => _ = repo.LocalRepoFolder);
                        Assert.Throws<NoLocalRepositoryException>(() => _ = repo.LocalRepository);

                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.CheckoutAsync("master"));
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.CheckoutOriginAsync("master"));
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.CreateBranchAsync("test", "master"));
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.CommitAsync());
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.FetchAsync());
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.MergeAsync("master"));
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.PullAsync());
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.PushAsync());
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.RemoveBranchAsync("master"));
                        await Assert.ThrowsAsync<NoLocalRepositoryException>(async () => await repo.UndoAsync());
                    }
                });
        }
    }
}
