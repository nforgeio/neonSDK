//-----------------------------------------------------------------------------
// FILE:        Test_GitHubIssues.cs
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
using Neon.GitHub;
using Neon.IO;
using Neon.Xunit;

using Octokit;

using Xunit;

using Release = Octokit.Release;

namespace TestGitHub
{
    [Trait(TestTrait.Category, TestArea.NeonGit)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public class Test_GitHubIssues
    {
        public Test_GitHubIssues()
        {
            GitHubTestHelper.EnsureMaintainer();
        }

        [MaintainerFact]
        public async Task Create()
        {
            // Verify that we can create an issue.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitHubTestHelper.RemoteTestRepoPath))
                    {
                        var newIssue = await repo.Remote.Issue.CreateAsync(
                            new NewIssue("Test Issue")
                            {
                                Body = "HELLO WORLD!"
                            });

                        var number = newIssue.Number;

                        var issue = await repo.Remote.Issue.GetAsync(number);

                        Assert.NotNull(newIssue);
                        Assert.Equal(newIssue.Title, issue.Title);
                        Assert.Equal(newIssue.Body, issue.Body);
                    }
                });
        }
    }
}
