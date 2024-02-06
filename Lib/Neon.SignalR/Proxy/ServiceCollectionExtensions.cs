// -----------------------------------------------------------------------------
// FILE:	    ServiceCollectionExtensions.cs
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
using System.Diagnostics;
using System.Net;
using System.Net.Http;

using DnsClient;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Yarp.ReverseProxy.Forwarder;

namespace Neon.SignalR
{
    /// <summary>
    /// Service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds SignalR proxy services.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSignalrProxy(this IServiceCollection services)
        {
            services.TryAddSingleton<ILookupClient>(new LookupClient());
            services.TryAddSingleton<IDnsCache, DnsCache>();

            services.TryAddSingleton<ForwarderRequestConfig>(
                    serviceProvider =>
                    {
                        return new ForwarderRequestConfig()
                        {
                            ActivityTimeout = TimeSpan.MaxValue,
                        };
                    });

            services
                .AddHttpForwarder()
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
                .AddHostedService<ServiceDiscovey>();

            return services;
        }

        /// <summary>
        /// Adds SignalR proxy services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection AddSignalrProxy(this IServiceCollection services, Action<ProxyConfig> options = null)
        {
            var config = new ProxyConfig();
            options?.Invoke(config);

            if (string.IsNullOrEmpty(config.Hostname))
            {
                config.Hostname = Dns.GetHostName();
            }

            services.AddSingleton(config)
                .AddSignalrProxy();

            return services;

        }
    }
}
