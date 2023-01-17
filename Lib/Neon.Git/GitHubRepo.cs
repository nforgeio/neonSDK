//-----------------------------------------------------------------------------
// FILE:	    GitHubRepo.cs
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
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Deployment;
using Neon.Tasks;

using LibGit2Sharp;
using LibGit2Sharp.Handlers;

using Octokit;
using Octokit.Internal;

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
    /// repository, as well as the local git repository,
    /// </para>
    /// <note>
    /// <see cref="GitHubRepo"/> implements <see cref="IDisposable"/> and instances should be disposed
    /// when you're done with them.
    /// </note>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>To clone a GitHub repository</b>, call the <c>static</c>
    /// <see cref="CloneAsync(string, string, string, string, string, string, string, IProfileClient)"/>
    /// method, passing the GitHub repository path, path the the local folder and optionally the branch to
    /// be checked out as well as the GitHub credentials.  This returns the <see cref="GitHubRepo"/>
    /// that you'll use for subsequent operations.
    /// </para>
    /// <para>
    /// <b>To manage a GitHub repository that doesn't have a local clone</b>,
    /// call <see cref="GitHubRepo.ConnectAsync(string, string, string, string, string, IProfileClient)"/>.
    /// </para>
    /// <para>
    /// <b>To open an existing local repository</b>, call <see cref="OpenAsync(string, string, string, string, string, IProfileClient)"/>.
    /// </para>
    /// <para>
    /// <b>To perform only GitHub account operations</b> Call the static <see cref="ConnectAsync(string, string, string, string, string, IProfileClient)"/>
    /// method to construct an instance without a local repository reference.
    /// </para>
    /// <para>
    /// The <see cref="Local"/> property provides some easy-to-use methods for managing the
    /// associated local git repository.
    /// </para>
    /// <para>
    /// The <see cref="Remote"/> property provides some easy-to-use methods for managing the associated
    /// GitHub repository.  These implement some common operations and are easier to use than the stock
    /// Octokit implementations.
    /// </para>
    /// <para>
    /// The lower-level <see cref="GitHubApi"/> can be used to manage GitHub assets directly and <see cref="GitApi"/>
    /// is the lower-level API that can be used to manage the local git repository.
    /// </para>
    /// </remarks>
    public partial class GitHubRepo : IDisposable
    {
        //---------------------------------------------------------------------
        // Static members

        /// <summary>
        /// Creates a <see cref="GitHubRepo"/> instance that's connected to GitHub account
        /// but is not associated with a local git repository.  This is useful when you only
        /// need to perform GitHub operations.
        /// </summary>
        /// <param name="remoteRepoPath">Specifies the GitHub remote repository path, like: <b>[SERVER/]OWNER/REPO</b></param>
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
        /// <returns>The new <see cref="GitHubRepo"/> instance.</returns>
        /// <remarks>
        /// <note>
        /// <see cref="NoLocalRepositoryException"/> will be thrown whenever operations on the non-existent
        /// repository are attempted for <see cref="GitHubRepo"/> instance returned by this method.
        /// </note>
        /// </remarks>
        public static async Task<GitHubRepo> ConnectAsync(
            string          remoteRepoPath,
            string          username      = null, 
            string          accessToken   = null, 
            string          email         = null, 
            string          userAgent     = null,
            IProfileClient  profileClient = null)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(remoteRepoPath), nameof(remoteRepoPath));

            if (string.IsNullOrEmpty(userAgent))
            {
                userAgent = "unknown";
            }

            var repo = new GitHubRepo();

            repo.Credentials = GitHubCredentials.Load(
                username:      username,
                accessToken:   accessToken,
                email:         email,
                profileClient: profileClient);

            repo.GitHubApi = new GitHubClient(new Octokit.ProductHeaderValue(userAgent))
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

            repo.Local  = new LocalRepoApi(repo, null);
            repo.Remote = await RemoteRepoApi.CreateAsync(repo, RemoteRepoPath.Parse(remoteRepoPath));

            repo.CreateHttpClient(userAgent);

            return await Task.FromResult(repo);
        }

        /// <summary>
        /// Clones a GitHub repository to a local folder.
        /// </summary>
        /// <param name="remoteRepoPath">Specifies the GitHub remote repository path, like: <b>[SERVER/]OWNER/REPO</b></param>
        /// <param name="localRepoFolder">Specifies the folder where the local git repository will be created or where it already exists.</param>
        /// <param name="branchName">
        /// Optionally specifies the branch to be checked out after the clone operation completes.
        /// This defaults to the GitHub remote repository's default branch (typically <b>main</b> or <b>master</b>).
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
        /// <returns>The new <see cref="GitHubRepo"/> instance.</returns>
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
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(remoteRepoPath), nameof(remoteRepoPath));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(localRepoFolder), nameof(localRepoFolder));

            if (string.IsNullOrEmpty(userAgent))
            {
                userAgent = "unknown";
            }

            var repo = new GitHubRepo();

            repo.Credentials = GitHubCredentials.Load(
                username:      username,
                accessToken:   accessToken,
                email:         email,
                profileClient: profileClient);

            repo.GitHubApi = new GitHubClient(new Octokit.ProductHeaderValue(userAgent))
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

            repo.Local  = new LocalRepoApi(repo, localRepoFolder);
            repo.Remote = await RemoteRepoApi.CreateAsync(repo, RemoteRepoPath.Parse(remoteRepoPath));

            if (Directory.Exists(localRepoFolder))
            {
                throw new LibGit2SharpException($"Local repository [{localRepoFolder}] folder already exists.");
            }

            Directory.CreateDirectory(localRepoFolder);

            var remoteRepo = $"https://{remoteRepoPath}";
            var options    = new CloneOptions() { BranchName = branchName };

            GitRepository.Clone(remoteRepo, localRepoFolder, options);

            repo.GitApi = new GitRepository(localRepoFolder);

            repo.CreateHttpClient(userAgent);

            return await Task.FromResult(repo);
        }

        /// <summary>
        /// <para>
        /// Creates a <see cref="GitHubRepo"/> that references an existing local git repository as well as
        /// the associated GitHub remote repository API.
        /// </para>
        /// <para>
        /// This requires GitHub credentials.  These can be passed explicitly as parameters or can be retrieved 
        /// automatically  from the <b>GITHUB_USERNAME</b> and <b>GITHUB_PAT</b> environment variables or from 
        /// the <b>GITHUB_PAT[username]</b> and <c>GITHUB_PAT[password]</c> secrets via an optional
        /// <see cref="IProfileClient"/> implementation.
        /// </para>
        /// </summary>
        /// <param name="localRepoFolder">Specifies the folder where the local git repository will be created or where it already exists.</param>
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
        /// <exception cref="RepositoryNotFoundException">Thrown when the local repository doesn't exist.</exception>
        public static async Task<GitHubRepo> OpenAsync(
            string          localRepoFolder,
            string          username      = null,
            string          accessToken   = null,
            string          email         = null,
            string          userAgent     = null,
            IProfileClient  profileClient = null)
        {
            await SyncContext.Clear;
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(localRepoFolder), nameof(localRepoFolder));

            if (string.IsNullOrEmpty(userAgent))
            {
                userAgent = "unknown";
            }

            if (!Directory.Exists(localRepoFolder))
            {
                throw new RepositoryNotFoundException($"The local repo [{localRepoFolder}] folder does not exist.");
            }

            var repo = new GitHubRepo();

            repo.Credentials = GitHubCredentials.Load(
                username:      username,
                accessToken:   accessToken,
                email:         email,
                profileClient: profileClient);

            repo.GitHubApi = new GitHubClient(new Octokit.ProductHeaderValue(userAgent))
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

            repo.CreateHttpClient(userAgent);

            // We're going to obtain the GitHub path for the repo from the origin's
            // push URL.  This will look something like:
            //
            //      https://github.com/neontest/neon-git
            //
            // We'll just strip off the scheme and and what remains is the path.

            repo.GitApi = new GitRepository(localRepoFolder);
            repo.Local  = new LocalRepoApi(repo, localRepoFolder);

            var pushUrl        = new Uri(repo.Origin.PushUrl);
            var remoteRepoPath = $"{pushUrl.Host}{pushUrl.AbsolutePath}";

            repo.Remote = await RemoteRepoApi.CreateAsync(repo, RemoteRepoPath.Parse(remoteRepoPath));

            return repo;
        }

        //---------------------------------------------------------------------
        // Instance members

        private bool                    isDisposed = false;
        private CredentialsHandler      credentialsProvider;
        private Remote                  cachedRemote;
        private LocalRepoApi            localRepoApi;
        private RemoteRepoApi           remoteRepoApi;
        private GitRepository           localRepository;
        private GitHubCredentials       githubCredentials;
        private GitHubClient            githubServer;
        private string                  userAgent;

        /// <summary>
        /// Private constructor.
        /// </summary>
        private GitHubRepo()
        {
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

            if (localRepository != null)
            {
                localRepository.Dispose();
                localRepository = null;
            }

            if (HttpClient != null)
            {
                HttpClient.Dispose();
                HttpClient = null;
            }

            isDisposed = true;
        }

        /// <summary>
        /// Returns the associated GitHub credentials.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the instance is disposed.</exception>
        public GitHubCredentials Credentials
        {
            get
            {
                EnsureNotDisposed();

                return githubCredentials;
            }

            set => githubCredentials = value;
        }

        /// <summary>
        /// Returns the [LibGit2Sharp] credentials provider.
        /// </summary>
        internal CredentialsHandler CredentialsProvider => credentialsProvider;

        /// <summary>
        /// Returns the lower-level OctoKit <see cref="GitHubClient"/> API that can be used
        /// to manage GitGub assets.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the instance is disposed.</exception>
        public GitHubClient GitHubApi
        {
            get
            {
                EnsureNotDisposed();

                return githubServer;
            }

            set => githubServer = value;
        }

        /// <summary>
        /// Returns the lower level <see cref="GitRepository"/> API for managing the associated local git repository.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the instance is disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        public GitRepository GitApi
        {
            get
            {
                EnsureNotDisposed();
                EnsureLocalRepo();

                return localRepository;
            }

            set => localRepository = value;
        }

        /// <summary>
        /// Returns the friendly API methods used to manage the local git repository.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the instance is disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        public LocalRepoApi Local
        {
            get
            {
                EnsureNotDisposed();
                EnsureLocalRepo();

                return localRepoApi;
            }

            set => localRepoApi = value;
        }

        /// <summary>
        /// Returns the friendly API methods used to manage the remote GitHub repository.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the instance is disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        public RemoteRepoApi Remote
        {
            get
            {
                EnsureNotDisposed();

                return remoteRepoApi;
            }

            set => remoteRepoApi = value;
        }

        /// <summary>
        /// Returns the repository's remote GitHub remote origin.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the instance is disposed.</exception>
        /// <exception cref="NoLocalRepositoryException">Thrown when the <see cref="GitHubRepo"/> is not associated with a local git repository.</exception>
        /// <exception cref="LibGit2SharpException">Thrown if the repository doesn't have a remote origin.</exception>
        public Remote Origin
        {
            get
            {
                EnsureNotDisposed();
                EnsureLocalRepo();

                if (cachedRemote != null)
                {
                    return cachedRemote;
                }

                cachedRemote = GitApi.Network.Remotes["origin"];

                if (cachedRemote == null)
                {
                    throw new LibGit2SharpException($"Local git repository [{Local.Folder}] has no associated remote origin repository.");
                }

                return cachedRemote;
            }
        }

        /// <summary>
        /// Returns a standard <see cref="HttpClient"/> configured to submit requests to GitHub using
        /// the user's credentials.  This can be useful for advanced scenarios where the Octokit built-in
        /// connection is lacking.
        /// </summary>
        internal HttpClient HttpClient { get; private set; }

        /// <summary>
        /// Creates and initializes <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="userAgent">Specifies the User-Agent to be included in HTTP requests.</param>
        private void CreateHttpClient(string userAgent)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(userAgent), nameof(userAgent));

            if (HttpClient != null)
            {
                return;
            }

            this.userAgent = userAgent;

            HttpClient = new HttpClient(
                new HttpClientHandler()
                {
                    AllowAutoRedirect        = true,
                    AutomaticDecompression   = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                    UseDefaultCredentials    = true,
                    MaxAutomaticRedirections = 30
                },
                disposeHandler: true);

            // Initialize the HTTP client User-Agent and bearer token.

            HttpClient.BaseAddress                         = GitHubApi.BaseAddress;
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Credentials.AccessToken);

            HttpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> when the instance is disposed.
        /// </summary>
        internal void EnsureNotDisposed()
        {
            if (isDisposed)
            {
                throw new ObjectDisposedException(nameof(GitHubRepo));
            }
        }

        /// <summary>
        /// Throws a <see cref="NoLocalRepositoryException"/> when the instance has no associated local repository.
        /// </summary>
        internal void EnsureLocalRepo()
        {
            if (localRepository == null)
            {
                throw new NoLocalRepositoryException();
            }
        }

        /// <summary>
        /// Normalizes a GitHub origin repository branch name by stripping off any leading "origin/" part.
        /// </summary>
        /// <param name="branchName">The branch name.</param>
        /// <returns>The normalized branch name.</returns>
        internal string NormalizeBranchName(string branchName)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(branchName), nameof(branchName));
            EnsureNotDisposed();

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

        /// <summary>
        /// Used to wait for GitHub to complete an operation.
        /// </summary>
        /// <param name="asyncPredicate">The async predicate verifying that the operation is complete.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="TimeoutException">Thrown when the operation didn't complete within the timeout period.</exception>
        /// <remarks>
        /// Some GitHub operations don't complete synchronously although it does seem like they complete
        /// within a few seconds.  We use this method to periodically execute a predicate function that
        /// returns <c>true</c> when operation completion has been confirmed.
        /// </remarks>
        internal async Task WaitForGitHubAsync(Func<Task<bool>> asyncPredicate)
        {
            await SyncContext.Clear;

            // $hack(jefflill):
            //
            // We're going to hardcode this to wait up to 30 seconds, polling the
            // predicate at one second intervals for 5 seconds and then 5 second
            // intervals for the remaining 25 seconds.

            for (int i = 0; i < 5; i++)
            {
                if (await asyncPredicate())
                {
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            for (int i = 0; i < 25; i++)
            {
                if (await asyncPredicate())
                {
                    return;
                }

                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            throw new TimeoutException("Timeout waiting for GitHub.");
        }
    }
}
