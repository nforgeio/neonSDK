//-----------------------------------------------------------------------------
// FILE:        Test_SimpleGitRepository.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using LibGit2Sharp;

using Neon.Common;
using Neon.Deployment;
using Neon.GitHub;
using Neon.IO;
using Neon.Xunit;

using Xunit;

namespace TestGitHub
{
    [Trait(TestTrait.Category, TestArea.NeonGit)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public class Test_GitHubCredentials
    {
        private string      savedUsername;
        private string      savedAccessToken;
        private string      savedPassword;
        private string      savedEmail;
        private string      savedNcUser;

        public Test_GitHubCredentials()
        {
            GitHubTestHelper.EnsureMaintainer();
        }

        /// <summary>
        /// Saves and then clears the credential environment variables including the
        /// <b>NC_USER variable which is used by NEONFORGE maintainers to identify
        /// the developer's NEONFORGE.COM Office username which is also used to 
        /// identify the developer's 1Password vault.
        /// </summary>
        private void SaveAndClearEnvCredentials()
        {
            savedUsername    = Environment.GetEnvironmentVariable("GITHUB_USERNAME");
            savedAccessToken = Environment.GetEnvironmentVariable("GITHUB_PAT");
            savedPassword    = Environment.GetEnvironmentVariable("GITHUB_PASSWORD");
            savedEmail       = Environment.GetEnvironmentVariable("GITHUB_EMAIL");
            savedNcUser      = Environment.GetEnvironmentVariable("NC_USER");

            Environment.SetEnvironmentVariable("GITHUB_USERNAME", null);
            Environment.SetEnvironmentVariable("GITHUB_PAT", null);
            Environment.SetEnvironmentVariable("GITHUB_PASSWORD", null);
            Environment.SetEnvironmentVariable("GITHUB_EMAIL", null);
            Environment.SetEnvironmentVariable("NC_USER", null);
        }

        /// <summary>
        /// Restores the credential environment variables.
        /// </summary>
        private void RestoreEnvCredentials()
        {
            Environment.SetEnvironmentVariable("GITHUB_USERNAME", savedUsername);
            Environment.SetEnvironmentVariable("GITHUB_PAT", savedAccessToken);
            Environment.SetEnvironmentVariable("GITHUB_PASSWORD", savedPassword);
            Environment.SetEnvironmentVariable("GITHUB_EMAIL", savedEmail);
            Environment.SetEnvironmentVariable("NC_USER", savedNcUser);
        }

        [MaintainerFact]
        public void Credentials_FromEnvironment()
        {
            // Ensure that [InvalidOperationException] is thrown when one or more
            // credential parts are not present as environment variables and [NC_USER]
            // does not exist, which disables 1Password lookups.
            //
            // We're going to save and clear the service container to ensure
            // that no [IProfileClient] is present, run the tests, and then
            // restore the service container afterwards.

            var savedServiceContainer = NeonHelper.ServiceContainer.Clone();

            NeonHelper.ServiceContainer.Clear();

            try
            {
                // Expecting failure with no credential environment variables.

                SaveAndClearEnvCredentials();
                Assert.Throws<InvalidOperationException>(() => new GitHubCredentials());

                // Expecting failure with only: GITHUB_USERNAME

                Environment.SetEnvironmentVariable("GITHUB_USERNAME", savedUsername);
                Assert.Throws<InvalidOperationException>(() => new GitHubCredentials());

                // Expecting failure with only: GITHUB_USERNAME and GITHUB_PAT

                Environment.SetEnvironmentVariable("GITHUB_PAT", savedAccessToken);
                Assert.Throws<InvalidOperationException>(() => new GitHubCredentials());

                // Expecting success with all credential variables exccept password.

                Environment.SetEnvironmentVariable("GITHUB_EMAIL", savedEmail);

                var credentials = new GitHubCredentials();

                Assert.Equal(savedUsername, credentials.Username);
                Assert.Equal(savedAccessToken, credentials.AccessToken);
                Assert.Equal(savedEmail, credentials.Email);

                // Expecting success with all credential variables including password.

                Environment.SetEnvironmentVariable("GITHUB_EMAIL", savedEmail);
                Environment.SetEnvironmentVariable("GITHUB_PASSWORD", "my-password");

                credentials = new GitHubCredentials();

                Assert.Equal(savedUsername, credentials.Username);
                Assert.Equal(savedAccessToken, credentials.AccessToken);
                Assert.Equal("my-password", credentials.Password);
                Assert.Equal(savedEmail, credentials.Email);

                // Expecting success with all credential with both password and PAT.

                Environment.SetEnvironmentVariable("GITHUB_EMAIL", savedEmail);
                Environment.SetEnvironmentVariable("GITHUB_PAT", savedAccessToken);
                Environment.SetEnvironmentVariable("GITHUB_PASSWORD", "my-password");

                credentials = new GitHubCredentials();

                Assert.Equal(savedUsername, credentials.Username);
                Assert.Equal(savedAccessToken, credentials.AccessToken);
                Assert.Equal("my-password", credentials.Password);
                Assert.Equal(savedEmail, credentials.Email);
            }
            finally
            {
                RestoreEnvCredentials();

                NeonHelper.ServiceContainer = savedServiceContainer;
            }
        }

        [MaintainerFact]
        public void Credentials_FromSecrets()
        {
            // Ensure that we can retrieve credentials as secrets when the
            // environment variables are not present.

            try
            {
                // Expecting failure with no credential variables.

                SaveAndClearEnvCredentials();
                Environment.SetEnvironmentVariable("NC_USER", savedNcUser);

                var credentials = new GitHubCredentials(profileClient: new MaintainerProfile());

                Assert.NotEmpty(credentials.Username);
                Assert.NotEmpty(credentials.AccessToken);
                Assert.NotEmpty(credentials.Email);
            }
            finally
            {
                RestoreEnvCredentials();
            }
        }

        [MaintainerFact]
        public void Credentials_FromMixed()
        {
            // Ensure that we can retrieve credentials using a mix of
            // environment variables and secrets.

            try
            {
                // Expecting failure with no credential variables.

                const string testUsername = "TEST_USERNAME_122737373";

                SaveAndClearEnvCredentials();
                Environment.SetEnvironmentVariable("NC_USER", savedNcUser);
                Environment.SetEnvironmentVariable("GITHUB_USERNAME", testUsername);

                var credentials = new GitHubCredentials(profileClient: new MaintainerProfile());

                Assert.Equal(testUsername, credentials.Username);
                Assert.NotEmpty(credentials.AccessToken);
                Assert.NotEmpty(credentials.Email);
            }
            finally
            {
                RestoreEnvCredentials();
            }
        }

        [MaintainerFact]
        public void Credentials_Explicit()
        {
            // Ensure that we can can explicitly specify the credentials.

            try
            {
                // Expecting failure with no credential variables.

                const string testUsername = "TEST_USERNAME_122737373";

                SaveAndClearEnvCredentials();

                var credentials = new GitHubCredentials(
                    username:    testUsername,
                    accessToken: "1234567890",
                    password:    "my-password",
                    email:       "sally@test.com");

                Assert.Equal(testUsername, credentials.Username);
                Assert.Equal("1234567890", credentials.AccessToken);
                Assert.Equal("my-password", credentials.Password);
                Assert.Equal("sally@test.com", credentials.Email);
            }
            finally
            {
                RestoreEnvCredentials();
            }
        }
    }
}
