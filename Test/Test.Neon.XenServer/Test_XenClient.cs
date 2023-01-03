//-----------------------------------------------------------------------------
// FILE:	    Test_XenClient.cs
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Deployment;
using Neon.XenServer;
using Neon.Xunit;

using Xunit;

namespace Test.Neon.XenServer
{
    [Trait(TestTrait.Category, TestArea.NeonXenServer)]
    [Trait(TestTrait.Category, TestTrait.RequiresProfile)]
    [Trait(TestTrait.Category, TestTrait.Slow)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public class Test_XenClient
    {
        //---------------------------------------------------------------------
        // Static members

        private static bool     loadAttempted = false;
        private static string   hostAddress;
        private static string   hostUsername;
        private static string   hostPassword;

        /// <summary>
        /// Retrieves the XenServer host properties via <b>NeonAssistant</b>.
        /// </summary>
        /// <exception cref="ProfileException">Thrown if when required profile values couldn't be retrieved.</exception>
        private static void LoadProfile()
        {
            if (loadAttempted)
            {
                if (string.IsNullOrEmpty(hostAddress) || string.IsNullOrEmpty(hostUsername) || string.IsNullOrEmpty(hostPassword))
                {
                    throw new ProfileException("Profile could not be loaded during a previous attempt.", string.Empty);
                }

                return;
            }

            try
            {
                var profileClient = new MaintainerProfileClient();

                hostAddress  = profileClient.GetProfileValue(name: "xen-test.host");
                hostUsername = profileClient.GetSecretValue(name: "xenserver_login[username]", vault: "group-devops");
                hostPassword = profileClient.GetSecretValue(name: "xenserver_login[password]", vault: "group-devops");
            }
            finally
            {
                loadAttempted = true;
            }
        }

        /// <summary>
        /// Create a <see cref="XenClient"/> connected to the local test XenServer.
        /// </summary>
        /// <returns>The new <see cref="XenClient"/>.</returns>
        /// <exception cref="ProfileException">Thrown if when required profile values couldn't be retrieved.</exception>
        /// <exception cref="SocketException">Thrown when a connection could not be established.</exception>
        private static XenClient CreateClient()
        {
            LoadProfile();

            return new XenClient(hostAddress, hostUsername, hostPassword);
        }

        //---------------------------------------------------------------------
        // Instance members

        // $todo(jefflill):
        //
        // For now, we're just doing a sanity check here to verify that our new
        // [ThinCliProtocol] implementation (which replaces the embedded [xe.exe]
        // CLI) doesn't blow up.
        //
        // Eventually, we should add some comprehensive tests.

        [Fact]
        public void HostInfo()
        {
            using (var client = CreateClient())
            {
                var hostInfo = client.GetHostInfo();
            }
        }

        [Fact]
        public void Machine()
        {
            using (var client = CreateClient())
            {
                client.Machine.List();
            }
        }

        [Fact]
        public void Storage()
        {
            // Exercise storage repository operations.

            using (var client = CreateClient())
            {
                Assert.NotEmpty(client.Storage.List());
            }
        }

        [Fact]
        public void Template()
        {
            using (var client = CreateClient())
            {
                Assert.NotEmpty(client.Template.List());
            }
        }
    }
}
