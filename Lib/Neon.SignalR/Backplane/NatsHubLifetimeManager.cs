//-----------------------------------------------------------------------------
// FILE:        NatsHubLifetimeManager.cs
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using AsyncKeyedLock;

using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;

using NATS.Client.Core;

using Neon.Common;
using Neon.Diagnostics;
using Neon.Tasks;

namespace Neon.SignalR
{
    /// <summary>
    /// The NATS scaleout provider for multi-server support.
    /// </summary>
    /// <typeparam name="THub">The type of <see cref="Hub"/> to manage connections for.</typeparam>
    public class NatsHubLifetimeManager<THub> : HubLifetimeManager<THub>, IAsyncDisposable where THub : Hub
    {
        private readonly HubConnectionStore                    hubConnections        = new HubConnectionStore();
        private readonly ClientResultsManager                  clientResultsManager  = new();
        private readonly AsyncKeyedLocker<string>              lockProvider;
        private readonly NatsSubscriptionManager               connections;
        private readonly NatsSubscriptionManager               groups;
        private readonly NatsSubscriptionManager               users;
        private readonly ILogger<NatsHubLifetimeManager<THub>> logger;
        private readonly NatsConnection                        nats;
        private readonly NatsSubjects                          subjects;
        private readonly string                                serverName;

        private int internalAckId;

        /// <summary>
        /// Constructs the <see cref="NatsHubLifetimeManager{THub}"/> with types from Dependency Injection.
        /// </summary>
        /// <param name="connection">The NATS <see cref="NatsConnection"/>.</param>
        /// <param name="lockProvider">Async lock provider.</param>
        public NatsHubLifetimeManager(
            NatsConnection           connection,
            AsyncKeyedLocker<string> lockProvider)
            => new NatsHubLifetimeManager<THub>(connection, lockProvider, loggerFactory: null);

        /// <summary>
        /// Constructs the <see cref="NatsHubLifetimeManager{THub}"/> with types from Dependency Injection.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="lockProvider">Async lock provider.</param>
        /// <param name="connection">The NATS <see cref="NatsConnection"/>.</param>
        public NatsHubLifetimeManager(
            NatsConnection           connection,
            AsyncKeyedLocker<string> lockProvider,
            ILoggerFactory           loggerFactory = null)
        {
            this.serverName   = GenerateServerName();
            this.nats         = connection;
            this.logger       = loggerFactory?.CreateLogger<NatsHubLifetimeManager<THub>>();
            this.users        = new NatsSubscriptionManager(lockProvider, loggerFactory);
            this.groups       = new NatsSubscriptionManager(lockProvider, loggerFactory);
            this.connections  = new NatsSubscriptionManager(lockProvider, loggerFactory);
            this.subjects     = new NatsSubjects($"Neon.SignalR.{typeof(THub).FullName}");
            this.lockProvider = lockProvider;

            _ = SubscribeToAllAsync();
            _ = SubscribeToGroupManagementSubjectAsync();
        }

        private async Task EnsureNatsServerConnection()
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            if (nats.ConnectionState == NatsConnectionState.Open)
            {
                return;
            }

            logger?.LogDebugEx(() => $"Nats connection is {nats.ConnectionState.ToMemberString}, connecting.");

            await nats.PingAsync();

            if (nats.ConnectionState == NatsConnectionState.Closed)
            {
                throw new NatsException("The connection to NATS is closed");
            }

            using (await lockProvider.LockAsync(typeof(THub).FullName))
            {
                await NeonHelper.WaitForAsync(
                    async () =>
                    {
                        await SyncContext.Clear;

                        return !(nats.ConnectionState == NatsConnectionState.Reconnecting);
                    },
                timeout: TimeSpan.FromSeconds(60),
                pollInterval: TimeSpan.FromMilliseconds(250));

                logger?.LogDebugEx(() => $"Connected to NATS.");
            }
        }

