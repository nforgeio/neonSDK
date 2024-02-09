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
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using DnsClient;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Diagnostics;
using Neon.Tasks;

namespace Neon.SignalR
{
    /// <summary>
    /// Service discovery background service.
    /// </summary>
    public class ServiceDiscovey : BackgroundService
    {
        private readonly ILogger<ServiceDiscovey> logger;
        private readonly IDnsCache                dnsCache;
        private readonly ProxyConfig              config;
        private readonly ILookupClient            lookupClient;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="dnsCache"></param>
        /// <param name="lookupClient"></param>
        /// <param name="logger"></param>
        public ServiceDiscovey(
            ProxyConfig config,
            IDnsCache dnsCache,
            ILookupClient lookupClient,
            ILogger<ServiceDiscovey> logger = null)
        {
            this.logger       = logger;
            this.config       = config;
            this.dnsCache     = dnsCache;
            this.lookupClient = lookupClient;
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

            await QueryAsync();

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await QueryAsync();
                }
            }
            catch (OperationCanceledException)
            {
                logger?.LogInformation("Timed Hosted Service is stopping.");
            }
        }

        private async Task QueryAsync()
        {
            await SyncContext.Clear;

            try
            {
                var dns  = await lookupClient.QueryAsync(config.PeerAddress, QueryType.SRV);

                if (dns.HasError || dns.Answers.IsEmpty())
                {
                    logger?.LogDebugEx(() => $"DNS error: [{NeonHelper.JsonSerialize(dns)}]");
                }

                var srv = dns.Answers.SrvRecords().Where(r => r.Port == config.Port).ToList();

                dnsCache.Hosts = srv.Select(s => s.Target.Value.Trim('.')).ToHashSet();

                foreach (var address in srv)
                {
                    try
                    {
                        if (address.DomainName.Value.Contains(config.Hostname)
                            || address.Target.Value.Contains(config.Hostname))
                        {
                            var self = address.Target.Value.TrimEnd('.');

                            dnsCache.SetSelfAddress(self);

                            logger?.LogDebugEx(() => $"Service discovery complete.");

                            continue;
                        }

                        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

                        var hostEntry   = await Dns.GetHostEntryAsync(
                            hostNameOrAddress: config.Hostname,
                            family:            System.Net.Sockets.AddressFamily.InterNetwork,
                            cancellationToken: cts.Token);

                        var ipString = address.Target.Value.Split('.').FirstOrDefault()?.Replace("-", ".").Trim();

                        if (hostEntry.AddressList.Any(a => a.Equals(IPAddress.Parse(ipString))))
                        {
                            var self = address.Target.Value.TrimEnd('.');

                            dnsCache.SetSelfAddress(self);

                            logger?.LogDebugEx(() => $"Service discovery complete.");

                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        if (!NeonHelper.IsDevWorkstation)
                        {
                            logger?.LogErrorEx(e, () => "Error processing record.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger?.LogErrorEx(e, () => "Error during service discovery");
            }
        }
    }
}
