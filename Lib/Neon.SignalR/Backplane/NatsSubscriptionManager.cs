//-----------------------------------------------------------------------------
// FILE:        NatsSubscriptionManager.cs
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

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

using AsyncKeyedLock;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

using Neon.Diagnostics;
using Neon.Tasks;

namespace Neon.SignalR
{
    internal sealed class NatsSubscriptionManager
    {
        private readonly ConcurrentDictionary<string, HubConnectionStore>      subscriptions     = new ConcurrentDictionary<string, HubConnectionStore>(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<string, CancellationTokenSource> natsSubscriptions = new ConcurrentDictionary<string, CancellationTokenSource>(StringComparer.Ordinal);
        private readonly AsyncKeyedLocker<string>                              lockProvider;
        private readonly ILogger<NatsSubscriptionManager>                      logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="lockProvider"></param>
        public NatsSubscriptionManager(
            AsyncKeyedLocker<string> lockProvider,
            ILoggerFactory           loggerFactory = null)
        {
            this.lockProvider  = lockProvider;
            this.logger        = loggerFactory?.CreateLogger<NatsSubscriptionManager>();
        }

        /// <summary>
        /// Add a subscription to the store.
        /// </summary>
        /// <param name="id">The subscription ID.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="cancellationTokenSource">Specifies the cancellation token source.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public async Task<HubConnectionStore> AddSubscriptionAsync(
            string                  id,
            HubConnectionContext    connection,
            CancellationTokenSource cancellationTokenSource)

        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            using (await lockProvider.LockAsync(GetLockKey(id, connection)))
            {
                logger?.LogDebugEx($"Subscribing to subject: [Subject={id}]");

                try
                {
                    // Avoid adding subscription if connection is closing/closed
                    // We're in a lockProvider and ConnectionAborted is triggered before OnDisconnectedAsync is called so this is guaranteed to be safe when adding while connection is closing and removing items
                    if (connection.ConnectionAborted.IsCancellationRequested)
                    {
                        throw new Exception("Connection is closing");
                    }

                    var subscription = subscriptions.GetOrAdd(id, _ => new HubConnectionStore());

                    subscription.Add(connection);

                    // Subscribe once.

                    if (subscription.Count == 1)
                    {
                        natsSubscriptions.GetOrAdd(id, _ => cancellationTokenSource);
                    }

                    return subscription;
                }
                catch (Exception e)
                {
                    logger?.LogErrorEx(e);
                    logger?.LogDebugEx($"Subscribing failed: [Subject={id}] [Connection={connection.ConnectionId}]");
                    throw;
                }
            }

        }

        /// <summary>
        /// Remove a subscription from the store.
        /// </summary>
        /// <param name="id">The subscriptn ID.</param>
        /// <param name="connection">The connection.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public async Task RemoveSubscriptionAsync(string id, HubConnectionContext connection)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            using (await lockProvider.LockAsync(GetLockKey(id, connection)))
            {
                logger?.LogDebugEx($"Unsubscribing from NATS subject: [Subject={id}] [Connection={connection.ConnectionId}]");

                try
                {
                    if (!subscriptions.TryGetValue(id, out var subscription))
                    {
                        return;
                    }

                    subscription.Remove(connection);

                    if (subscription.Count == 0)
                    {
                        subscriptions.TryRemove(id, out _);

                        if (natsSubscriptions.TryGetValue(id, out var cts))
                        {
                            cts.Cancel();
                            cts.Dispose();
                        }

                        natsSubscriptions.TryRemove(id, out _);
                    }
                }
                catch (Exception e)
                {
                    logger?.LogErrorEx(e);
                    logger?.LogDebugEx($"Unubscribing failed: [Subject={id}] [Connection={connection.ConnectionId}]");
                }
            }
        }

        private string GetLockKey(string id, HubConnectionContext context)
        {
            return $"{context.ConnectionId}-{id}";
        }
    }
}