        /// <inheritdoc/>
        public override async Task OnConnectedAsync(HubConnectionContext connection)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            logger?.LogDebugEx(() => $"Connection [{connection.ConnectionId}] connected.");

            await EnsureNatsServerConnection();

            var feature = new NatsFeature();

            connection.Features.Set<INatsFeature>(feature);

            hubConnections.Add(connection);

            var tasks = new List<Task>();

            tasks.Add(SubscribeToConnectionAsync(connection));

            if (!string.IsNullOrEmpty(connection.UserIdentifier))
            {
                tasks.Add(SubscribeToUserAsync(connection));
            }

            await Task.WhenAll(tasks);
        }

        /// <inheritdoc/>
        public override async Task OnDisconnectedAsync(HubConnectionContext connection)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            logger?.LogDebugEx(() => $"Connection [{connection.ConnectionId}] disconnected, removing hub.");

            hubConnections.Remove(connection);

            // If the nats is null then the connection failed to be established and none of the other connection setup ran.

            if (nats is null)
            {
                return;
            }

            var connectionSubject = subjects.Connection(connection.ConnectionId);

            var tasks = new List<Task>();

            tasks.Add(RemoveConnectionSubscriptionAsync(connection));
            tasks.Add(groups.RemoveSubscriptionAsync(connectionSubject, connection));

            var feature    = connection.Features.Get<INatsFeature>();
            var groupNames = feature.Groups;

            if (groupNames != null)
            {
                // Copy the groups to an array here because they get removed from this collection
                // in RemoveFromGroupAsync.

                foreach (var group in groupNames.ToArray())
                {
                    // Use RemoveGroupAsyncCore because the connection is local and we don't want to
                    // accidentally go to other servers with our remove request.

                    tasks.Add(RemoveGroupAsyncCore(connection, group));
                }
            }

            if (!string.IsNullOrEmpty(connection.UserIdentifier))
            {
                tasks.Add(RemoveUserAsync(connection));
            }

            await Task.WhenAll(tasks);
        }

        /// <inheritdoc/>
        public override async Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            Covenant.Requires<ArgumentNullException>(connectionId != null, nameof(connectionId));
            Covenant.Requires<ArgumentNullException>(groupName != null, nameof(groupName));

            var connection = hubConnections[connectionId];

            if (connection != null)
            {
                // Short circuit if connection is on this server.

                await AddGroupAsyncCore(connection, groupName);

                return;
            }

            await SendGroupActionAndWaitForAckAsync(connectionId, groupName, GroupAction.Add);
        }

        /// <inheritdoc/>
        public override async Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            Covenant.Requires<ArgumentNullException>(connectionId != null, nameof(connectionId));
            Covenant.Requires<ArgumentNullException>(groupName != null, nameof(groupName));

            var connection = hubConnections[connectionId];

            if (connection != null)
            {
                // Short circuit if connection is on this server.

                await RemoveGroupAsyncCore(connection, groupName);

                return;
            }

            await SendGroupActionAndWaitForAckAsync(connectionId, groupName, GroupAction.Remove);
        }

        /// <inheritdoc/>
        public override async Task SendAllAsync(string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            Covenant.Requires<ArgumentNullException>(methodName != null, nameof(methodName));
            Covenant.Requires<ArgumentNullException>(args != null, nameof(args));

            await PublishAsync(subjects.All, Invocation.Write(methodName: methodName, args: args));
        }

        /// <inheritdoc/>
        public override async Task SendAllExceptAsync(string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            Covenant.Requires<ArgumentNullException>(methodName != null, nameof(methodName));
            Covenant.Requires<ArgumentNullException>(args != null, nameof(args));
            Covenant.Requires<ArgumentNullException>(excludedConnectionIds != null, nameof(excludedConnectionIds));

            await PublishAsync(subjects.All, Invocation.Write(methodName: methodName, args: args, excludedConnectionIds: excludedConnectionIds));
        }

        /// <inheritdoc/>
        public override async Task SendConnectionAsync(string connectionId, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            Covenant.Requires<ArgumentNullException>(connectionId != null, nameof(connectionId));
            Covenant.Requires<ArgumentNullException>(methodName != null, nameof(methodName));
            Covenant.Requires<ArgumentNullException>(args != null, nameof(args));

            await PublishAsync(subjects.Connection(connectionId), Invocation.Write(methodName: methodName, args: args));
        }

        /// <inheritdoc/>
        public override async Task SendConnectionsAsync(IReadOnlyList<string> connectionIds, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            Covenant.Requires<ArgumentNullException>(connectionIds != null, nameof(connectionIds));
            Covenant.Requires<ArgumentNullException>(methodName != null, nameof(methodName));
            Covenant.Requires<ArgumentNullException>(args != null, nameof(args));

            var tasks   = new List<Task>();
            var message = Invocation.Write(methodName: methodName, args: args);

            foreach (var connectionId in connectionIds)
            {
                tasks.Add(PublishAsync(subjects.Connection(connectionId), message));
            }

            await Task.WhenAll(tasks);
        }

        /// <inheritdoc/>
        public override async Task SendGroupAsync(string groupName, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            Covenant.Requires<ArgumentNullException>(groupName != null, nameof(groupName));
            Covenant.Requires<ArgumentNullException>(methodName != null, nameof(methodName));
            Covenant.Requires<ArgumentNullException>(args != null, nameof(args));

            await PublishAsync(subjects.Group(groupName), Invocation.Write(methodName: methodName, args: args));
        }

        /// <inheritdoc/>
        public override async Task SendGroupExceptAsync(string groupName, string methodName, object[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            Covenant.Requires<ArgumentNullException>(groupName != null, nameof(groupName));
            Covenant.Requires<ArgumentNullException>(methodName != null, nameof(methodName));
            Covenant.Requires<ArgumentNullException>(args != null, nameof(args));
            Covenant.Requires<ArgumentNullException>(excludedConnectionIds != null, nameof(excludedConnectionIds));

            await PublishAsync(subjects.Group(groupName), Invocation.Write(methodName: methodName, args: args, excludedConnectionIds: excludedConnectionIds));
        }

        /// <inheritdoc/>
        public override async Task SendGroupsAsync(IReadOnlyList<string> groupNames, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            Covenant.Requires<ArgumentNullException>(groupNames != null, nameof(groupNames));
            Covenant.Requires<ArgumentNullException>(methodName != null, nameof(methodName));
            Covenant.Requires<ArgumentNullException>(args != null, nameof(args));

            var tasks   = new List<Task>();
            var message = Invocation.Write(methodName: methodName, args: args);

            foreach (var groupName in groupNames)
            {
                tasks.Add(PublishAsync(subjects.Group(groupName), message));
            }

            await Task.WhenAll(tasks);
        }

        /// <inheritdoc/>
        public override async Task SendUserAsync(string userId, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            Covenant.Requires<ArgumentNullException>(userId != null, nameof(userId));
            Covenant.Requires<ArgumentNullException>(methodName != null, nameof(methodName));
            Covenant.Requires<ArgumentNullException>(args != null, nameof(args));

            await PublishAsync(subjects.User(userId), Invocation.Write(methodName: methodName, args: args));
        }

        /// <inheritdoc/>
        public override async Task SendUsersAsync(IReadOnlyList<string> userIds, string methodName, object[] args, CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            Covenant.Requires<ArgumentNullException>(userIds != null, nameof(userIds));
            Covenant.Requires<ArgumentNullException>(methodName != null, nameof(methodName));
            Covenant.Requires<ArgumentNullException>(args != null, nameof(args));

            var tasks   = new List<Task>();
            var message = Invocation.Write(methodName: methodName, args: args);

            foreach (var userId in userIds)
            {
                tasks.Add(PublishAsync(subjects.User(userId), message));
            }

            await Task.WhenAll(tasks);
        }

        private static string GenerateServerName()
        {
            // Use the machine name for convenient diagnostics, but add a guid to make it unique.
            // Example: MyServerName_02db60e5fab243b890a847fa5c4dcb29
            return $"{Environment.MachineName}_{Guid.NewGuid():N}";
        }

        private async Task PublishAsync(string subject, byte[] payload)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            await EnsureNatsServerConnection();

            logger?.LogDebugEx($"Publishing message to NATS subject: [Subject={subject}].");

            await nats.PublishAsync(subject, payload);
        }

        private async Task RemoveUserAsync(HubConnectionContext connection)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            var userSubject = subjects.User(connection.UserIdentifier!);

            await users.RemoveSubscriptionAsync(userSubject, connection);
        }

        private async Task SubscribeToConnectionAsync(HubConnectionContext connection)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            var connectionSubject = subjects.Connection(connection.ConnectionId);

            var cts = new CancellationTokenSource();
            await connections.AddSubscriptionAsync(connectionSubject, connection, cts);

            _ = SubscribeToConnectionCoreAsync(connection, connectionSubject, cts.Token);
        }

        private async Task SubscribeToConnectionCoreAsync(
            HubConnectionContext connection,
            string               connectionSubject,
            CancellationToken    cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await foreach (var msg in nats.SubscribeAsync<byte[]>(connectionSubject).WithCancellation(cancellationToken))
                {
                    await SyncContext.Clear;


                    using var _activity = TraceContext.ActivitySource?.StartActivity("message-event-handler");

                    logger?.LogDebugEx($"Received message from NATS subject: [Subject={connectionSubject}].");

                    try
                    {
                        var invocation = Invocation.Read(msg.Data);
                        var message    = new InvocationMessage(invocation.MethodName, invocation.Args);

                        await connection.WriteAsync(message).AsTask();
                    }
                    catch (Exception e)
                    {
                        logger?.LogErrorEx(e);
                        logger?.LogDebugEx($"Failed writing message: [Subject={connectionSubject}] [Connection{connection.ConnectionId}]");
                    }

                };
            }
        }

        private async Task RemoveConnectionSubscriptionAsync(HubConnectionContext connection)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            var connectionSubject = subjects.Connection(connection.ConnectionId);

            await connections.RemoveSubscriptionAsync(connectionSubject, connection);
        }

        private async Task SubscribeToUserAsync(HubConnectionContext connection)
        {
            await SyncContext.Clear;

            using var activity = TraceContext.ActivitySource?.StartActivity();

            var userSubject = subjects.User(connection.UserIdentifier!);

            var cts = new CancellationTokenSource();
            var subscriptions = await users.AddSubscriptionAsync(userSubject, connection, cts);

            _ = SubscribeToUserCoreAsync(connection, subscriptions, userSubject, cts.Token);
        }

        private async Task SubscribeToUserCoreAsync(
            HubConnectionContext connection,
            HubConnectionStore   subscriptions,
            string               userSubject,
            CancellationToken    cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await foreach (var msg in nats.SubscribeAsync<byte[]>(userSubject).WithCancellation(cancellationToken))
                {
                    await SyncContext.Clear;

                    using var _activity = TraceContext.ActivitySource?.StartActivity("user-event");

                    logger?.LogDebugEx($"Received message from NATS subject: [Subject={userSubject}].");

                    try
                    {
                        var invocation = Invocation.Read(msg.Data);
                        var tasks      = new List<Task>(subscriptions.Count);
                        var message    = new InvocationMessage(invocation.MethodName, invocation.Args);

                        foreach (var userConnection in subscriptions)
                        {
                            tasks.Add(userConnection.WriteAsync(message).AsTask());
                        }

                        await Task.WhenAll(tasks);
                    }
                    catch (Exception e)
                    {
                        logger?.LogErrorEx(e);
                        logger?.LogDebugEx($"Failed writing message: [Subject={userSubject}].");
                    }
                };
            }

        }
        private async Task SubscribeToGroupAsync(
            string             groupSubject,
            HubConnectionStore groupConnections,
            CancellationToken  cancellationToken)
        {
            await SyncContext.Clear;

            while (!cancellationToken.IsCancellationRequested)
            {
                await foreach (var msg in nats.SubscribeAsync<byte[]>(groupSubject).WithCancellation(cancellationToken))
                {
                    await SyncContext.Clear;

                    logger?.LogDebugEx($"Received message from NATS subject: [Subject={groupSubject}].");

                    try
                    {
                        var invocation = Invocation.Read(msg.Data);
                        var tasks      = new List<Task>(groupConnections.Count);
                        var message    = new InvocationMessage(invocation.MethodName, invocation.Args);

                        foreach (var groupConnection in groupConnections)
                        {
                            if (invocation.ExcludedConnectionIds?.Contains(groupConnection.ConnectionId) == true)
                            {
                                continue;
                            }

                            tasks.Add(groupConnection.WriteAsync(message).AsTask());
                        }

                        await Task.WhenAll(tasks);
                    }
                    catch (Exception e)
                    {
                        logger?.LogErrorEx(e);
                        logger?.LogDebugEx($"Failed writing message: [Subject={groupSubject}].");
                    }
                };
            }
        }

        private async Task AddGroupAsyncCore(HubConnectionContext connection, string groupName)
        {
            await SyncContext.Clear;

            var feature    = connection.Features.Get<INatsFeature>()!;
            var groupNames = feature.Groups;

            using (await lockProvider.LockAsync(subjects.Group(groupName)))
            {
                // Connection already in group
                if (!groupNames.Add(groupName))
                {
                    return;
                }
            }

            var groupSubject = subjects.Group(groupName);

            var cts = new CancellationTokenSource();

            var subscriptions = await groups.AddSubscriptionAsync(groupSubject, connection, cts);

            _ = SubscribeToGroupAsync(groupSubject, subscriptions, cts.Token);
        }

        /// <summary>
        /// This takes <see cref="HubConnectionContext"/> because we want to remove the connection from the
        /// _connections list in OnDisconnectedAsync and still be able to remove groups with this method.
        /// </summary>
        private async Task RemoveGroupAsyncCore(HubConnectionContext connection, string groupName)
        {
            await SyncContext.Clear;

            var groupSubject = subjects.Group(groupName);

            await groups.RemoveSubscriptionAsync(groupSubject, connection);

            var feature    = connection.Features.Get<INatsFeature>();
            var groupNames = feature.Groups;

            if (groupNames != null)
            {
                using (await lockProvider.LockAsync(subjects.Group(groupName)))
                {
                    groupNames.Remove(groupName);
                }
            }
        }

        private async Task SendGroupActionAndWaitForAckAsync(string connectionId, string groupName, GroupAction action)
        {
            await SyncContext.Clear;

            logger?.LogDebugEx($"Publishing message to NATS subject: [Subject={subjects.GroupManagement}].");

            try
            {
                var id = Interlocked.Increment(ref internalAckId);

                // Send Add/Remove Group to other servers and wait for an ack or timeout.

                var message = GroupCommand.Write(id, serverName, action, groupName, connectionId);

                logger?.LogDebugEx($"Sending group command [ID={id}] [ServerName={serverName}] [Action={action}] [GroupName={groupName}] [ConnId={connectionId}] [Subject={subjects.GroupManagement}].");

                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await nats.RequestAsync<byte[], byte[]>(subjects.GroupManagement, message);
            }
            catch (Exception e)
            {
                logger?.LogErrorEx(e);
                logger?.LogDebugEx($"Ack timed out: [Connection={connectionId}] [Group={groupName}]");
            }
        }

        private async Task SubscribeToAllAsync()
        {
            await SyncContext.Clear;

            await EnsureNatsServerConnection();

            logger?.LogDebugEx($"Subscribing to subject: [Subject={subjects.All}].");

            var cts = new CancellationTokenSource();

            while (!cts.Token.IsCancellationRequested)
            {
                await foreach (var msg in nats.SubscribeAsync<byte[]>(subjects.All).WithCancellation(cts.Token))
                {
                    await SyncContext.Clear;

                    logger?.LogDebugEx($"Received message from NATS subject: [Subject={subjects.All}].");

                    try
                    {
                        var invocation = Invocation.Read(msg.Data);
                        var tasks      = new List<Task>(hubConnections.Count);
                        var message    = new InvocationMessage(invocation.MethodName, invocation.Args);

                        foreach (var connection in hubConnections)
                        {
                            if (invocation.ExcludedConnectionIds == null || !invocation.ExcludedConnectionIds.Contains(connection.ConnectionId))
                            {

                                tasks.Add(connection.WriteAsync(message).AsTask());
                            }
                        }

                        await Task.WhenAll(tasks);
                    }
                    catch (Exception e)
                    {
                        logger?.LogErrorEx(e);
                        logger?.LogDebugEx($"Failed writing message: [Subject={subjects.All}].");
                    }
                };
            }
        }

        private async Task SubscribeToGroupManagementSubjectAsync()
        {
            await SyncContext.Clear;

            await EnsureNatsServerConnection();

            logger?.LogDebug($"Subscribing to subject: [Subject={subjects.GroupManagement}].");

            var cts = new CancellationTokenSource();

            while (!cts.Token.IsCancellationRequested)
            {

                await foreach (var msg in nats.SubscribeAsync<byte[]>(subjects.GroupManagement).WithCancellation(cts.Token))
                {
                    await SyncContext.Clear;

                    logger?.LogDebugEx($"Received group management message from NATS subject: [Subject={subjects.GroupManagement}].");

                    try
                    {
                        var groupMessage = GroupCommand.Read(msg.Data);
                        var connection   = hubConnections[groupMessage.ConnectionId];

                        if (connection == null)
                        {
                            // user not on this server
                            logger?.LogDebugEx($"Connection [{groupMessage.ConnectionId}] not on this server.");

                            return;
                        }

                        if (groupMessage.Action == GroupAction.Remove)
                        {
                            logger?.LogDebugEx($"Removing connection [{groupMessage.ConnectionId}] to group [{groupMessage.GroupName}].");

                            await RemoveGroupAsyncCore(connection, groupMessage.GroupName);
                        }

                        if (groupMessage.Action == GroupAction.Add)
                        {
                            logger?.LogDebugEx($"Adding connection [{groupMessage.ConnectionId}] to group [{groupMessage.GroupName}].");

                            await AddGroupAsyncCore(connection, groupMessage.GroupName);
                        }

                        logger?.LogDebug($"Publishing message to NATS subject: [Subject={subjects.GroupManagement}].");
                        logger?.LogDebug($"ReplyTo: [Subject={msg.ReplyTo}].");

                        // Send an ack to the server that sent the original command.

                        await msg.ReplyAsync(Encoding.UTF8.GetBytes($"{groupMessage.Id}"), replyTo: msg.ReplyTo);
                    }
                    catch (Exception e)
                    {
                        logger?.LogErrorEx(e);
                        logger?.LogDebugEx($"Error processing message for internal server message: [Subject={subjects.GroupManagement}]");
                    }
                };
            }
        }

        public ValueTask DisposeAsync()
        {
            nats?.DisposeAsync();

            return ValueTask.CompletedTask;
        }

        private interface INatsFeature
        {
            HashSet<string> Groups { get; }
        }

        private sealed class NatsFeature : INatsFeature
        {
            public HashSet<string> Groups { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
