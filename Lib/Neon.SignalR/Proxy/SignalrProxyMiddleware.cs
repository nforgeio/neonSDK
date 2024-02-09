// -----------------------------------------------------------------------------
// FILE:	    SignalrProxyMiddleware.cs
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
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Diagnostics;
using Neon.SignalR.Proxy;

using Yarp.ReverseProxy.Forwarder;

namespace Neon.SignalR
{
    /// <summary>
    /// SignalR proxy middleware.
    /// </summary>
    public class SignalrProxyMiddleware
    {
        private const string cookieKey = "signalr-upstream";

        private readonly RequestDelegate next;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="next"></param>
        public SignalrProxyMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">The current http context.</param>
        /// <param name="config">The proxy configuration.</param>
        /// <param name="dnsCache">The DNS cache.</param>
        /// <param name="forwarder">The forwarder used to forward requests upstream.</param>
        /// <param name="httpClient">The http client.</param>
        /// <param name="forwarderRequestConfig">The http forwarding configuration.</param>
        /// <param name="dataProtectionProvider">An optional data protection provider.</param>
        /// <param name="logger">An optional logger.</param>
        /// <returns></returns>
        public async Task InvokeAsync(
            HttpContext                     context,
            ProxyConfig                     config,
            IDnsCache                       dnsCache,
            IHttpForwarder                  forwarder,
            HttpMessageInvoker              httpClient,
            ForwarderRequestConfig          forwarderRequestConfig,
            IDataProtectionProvider         dataProtectionProvider = null,
            ILogger<SignalrProxyMiddleware> logger = null)
        {
            using var activity = TraceContext.ActivitySource?.StartActivity();

            var dataProtector = dataProtectionProvider?.CreateProtector(TraceContext.ActivitySourceName);

            if (context.Request.Cookies.TryGetValue(cookieKey, out var upstream))
            {
                try
                {
                    upstream = dataProtector?.Unprotect(upstream) ?? upstream;

                    if (!dnsCache.ContainsKey(upstream))
                    {
                        throw new InvalidDnsException($"Upstream {upstream} not found in DNS cache");
                    }

                    if (upstream == dnsCache.GetSelfAddress())
                    {
                        await next(context);
                        return;
                    }
                }
                catch (Exception e) when (e is CryptographicException)
                {
                    logger?.LogErrorEx(e, "Error decrypting cookie");
                    context.Response.Cookies.Delete(cookieKey);
                    upstream = null;
                }
                catch (Exception e) when (e is InvalidDnsException)
                {
                    logger?.LogErrorEx(e, "Upstream no longer available");
                    context.Response.Cookies.Delete(cookieKey);
                    upstream = null;
                }
            }

            if (!string.IsNullOrEmpty(upstream))
            { 
                logger?.LogDebugEx(() => $"Forwarding to existing upstream: {upstream}");

                var error = await forwarder.SendAsync(context, $"{context.Request.Scheme}://{upstream}:{config.Port}", httpClient, forwarderRequestConfig);

                // Check if the proxy operation was successful
                if (error != ForwarderError.None)
                {
                    var errorFeature = context.Features.Get<IForwarderErrorFeature>();
                    var exception = errorFeature.Exception;

                    logger?.LogErrorEx(exception);
                }

                return;
            }

            logger?.LogDebugEx(() => $"Handling request locally: {dnsCache.GetSelfAddress()}");

            var cookieContent = dataProtector?.Protect(dnsCache.GetSelfAddress()) ?? dnsCache.GetSelfAddress();

            context.Response.Cookies.Append(cookieKey, cookieContent);

            await next(context);
        }
    }

    /// <summary>
    /// Extension methods for the <see cref="SignalrProxyMiddleware"/>.
    /// </summary>
    public static class SignalrProxyMiddlewareExtensions
    {
        /// <summary>
        /// Extension method to add the <see cref="SignalrProxyMiddleware"/> to the pipeline.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSignalrProxy(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SignalrProxyMiddleware>();
        }
    }
}
