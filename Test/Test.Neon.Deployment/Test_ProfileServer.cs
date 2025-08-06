//-----------------------------------------------------------------------------
// FILE:        Test_ProfileServer.cs
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
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

using Neon.Common;
using Neon.Deployment;
using Neon.IO;
using Neon.Xunit;

namespace TestDeployment
{
    [Trait(TestTrait.Category, TestArea.NeonDeployment)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public partial class Test_ProfileServer
    {
        public Test_ProfileServer()
        {
            MaintainerProfile.ClearEnvironmentCache();
        }

        /// <summary>
        /// The profile server has the potential for race condition type bugs
        /// so we can use this constant to repeat the tests several times to
        /// gain more confidence.
        //// </summary>
        private const int repeatCount = 10;

        /// <summary>
        /// Use a unique pipe name so we won't conflict with the real profile
        /// server if it's running on this machine.
        /// </summary>
        private const string pipeName = "9621a996-b35f-4f84-8c6c-7ff72cb69106";

        /// <summary>
        /// Sets handlers that return reasonable default values.
        /// </summary>
        /// <param name="server">The assistant erver.</param>
        private void SetDefaultHandlers(ProfileServer server)
        {
            server.EnsureAuthenticatedHandler = request => ProfileHandlerResult.Create(string.Empty);
            server.GetProfileValueHandler     = (request, name) => ProfileHandlerResult.Create($"{name}-profile");

            server.GetSecretPasswordHandler = 
                (request, name, vault) =>
                {
                    var sb = new StringBuilder();

                    sb.Append(name);

                    if (vault != null)
                    {
                        sb.AppendWithSeparator(vault, "-");
                    }

                    sb.Append("-password");

                    return ProfileHandlerResult.Create(sb.ToString());
                };

            server.GetSecretValueHandler =
                (request, name, vault) =>
                {
                    var sb = new StringBuilder();

                    sb.Append(name);

                    if (vault != null)
                    {
                        sb.AppendWithSeparator(vault, "-");
                    }

                    sb.Append("-secret");

                    return ProfileHandlerResult.Create(sb.ToString());
                };

            server.CallHandler =
                request =>
                {
                    // We're just going to echo the value of the "command" argument.

                    return ProfileHandlerResult.Create(request.Args["command"]);
                };

            server.SignoutHandler =
                request =>
                {
                    return ProfileHandlerResult.Create(string.Empty);
                };
        }

        [Theory]
        [Repeat(repeatCount)]
        public void MultipleRequests_Sequential(int repeatCount)
        {
            // Verify that the server is able to handle multiple requests
            // submitted one at a time.

            var client = new MaintainerProfile(pipeName);

            using (var server = new ProfileServer(pipeName))
            {
                SetDefaultHandlers(server);
                server.Start();

                Assert.Equal("zero-profile", client.GetProfileValue("zero"));
                Assert.Equal("one-profile", client.GetProfileValue("one"));
                Assert.Equal("two-profile", client.GetProfileValue("two"));
                Assert.Equal("three-profile", client.GetProfileValue("three"));
                Assert.Equal("four-profile", client.GetProfileValue("four"));
                Assert.Equal("five-profile", client.GetProfileValue("five"));
                Assert.Equal("six-profile", client.GetProfileValue("six"));
                Assert.Equal("seven-profile", client.GetProfileValue("seven"));
                Assert.Equal("eight-profile", client.GetProfileValue("eight"));
                Assert.Equal("nine-profile", client.GetProfileValue("nine"));

                var callArgs = new Dictionary<string, string>();

                callArgs["command"] = "Hello World!";

                Assert.Equal("Hello World!", client.Call(callArgs));
            }
        }

        [Theory]
        [Repeat(repeatCount)]
        public async Task MultipleRequests_Parallel(int repeatCount)
        {
            // Verify that the server is able to handle multiple requests
            // submitted in parallel but with only one server thread.

            var client = new MaintainerProfile(pipeName);

            using (var server = new ProfileServer(pipeName, threadCount: 1))
            {
                SetDefaultHandlers(server);
                server.Start();

                await Task.Run(() => Assert.Equal("zero-profile", client.GetProfileValue("zero")));
                await Task.Run(() => Assert.Equal("one-profile", client.GetProfileValue("one")));
                await Task.Run(() => Assert.Equal("two-profile", client.GetProfileValue("two")));
                await Task.Run(() => Assert.Equal("three-profile", client.GetProfileValue("three")));
                await Task.Run(() => Assert.Equal("four-profile", client.GetProfileValue("four")));
                await Task.Run(() => Assert.Equal("five-profile", client.GetProfileValue("five")));
                await Task.Run(() => Assert.Equal("six-profile", client.GetProfileValue("six")));
                await Task.Run(() => Assert.Equal("seven-profile", client.GetProfileValue("seven")));
                await Task.Run(() => Assert.Equal("eight-profile", client.GetProfileValue("eight")));
                await Task.Run(() => Assert.Equal("nine-profile", client.GetProfileValue("nine")));
            }

            // Verify that the server is able to handle multiple requests
            // submitted in parallel with multiple server threads.

            using (var server = new ProfileServer(pipeName, threadCount: 10))
            {
                SetDefaultHandlers(server);
                server.Start();

                await Task.Run(() => Assert.Equal("zero-profile", client.GetProfileValue("zero")));
                await Task.Run(() => Assert.Equal("one-profile", client.GetProfileValue("one")));
                await Task.Run(() => Assert.Equal("two-profile", client.GetProfileValue("two")));
                await Task.Run(() => Assert.Equal("three-profile", client.GetProfileValue("three")));
                await Task.Run(() => Assert.Equal("four-profile", client.GetProfileValue("four")));
                await Task.Run(() => Assert.Equal("five-profile", client.GetProfileValue("five")));
                await Task.Run(() => Assert.Equal("six-profile", client.GetProfileValue("six")));
                await Task.Run(() => Assert.Equal("seven-profile", client.GetProfileValue("seven")));
                await Task.Run(() => Assert.Equal("eight-profile", client.GetProfileValue("eight")));
                await Task.Run(() => Assert.Equal("nine-profile", client.GetProfileValue("nine")));
            }
        }

        [Theory]
        [Repeat(repeatCount)]
        public void GetProfileValue(int repeatCount)
        {
            var client = new MaintainerProfile(pipeName);

            using (var server = new ProfileServer(pipeName))
            {
                SetDefaultHandlers(server);
                server.Start();

                Assert.Equal("test-profile", client.GetProfileValue("test"));
            }
        }

        [Theory]
        [Repeat(repeatCount)]
        public void GetProfileValue_Exception(int repeatCount)
        {
            var client = new MaintainerProfile(pipeName);

            using (var server = new ProfileServer(pipeName))
            {
                SetDefaultHandlers(server);

                server.GetProfileValueHandler = (request, name) => throw new Exception("test exception");

                server.Start();

                Assert.Throws<ProfileException>(() => client.GetProfileValue("test"));
            }
        }

        [Theory]
        [Repeat(repeatCount)]
        public void GetSecretPassword(int repeatCount)
        {
            var client = new MaintainerProfile(pipeName);

            using (var server = new ProfileServer(pipeName))
            {
                SetDefaultHandlers(server);
                server.Start();

                Assert.Equal("test-password", client.GetSecretPassword("test"));
            }
        }

        [Theory]
        [Repeat(repeatCount)]
        public void GetSecretPassword_Exception(int repeatCount)
        {
            var client = new MaintainerProfile(pipeName);

            using (var server = new ProfileServer(pipeName))
            {
                SetDefaultHandlers(server);

                server.GetSecretPasswordHandler = (request, name, value) => throw new Exception("test exception");

                server.Start();

                Assert.Throws<ProfileException>(() => client.GetSecretPassword("test"));
            }
        }

        [Theory]
        [Repeat(repeatCount)]
        public void GetSecretValue(int repeatCount)
        {
            var client = new MaintainerProfile(pipeName);

            using (var server = new ProfileServer(pipeName))
            {
                SetDefaultHandlers(server);
                server.Start();

                Assert.Equal("test-secret", client.GetSecretValue("test"));
            }
        }

        [Theory]
        [Repeat(repeatCount)]
        public void GetSecretValue_Exception(int repeatCount)
        {
            var client = new MaintainerProfile(pipeName);

            using (var server = new ProfileServer(pipeName))
            {
                SetDefaultHandlers(server);

                server.GetSecretValueHandler = (request, name, value) => throw new Exception("test exception");

                server.Start();

                Assert.Throws<ProfileException>(() => client.GetSecretValue("test"));
            }
        }
    }
}
