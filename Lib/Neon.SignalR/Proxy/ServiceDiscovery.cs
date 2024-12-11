// -----------------------------------------------------------------------------
// FILE:	    ServiceDiscovery.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Neon.SignalR.Proxy;

namespace Neon.SignalR
{
    /// <summary>
    /// Service discovery background service.
    /// </summary>
    public class ServiceDiscovery : BackgroundService
    {
        private readonly ILogger<ServiceDiscovery> logger;
        private readonly DnsProvider               dnsProvider;
        private readonly ProxyConfig               config;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="dnsProvider"></param>
        /// <param name="logger"></param>
        public ServiceDiscovery(
            DnsProvider               dnsProvider,
            ProxyConfig               config,
            ILogger<ServiceDiscovery> logger = null)
        {
            this.dnsProvider = dnsProvider;
            this.config      = config;
            this.logger      = logger;
        }

        /// <summary>
        /// Executes the service.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger?.LogInformation("Timed Hosted Service running.");

            using PeriodicTimer timer = new PeriodicTimer(config.DnsProbeInterval);

            await this.dnsProvider.QueryAsync();

            if (config.ReadyFunction != null)
            {
                await config.ReadyFunction();
            }

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await this.dnsProvider.QueryAsync();
                }
            }
            catch (OperationCanceledException)
            {
                logger?.LogInformation("Timed Hosted Service is stopping.");
            }
        }
    }
}
