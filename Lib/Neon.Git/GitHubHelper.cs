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
    /// Static <b>GitHub</b> related utilities.
    /// </summary>
    public static class GitHubHelper
    {
        /// <summary>
        /// Creates a <see cref="GitHubClient"/> using the username and access token passed explicitly 
        /// or retrieved from the <b>GITHUB_USERNAME</b> and <b>GITHUB_PAT</b> environment variables
        /// or from the current user's 1Password <b>GITHUB_PAT[username]</b> and <c>GITHUB_PAT[password]</c>
        /// secrets via <b>neon-assistant</b> (NEONFORGE maintainers only).
        /// </summary>
        /// <param name="username">Optionally specifies the GitHub username.</param>
        /// <param name="accessToken">Optionally specifies the GitHub Personal Access Token (PAT).</param>
        /// <param name="email">Optionally specifies the GitHub email address for the current user.</param>
        /// <param name="userAgent">Optionally specifies the user-agent to be submitted with REST calls.  This defaults to <b>"unknown"</b>.</param>
        /// <returns>The <see cref="GitHubClient"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when GitHub PAT could be located.</exception>
        /// <remarks>
        /// <note>
        /// This method also sets the process <b>GITHUB_PAT</b>environment variable so this can be 
        /// easily retrieved by scripts or other programs launched by the current process.
        /// </note>
        /// </remarks>
        public static GitHubClient GetGitHubClient(string username = null, string accessToken = null, string email = null, string userAgent = null)
        {
            var profile = new ProfileClient();

            if (string.IsNullOrEmpty(userAgent))
            {
                userAgent = "unknown";
            }

            //-----------------------------------------------------------------
            // Fetch the username:

            if (string.IsNullOrEmpty(username))
            {
                username = Environment.GetEnvironmentVariable("GITHUB_USERNAME");

                if (string.IsNullOrEmpty(username))
                {
                    username = profile.GetSecretValue("GITHUB_PAT[username]");
                }
            }

            if (!string.IsNullOrEmpty(username))
            {
                Environment.SetEnvironmentVariable("GITHUB_USERNAME", username);
            }
            else
            {
                throw new InvalidOperationException("Could not locate GitHub username.");
            }

            //-----------------------------------------------------------------
            // Fetch the access token:

            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = Environment.GetEnvironmentVariable("GITHUB_PAT");

                if (string.IsNullOrEmpty(accessToken))
                {
                    accessToken = profile.GetSecretValue("GITHUB_PAT[password]");
                }
            }

            if (string.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException("Could not locate GitHub email address.");
            }

            //-----------------------------------------------------------------
            // Fetch the email address:

            if (string.IsNullOrEmpty(email))
            {
                email = Environment.GetEnvironmentVariable("GITHUB_EMAIL");

                if (string.IsNullOrEmpty(email))
                {
                    accessToken = profile.GetSecretValue("GITHUB_PAT[email]");
                }
            }

            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException("Could not locate GitHub email address.");
            }

            //-----------------------------------------------------------------
            // Return the client.

            return new GitHubClient(new Octokit.ProductHeaderValue(userAgent))
            {
                Credentials = new Octokit.Credentials(accessToken)
            };
        }
    }
}
