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
using System.Diagnostics;
using System.Net;
using System.Net.Http;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Neon.Common;
using Neon.Diagnostics;
using Neon.Web;

using Prometheus;

using StackExchange.Redis;

using Yarp.ReverseProxy.Forwarder;

namespace NeonSignalRProxy
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
        public Service NeonSignalRProxyService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configuration">Specifies the service configuration.</param>
        /// <param name="service">Specifies the service.</param>
        public Startup(IConfiguration configuration, Service service)
        {
            this.Configuration = configuration;
            this.NeonSignalRProxyService = service;
        }

        /// <summary>
        /// Configures depdendency injection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            switch (NeonSignalRProxyService.Config.Cache.Backend)
            {
                case CacheType.Memcached:

                    NeonSignalRProxyService.Logger.LogInformationEx(() => $"Connecting to Memcached: [{NeonSignalRProxyService.Config.Cache.Memcached.Address}]");
                    services.AddEnyimMemcached(options => 
                    {
                        options.AddServer(
                            address: NeonSignalRProxyService.Config.Cache.Memcached.Address,
                            port: NeonSignalRProxyService.Config.Cache.Memcached.Port);
                        
                        options.SuppressException = false;
                        options.Protocol          = Enyim.Caching.Memcached.MemcachedProtocol.Text;

                        NeonSignalRProxyService.Logger.LogDebugEx(() => NeonHelper.JsonSerialize(options));
                    });

                    break;

                case CacheType.Redis:

                    NeonSignalRProxyService.Logger.LogInformationEx(() => $"Connecting to Redis: [{NeonSignalRProxyService.Config.Cache.Redis.Host}]");
                    services.AddStackExchangeRedisCache(options => 
                    {
                        options.ConfigurationOptions = new ConfigurationOptions()
                        {
                            EndPoints = { NeonSignalRProxyService.Config.Cache.Redis.Host },
                            Proxy     = NeonSignalRProxyService.Config.Cache.Redis.Proxy
                        };

                        NeonSignalRProxyService.Logger.LogDebugEx(() => NeonHelper.JsonSerialize(options));
                    });

                    break;

                case CacheType.InMemory:
                default:

                    NeonSignalRProxyService.Logger.LogInformationEx("Using Local cache.");
                    services.AddDistributedMemoryCache();

                    break;
            }

            services.AddHttpClient(Options.DefaultName).UseHttpClientMetrics();
            
            services.AddSingleton(NeonSignalRProxyService)
                .AddSingleton(NeonSignalRProxyService.Config)
                .AddSingleton<ILogger>(NeonSignalRProxyService.Logger)
                .AddSingleton(TelemetryHub.LoggerFactory)
                .AddSingleton(NeonSignalRProxyService.DnsClient)
                .AddSingleton<CacheHelper>()
                .AddSingleton<ForwarderRequestConfig>(
                    serviceProvider =>
                    {
                        return new ForwarderRequestConfig()
                        {
                            ActivityTimeout = TimeSpan.FromSeconds(100)
                        };
                    })
                .AddHttpForwarder()
                .AddAllPrometheusMetrics()
                .AddSingleton<HttpMessageInvoker>(
                    serviceProvider =>
                    {
                        return new HttpMessageInvoker(
                            new SocketsHttpHandler()
                            {
                                UseProxy                  = false,
                                AllowAutoRedirect         = false,
                                AutomaticDecompression    = DecompressionMethods.None,
                                UseCookies                = false,
                                ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current)
                            });
                    })
                .AddSingleton(NeonSignalRProxyService.AesCipher)
                .AddSingleton<SessionHelper>()
                .AddSingleton<DistributedCacheEntryOptions>(
                    serviceProvider =>
                    {
                        return new DistributedCacheEntryOptions()
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(NeonSignalRProxyService.Config.Cache.DurationSeconds)
                        };
                    })
                .AddSingleton<SessionTransformer>()
                .AddHealthChecks();

            services
                .AddControllers()
                .AddNeon();
        }

        /// <summary>
        /// Configures the service controllers.
        /// </summary>
        /// <param name="app">Specifies the application builder.</param>
        /// <param name="env">Specifies the web hosting environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (NeonSignalRProxyService.InDevelopment || !string.IsNullOrEmpty(NeonSignalRProxyService.GetEnvironmentVariable("DEBUG")))
            {
                app.UseDeveloperExceptionPage();
            }

            if (NeonSignalRProxyService.Config.Cache.Backend == CacheType.Memcached)
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
