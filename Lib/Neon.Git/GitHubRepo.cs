//-----------------------------------------------------------------------------
// FILE:	    GitHubRepo.cs
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
    /// <para>
    /// Wraps a <see cref="GitHubClient"/> and <see cref="GitRepository"/> into a single object that provides
    /// easy to use high-level methods while also including properties for the GitHub server, remote GitHub
    /// repoistory, as well as the local git repository,
    /// </para>
    /// <note>
    /// <see cref="GitHubRepo"/> implements <see cref="IDisposable"/> and instances should be disposed
    /// when you're done with them.
    /// </note>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Toclone a remote GitHub repository</b>, call the <c>static</c>
    /// <see cref="CloneAsync(string, string, string, string, string, string, string, IProfileClient)"/>
    /// method, passing the GitHub repo path, path the the local folder and optionally the branch to
    /// be checked out as well as the GitHub credentials.  This returns the <see cref="GitHubRepo"/>
    /// that you'll use for subsequent operations.
    /// </para>
    /// <para>
    /// <b>To manage an existing local repo</b>, use the <see cref="GitHubRepo(string, string, string, string, string, IProfileClient)"/>
    /// constructor, passing the path the the local folder and optionally the branch to
    /// be checked out as well as the GitHub credentials.  This returns the <see cref="GitHubRepo"/>
    /// that you'll use for subsequent operations.
    /// </para>
    /// </remarks>
    public partial class GitHubRepo : IDisposable
    {
        //---------------------------------------------------------------------
        // Static members

        /// <summary>
        /// Clones a remote GitHub repository to a local folder.
        /// </summary>
        /// <param name="remoteRepoPath">Specifies the remote (GitHub) repository path, like: <b>[SERVER/]OWNER/REPO</b></param>
        /// <param name="localRepoFolder">Specifies the folder where the local git repo will be created or where it already exists.</param>
        /// <param name="branchName">
        /// Optionally specifies the branch to be checked out after the clone operation completes.
        /// This defaults to the remote repo's default branch (typically <b>main</b> or <b>master</b>).
        /// </param>
        /// <param name="username">Optionally specifies the GitHub username.</param>
        /// <param name="accessToken">Optionally specifies the GitHub Personal Access Token (PAT).</param>
        /// <param name="email">Optionally specifies the GitHub email address for the current user.</param>
        /// <param name="userAgent">
        /// Optionally specifies the user-agent to be submitted with GitHub REST API calls.  This defaults to <b>"unknown"</b>.
        /// </param>
        /// <param name="profileClient">
        /// Optionally specifies the <see cref="IProfileClient"/> instance to be used for retrieving secrets.
        /// You may also add your <see cref="IProfileClient"/> to <see cref="NeonHelper.ServiceContainer"/>
        /// and the instance will use that if this parameter is <c>null</c>.  Secrets will be queried only
        /// when a profile client is available.
        /// </param>
        /// <exception cref="LibGit2SharpException">Thrown when the local folder already exists.</exception>
        public static async Task<GitHubRepo> CloneAsync(
            string          remoteRepoPath, 
            string          localRepoFolder,
            string          branchName    = null,
            string          username      = null, 
            string          accessToken   = null, 
            string          email         = null, 
            string          userAgent     = null,
            IProfileClient  profileClient = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(remoteRepoPath), nameof(remoteRepoPath));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(localRepoFolder), nameof(localRepoFolder));

            var repo = new GitHubRepo();

            repo.LocalRepoFolder = localRepoFolder;
            repo.ServerRepoPath  = RemoteRepoPath.Parse(remoteRepoPath);

            if (Directory.Exists(localRepoFolder))
            {
                throw new LibGit2SharpException($"Local repo [{localRepoFolder}] folder already exists.");
            }

            Directory.CreateDirectory(localRepoFolder);

            if (string.IsNullOrEmpty(userAgent))
            {
                userAgent = "unknown";
            }

            repo.Credentials = GitHubCredentials.Load(
                username:      username,
                accessToken:   accessToken,
                email:         email,
                profileClient: profileClient);

            repo.Server = new GitHubClient(new Octokit.ProductHeaderValue(userAgent))
            {
                Credentials = new Octokit.Credentials(repo.Credentials.AccessToken)
            };

            repo.credentialsProvider = new CredentialsHandler(
                (url, usernameFromUrl, types) =>
                    new UsernamePasswordCredentials()
                    {
                        Username = repo.Credentials.Username,
                        Password = repo.Credentials.AccessToken
                    });

            var remoteUri = $"https://{repo.ServerRepoPath}";
            var options   = new CloneOptions() { BranchName = branchName };

            GitRepository.Clone(remoteUri, repo.LocalRepoFolder, options);

            repo.LocalRepo  = new GitRepository(repo.LocalRepoFolder);
            repo.ServerRepo = await repo.Server.Repository.Get(repo.ServerRepoPath.Owner, repo.ServerRepoPath.Name);

            return await Task.FromResult(repo);
        }

        //---------------------------------------------------------------------
        // Instance members

        private bool                isDisposed = false;
        private CredentialsHandler  credentialsProvider;
        private Remote              cachedOrigin;

        /// <summary>
        /// Private constructor.
        /// </summary>
        private GitHubRepo()
        {
        }

        /// <summary>
        /// <para>
        /// Constructs a <see cref="GitHubRepo"/> that references an existing local git repo as well as
        /// the associated remote GitHub API.
        /// </para>
        /// <para>
        /// This requires GitHub credentials.  These can be passed explicitly as parameters or can be retrieved 
        /// automatically  from the <b>GITHUB_USERNAME</b> and <b>GITHUB_PAT</b> environment variables or from 
        /// the <b>GITHUB_PAT[username]</b> and <c>GITHUB_PAT[password]</c> secrets via an optional
        /// <see cref="IProfileClient"/> implementation.
        /// </para>
        /// </summary>
        /// <param name="localRepoFolder">Specifies the folder where the local git repo will be created or where it already exists.</param>
        /// <param name="username">Optionally specifies the GitHub username.</param>
        /// <param name="accessToken">Optionally specifies the GitHub Personal Access Token (PAT).</param>
        /// <param name="email">Optionally specifies the GitHub email address for the current user.</param>
        /// <param name="userAgent">
        /// Optionally specifies the user-agent to be submitted with GitHub REST API calls.  This defaults to <b>"unknown"</b>.
        /// </param>
        /// <param name="profileClient">
        /// Optionally specifies the <see cref="IProfileClient"/> instance to be used for retrieving secrets.
        /// You may also add your <see cref="IProfileClient"/> to <see cref="NeonHelper.ServiceContainer"/>
        /// and the instance will use that if this parameter is <c>null</c>.  Secrets will be queried only
        /// when a profile client is available.
        /// </param>
        /// <exception cref="RepositoryNotFoundException">Thrown when the local repo doesn't exist.</exception>
        public GitHubRepo(
            string          localRepoFolder,
            string          username      = null, 
            string          accessToken   = null, 
            string          email         = null, 
            string          userAgent     = null,
            IProfileClient  profileClient = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(localRepoFolder), nameof(localRepoFolder));

            if (!Directory.Exists(localRepoFolder))
            {
                throw new RepositoryNotFoundException($"The local repo [{localRepoFolder}] folder does not exist.");
            }

            this.LocalRepo = new GitRepository(localRepoFolder);

            // We're going to obtain the GitHub path for the repo from the origin's
            // push URL.  This will look something like:
            //
            //      https://github.com/neontest/neon-git
            //
            // We'll just strip off the scheme and and what remains is the path.\

            var pushUrl        = new Uri(Origin.PushUrl);
            var remoteRepoPath = $"{pushUrl.Host}{pushUrl.AbsolutePath}";

            this.ServerRepoPath  = RemoteRepoPath.Parse(remoteRepoPath);
            this.LocalRepoFolder = localRepoFolder;

            if (string.IsNullOrEmpty(userAgent))
            {
                userAgent = "unknown";
            }

            this.Credentials = GitHubCredentials.Load(
                username:      username,
                accessToken:   accessToken,
                email:         email,
                profileClient: profileClient);

            this.Server = new GitHubClient(new Octokit.ProductHeaderValue(userAgent))
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
        ~GitHubRepo()
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
        public RemoteRepoPath ServerRepoPath { get; private set; }

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
        public GitHubClient Server { get; private set; }

        /// <summary>
        /// Returns the GitHub server <see cref="GitHubRepository"/> corresponding to
        /// the local repository.
        /// </summary>
        public GitHubRepository ServerRepo { get; private set; }

        /// <summary>
        /// Returns the associated local git repo if it exists, <c>null</c> otherwise.
        /// </summary>
        public GitRepository LocalRepo { get; private set; }

        /// <summary>
        /// Returns the repository's remote origin.
        /// </summary>
        /// <exception cref="LibGit2SharpException">Thrown if the repository doesn't have an origin.</exception>
        public Remote Origin
        {
            get
            {
                if (cachedOrigin != null)
                {
                    return cachedOrigin;
                }

                cachedOrigin = LocalRepo.Network.Remotes["origin"];

                if (cachedOrigin == null)
                {
                    throw new LibGit2SharpException($"Local git repo [{LocalRepoFolder}] has no remote origin.");
                }

                return cachedOrigin;
            }
        }

        /// <summary>
        /// Returns <c>true</c> when the local repos has uncommitted changes.
        /// </summary>
        public bool IsDirty => LocalRepo.IsDirty();

        /// <summary>
        /// Returns the local repo's branches.
        /// </summary>
        public BranchCollection Branches => LocalRepo.Branches;

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
