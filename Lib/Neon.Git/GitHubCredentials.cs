//-----------------------------------------------------------------------------
// FILE:	    GitHubHelper.cs
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
using System.Diagnostics.CodeAnalysis;
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

using Octokit;

namespace Neon.Git
{
    /// <summary>
    /// Used internally to obtain the current user's GitHub from enviroment
    /// variables or 1Password.
    /// </summary>
    public class GitHubCredentials
    {
        //-----------------------------------------------------------------
        // Static members

        /// <summary>
        /// Loads the current user's GitHub credentials and email address.  These are
        /// obtained from environment variables or from 1Password via <b>neon</b>
        /// </summary>
        /// <param name="username">Optionally specifies the GitHub username.</param>
        /// <param name="accessToken">Optionally specifies the GitHub Personal Access Token (PAT).</param>
        /// <param name="email">Optionally specifies the GitHub email address for the current user.</param>
        /// <returns>The <see cref="GitHubCredentials"/>.</returns>
        /// <remarks>
        /// <para>
        /// This works by first trying to obtain each part of the credentials via environment
        /// variables: <b>GITHUB_USERNAME</b>, <b>GITHUB_PAT</b>, and <b>GITHIB_EMAIL</b>.
        /// </para>
        /// <para>
        /// For any credential parts that couldn't be located as environment variables, the 
        /// method will attempt to load the missing parts as 1Password secrets via 
        /// <b>neon-assistant</b> (NEONFORGE maintainers only).
        /// </para>
        /// <para>
        /// The method extracts the credentials from the <b>GITHUB_PAT</b> secret in the
        /// current user's vault as <b>GITHUB_PAT[username]</b>, <b>GITHUB_PAT[password]</b>
        /// (the token), and <b>GITHUB_PAT[email]</b>.
        /// </para>
        /// </remarks>
        public static GitHubCredentials Load(string username = null, string accessToken = null, string email = null) 
            => new GitHubCredentials(username, accessToken, email);

        //-----------------------------------------------------------------
        // Instance members

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="username">Optionally specifies the GitHub username.</param>
        /// <param name="accessToken">Optionally specifies the GitHub Personal Access Token (PAT).</param>
        /// <param name="email">Optionally specifies the GitHub email address for the current user.</param>
        public GitHubCredentials(string username = null, string accessToken = null, string email = null)
        {
            var profile   = new ProfileClient();
            var hasNcUser = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NC_USER"));

            //-----------------------------------------------------------------
            // Fetch the username:

            if (string.IsNullOrEmpty(username))
            {
                username = Environment.GetEnvironmentVariable("GITHUB_USERNAME");

                if (string.IsNullOrEmpty(username) && hasNcUser)
                {
                    username = profile.GetSecretValue("GITHUB_PAT[username]");
                }

                if (string.IsNullOrEmpty(username))
                {
                    throw new InvalidOperationException("Could not locate GitHub username.");
                }
            }

            //-----------------------------------------------------------------
            // Fetch the access token:

            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = Environment.GetEnvironmentVariable("GITHUB_PAT");

                if (string.IsNullOrEmpty(accessToken) && hasNcUser)
                {
                    accessToken = profile.GetSecretValue("GITHUB_PAT[password]");
                }

                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new InvalidOperationException("Could not locate GitHub email address.");
                }
            }

            //-----------------------------------------------------------------
            // Fetch the email address:

            if (string.IsNullOrEmpty(email))
            {
                email = Environment.GetEnvironmentVariable("GITHUB_EMAIL");

                if (string.IsNullOrEmpty(Email) && hasNcUser)
                {
                    email = profile.GetSecretValue("GITHUB_PAT[email]");
                }

                if (string.IsNullOrEmpty(email))
                {
                    throw new InvalidOperationException("Could not locate GitHub email address.");
                }
            }

            //-----------------------------------------------------------------
            // Initialize the credential properties.

            this.Username    = username;
            this.AccessToken = accessToken;
            this.Email       = email;
        }

        /// <summary>
        /// Returns the user's GitHub username.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// Returns the user's GitHub Personal Access Token (PAT).
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// Returns the user's GitHub email address.
        /// </summary>
        public string Email { get; private set; }
    }
}
