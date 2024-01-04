//-----------------------------------------------------------------------------
// FILE:        SessionTransformer.cs
// CONTRIBUTOR: Marcus Bowyer
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Cryptography;
using Neon.Diagnostics;
using Neon.Tasks;

using Yarp.ReverseProxy.Forwarder;

namespace NeonSignalRProxy
{
    /// <summary>
    /// 
    /// </summary>
    public class SessionTransformer : HttpTransformer
    {
        private CacheHelper                     cache;
        private ILogger<SessionTransformer>     logger;
        private DistributedCacheEntryOptions    cacheOptions;
        private AesCipher                       cipher;
        private ProxyConfig                     proxyConfig;
        private SessionHelper                   sessionHelper;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="cacheOptions"></param>
        /// <param name="cipher"></param>
        /// <param name="proxyConfig"></param>
        /// <param name="sessionHelper"></param>
        /// <param name="loggerFactory"></param>
        public SessionTransformer(
            CacheHelper                  cache,
            DistributedCacheEntryOptions cacheOptions,
            AesCipher                    cipher,
            ProxyConfig                  proxyConfig,
            SessionHelper                sessionHelper,
            ILoggerFactory               loggerFactory = null)
        {
            this.cache         = cache;
            this.cacheOptions  = cacheOptions;
            this.cipher        = cipher;
            this.proxyConfig   = proxyConfig;
            this.sessionHelper = sessionHelper;
            this.logger        = loggerFactory?.CreateLogger<SessionTransformer>();;
        }

        /// <summary>
        /// Transforms requests before sending them upstream.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="proxyRequest"></param>
        /// <param name="destinationPrefix"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The tracking <see cref="ValueTask"/>.</returns>
        public override async ValueTask TransformRequestAsync(
            HttpContext httpContext, 
            HttpRequestMessage proxyRequest, 
            string destinationPrefix,
            CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            using (var activity = TelemetryHub.ActivitySource.StartActivity())
            {
                await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);
            }
        }

        /// <summary>
        /// <para>
        /// Transforms responses before sending them back to the client. In this case it intercepts the initial Bla
        /// </para>
        /// </summary>
        /// <param name="httpContext">The HTTP Context.</param>
        /// <param name="proxyResponse">The Proxied Response.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The tracking <see cref="ValueTask"/>.</returns>
        public override async ValueTask<bool> TransformResponseAsync(
            HttpContext httpContext, 
            HttpResponseMessage proxyResponse,
            CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            using (var activity = TelemetryHub.ActivitySource.StartActivity())
            {
                await base.TransformResponseAsync(httpContext, proxyResponse, cancellationToken);

                var mediaType = proxyResponse?.Content?.Headers?.ContentType?.MediaType ?? "";

                var session = new Session()
                {
                    Id           = NeonHelper.CreateBase36Uuid(),
                    UpstreamHost = httpContext.Request.Headers[SessionHelper.UpstreamHostHeaderName]
                };

                if (proxyConfig.Debug == true)
                {
                    httpContext.Response.Headers.Append(SessionHelper.UpstreamHostHeaderName, session.UpstreamHost);
                }

                if (proxyConfig.SessionStore == SessionStoreType.Cache)
                {
                    await cache.SetAsync(session.Id, session);
                }

                if (httpContext.Request.Cookies == null
                    || !httpContext.Request.Cookies.ContainsKey(sessionHelper.GetCookieName(httpContext)) || (mediaType == "text/html" && httpContext.Response.StatusCode == 200))
                {
                    if (proxyConfig.SessionStore == SessionStoreType.Cache)
                    {
                        session.ConnectionId = null;
                        session.UpstreamHost = null;
                    }

                    httpContext.Response.Cookies.Append(sessionHelper.GetCookieName(httpContext), cipher.EncryptToBase64(NeonHelper.JsonSerialize(session)));
                }

                return true;
            }
        }
    }
}
