//-----------------------------------------------------------------------------
// FILE:	    SimpleRepository.cs
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
using Neon.Deployment;

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
    /// Wraps a <see cref="GitHubClient"/> and <see cref="GitRepository"/> into a single object that provides
    /// easy to use high-level methods while also exposing the <see cref="GitHubClient"/> and <see cref="GitRepository"/>
    /// as the <see cref="GitHubApi"/> and <see cref="LocalRepo"/> properties for more advanced scenarios.
    /// </summary>
    public partial class SimpleRepository : IDisposable
    {
        private bool                isDisposed = false;
        private CredentialsHandler  credentialsProvider;

        /// <summary>
        /// <para>
        /// Constructs a <see cref="SimpleRepository"/> that references a local git repo as well as
        /// the associated remote GitHub API.
        /// </para>
        /// <para>
        /// This requires GitHub credentials.  These can be passed explicitly as parameters or can be retrieved 
        /// automatically  from the <b>GITHUB_USERNAME</b> and <b>GITHUB_PAT</b> environment variables or from 
        /// the current user's 1Password <b>GITHUB_PAT[username]</b> and <c>GITHUB_PAT[password]</c> secrets via
        /// <b>neon-assistant</b> (NEONFORGE maintainers only).
        /// </para>
        /// </summary>
        /// <param name="remoteRepoPath">Specifies the remote (GitHub) repository path, like: <b>[SERVER/]OWNER/REPO</b></param>
        /// <param name="localRepoFolder">Specifies the folder where the local git repo will be created or where it already exists.</param>
        /// <param name="username">Optionally specifies the GitHub username.</param>
        /// <param name="accessToken">Optionally specifies the GitHub Personal Access Token (PAT).</param>
        /// <param name="email">Optionally specifies the GitHub email address for the current user.</param>
        /// <param name="userAgent">
        /// Optionally specifies the user-agent to be submitted with GitHub REST API calls.  This defaults to <b>"unknown"</b>.
        /// </param>
        /// <exception cref="InvalidOperationException">Thrown when GitHub credentials could be located.</exception>
        public SimpleRepository(string remoteRepoPath, string localRepoFolder, string username = null, string accessToken = null, string email = null, string userAgent = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(remoteRepoPath), nameof(remoteRepoPath));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(localRepoFolder), nameof(localRepoFolder));

            this.RemoteRepoPath  = GitHubRepoPath.Parse(remoteRepoPath);
            this.LocalRepoFolder = localRepoFolder;

            if (Directory.Exists(localRepoFolder) && Directory.GetFiles(localRepoFolder, "*", SearchOption.AllDirectories).Length > 0)
            {
                this.LocalRepo = new GitRepository(localRepoFolder);
            }

            if (string.IsNullOrEmpty(userAgent))
            {
                userAgent = "unknown";
            }

            this.Credentials = GitHubCredentials.Load(
                username:    username,
                accessToken: accessToken,
                email:       email);

            this.GitHubApi = new GitHubClient(new Octokit.ProductHeaderValue(userAgent))
            {
                Credentials = new Octokit.Credentials(Credentials.AccessToken)
            };

            credentialsProvider = new CredentialsHandler(
                (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials()
                    {
                        Username = Credentials.Username,
                        Password = Credentials.AccessToken
                    });
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~SimpleRepository()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Provides a way for subclasses to handle disposal of any additional
        /// objects or resources.  Not that any overrides should be sure to call
        /// this base method.
        /// </summary>
        /// <param name="disposing">Passed as <c>true</c> when disposing, <c>false</c> when finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (LocalRepo != null)
            {
                LocalRepo.Dispose();
                LocalRepo = null;
            }

            isDisposed = true;
        }

        /// <summary>
        /// Returns the path to the remote GitHub repo.
        /// </summary>
        public GitHubRepoPath RemoteRepoPath { get; private set; }

        /// <summary>
        /// Returns the path to the local repo folder.
        /// </summary>
        public string LocalRepoFolder { get; private set; }

        /// <summary>
        /// Returns the associated GitHub credentials.
        /// </summary>
        public GitHubCredentials Credentials { get; private set; }

        /// <summary>
        /// Returns the <see cref="GitHubClient"/> REST API client associated with the instance.
        /// </summary>
        public GitHubClient GitHubApi { get; private set; }

        /// <summary>
        /// Returns the associated local git repo if it exists, <c>null</c> otherwise.
        /// </summary>
        public GitRepository LocalRepo { get; private set; }

        /// <summary>
        /// Returns <c>true</c> when the local repository has changes waiting to be committed.
        /// </summary>
        public bool IsDirty => LocalRepo.IsDirty();

        /// <summary>
        /// Creates a <see cref="GitSignature"/> from the repo's credentials.
        /// </summary>
        /// <returns></returns>
        public GitSignature CreateSignature() => new GitSignature(Credentials.Username, Credentials.Email, DateTimeOffset.Now);
    }
}
