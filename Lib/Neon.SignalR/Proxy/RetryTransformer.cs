// -----------------------------------------------------------------------------
// FILE:	    RetryTransformer.cs
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

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Neon.Tasks;

using Yarp.ReverseProxy.Forwarder;

namespace Neon.SignalR
{
    public class RetryTransformer : HttpTransformer
    {
        /// the RequestUri.</param>
        public override async ValueTask<bool> TransformResponseAsync(
                    HttpContext httpContext,
                    HttpResponseMessage proxyResponse,
                    CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            // Copy all request headers
            await base.TransformResponseAsync(httpContext, proxyResponse, cancellationToken);

            if (httpContext.Response.StatusCode >= 500)
            {
                httpContext.Features.Set<IForwarderErrorFeature>(
                    new ForwarderRetryFeature()
                    {
                        Error = ForwarderError.NoAvailableDestinations,
                        Exception = new HttpRequestException(HttpRequestError.ConnectionError,
                            $"Upstream service returned status code {httpContext.Response.StatusCode}. Retrying request.")
                    });

                return false; // Indicate that the response should not be sent to the client
            }

            return true;
        }
    }
}
