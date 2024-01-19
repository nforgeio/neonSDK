// -----------------------------------------------------------------------------
// FILE:	    ServiceDiscovery.cs.cs
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
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using DnsClient;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Diagnostics;

namespace Neon.SignalR
{
    /// <summary>
    /// Service discovery background service.
    /// </summary>
    public class ServiceDiscovey : BackgroundService
    {
        private readonly ILogger<ServiceDiscovey> logger;
        private readonly DnsCache                 dnsCache;
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
            DnsCache dnsCache,
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

            using PeriodicTimer timer = new(TimeSpan.FromSeconds(30));

            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    var dns  = await lookupClient.QueryAsync(config.PeerAddress, QueryType.SRV);

                    if (dns.HasError || dns.Answers.IsEmpty())
                    {
                        logger?.LogDebugEx(() => $"DNS error: [{NeonHelper.JsonSerialize(dns)}]");
                    }

                    var srv = dns.Answers.SrvRecords().Where(r => r.Port == config.Port).ToList();

                    logger?.LogDebugEx(() => $"SRV: [{NeonHelper.JsonSerialize(srv)}]");

                    foreach (var address in srv)
                    {
                        dnsCache.Hosts.Add(address.Target.Value);

                        if (address.Target.Value.Contains(Dns.GetHostName()))
                        {
                            config.SelfAddress = address.Target.Value;
                            continue;
                        }

                        string hostName = Dns.GetHostName();
                        string ip       = (await Dns.GetHostEntryAsync(hostName)).AddressList[0].ToString();

                        if (address.Target.Value.Replace('-', '.')
                            .Contains(ip))
                        {
                            config.SelfAddress = address.Target.Value;
                            continue;
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger?.LogInformation("Timed Hosted Service is stopping.");
            }
        }
    }
}
