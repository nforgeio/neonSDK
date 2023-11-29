//-----------------------------------------------------------------------------
// FILE:        Service.cs
// CONTRIBUTOR: Marcus Bowyer
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

using Neon;
using Neon.Common;
using Neon.Cryptography;
using Neon.Diagnostics;
using Neon.Service;

using DnsClient;

using OpenTelemetry;
using OpenTelemetry.Trace;

using Prometheus;

namespace NeonSignalRProxy
{
    /// <summary>
    /// Implements the <b>neon-signalr-proxy</b> service.
    /// </summary>
    public class Service : NeonService
    {
        //---------------------------------------------------------------------
        // Static members

        /// <summary>
        /// The Default <see cref="NeonService"/> name.
        /// </summary>
        public const string ServiceName = "neon-signalr-proxy";

        /// <summary>
        /// Config file location.
        /// </summary>
        public const string ConfigFile = "/etc/neonsdk/neon-signalr-proxy/config.yaml";

        /// <summary>
        /// Session cookie name.
        /// </summary>
        public const string SessionCookiPrefix = ".Neon.SignalR.Proxy";

        /// <summary>
        /// Lock used for updating the load balancer status.
        /// </summary>
        public static readonly object ServerLock = new object();

        /// <summary>
        /// The host name of the last server to be sent a request.
        /// </summary>
        public static string LastServer { get; set; }

        /// <summary>
        /// Counts cache lookups.
        /// </summary>
        public static readonly Counter CacheLookupsRequested = Metrics.CreateCounter(
            "neonsignalrproxy_cache_lookups_total",
            "Number of Cache lookups requested");

        /// <summary>
        /// Counts the items persisted to the cache.
        /// </summary>
        public static readonly Counter CacheItemsStored = Metrics.CreateCounter(
            "neonsignalrproxy_cache_items_stored_total",
            "Number of items stored in the Cache");

        /// <summary>
        /// Counts cache hits.
        /// </summary>
        public static readonly Counter CacheHits = Metrics.CreateCounter(
            "neonsignalrproxy_cache_hits_total",
            "Number of Cache hits");

        /// <summary>
        /// Counts cache misses.
        /// </summary>
        public static readonly Counter CacheMisses = Metrics.CreateCounter(
            "neonsignalrproxy_cache_misses_total",
            "Number of Cache misses");

        //---------------------------------------------------------------------
        // Instance members

        private IWebHost webHost;

        /// <summary>
        /// Proxy Configuration.
        /// </summary>
        public ProxyConfig Config;

        /// <summary>
        /// Dns Client.
        /// </summary>
        public LookupClient DnsClient;

        /// <summary>
        /// AES Cipher.
        /// </summary>
        public AesCipher AesCipher;

        /// <summary>
        /// HashSet containing current open websocket connection IDs. 
        /// </summary>
        public HashSet<string> CurrentConnections;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The service name.</param>
        public Service(string name)
             : base(name, version: Build.NeonSdkVersion)
        {
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // Dispose web host if it's still running.

            if (webHost != null)
            {
                webHost.Dispose();
                webHost = null;
            }
        }

        /// <inheritdoc/>
        protected async override Task<int> OnRunAsync()
        {
            await SetStatusAsync(NeonServiceStatus.Starting);

            Config = await ProxyConfig.FromFileAsync(GetConfigFilePath(ConfigFile));

            DnsClient = new LookupClient(new LookupClientOptions()
            {
                UseCache            = Config.Dns.UseCache,
                MaximumCacheTimeout = TimeSpan.FromSeconds(Config.Dns.MaximumCacheTimeoutSeconds),
                MinimumCacheTimeout = TimeSpan.FromSeconds(Config.Dns.MinimumCacheTimeoutSeconds),
                CacheFailedResults  = Config.Dns.CacheFailedResults
            });

            var redact =
#if DEBUG
                false;
#else
                true;
#endif
            AesCipher = new AesCipher(GetEnvironmentVariable("COOKIE_CIPHER", AesCipher.GenerateKey(), redact: redact));

            CurrentConnections = new HashSet<string>();

            // Start the web service.

            webHost = new WebHostBuilder()
                .ConfigureAppConfiguration(
                    (hostingcontext, config) =>
                    {
                        config.Sources.Clear();
                    })
                .UseStartup<Startup>()
                .UseKestrel(options => options.Listen(IPAddress.Any, Config.Port))
                .ConfigureServices(services => services.AddSingleton(typeof(Service), this))
                .UseStaticWebAssets()
                .Build();

            _ = webHost.RunAsync();

            Logger.LogInformationEx(() => $"Listening on: {IPAddress.Any}:{Config.Port}");

            // Indicate that the service is ready for business.

            await SetStatusAsync(NeonServiceStatus.Running);

            // Handle termination gracefully.

            await Terminator.StopEvent.WaitAsync();
            Terminator.ReadyToExit();

            return 0;
        }

        /// <inheritdoc/>
        protected override bool OnTracerConfig(TracerProviderBuilder builder)
        {
            builder.AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter(
                    options =>
                    {
                        options.ExportProcessorType = ExportProcessorType.Batch;
                        options.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>();
                        options.Endpoint = new Uri(NeonHelper.NeonKubeOtelCollectorUri);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });

            return true;
        }
    }
}
