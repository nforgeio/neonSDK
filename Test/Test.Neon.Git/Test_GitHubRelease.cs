//-----------------------------------------------------------------------------
// FILE:        Test_GitHubReleasey.cs
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
    public class Test_GitHubRelease
    {
        public Test_GitHubRelease()
        {
            GitTestHelper.EnsureMaintainer();
        }

        [MaintainerFact]
        public async Task List()
        {
            // Verify that we can list releases without crashing.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitTestHelper.RemoteTestRepo))
                    {
                        await repo.OriginRepoApi.GetReleasesAsync();
                    }
                });
        }

        [MaintainerFact]
        public async Task CreateGetRemove()
        {
            // Verify that we can create, get and then remove a GitHub release.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitTestHelper.RemoteTestRepo))
                    {
                        var newTestRelease = $"test-{Guid.NewGuid()}";
                        var newTestTag     = $"test-{Guid.NewGuid()}";
                        var release        = await repo.OriginRepoApi.CreateRelease(tagName: newTestTag, releaseName: newTestRelease);

                        Assert.NotNull(release);
                        Assert.NotNull(await repo.OriginRepoApi.GetReleaseAsync(newTestRelease));
                        Assert.True(await repo.OriginRepoApi.RemoveReleaseAsync(newTestRelease));
                        Assert.Null(await repo.OriginRepoApi.GetReleaseAsync(newTestRelease));

                        // Verify that trying to delete a non-existent release returns FALSE.

                        Assert.False(await repo.OriginRepoApi.RemoveReleaseAsync(newTestRelease));
                    }
                });
        }
    }
}
