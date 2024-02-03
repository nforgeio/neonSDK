// -----------------------------------------------------------------------------
// FILE:	    Program.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
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
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using DnsClient;
using DnsClient.Protocol;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

using Neon.Blazor;
using Neon.Cryptography;
using Neon.SignalR;
using Neon.Web;

using TestBlazor.Components;

using Yarp.ReverseProxy.Configuration;

namespace TestBlazor
{
    /// <summary>
    /// Program entry point.
    /// </summary>
    public static class Program
    {

        const string DEBUG_HEADER = "Debug";
        const string DEBUG_METADATA_KEY = "debug";
        const string DEBUG_VALUE = "true";
        const string DNS_NAME = "test";
        const int PORT = 11054;

        /// <summary>
        /// IP addresses of the servers.
        /// </summary>
        public static List<IPAddress> Addresses { get; private set; } = new List<IPAddress>();

        /// <summary>
        /// Program Main.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            var tasks = new List<Task>();

            var servers = new WebApplicationBuilder[]
            {
                WebApplication.CreateBuilder(args),
                WebApplication.CreateBuilder(args),
                WebApplication.CreateBuilder(args),
                WebApplication.CreateBuilder(args),
                WebApplication.CreateBuilder(args)
            };

            var cipherKey = AesCipher.GenerateKey();

            var mock = new Mock<ILookupClient>();

            var answers = new List<DnsResourceRecord>();
            for (int i = 0; i < servers.Length; i++)
            {
                answers.Add(new SrvRecord(
                    info:     new ResourceRecordInfo($"{Dns.GetHostName()}-{i}", ResourceRecordType.SRV, QueryClass.IN, 1000, 0),
                    priority: 0,
                    weight:   50,
                    port:     PORT,
                    target:   DnsString.Parse($"127.0.0.{10 + i}")));
            }

            mock.Setup(dns => dns.QueryAsync(DNS_NAME, QueryType.SRV, QueryClass.IN, CancellationToken.None))
                .ReturnsAsync(new DnsQueryResponse()
                {
                    Answers = answers
                });

            for (int i = 0; i < servers.Length; i++)
            {
                var builder  = servers[i];
                var ipString = $"127.0.0.{10 + i}";
                var hostname = $"{Dns.GetHostName()}-{i}";

                builder.Services.AddSingleton<ILookupClient>(mock.Object);

                var dnsMock = new Mock<IDnsCache>();
                dnsMock.Setup(dns => dns.GetSelfAddress())
                    .Returns(ipString);
                dnsMock.Setup(dns => dns.ContainsKey(It.IsAny<string>())).Returns(true);
                dnsMock.SetupGet(dns => dns.Hosts).Returns(new HashSet<string>());

                builder.Services.AddSingleton<IDnsCache>(dnsMock.Object);
                builder.Services.AddMemoryCache();

                // Add services to the container.
                builder.Services
                    .AddRazorComponents()
                    .AddInteractiveServerComponents()
                    .AddInteractiveWebAssemblyComponents();

                builder.Services.AddNeonBlazor();

                builder.Services.AddDataProtection().UseAesDataProtectionProvider(cipherKey);

                var address = IPAddress.Parse(ipString);

                Addresses.Add(address);

                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.Listen(address, PORT);
                });

                builder.Services.AddSignalrProxy(options =>
                {
                    options.PeerAddress = "test";
                    options.Port        = PORT;
                    options.Hostname    = hostname;
                });

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Error");
                    app.UseWebAssemblyDebugging();
                }

                app.UseSignalrProxy();

                app.UseStaticFiles();
                app.UseRouting();
                app.UseAntiforgery();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorComponents<App>()
                        .AddInteractiveServerRenderMode()
                        .AddInteractiveWebAssemblyRenderMode()
                        .AddAdditionalAssemblies(typeof(Client.Program).Assembly);
                });

                tasks.Add(app.RunAsync());

            }

            var proxyBuilder = WebApplication.CreateBuilder(args);

            proxyBuilder.Services.AddReverseProxy()
                    .LoadFromMemory(GetRoutes(), GetClusters());

            proxyBuilder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Parse($"127.0.0.{10 + servers.Length}"), PORT);
            });

            var proxy = proxyBuilder.Build();

            // We can customize the proxy pipeline and add/remove/replace steps
            proxy.MapReverseProxy(proxyPipeline =>
            {
                proxyPipeline.UseLoadBalancing();
            });

            tasks.Add(proxy.RunAsync());

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Gets the routes for YARP config.
        /// </summary>
        /// <returns></returns>
        public static RouteConfig[] GetRoutes()
        {
            return
            [
                new RouteConfig()
                {
                    RouteId = "route" + Random.Shared.Next(), // Forces a new route id each time GetRoutes is called.
                    ClusterId = "cluster1",
                    Match = new RouteMatch
                    {
                        // Path or Hosts are required for each route. This catch-all pattern matches all request paths.
                        Path = "{**catch-all}"
                    }
                }
            ];
        }

        /// <summary>
        /// Gets the clusters for YARP config.
        /// </summary>
        /// <returns></returns>
        public static ClusterConfig[] GetClusters()
        {
            var debugMetadata = new Dictionary<string, string>
            {
                { DEBUG_METADATA_KEY, DEBUG_VALUE }
            };

            var destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase);


            foreach (var address in Addresses)
            {
                destinations.Add(address.ToString(), new DestinationConfig()
                {
                    Address = $"http://{address}:{PORT}"
                });
            }

            var config = new ClusterConfig
            {
                ClusterId = "cluster1",
                Destinations = destinations
            };


            return [ config ];
        }
    }
}
