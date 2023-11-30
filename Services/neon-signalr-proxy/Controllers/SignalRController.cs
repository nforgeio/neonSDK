//-----------------------------------------------------------------------------
// FILE:        SignalRController.cs
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
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using DnsClient;
using DnsClient.Protocol;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Neon.Common;
using Neon.Cryptography;
using Neon.Diagnostics;
using Neon.Tasks;
using Neon.Web;

using Prometheus;

using Yarp.ReverseProxy.Forwarder;

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
        private SessionHelper          sessionHelper;

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
        /// <param name="sessionHelper">The <see cref="SessionHelper"/>.</param>
        public SignalRController(
            Service                      signalrProxyService,
            ProxyConfig                  config,
            HttpMessageInvoker           httpClient,
            IHttpForwarder               forwarder,
            CacheHelper                  cache,
            AesCipher                    aesCipher,
            LookupClient                 dnsClient,
            SessionTransformer           sessionTransformer,
            ForwarderRequestConfig       forwarderRequestConfig,
            SessionHelper                sessionHelper)
        {
            this.signalrProxyService    = signalrProxyService;
            this.config                 = config;
            this.httpClient             = httpClient;
            this.forwarder              = forwarder;
            this.cache                  = cache;
            this.cipher                 = aesCipher;
            this.transformer            = sessionTransformer;
            this.dnsClient              = dnsClient;
            this.forwarderRequestConfig = forwarderRequestConfig;
            this.sessionHelper          = sessionHelper;
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

            using var activity = TelemetryHub.ActivitySource.StartActivity();

            var isWebsocket = false;
            if (HttpContext.Request.Headers.SecWebSocketAccept.Count > 0
                || HttpContext.Request.Headers.SecWebSocketExtensions.Count > 0
                || HttpContext.Request.Headers.SecWebSocketKey.Count > 0
                || HttpContext.Request.Headers.SecWebSocketProtocol.Count > 0
                || HttpContext.Request.Headers.SecWebSocketVersion.Count > 0)
            {
                isWebsocket = true;
                WebsocketMetrics.ConnectionsEstablished.Inc();
            }
            using var gauge    = isWebsocket ? WebsocketMetrics.CurrentConnections.TrackInProgress() : new Disposable();

            backend = config.Backends.Where(b => b.Hosts.Contains(HttpContext.Request.Host.Host)).Single();

            ForwarderError error;
            bool           upstreamIsvalid = false;

            var session = sessionHelper.GetSession(HttpContext);

            if (session != null)
            {
                if (config.SessionStore == SessionStoreType.Cache)
                {
                    session = await cache.GetAsync<Session>(session.Id);
                }

                upstreamIsvalid = await IsValidTargetAsync(session.UpstreamHost);
            }
            else
            {
                session = new Session();
            }

            if (!upstreamIsvalid)
            {
                var host = await GetHostAsync();

                error    = await forwarder.SendAsync(HttpContext, $"{backend.Scheme}://{host}:{backend.Port}", httpClient, forwarderRequestConfig, transformer);

                if (error != ForwarderError.None)
                {
                    var errorFeature = HttpContext.GetForwarderErrorFeature();
                    var exception    = errorFeature.Exception;

                    Logger.LogErrorEx(exception, "CatchAll");
                }

                return;
            }

            Logger.LogDebugEx(() => NeonHelper.JsonSerialize(session));

            session.ConnectionId = HttpContext.Connection.Id;

            if (config.SessionStore == SessionStoreType.Cache)
            {
                await cache.SetAsync(session.Id, session);
            }

            signalrProxyService.CurrentConnections.Add(session.ConnectionId);

            Logger.LogDebugEx(() => $"Forwarding connection: [{NeonHelper.JsonSerializeToBytes(session)}]");
                
            error = await forwarder.SendAsync(HttpContext, $"{backend.Scheme}://{session.UpstreamHost}", httpClient, forwarderRequestConfig, transformer);

            Logger.LogDebugEx(() => $"Session closed: [{NeonHelper.JsonSerializeToBytes(session)}]");

            signalrProxyService.CurrentConnections.RemoveWhere(x => x == session.ConnectionId);

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
                .Where(r => r.Port == backend.Port && r.Target.Value == target).Any();

            return isValid;
        }
    }

    internal class Disposable : IDisposable
    {
        public void Dispose() { }
    }
}
