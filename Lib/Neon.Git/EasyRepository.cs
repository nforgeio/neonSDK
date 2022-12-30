//-----------------------------------------------------------------------------
// FILE:	    EasyRepository.cs
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
    /// easy to use high-level methods while also exposing the lower-level <see cref="GitHubClient"/> and <see cref="GitRepository"/>
    /// properties as the <see cref="RemoteApi"/> and <see cref="Local"/> properties for more advanced scenarios.
    /// </summary>
    public partial class SimpleRepository : IDisposable
    {
        private bool                isDisposed = false;
        private CredentialsHandler  credentialsProvider;
        private Remote              cachedOrigin;

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
                this.Local = new GitRepository(localRepoFolder);
            }

            if (string.IsNullOrEmpty(userAgent))
            {
                userAgent = "unknown";
            }

            this.Credentials = GitHubCredentials.Load(
                username:    username,
                accessToken: accessToken,
                email:       email);

            this.RemoteApi = new GitHubClient(new Octokit.ProductHeaderValue(userAgent))
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

            if (Local != null)
            {
                Local.Dispose();
                Local = null;
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
        public GitHubClient RemoteApi { get; private set; }

        /// <summary>
        /// Returns the associated local git repo if it exists, <c>null</c> otherwise.
        /// </summary>
        public GitRepository Local { get; private set; }

        /// <summary>
        /// Returns the repository's remote origin.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the repositoru doesn't have an origin.</exception>
        public Remote Origin
        {
            get
            {
                if (cachedOrigin != null)
                {
                    return cachedOrigin;
                }

                cachedOrigin = Local.Network.Remotes["origin"];

                if (cachedOrigin == null)
                {
                    throw new InvalidOperationException($"Local git repo [{LocalRepoFolder}] has no remote origin.");
                }

                return cachedOrigin;
            }
        }

        /// <summary>
        /// Returns <c>true</c> when the local repos has uncommitted changes.
        /// </summary>
        public bool IsDirty => Local.IsDirty();

        /// <summary>
        /// Returns the local repo's branches.
        /// </summary>
        public BranchCollection Branches => Local.Branches;

        /// <summary>
        /// Creates a <see cref="GitSignature"/> from the repo's credentials.
        /// </summary>
        /// <returns></returns>
        public GitSignature CreateSignature() => new GitSignature(Credentials.Username, Credentials.Email, DateTimeOffset.Now);

        /// <summary>
        /// Returns a <see cref="PushOptions"/> instance initialized with the credentials provider.
        /// </summary>
        /// <returns>The new <see cref="PushOptions"/>.</returns>
        public PushOptions CreatePushOptions()
        {
            return new PushOptions()
            {
                CredentialsProvider = credentialsProvider
            };
        }

        /// <summary>
        /// Normalizes a remote branch name by stripping off any leading "origin/".
        /// </summary>
        /// <param name="branchName">The branch name.</param>
        /// <returns>The normalized branch name.</returns>
        internal string NormalizeRemoteBranchName(string branchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));

            const string originPrefix = "origin/";

            if (branchName.StartsWith(originPrefix))
            {
                return branchName.Substring(originPrefix.Length);
            }
            else
            {
                return branchName;
            }
        }
    }
}
