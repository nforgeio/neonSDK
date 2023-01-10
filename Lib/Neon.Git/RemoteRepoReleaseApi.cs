//-----------------------------------------------------------------------------
// FILE:	    RemoteRepoReleaseApi.cs
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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Cryptography;
using Neon.Deployment;
using Neon.IO;
using Neon.Net;
using Neon.Tasks;

using LibGit2Sharp;
using LibGit2Sharp.Handlers;

using Octokit;

using GitHubBranch     = Octokit.Branch;
using GitHubRepository = Octokit.Repository;
using GitHubSignature  = Octokit.Signature;

using GitBranch     = LibGit2Sharp.Branch;
using GitRepository = LibGit2Sharp.Repository;
using GitSignature  = LibGit2Sharp.Signature;

namespace Neon.Git
{
    /// <summary>
    /// Implements the friendly GitHub repository release related APIs.
    /// </summary>
    public class RemoteRepoReleaseApi
    {
        private GitHubRepo  root;

        /// <summary>
        /// Internal constructor.
        /// </summary>
        /// <param name="root">The root <see cref="GitHubRepo"/>.</param>
        internal RemoteRepoReleaseApi(GitHubRepo root)
        {
            this.root = root;
        }

        /// <summary>
        /// Creates a GitHub release.
        /// </summary>
        /// <param name="tagName">Specifies the release name.</param>
        /// <param name="releaseName">Optionally specifies the release name (defaults to <paramref name="tagName"/>).</param>
        /// <param name="body">Optionally specifies the markdown formatted release notes.</param>
        /// <param name="draft">Optionally indicates that the release won't be published immediately.</param>
        /// <param name="prerelease">Optionally indicates that the release is not production ready.</param>
        /// <returns>The new release.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        public async Task<Release> CreateAsync(string tagName, string releaseName = null, string body = null, bool draft = false, bool prerelease = false)
        {
            await SyncContext.Clear;
            root.EnsureNotDisposed();

            releaseName ??= tagName;

            var release = new NewRelease(tagName)
            {
                Name       = releaseName,
                Draft      = draft,
                Prerelease = prerelease,
                Body       = body
            };

            var newRelease = await root.GitHubApi.Repository.Release.Create(root.Remote.Id, release);

            // GitHub doesn't appear to create releases synchronously, so we're going to wait for the new release to show up.

            await root.WaitForGitHubAsync(
                async () =>
                {
                    return await root.Remote.Release.FindAsync(releaseName) != null;
                });

            return newRelease;
        }

        /// <summary>
        /// Returns all releases from the GitHub origin repository.
        /// </summary>
        /// <returns>The list of releases.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        public async Task<IReadOnlyList<Release>> GetAllAsync()
        {
            await SyncContext.Clear;
            root.EnsureNotDisposed();

            return await root.GitHubApi.Repository.Release.GetAll(root.Remote.Id);
        }

        /// <summary>
        /// Returns a specific GitHub origin repository release.
        /// </summary>
        /// <param name="releaseName">Specifies the origin repository release name.</param>
        /// <returns>The requested <see cref="Octokit.Release"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="Octokit.NotFoundException">Thrown when the release does not exist.</exception>
        public async Task<Octokit.Release> GetAsync(string releaseName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(releaseName), nameof(releaseName));
            root.EnsureNotDisposed();

            try
            {
                return await root.GitHubApi.Repository.Release.Get(root.Remote.Id, releaseName);
            }
            catch (Octokit.NotFoundException)
            {
                // $hack(jefflill):
                //
                // The GitHub [Get()] called above only returns published (non-draft) releases so we'll
                // revert to listing all of the releases and selecting from that.  This will optimize for
                // the presumably common case where the release exists.

                var allReleases = await root.GitHubApi.Repository.Release.GetAll(root.Remote.Id);
                var release     = allReleases.FirstOrDefault(release => release.Name.Equals(releaseName, StringComparison.InvariantCultureIgnoreCase));

                if (release != null)
                {
                    return release;
                }

                throw;
            }
        }

        /// <summary>
        /// Searches for a specific GitHub origin repository release.
        /// </summary>
        /// <param name="releaseName">Specifies the origin repository release name.</param>
        /// <returns>The requested <see cref="Octokit.Release"/> or <c>null</c> when it doesn't exist.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        public async Task<Octokit.Release> FindAsync(string releaseName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(releaseName), nameof(releaseName));
            root.EnsureNotDisposed();

