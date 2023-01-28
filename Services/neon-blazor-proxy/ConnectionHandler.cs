//-----------------------------------------------------------------------------
// FILE:	    ConnectionHandler.cs
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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Cryptography;
using Neon.Diagnostics;
using Neon.Tasks;
using Neon.Web;

namespace NeonBlazorProxy
{
    /// <summary>
    /// This middleware takes care of removing closed sessions from the Cache after the expiration defined in
    /// <see cref="Cache.DurationSeconds"/>.
    /// </summary>
    public class ConnectionHandler
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ConnectionHandler(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        /// <summary>
        /// <para>
        /// Upserts the cache entry with an expiration time defined by <see cref="Cache.DurationSeconds"/>. 
        /// After this period, it's no longer possible to reconnect a Blazor session back to the server, 
        /// so we remove the entry from the cache.
        /// </para>
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="service"></param>
        /// <param name="cache">The Cache.</param>
        /// <param name="cipher">The AES Cipher used to encrypt/decrypt cookies.</param>
        /// <param name="cacheOptions">The Cache options.</param>
        /// <returns></returns>
        public async Task InvokeAsync(
            HttpContext                     context,
            Service                         service,
            CacheHelper                     cache, 
            AesCipher                       cipher,
            DistributedCacheEntryOptions    cacheOptions)
        {
            await SyncContext.Clear;

            using (var activity = TelemetryHub.ActivitySource.StartActivity())
            {
                await _next(context);

                if (service.CurrentConnections.Contains(context.Connection.Id))
                {
                    var cookie    = context.Request.Cookies.Where(c => c.Key == Service.SessionCookieName).First();
                    var sessionId = cipher.DecryptStringFrom(cookie.Value);
                    var session   = await cache.GetAsync<Session>(sessionId);

                    if (session.ConnectionId == context.Connection.Id)
                    {
                        await cache.SetAsync(session.Id, session, cacheOptions);
                        WebsocketMetrics.CurrentConnections.Dec();
                        service.CurrentConnections.Remove(context.Connection.Id);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Helper method to add this middleware.
    /// </summary>
    public static class ConnectionHandlerHelper
    {
        /// <summary>
        /// Adds a <see cref="ConnectionHandler"/> to the ASP.NET middleware.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>The builder.</returns>
        public static IApplicationBuilder UseConnectionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ConnectionHandler>();
        }
    }
}
