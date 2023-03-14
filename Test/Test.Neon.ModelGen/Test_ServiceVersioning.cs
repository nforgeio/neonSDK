//-----------------------------------------------------------------------------
// FILE:	    Test_ServiceVersioning.cs
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
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;
using Neon.CSharp;
using Neon.ModelGen;
using Neon.Xunit;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;

namespace TestModelGen.ServiceVersioning
{
    [Target("Default")]
    [ServiceModel]
    [ApiVersion("0.1")]
    [ApiVersion("1.0")]
    public interface VersionedService
    {
        string GetControllerVersion();

        [ApiVersion("2.0")]
        string GetSingleMethodVersion();

        [ApiVersion("2.0")]
        [ApiVersion("3.0")]
        string GetMaxMethodVersion();
    }

    public class Test_ServiceVersioning
    {
        internal static class TestSettings
        {
            public const string BaseAddress = "http://127.0.0.1:888/";
        }

        [Fact]
        public async Task VerifyVersions()
        {
            // Verify that we're generating [api-version] query parameters correctly.

            var settings = new ModelGeneratorSettings("Default")
            {
                SourceNamespace = typeof(Test_ServiceVersioning).Namespace,
            };

            var generator = new ModelGenerator(settings);
            var output    = generator.Generate(Assembly.GetExecutingAssembly());

            Assert.False(output.HasErrors);

            var assemblyStream = CSharpHelper.Compile(output.SourceCode, "test-assembly", references => ModelGenTestHelper.ReferenceHandler(references));

            // Spin up a mock service and a service client and then call the service
            // via the client.  The mock service will record the HTTP method, URI, and
            // JSON text received in the request body and then return so that the
            // caller can verify that these were passed correctly.

            var requestMethod      = string.Empty;
            var requestPath        = string.Empty;
            var requestQueryString = string.Empty;
            var requestContentType = string.Empty;
            var requestBody        = string.Empty;

            using (new MockHttpServer(TestSettings.BaseAddress,
                async context =>
                {
                    var request  = context.Request;
                    var response = context.Response;

                    requestMethod      = request.Method;
                    requestPath        = request.Path;
                    requestQueryString = request.QueryString;
                    requestContentType = request.ContentType;

                    if (request.HasEntityBody)
                    {
                        requestBody = request.GetBodyText();
                    }
                    else
                    {
                        requestBody = null;
                    }

                    response.ContentType = "application/json";

                    await Task.CompletedTask;
                }))
            {
                using (var context = new AssemblyContext("Neon.ModelGen.Output", assemblyStream))
                {
                    using (var client = context.CreateServiceWrapper<VersionedService>(TestSettings.BaseAddress))
                    {
                        await client.CallAsync("GetControllerVersion");
                        Assert.Equal("?api-version=1.0", requestQueryString);

                        await client.CallAsync("GetSingleMethodVersion");
                        Assert.Equal("?api-version=2.0", requestQueryString);

                        await client.CallAsync("GetMaxMethodVersion");
                        Assert.Equal("?api-version=3.0", requestQueryString);
                    }
                }
            }
        }
    }
}