            try
            {
                return await GetAsync(releaseName);
            }
            catch (Octokit.NotFoundException)
            {
                // $hack(jefflill):
                //
                // The GitHub [[Get()]] called above only returns published (non-draft) releases so we'll
                // revert to listing all of the releases and selecting from that.  This will optimize for
                // the presumably common case where the release exists.

                var allReleases = await root.GitHubApi.Repository.Release.GetAll(root.Remote.Id);

                return allReleases.FirstOrDefault(release => release.Name.Equals(releaseName, StringComparison.InvariantCultureIgnoreCase));
            }
        }

        /// <summary>
        /// Returns the latest version of a release.
        /// </summary>
        /// <param name="release">The release being refreshed.</param>
        /// <returns>The updated release.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the relase no longer exists.</exception>
        public async Task<Release> RefreshAsync(Release release)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(release != null, nameof(release));
            root.EnsureNotDisposed();

            var update = await GetAsync(release.Name);

            if (update == null)
            {
                throw new InvalidOperationException($"Release [{release.Name}] no longer exists.");
            }

            return update;
        }

        /// <summary>
        /// Updates an existing GitHub release.
        /// </summary>
        /// <param name="release">Specifies the release being changed.</param>
        /// <param name="releaseUpdate">Specifies the release revisions.</param>
        /// <returns>The updated release.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <remarks>
        /// <para>
        /// To update a release, you'll first need to:
        /// </para>
        /// <list type="number">
        /// <item>
        /// Create a new release or get and existing one.
        /// </item>
        /// <item>
        /// Obtain a <see cref="ReleaseUpdate"/> by calling <see cref="Release.ToUpdate"/>.
        /// </item>
        /// <item>
        /// Make your changes to the release update.
        /// </item>
        /// <item>
        /// Call <see cref="UpdateAsync(Release, ReleaseUpdate)"/>, passing the 
        /// original release along with the update.
        /// </item>
        /// </list>
        /// </remarks>
        public async Task<Release> UpdateAsync(Release release, ReleaseUpdate releaseUpdate)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(release != null, nameof(release));
            Covenant.Requires<ArgumentNullException>(releaseUpdate != null, nameof(releaseUpdate));
            root.EnsureNotDisposed();

            return await root.GitHubApi.Repository.Release.Edit(root.Remote.Id, release.Id, releaseUpdate);
        }

        /// <summary>
        /// Removes an origin release, if it exists.
        /// </summary>
        /// <param name="releaseName">Specifies the release name.</param>
        /// <returns><c>true</c> when the release existed and was removed, <c>false</c> otherwise.</returns>
        public async Task<bool> RemoveAsync(string releaseName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(releaseName), nameof(releaseName));
            root.EnsureNotDisposed();

            var release = await FindAsync(releaseName);

            if (release == null)
            {
                return false;
            }

            await root.GitHubApi.Repository.Release.Delete(root.Remote.Id, release.Id);

            return true;
        }

        /// <summary>
        /// <para>
        /// Uploads an asset file to a GitHub release.  Any existing asset with same name will be replaced.
        /// </para>
        /// <note>
        /// This only works for unpublished releases where <c>Draft=true</c>.
        /// </note>
        /// </summary>
        /// <param name="release">The target release.</param>
        /// <param name="assetPath">Path to the source asset file.</param>
        /// <param name="assetName">Optionally specifies the file name to assign to the asset.  This defaults to the file name in <paramref name="assetPath"/>.</param>
        /// <param name="contentType">Optionally specifies the asset's <b>Content-Type</b>.  This defaults to: <b> application/octet-stream</b></param>
        /// <returns>The new <see cref="ReleaseAsset"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="NotSupportedException">Thrown when the releas has already been published.</exception>
        public async Task<ReleaseAsset> AddAssetAsync(Release release, string assetPath, string assetName = null, string contentType = "application/octet-stream")
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(release != null, nameof(release));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(assetPath), nameof(assetPath));
            root.EnsureNotDisposed();

            using (var stream = File.OpenRead(assetPath))
            {
                return await AddAssetAsync(release, stream, assetName, contentType);
            }
        }

        /// <summary>
        /// <para>
        /// Uploads an asset stream to a GitHub release.  Any existing asset with same name will be replaced.
        /// </para>
        /// <note>
        /// This only works for unpublished releases where <c>Draft=true</c>.
        /// </note>
        /// </summary>
        /// <param name="release">The target release.</param>
        /// <param name="stream">The asset source stream.</param>
        /// <param name="assetName">Specifies the file name to assign to the asset.</param>
        /// <param name="contentType">Optionally specifies the asset's <b>Content-Type</b>.  This defaults to: <b> application/octet-stream</b></param>
        /// <returns>The new <see cref="ReleaseAsset"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        public async Task<ReleaseAsset> AddAssetAsync(Release release, Stream stream, string assetName, string contentType = "application/octet-stream")
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(release != null, nameof(release));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(assetName), nameof(assetName));
            Covenant.Requires<ArgumentNullException>(stream != null, nameof(stream));
            root.EnsureNotDisposed();

            var releaseName = release.Name;

            var upload = new ReleaseAssetUpload()
            {
                FileName    = assetName,
                ContentType = contentType,
                RawData     = stream
            };

            var newAsset = await root.GitHubApi.Repository.Release.UploadAsset(release, upload);

            // GitHub doesn't appear to upload assets synchronously, so we're going to wait for the new asset to show up.

            await root.WaitForGitHubAsync(
                async () =>
                {
                    var release = await root.Remote.Release.FindAsync(releaseName);

                    return release.Assets.Any(asset => asset.Id == newAsset.Id && newAsset.State == "uploaded");
                });

            return newAsset;
        }

        /// <summary>
        /// <para>
        /// Returns the URI that can be used to download a GitHub release asset.
        /// </para>
        /// <note>
        /// This works only for published releases.
        /// </note>
        /// </summary>
        /// <param name="release">The target release.</param>
        /// <param name="asset">The target asset.</param>
        /// <returns>The asset URI.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the asset passed doesn't exist in the release.</exception>
        public string GetAssetUri(Release release, ReleaseAsset asset)
        {
            Covenant.Requires<ArgumentNullException>(release != null, nameof(release));
            Covenant.Requires<ArgumentNullException>(asset != null, nameof(asset));
            root.EnsureNotDisposed();

            var releasedAsset = release.Assets.SingleOrDefault(a => a.Id == asset.Id);

            if (releasedAsset == null)
            {
                throw new InvalidOperationException($"Asset [id={asset.Id}] is not present in release [id={release.Id}].");
            }

            return releasedAsset.BrowserDownloadUrl;
        }

        /// <summary>
        /// Publishes a release.
        /// </summary>
        /// <param name="releaseName">Specifies the release name.</param>
        /// <returns>The published release.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the release doesn't exist or when it's already published.</exception>
        public async Task<Release> PublishAsync(string releaseName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(releaseName), nameof(releaseName));
            root.EnsureNotDisposed();

            var release = await GetAsync(releaseName);

            if (release == null)
            {
                throw new InvalidOperationException($"Release [{releaseName}] does not exist.");
            }

            if (!release.Draft)
            {
                throw new InvalidOperationException($"Release [{releaseName}] has already been published.");
            }

            var update = release.ToUpdate();

            update.Draft = false;

            await root.Remote.Release.UpdateAsync(release, update);

            // GitHub doesn't appear to publish releases synchronously, so we're going to wait for the new release to show up.

            await root.WaitForGitHubAsync(
                async () =>
                {
                    release = await root.Remote.Release.FindAsync(releaseName);

                    return release != null && !release.Draft;
                });

            return release;
        }

        /// <summary>
        /// Returns the URI for the zipped source code (Zipball) for a published release.
        /// </summary>
        /// <param name="releaseName">Specifies the release name.</param>
        /// <returns>The zipball URI.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the release does not exist or it has not been published.</exception>
        public async Task<string> GetZipballUri(string releaseName)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(releaseName), nameof(releaseName));
            root.EnsureNotDisposed();

            var release = await GetAsync(releaseName);

            if (release == null)
            {
                throw new InvalidOperationException($"Release [{releaseName}] does not exist.");
            }

            if (release.Draft)
            {
                throw new InvalidOperationException($"Release [{releaseName}] has not been published.");
            }

            return release.ZipballUrl;
        }

        /// <summary>
        /// <para>
        /// Returns the zipped source code (Zipball) for a published release.
        /// </para>
        /// <note>
        /// The repository files in the Zipball are are persisted to a root folder named like
        /// <b>REPONAME-COMMIT</b> where <b>REPONAME</b> is the name of the GitHub repository
        /// and <b>COMMIT</b> is the latest commit for the repo.
        /// </note>
        /// </summary>
        /// <param name="releaseName">Specifies the release name.</param>
        /// <param name="output">Specifies the stream where the Zipball will be written.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the release does not exist or it has not been published.</exception>
        public async Task DownloadZipballAsync(string releaseName, Stream output)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(releaseName), nameof(releaseName));
            Covenant.Requires<ArgumentNullException>(output != null, nameof(output));
            root.EnsureNotDisposed();

            var response = await root.HttpClient.GetSafeAsync(await GetZipballUri(releaseName));

            await response.Content.CopyToAsync(output);
        }

        /// <summary>
        /// Uploads a multi-part download to a release as an asset and then publishes the release.
        /// </summary>
        /// <param name="release">The target release.</param>
        /// <param name="sourcePath">Path to the file being uploaded.</param>
        /// <param name="version">The download version.</param>
        /// <param name="name">Optionally overrides the download file name specified by <paramref name="sourcePath"/> to initialize <see cref="DownloadManifest.Name"/>.</param>
        /// <param name="filename">Optionally overrides the download file name specified by <paramref name="sourcePath"/> to initialize <see cref="DownloadManifest.Filename"/>.</param>
        /// <param name="noMd5File">
        /// This method creates a file named [<paramref name="sourcePath"/>.md5] with the MD5 hash for the entire
        /// uploaded file by default.  You may override this behavior by passing <paramref name="noMd5File"/>=<c>true</c>.
        /// </param>
        /// <param name="maxPartSize">Optionally overrides the maximum part size (defaults to 75 MiB).</param>d
        /// <returns>The <see cref="DownloadManifest"/>.</returns>
        /// <exception cref="ObjectDisposedException">Thrown then the <see cref="GitHubRepo"/> has been disposed.</exception>
        /// <remarks>
        /// <para>
        /// The release passed must be unpublished and you may upload other assets before calling this.
        /// </para>
        /// <note>
        /// Take care that any assets already published have names that won't conflict with the asset
        /// part names, which will be formatted like: <b>part-##</b>
        /// </note>
        /// </remarks>
        public async Task<DownloadManifest> AddMultipartAssetAsync(
            Release     release, 
            string      sourcePath, 
            string      version, 
            string      name        = null,
            string      filename    = null,
            bool        noMd5File   = false,
            long        maxPartSize = (long)(75 * ByteUnits.MebiBytes))
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(release != null, nameof(release));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(sourcePath), nameof(sourcePath));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(version), nameof(version));
            root.EnsureNotDisposed();

            name     = name ?? Path.GetFileName(sourcePath);
            filename = filename ?? Path.GetFileName(sourcePath);

            using (var input = File.OpenRead(sourcePath))
            {
                if (input.Length == 0)
                {
                    throw new IOException($"Asset at [{sourcePath}] cannot be empty.");
                }

                var assetPartMap = new List<Tuple<ReleaseAsset, DownloadPart>>();
                var manifest     = new DownloadManifest() { Name = name, Version = version, Filename = filename };
                var partCount    = NeonHelper.PartitionCount(input.Length, maxPartSize);
                var partNumber   = 0;
                var partStart    = 0L;
                var cbRemaining  = input.Length;

                manifest.Md5   = CryptoHelper.ComputeMD5String(input);
                input.Position = 0;

                while (cbRemaining > 0)
                {
                    var partSize = Math.Min(cbRemaining, maxPartSize);
                    var part     = new DownloadPart()
                    {
                        Number = partNumber,
                        Size   = partSize,
                    };

                    // We're going to use a substream to compute the MD5 hash for the part
                    // as well as to actually upload the part to the GitHub release.

                    using (var partStream = new SubStream(input, partStart, partSize))
                    {
                        part.Md5            = CryptoHelper.ComputeMD5String(partStream);
                        partStream.Position = 0;

                        var asset = await AddAssetAsync(release, partStream, $"part-{partNumber:0#}");

                        assetPartMap.Add(new Tuple<ReleaseAsset, DownloadPart>(asset, part));
                    }

                    manifest.Parts.Add(part);

                    // Loop to handle the next part (if any).

                    partNumber++;
                    partStart   += partSize;
                    cbRemaining -= partSize;
                }

                manifest.Size = manifest.Parts.Sum(part => part.Size);

                // Publish the release.

                var releaseUpdate = release.ToUpdate();

                releaseUpdate.Draft = false;

                release = await UpdateAsync(release, releaseUpdate);

                // Now that the release has been published, we can go back and fill in
                // the asset URIs for each of the download parts.

                foreach (var item in assetPartMap)
                {
                    item.Item2.Uri = GetAssetUri(release, item.Item1);
                }

                // Write the MD5 file unless disabled.

                if (!noMd5File)
                {
                    File.WriteAllText($"{sourcePath}.md5", manifest.Md5);
                }

                return manifest;
            }
        }
    }
}
