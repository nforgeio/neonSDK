//-----------------------------------------------------------------------------
// FILE:	    Startup.cs
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Neon.Common;
using Neon.Cryptography;
using Neon.Diagnostics;
using Neon.Web;

using DnsClient;

using Enyim;
using Enyim.Caching;
using Enyim.Caching.Configuration;

using Prometheus;

using StackExchange.Redis;

using Yarp;
using Yarp.Telemetry.Consumption;
using Yarp.ReverseProxy.Forwarder;
using Microsoft.Extensions.Logging;

namespace NeonBlazorProxy
{
    /// <summary>
    /// Configures the operator's service controllers.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// The <see cref="IConfiguration"/>.
        /// </summary>
        public IConfiguration Configuration { get; }
        
        /// <summary>
        /// The <see cref="Service"/>.
        /// </summary>
        public Service NeonBlazorProxyService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configuration">Specifies the service configuration.</param>
        /// <param name="service">Specifies the service.</param>
        public Startup(IConfiguration configuration, Service service)
        {
            this.Configuration = configuration;
            this.NeonBlazorProxyService = service;
        }

        /// <summary>
        /// Configures depdendency injection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            switch (NeonBlazorProxyService.Config.Cache.Backend)
            {
                case CacheType.Memcached:

                    NeonBlazorProxyService.Logger.LogInformationEx(() => $"Connecting to Memcached: [{NeonBlazorProxyService.Config.Cache.Memcached.Address}]");
                    services.AddEnyimMemcached(options => 
                    {
                        options.AddServer(
                            address: NeonBlazorProxyService.Config.Cache.Memcached.Address,
                            port: NeonBlazorProxyService.Config.Cache.Memcached.Port);
                        
                        options.SuppressException = false;
                        options.Protocol          = Enyim.Caching.Memcached.MemcachedProtocol.Text;

                        NeonBlazorProxyService.Logger.LogDebugEx(() => NeonHelper.JsonSerialize(options));
                    });

                    break;

                case CacheType.Redis:

                    NeonBlazorProxyService.Logger.LogInformationEx(() => $"Connecting to Redis: [{NeonBlazorProxyService.Config.Cache.Redis.Host}]");
                    services.AddStackExchangeRedisCache(options => 
                    {
                        options.ConfigurationOptions = new ConfigurationOptions()
                        {
                            EndPoints = { NeonBlazorProxyService.Config.Cache.Redis.Host },
                            Proxy     = NeonBlazorProxyService.Config.Cache.Redis.Proxy
                        };

                        NeonBlazorProxyService.Logger.LogDebugEx(() => NeonHelper.JsonSerialize(options));
                    });

                    break;

                case CacheType.InMemory:
                default:

                    NeonBlazorProxyService.Logger.LogInformationEx("Using Local cache.");
                    services.AddDistributedMemoryCache();

                    break;
            }

            services.AddSingleton(NeonBlazorProxyService);
            services.AddSingleton(NeonBlazorProxyService.Config);
            services.AddSingleton<ILogger>(NeonBlazorProxyService.Logger);
            services.AddSingleton(TelemetryHub.LoggerFactory);
            services.AddSingleton(NeonBlazorProxyService.DnsClient);
            services.AddSingleton<CacheHelper>();
            services.AddSingleton(new ForwarderRequestConfig()
            {
                ActivityTimeout = TimeSpan.FromSeconds(100)
            });
            services.AddHealthChecks();
            services.AddHttpForwarder();
            services.AddHttpClient();
            services.AddAllPrometheusMetrics();

            // Http client for Yarp.

            services.AddSingleton<HttpMessageInvoker>(
                serviceProvider =>
                {
                    return new HttpMessageInvoker(new SocketsHttpHandler()
                    {
                        UseProxy                  = false,
                        AllowAutoRedirect         = false,
                        AutomaticDecompression    = DecompressionMethods.None,
                        UseCookies                = false,
                        ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current)
                    });
                });

            // Cookie encryption cipher.

            services.AddSingleton(NeonBlazorProxyService.AesCipher);

            var cacheOptions = new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(NeonBlazorProxyService.Config.Cache.DurationSeconds)
            };

            services.AddSingleton(cacheOptions);

            services.AddSingleton<SessionTransformer>(
                serviceProvider =>
                {
                    return new SessionTransformer(serviceProvider.GetService<CacheHelper>(), NeonBlazorProxyService.Logger, cacheOptions, NeonBlazorProxyService.AesCipher);
                });

            services.AddControllers()
                .AddNeon();
        }

        /// <summary>
        /// Configures the service controllers.
        /// </summary>
        /// <param name="app">Specifies the application builder.</param>
        /// <param name="env">Specifies the web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (NeonBlazorProxyService.InDevelopment || !string.IsNullOrEmpty(NeonBlazorProxyService.GetEnvironmentVariable("DEBUG")))
            {
                app.UseDeveloperExceptionPage();
            }

            if (NeonBlazorProxyService.Config.Cache.Backend == CacheType.Memcached)
            {
                app.UseEnyimMemcached();
            }

            app.UseRouting();
            app.UseHttpMetrics(options =>
            {
                // This identifies the page when using Razor Pages.
                options.AddRouteParameter("page");
            });

            app.UseConnectionHandler();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthz");
                endpoints.MapControllers();
            });
        }
    }
}
