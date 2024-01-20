//-----------------------------------------------------------------------------
// FILE:        DependencyInjectionExtensions.cs
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

using AsyncKeyedLock;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

using NATS.Client;

namespace Neon.SignalR
{
    /// <summary>
    /// Helpers for adding Neon NATS backplane via dependency injection.
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Adds scale-out to a <see cref="ISignalRServerBuilder"/>, using a shared Nats server.
        /// </summary>
        /// <param name="signalrBuilder">The <see cref="ISignalRServerBuilder"/>.</param>
        /// <param name="options">An optional <see cref="Action{T}" /> to configure the provided <see cref="Options" />.</param>
        /// <returns>The same instance of the <see cref="IServiceCollection"/> for chaining.</returns>
        public static IServiceCollection AddNats(this ISignalRServerBuilder signalrBuilder, Action<Options> options = null)
        {
            var opts = ConnectionFactory.GetDefaultOptions();
            options?.Invoke(opts);

            signalrBuilder.Services.AddSingleton<IConnection>(_ => new ConnectionFactory().CreateConnection(opts));

            signalrBuilder.AddMessagePackProtocol()
               .Services.AddSingleton(new AsyncKeyedLocker<string>(options =>
               {
                   options.PoolSize = 20;
                   options.PoolInitialFill = 1;
               }))
               .AddResponseCompression(opts =>
               {
                   opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
               })
               .AddSingleton(typeof(HubLifetimeManager<>), typeof(NatsHubLifetimeManager<>));

            return signalrBuilder.Services;
        }
    }
}
