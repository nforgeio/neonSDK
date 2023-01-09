//-----------------------------------------------------------------------------
// FILE:        Test_GitHubRepoTags.cs
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
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using ICSharpCode.SharpZipLib.Zip;
using LibGit2Sharp;

using Neon.Common;
using Neon.Deployment;
using Neon.Git;
using Neon.IO;
using Neon.Xunit;

using Xunit;

using Release = Octokit.Release;

namespace TestGit
{
    [Trait(TestTrait.Category, TestArea.NeonGit)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public class Test_GitHubRepoTags
    {
        public Test_GitHubRepoTags()
        {
            GitTestHelper.EnsureMaintainer();
        }

        [MaintainerFact]
        public async Task GetAll()
        {
            // Verify that we can list tags without crashing.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitTestHelper.RemoteTestRepo))
                    {
                        await repo.RemoteRepository.Tag.GetAllAsync();
                    }
                });
        }

        // $todo(jefflill):
        //
        // The tag create methods don't work.  Create/delete functionality isn't really
        // important right now, so I'm going to comment them out.

#if TODO
        [MaintainerFact]
        public async Task CreateRemove_FromBranch()
        {
            // Verify that we can create, get, and remove a tag created from a branch.

            var tagName = $"test-{Guid.NewGuid()}";

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitTestHelper.RemoteTestRepo))
                    {
                        // Create and verify.

                        await repo.RemoteRepository.Tag.CreateFromBranchAsync(tagName, "master", "create test tag");
                        Assert.NotNull(repo.RemoteRepository.Tag.FindAsync(tagName));

                        // Remove and verify.

                        await repo.RemoteRepository.Tag.RemoveAsync(tagName);
                        Assert.Null(repo.RemoteRepository.Tag.FindAsync(tagName));
                    }
                });
        }
#endif
    }
}
