//-----------------------------------------------------------------------------
// FILE:	    SignalRController.cs
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Cryptography;
using Neon.Diagnostics;
using Neon.Service;
using Neon.Tasks;
using Neon.Web;

using DnsClient;
using DnsClient.Protocol;

using Newtonsoft.Json;

using Yarp;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Configuration;

namespace NeonSignalRProxy.Controllers
{
    /// <summary>
    /// Implements SignalR proxy service methods.
    /// </summary>
    [ApiController]
    public class SignalRController : NeonControllerBase
    {
        private Service                signalrProxyService;
        private ProxyConfig            config;
        private HttpMessageInvoker     httpClient;
        private IHttpForwarder         forwarder;
        private SessionTransformer     transformer;
        private CacheHelper            cache;
        private AesCipher              cipher;
        private LookupClient           dnsClient;
        private ForwarderRequestConfig forwarderRequestConfig;
        private Backend                backend;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="signalrProxyService">The <see cref="Service"/></param>
        /// <param name="config">The <see cref="ProxyConfig"/></param>
        /// <param name="httpClient">HttpClient for forwarding requests.</param>
        /// <param name="forwarder">The YARP forwarder.</param>
        /// <param name="cache">The cache used for storing session information.</param>
        /// <param name="aesCipher">The <see cref="AesCipher"/> used for cookie encryption.</param>
        /// <param name="dnsClient">The <see cref="LookupClient"/> for service discovery.</param>
        /// <param name="sessionTransformer">The <see cref="SessionTransformer"/>.</param>
        /// <param name="forwarderRequestConfig">The <see cref="ForwarderRequestConfig"/>.</param>
        public SignalRController(
            Service                      signalrProxyService,
            ProxyConfig                  config,
            HttpMessageInvoker           httpClient,
            IHttpForwarder               forwarder,
            CacheHelper                  cache,
            AesCipher                    aesCipher,
            LookupClient                 dnsClient,
            SessionTransformer           sessionTransformer,
            ForwarderRequestConfig       forwarderRequestConfig)
        {
            this.signalrProxyService     = signalrProxyService;
            this.config                 = config;
            this.httpClient             = httpClient;
            this.forwarder              = forwarder;
            this.cache                  = cache;
            this.cipher                 = aesCipher;
            this.transformer            = sessionTransformer;
            this.dnsClient              = dnsClient;
            this.forwarderRequestConfig = forwarderRequestConfig;
        }

        /// <summary>
        /// <para>
        /// Proxies the SignalR websocket request to the correct SignalR backend server. This is implemented by inspecting the Session Cookie which contains a 
        /// reference to the Session ID. The Session Backend is retreived from the <see cref="cache"/> using the Session ID as the key. Once the correct 
        /// SignalR backend server is identified, the request is proxied upstream using the <see cref="forwarder"/>.
        /// </para>
        /// </summary>
        /// <returns></returns>
        [Route("{**catchAll}")]
        public async Task SignalRAsync()
        {
            await SyncContext.Clear;

            using (var activity = TelemetryHub.ActivitySource.StartActivity())
            {
                backend = config.Backends.Where(b => b.Hosts.Contains(HttpContext.Request.Host.Host)).Single();

                var cookies      = HttpContext.Request.Cookies.Where(c => c.Key == Service.SessionCookieName);

                ForwarderError error;
                Session        session = new Session();
                bool           upstreamIsvalid = false;

                if (cookies.Any())
                {
                    var cookie       = cookies.Single();
                    var cookieString = cipher.DecryptStringFrom(cookie.Value);
                    session          = NeonHelper.JsonDeserialize<Session>(cookieString);

                    if (config.SessionStore == SessionStoreType.Cache)
                    {
                        session = await cache.GetAsync<Session>(session.Id);
                    }

                    upstreamIsvalid = await IsValidTargetAsync(session.UpstreamHost);
                }

                if (!upstreamIsvalid)
                {
                    var host = await GetHostAsync();
                    error    = await forwarder.SendAsync(HttpContext, $"{backend.Scheme}://{host}:{backend.Port}", httpClient, forwarderRequestConfig, transformer);

                    if (error != ForwarderError.None)
                    {
                        var errorFeature = HttpContext.GetForwarderErrorFeature();
                        var exception = errorFeature.Exception;

                        Logger.LogErrorEx(exception, "CatchAll");
                    }
                }

                Logger.LogDebugEx(() => NeonHelper.JsonSerialize(session));

                session.ConnectionId = HttpContext.Connection.Id;

                if (config.SessionStore == SessionStoreType.Cache)
                {
                    await cache.SetAsync(session.Id, session);
                }

                WebsocketMetrics.CurrentConnections.Inc();
                WebsocketMetrics.ConnectionsEstablished.Inc();
                signalrProxyService.CurrentConnections.Add(session.ConnectionId);

                Logger.LogDebugEx(() => $"Forwarding connection: [{NeonHelper.JsonSerializeToBytes(session)}]");
                
                error = await forwarder.SendAsync(HttpContext, $"{backend.Scheme}://{session.UpstreamHost}", httpClient, forwarderRequestConfig, transformer);

                Logger.LogDebugEx(() => $"Session closed: [{NeonHelper.JsonSerializeToBytes(session)}]");

                if (error != ForwarderError.None)
                {
                    var errorFeature = HttpContext.GetForwarderErrorFeature();
                    var exception    = errorFeature.Exception;

                    if (exception.GetType() != typeof(TaskCanceledException) && exception.GetType() != typeof(OperationCanceledException))
                    {
                        Logger.LogErrorEx(exception);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the next server using round-robin load balancing over SignalR backends.
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetHostAsync()
        {
            await SyncContext.Clear;

            using (var activity = TelemetryHub.ActivitySource.StartActivity())
            {
                if (NeonHelper.IsDevWorkstation)
                {
                    return backend.Destination;
                }

                var host = backend.Destination;
                var dns  = await dnsClient.QueryAsync(backend.Destination, QueryType.SRV);

                if (dns.HasError || dns.Answers.IsEmpty())
                {
                    Logger.LogDebugEx(() => $"DNS error: [{NeonHelper.JsonSerialize(dns)}]");
                    return host;
                }

                var srv = dns.Answers.SrvRecords().Where(r => r.Port == backend.Port).ToList();

                Logger.LogDebugEx(() => $"SRV: [{NeonHelper.JsonSerialize(srv)}]");

                host = srv.SelectRandom<SrvRecord>(1).First().Target.Value;

                Logger.LogDebugEx(() => $"DNS host: [{host}]");

                return host;
            }
        }

        private async Task<bool> IsValidTargetAsync(string target)
        {
            var dns = await dnsClient.QueryAsync(backend.Destination, QueryType.SRV);

            if (dns.HasError || dns.Answers.IsEmpty())
            {
                Logger.LogDebugEx(() => $"DNS error: [{NeonHelper.JsonSerialize(dns)}]");
                return false;
            }

            var isValid = dns.Answers.SrvRecords()
                .Where(r => r.Port == backend.Port 
                            && r.Target.Value == target).Any();

            return isValid;
        }
    }
}