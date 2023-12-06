//-----------------------------------------------------------------------------
// FILE:        Test_GitHubIssues.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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

        [MaintainerFact]
        public async Task GetSearch()
        {
            // Verify that we can get and search for issues.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitHubTestHelper.RemoteTestRepoPath))
                    {
                        var title1 = $"Test Issue: {Guid.NewGuid()}";
                        var title2 = $"Test Issue: {Guid.NewGuid()}";

                        var newIssue1 = await repo.Remote.Issue.CreateAsync(
                            new NewIssue(title1)
                            {
                                Body = "HELLO WORLD!"
                            });

                        var newIssue2 = await repo.Remote.Issue.CreateAsync(
                            new NewIssue(title2)
                            {
                                Body = "HELLO WORLD!"
                            });

                        //-----------------------------------------------------
                        // Verify that we can fetch the issue by ID.

                        var issue = await repo.Remote.Issue.GetAsync(newIssue1.Number);

                        Assert.NotNull(issue);
                        Assert.Equal(newIssue1.Id, issue.Id);
                        Assert.Equal(newIssue1.Title, issue.Title);

                        //-----------------------------------------------------
                        // Verify that GetAllAsync() with a request works.

                        Assert.Contains(await repo.Remote.Issue.GetAllAsync(), issue => issue.Id == newIssue1.Id && newIssue1.Title == title1);

                        //-----------------------------------------------------
                        // Verify that GetAllAsync() works.

                        var request = new RepositoryIssueRequest()
                        {
                            State = ItemStateFilter.Open
                        };

                        Assert.Contains(await repo.Remote.Issue.GetAllAsync(request), issue => issue.Id == newIssue1.Id && newIssue1.Title == title1);

                        //-----------------------------------------------------
                        // Verify that SearchAsync() works.

                        // Looks like GitHub may take some time to index new issues or something
                        // and sometimes the search test below fails.  We'll mitigate this with
                        // a short delay.

                        await Task.Delay(TimeSpan.FromSeconds(15));

                        var searchRequest = new SearchIssuesRequest()
                        {
                            State   = ItemState.Open,
                            PerPage = 100
                        };

                        var found = await repo.Remote.Issue.SearchAsync(searchRequest);

                        Assert.Contains(found.Items, issue => issue.Id == newIssue1.Id && newIssue1.Title == title1);
                        Assert.Contains(found.Items, issue => issue.Id == newIssue2.Id && newIssue2.Title == title2);

                        //-----------------------------------------------------
                        // Verify that SearchAllAsync() works.

                        searchRequest = new SearchIssuesRequest()
                        {
                            State   = ItemState.Open,
                            PerPage = 1
                        };

                        var issues = await repo.Remote.Issue.SearchAllAsync(searchRequest);

                        Assert.Contains(issues, issue => issue.Id == newIssue1.Id && newIssue1.Title == title1);
                        Assert.Contains(issues, issue => issue.Id == newIssue2.Id && newIssue2.Title == title2);
                    }
                });
        }

        [MaintainerFact]
        public async Task Comments()
        {
            // Verify that the issue comment API works.

            await GitHubTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitHubTestHelper.RemoteTestRepoPath))
                    {
                        var title = $"Test Issue: {Guid.NewGuid()}";
                        var issue = await repo.Remote.Issue.CreateAsync(
                            new NewIssue(title)
                            {
                                Body = "HELLO WORLD!"
                            });

                        //-----------------------------------------------------
                        // Verify that the issue body does not count as a comment.

                        var comments = await repo.Remote.Issue.Comment.GetAllForIssue(issue.Number);

                        Assert.Empty(comments);

                        //-----------------------------------------------------
                        // Add a comment and verify.

                        var comment1 = await repo.Remote.Issue.Comment.CreateAsync(issue.Number, "COMMENT: 1");

                        Assert.NotNull(comment1);
                        Assert.Equal("COMMENT: 1", comment1.Body);

                        comments = await repo.Remote.Issue.Comment.GetAllForIssue(issue.Number);

                        Assert.Single(comments);
                        Assert.Equal("COMMENT: 1", comments.First().Body);

                        //-----------------------------------------------------
                        // Add a second comment and verify.

                        var comment2 = await repo.Remote.Issue.Comment.CreateAsync(issue.Number, "COMMENT: 2");

                        Assert.NotNull(comment2);
                        Assert.Equal("COMMENT: 2", comment2.Body);

                        comments = await repo.Remote.Issue.Comment.GetAllForIssue(issue.Number);

                        Assert.Equal(2, comments.Count());
                        Assert.Equal("COMMENT: 1", comments.First().Body);
                        Assert.Equal("COMMENT: 2", comments.Skip(1).First().Body);

                        //-----------------------------------------------------
                        // Fetch comments by ID.

                        var comment = await repo.Remote.Issue.Comment.GetAsync(comment1.Id);

                        Assert.Equal(comment1.Body, comment.Body);

                        comment = await repo.Remote.Issue.Comment.GetAsync(comment2.Id);

                        Assert.Equal(comment2.Body, comment.Body);

                        //-----------------------------------------------------
                        // Fetch all open comments for the repository and verify that the
                        // two comments exist.

                        comments = await repo.Remote.Issue.Comment.GetAllForRepository();

                        Assert.Contains(comments, comment => comment.Id == comment1.Id);
                        Assert.Contains(comments, comment => comment.Id == comment2.Id);

                        //-----------------------------------------------------
                        // Update a comment.

                        var updatedComment = await repo.Remote.Issue.Comment.UpdateAsync(comment2.Id, "COMMENT: 2A");

                        Assert.Equal("COMMENT: 2A", updatedComment.Body);

                        comment = await repo.Remote.Issue.Comment.GetAsync(comment2.Id);

                        Assert.Equal(updatedComment.Body, comment.Body);

                        //-----------------------------------------------------
                        // Delete a comment.

                        await repo.Remote.Issue.Comment.DeleteAsync(comment2.Id);

                        comments = await repo.Remote.Issue.Comment.GetAllForRepository();

                        Assert.Contains(comments, comment => comment.Id == comment1.Id);
                        Assert.DoesNotContain(comments, comment => comment.Id == comment2.Id);
                    }
                });
        }
    }
}
