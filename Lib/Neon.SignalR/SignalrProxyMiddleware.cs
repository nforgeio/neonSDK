// -----------------------------------------------------------------------------
// FILE:	    SignalrProxyMiddleware.cs
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

using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Neon.Diagnostics;

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
        /// <param name="context"></param>
        /// <param name="config"></param>
        /// <param name="forwarder"></param>
        /// <param name="httpClient"></param>
        /// <param name="forwarderRequestConfig"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public async Task InvokeAsync(
            HttpContext                     context,
            ProxyConfig                     config,
            IHttpForwarder                  forwarder,
            HttpMessageInvoker              httpClient,
            ForwarderRequestConfig          forwarderRequestConfig,
            ILogger<SignalrProxyMiddleware> logger = null)
        {
            using var activity = TraceContext.ActivitySource?.StartActivity();

            if (context.Request.Cookies.TryGetValue(cookieKey, out var upstream))
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
            }
            else
            {
                logger?.LogDebugEx(() => $"Handling request locally: {config.SelfAddress}");

                context.Response.Cookies.Append(cookieKey, config.SelfAddress);

                await next(context);
            }
        }
    }

    /// <summary>
    /// Extension methods for the <see cref="SignalrProxyMiddleware"/>.
    /// </summary>
    public static class RequestCultureMiddlewareExtensions
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
