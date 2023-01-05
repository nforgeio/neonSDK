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
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
                        var newTestName = $"test-{Guid.NewGuid()}";
                        var newTestTag  = $"test-{Guid.NewGuid()}";
                        var release     = await repo.OriginRepoApi.CreateRelease(tagName: newTestTag, releaseName: newTestName, draft: true);

                        Assert.NotNull(release);
                        Assert.NotNull(await repo.OriginRepoApi.GetReleaseAsync(newTestName));
                        Assert.True(await repo.OriginRepoApi.RemoveReleaseAsync(newTestName));
                        Assert.Null(await repo.OriginRepoApi.GetReleaseAsync(newTestName));
                        Assert.Equal(newTestName, release.Name);
                        Assert.True(release.Draft);
                        Assert.True(string.IsNullOrEmpty(release.Body));

                        // Verify that trying to delete a non-existent release returns FALSE.

                        Assert.False(await repo.OriginRepoApi.RemoveReleaseAsync(newTestName));
                    }
                });
        }

        [MaintainerFact]
        public async Task Update()
        {
            // Verify that we can edit and existing release.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitTestHelper.RemoteTestRepo))
                    {
                        var newTestName = $"test-{Guid.NewGuid()}";
                        var newTestTag  = $"test-{Guid.NewGuid()}";
                        var release     = await repo.OriginRepoApi.CreateRelease(tagName: newTestTag, releaseName: newTestName, draft: true);

                        Assert.Equal(newTestName, release.Name);
                        Assert.True(release.Draft);
                        Assert.True(string.IsNullOrEmpty(release.Body));

                        var update = release.ToUpdate();

                        update.Name       = $"{newTestName}-changed";
                        update.Body       = "HELLO WORLD!";
                        update.Prerelease = false;

                        await repo.OriginRepoApi.UpdateReleaseAsync(release, update);
                    }
                });
        }

        [MaintainerFact]
        public async Task AddFileAsset()
        {
            // Verify that we can add a file asset to a release.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitTestHelper.RemoteTestRepo))
                    {
                        var releaseName = $"test-{Guid.NewGuid()}";
                        var newTestTag  = $"test-{Guid.NewGuid()}";
                        var release     = await repo.OriginRepoApi.CreateRelease(tagName: newTestTag, releaseName: releaseName, draft: true);

                        using (var tempFile = new TempFile())
                        {
                            File.WriteAllText(tempFile.Path, "HELLO WORLD!", Encoding.UTF8);

                            var asset = await repo.OriginRepoApi.AddReleaseAssetAsync(release, tempFile.Path, "asset-1");

                            release = await repo.OriginRepoApi.PublishReleaseAsync(releaseName);

                            var assetUri = repo.OriginRepoApi.GetAssetUri(release, asset);

                            using (var httpClient = new HttpClient())
                            {
                                var response = await httpClient.GetAsync(assetUri);

                                Assert.Equal("HELLO WORLD!", await response.Content.ReadAsStringAsync());
                            }
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task AddStreamAsset()
        {
            // Verify that we can add a stream asset to a release.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitTestHelper.RemoteTestRepo))
                    {
                        var releaseName = $"test-{Guid.NewGuid()}";
                        var newTestTag  = $"test-{Guid.NewGuid()}";
                        var release     = await repo.OriginRepoApi.CreateRelease(tagName: newTestTag, releaseName: releaseName, draft: true);

                        using (var ms = new MemoryStream())
                        {
                            ms.Write(Encoding.UTF8.GetBytes("HELLO WORLD!"));
                            ms.Seek(0, SeekOrigin.Begin);

                            var asset = await repo.OriginRepoApi.AddReleaseAssetAsync(release, ms, "asset-1");

                            release = await repo.OriginRepoApi.PublishReleaseAsync(releaseName);

                            var assetUri = repo.OriginRepoApi.GetAssetUri(release, asset);

                            using (var httpClient = new HttpClient())
                            {
                                var response = await httpClient.GetAsync(assetUri);

                                Assert.Equal("HELLO WORLD!", await response.Content.ReadAsStringAsync());
                            }
                        }
                    }
                });
        }

        [MaintainerFact]
        public async Task Publish()
        {
            // Verify that we can publish a release.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitTestHelper.RemoteTestRepo))
                    {
                        var releaseName = $"test-{Guid.NewGuid()}";
                        var newTestTag  = $"test-{Guid.NewGuid()}";
                        var release     = await repo.OriginRepoApi.CreateRelease(tagName: newTestTag, releaseName: releaseName, body: "HELLO WORLD!", draft: true);

                        Assert.Equal(releaseName, release.Name);
                        Assert.True(release.Draft);
                        Assert.True(!string.IsNullOrEmpty(release.Body));

                        await repo.OriginRepoApi.PublishReleaseAsync(releaseName);

                        release = await repo.OriginRepoApi.GetReleaseAsync(releaseName);

                        Assert.False(release.Draft);
                    }
                });
        }

        [MaintainerFact]
        public async Task AddDownloadManifest()
        {
            // Verify that we can publish a release with a multi-part download.

            await GitTestHelper.RunTestAsync(
                async () =>
                {
                    using (var repo = await GitHubRepo.ConnectAsync(GitTestHelper.RemoteTestRepo))
                    {
                        var releaseName = $"test-{Guid.NewGuid()}";
                        var newTestTag  = $"test-{Guid.NewGuid()}";
                        var release     = await repo.OriginRepoApi.CreateRelease(tagName: newTestTag, releaseName: releaseName, body: "HELLO WORLD!", draft: true);

                        Assert.Equal(releaseName, release.Name);
                        Assert.True(release.Draft);
                        Assert.True(!string.IsNullOrEmpty(release.Body));

                        var partCount = 10;
                        var partSize  = 1024;
                        var download  = await PublishMultipartAssetAsync(repo, release, "test.dat", "v1.0", partCount, partSize);

                        release = await repo.OriginRepoApi.RefreshReleaseAsync(release);

                        Assert.False(release.Draft);
                        Assert.Equal("test.dat", download.Name);
                        Assert.Equal("v1.0", download.Version);
                        Assert.NotNull(download.Md5);
                        Assert.Equal(10, download.Parts.Count);
                        Assert.Equal(partCount * partSize, download.Parts.Sum(part => part.Size));
                        Assert.Equal(partCount * partSize, download.Size);

                        for (int partNumber = 0; partNumber < download.Parts.Count; partNumber++)
                        {
                            Assert.Contains(release.Assets, asset => asset.Name == $"part-{partNumber:0#}");
                        }
                    }
                });
        }

        /// <summary>
        /// Uploads a file as multi-part assets to a release, publishes the release and then 
        /// returns the <see cref="Download"/> details.
        /// </summary>
        /// <param name="repo">Specifies the target repo.</param>
        /// <param name="release">Specifies the target release.</param>
        /// <param name="name">Specifies the download name.</param>
        /// <param name="version">Specifies the download version.</param>
        /// <param name="partCount">Specifies the number of parts to be uploaded.</param>
        /// <param name="partSize">Specifies the size of each part.</param>
        /// <returns>The <see cref="Download"/> describing how to download the parts.</returns>
        /// <remarks>
        /// Each part will be filled with bytes where the byte of each part will start
        /// with the part number and the following bytes will increment the previous byte
        /// value.
        /// </remarks>
        private async Task<DownloadManifest> PublishMultipartAssetAsync(GitHubRepo repo, Release release, string name, string version, int partCount, long partSize)
        {
            Covenant.Requires<ArgumentNullException>(release != null, nameof(release));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name), nameof(name));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(version), nameof(version));
            Covenant.Requires<ArgumentException>(partCount > 0, nameof(release));
            Covenant.Requires<ArgumentException>(partSize > 0, nameof(release));

            using (var tempFile = new TempFile())
            {
                using (var output = new FileStream(tempFile.Path, System.IO.FileMode.Create, FileAccess.ReadWrite))
                {
                    for (int partNumber = 0; partNumber < partCount; partNumber++)
                    {
                        for (long i = 0; i < partSize; i++)
                        {
                            output.WriteByte((byte)i);
                        }
                    }
                }

                return await repo.OriginRepoApi.AddMultipartReleaseAssetAsync(release, tempFile.Path, version: version, name: name, maxPartSize: partSize);
            }
        }
    }
}
