//-----------------------------------------------------------------------------
// FILE:	    SessionTransformer.cs
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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Cryptography;
using Neon.Diagnostics;
using Neon.Tasks;

using DnsClient;

using Yarp.ReverseProxy.Forwarder;
using Yarp.ReverseProxy.Transforms;

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

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="cacheOptions"></param>
        /// <param name="cipher"></param>
        /// <param name="proxyConfig"></param>
        /// <param name="LoggerFactory"></param>
        public SessionTransformer(
            CacheHelper                  cache,
            DistributedCacheEntryOptions cacheOptions,
            AesCipher                    cipher,
            ProxyConfig                  proxyConfig,
            ILoggerFactory               LoggerFactory = null)
        {
            this.cache        = cache;
            this.cacheOptions = cacheOptions;
            this.cipher       = cipher;
            this.proxyConfig  = proxyConfig;
            this.logger       = LoggerFactory?.CreateLogger<SessionTransformer>();;
        }

        /// <summary>
        /// Transforms requests before sending them upstream.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="proxyRequest"></param>
        /// <param name="destinationPrefix"></param>
        /// <returns>The tracking <see cref="ValueTask"/>.</returns>
        public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix)
        {
            await SyncContext.Clear;

            using (var activity = TelemetryHub.ActivitySource.StartActivity())
            {
                await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix);
            }
        }

        /// <summary>
        /// <para>
        /// Transforms responses before sending them back to the client. In this case it intercepts the initial Bla
        /// </para>
        /// </summary>
        /// <param name="httpContext">The HTTP Context.</param>
        /// <param name="proxyResponse">The Proxied Response.</param>
        /// <returns>The tracking <see cref="ValueTask"/>.</returns>
        public override async ValueTask<bool> TransformResponseAsync(HttpContext httpContext, HttpResponseMessage proxyResponse)
        {
            await SyncContext.Clear;

            using (var activity = TelemetryHub.ActivitySource.StartActivity())
            {
                await base.TransformResponseAsync(httpContext, proxyResponse);

                var session = new Session()
                {
                    Id           = NeonHelper.CreateBase36Uuid(),
                    UpstreamHost = proxyResponse.RequestMessage.RequestUri.Authority
                };

                var headers = proxyResponse.Content.Headers;
                var mediaType = headers.ContentType?.MediaType ?? "";

                if (proxyConfig.SessionStore == SessionStoreType.Cache)
                {
                    await cache.SetAsync(session.Id, session);
                }

                if (!httpContext.Request.Cookies.ContainsKey(Service.SessionCookieName) || (mediaType == "text/html" && httpContext.Response.StatusCode == 200))
                {
                    if (proxyConfig.SessionStore != SessionStoreType.Cookie)
                    {
                        session.ConnectionId = null;
                        session.UpstreamHost = null;
                    }

                    httpContext.Response.Cookies.Append(Service.SessionCookieName, cipher.EncryptToBase64(NeonHelper.JsonSerialize(session)));
                }

                return true;
            }
        }
    }
}