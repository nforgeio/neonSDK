//-----------------------------------------------------------------------------
// FILE:	    WatchStream.cs
// CONTRIBUTOR: Marcus Bowyer
// COPYRIGHT:	Copyright © 2005-2025 by NEONFORGE LLC.  All rights reserved.
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
using System.Diagnostics.Contracts;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using k8s;
using k8s.Autorest;
using k8s.Models;

using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.K8s;
using Neon.Diagnostics;
using Neon.Tasks;

using WatchEventType = k8s.WatchEventType;

namespace Neon.K8s
{
    /// <summary>
    /// A kubernetes watch event.
    /// </summary>
    /// <typeparam name="T">Specifies the Kubernetes entity type being watched.</typeparam>
    public class WatchEvent<T>
    {
        /// <summary>
        /// The <see cref="WatchEventType"/>
        /// </summary>
        public WatchEventType? Type { get; set; }

        /// <summary>
        /// The <see cref="ModifiedEventType"/>
        /// </summary>
        public ModifiedEventType ModifiedEventType { get; set; }

        /// <summary>
        /// The watch event value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// The number of attempts for reconciling the resource.
        /// </summary>
        public int Attempt { get; set; } = 0;

        /// <summary>
        /// Force a reconciliation for the resource.
        /// </summary>
        public bool Force { get; set; } = false;

        /// <summary>
        /// The time the event was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public WatchEvent()
        {
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">Identifies the event being watched.</param>
        /// <param name="value">Identifies the type being watched.</param>
        /// <param name="attempt">Optionally specifies the watch attempt (defaults to 0).</param>
        /// <param name="force">Optionally forece a resource reconcile.</param>
        public WatchEvent(WatchEventType type, T value, int attempt = 0, bool force = false)
            : base()
        {
            Type    = type;
            Value   = value; 
            Attempt = attempt;
            Force   = force;
        }
    }

    /// <summary>
    /// A generic Kubernetes watcher.
    /// </summary>
    /// <typeparam name="T">Specifies the Kubernetes entity type being watched.</typeparam>
    public sealed class Watcher<T> : IAsyncDisposable, IDisposable
        where T : IKubernetesObject<V1ObjectMeta>
    {
        private string                  resourceVersion;
        private Channel<WatchEvent<T>>  eventChannel;
        private IKubernetes             k8s;
        private ILogger                 logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="k8s">The Kubernetes clien.</param>
        /// <param name="logger">Optionally specifies the logger to use.</param>
        public Watcher(
            IKubernetes k8s,
            ILogger logger = null)
        {
            this.k8s          = k8s;
            this.logger       = logger;
            this.eventChannel = Channel.CreateUnbounded<WatchEvent<T>>();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await SyncContext.Clear;

            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (eventChannel != null)
            {
                eventChannel = null;
            }

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// A generic Watcher to watch Kubernetes resources, and respond with a custom (async) callback method.
        /// </summary>
        /// <param name="actionAsync">The async action called as watch events are received.</param>
        /// <param name="namespaceParameter">The target Kubernetes namespace.</param>
        /// <param name="fieldSelector">The optional field selector</param>
        /// <param name="labelSelector">The optional label selector</param>
        /// <param name="resourceVersion">The start resource version.</param>
        /// <param name="resourceVersionMatch">The optional resourceVersionMatch setting.</param>
        /// <param name="timeoutSeconds">Optional timeout override.</param>
        /// <param name="retryDelay">Optionally specifies a delay period to wait between watch errors.</param>
        /// <param name="cancellationToken">Optionally specifies a cancellation token.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public async Task WatchAsync(
            Func<WatchEvent<T>, Task>   actionAsync, 
            string                      namespaceParameter   = null,
            string                      fieldSelector        = null,
            string                      labelSelector        = null,
            string                      resourceVersion      = null,
            string                      resourceVersionMatch = null,
            int?                        timeoutSeconds       = null,
            TimeSpan?                   retryDelay           = null,
            CancellationToken           cancellationToken    = default)
        {
            await SyncContext.Clear;

            // Start the loop that handles the async action callbacks.

            _ = EventHandlerAsync(actionAsync);

            // This is where you'll actually listen for watch events from Kubernetes.
            // When you receive an event, do this:

            Task<HttpOperationResponse<object>> listResponse = default;

            while (true)
            {
                logger?.LogDebugEx($"Watching {typeof(T)} with resource version [{this.resourceVersion}].");

                try
                {
                    // Validate the resource version we're being given.

                    if (!string.IsNullOrEmpty(this.resourceVersion)
                        && this.resourceVersion != "0")
                    {
                        await ValidateResourceVersionAsync(
                            fieldSelector:        fieldSelector,
                            labelSelector:        labelSelector,
                            resourceVersion:      this.resourceVersion,
                            timeoutSeconds:       timeoutSeconds);
                    }

                    if (string.IsNullOrEmpty(namespaceParameter))
                    {
                        listResponse = k8s.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync<T>(
                            allowWatchBookmarks:  true,
                            fieldSelector:        fieldSelector,
                            labelSelector:        labelSelector,
                            resourceVersion:      this.resourceVersion,
                            resourceVersionMatch: resourceVersionMatch,
                            timeoutSeconds:       timeoutSeconds,
                            watch:                true,
                            cancellationToken:    cancellationToken);
                    }
                    else
                    {
                        listResponse = k8s.CustomObjects.ListNamespacedCustomObjectWithHttpMessagesAsync<T>(
                            namespaceParameter,
                            allowWatchBookmarks:  true,
                            fieldSelector:        fieldSelector,
                            labelSelector:        labelSelector,
                            resourceVersion:      this.resourceVersion,
                            resourceVersionMatch: resourceVersionMatch,
                            timeoutSeconds:       timeoutSeconds,
                            cancellationToken:    cancellationToken,
                            watch:                true);
                    }

                    await foreach (var (type, item) in listResponse.WatchAsync<T, object>(
                        onError: (exception) =>
                        {
                            logger?.LogErrorEx(exception);

                            if (exception is KubernetesException)
                            {
                                if (string.Equals(((KubernetesException)exception).Status.Reason, "Expired", StringComparison.Ordinal))
                                {
                                    this.resourceVersion = null;
                                }
                            }
                        },
                        cancellationToken: cancellationToken))
                    {
                        var sb = new StringBuilder();

                        sb.Append(item.ApiVersion);
                        sb.Append($"/{item.Kind}");

                        if (!string.IsNullOrEmpty(item.Namespace()))
                        {
                            sb.Append($"/{item.Namespace()}");
                        }

                        if (!string.IsNullOrEmpty(item.Name()))
                        {
                            sb.Append($"/{item.Name()}");
                        }

                        var crdId = sb.ToString();

                        logger?.LogDebugEx(() => $"Received {type.ToMemberString()} event for type {typeof(T)} [{crdId}].");

                        await eventChannel.Writer.WriteAsync(new WatchEvent<T>() { Type = type, Value = item });
                    }
                }
                catch (OperationCanceledException canceledException)
                {
                    // This is the signal to quit.
                    logger?.LogDebugEx(canceledException);
                    logger?.LogDebugEx(() => "Operation canceled, quitting.");

                    return;
                }
                catch (KubernetesException kubernetesException)
                {
                    logger?.LogErrorEx(kubernetesException);

                    // Deal with this non-recoverable condition "too old resource version"

                    if (string.Equals(kubernetesException.Status.Reason, "Expired", StringComparison.Ordinal))
                    {
                        this.resourceVersion = null;
                    }
                }
                catch (Exception e)
                {
                    logger?.LogErrorEx(e);
                }
                finally
                {
                    listResponse.Dispose();
                }

                if (retryDelay.HasValue)
                {
                    logger?.LogDebugEx(() => $"Sleeping for {retryDelay.Value.TotalSeconds} seconds before restarting watch.");

                    await Task.Delay(retryDelay.Value);
                }
            }
        }

        /// <summary>
        /// Handles received events.
        /// </summary>
        /// <param name="action">The event handler.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="KubernetesException"></exception>
        private async Task EventHandlerAsync(Func<WatchEvent<T>, Task> action, CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            Covenant.Requires<ArgumentNullException>(action != null, nameof(action));

            try
            {
                while (true)
                {
                    WatchEvent<T> @event;

                    cancellationToken.ThrowIfCancellationRequested();

                    @event = await eventChannel.Reader.ReadAsync();

                    logger?.LogDebugEx(() => $"Processing watch event: {(string.IsNullOrEmpty(@event.Value.Uid()) ? "BOOKMARK" : @event.Value.Uid())}");

                    switch (@event.Type)
                    {
                        case WatchEventType.Bookmark:

                            resourceVersion = @event.Value.ResourceVersion();

                            break;

                        case WatchEventType.Error:

                            break;

                        case WatchEventType.Added:
                        case WatchEventType.Modified:
                        case WatchEventType.Deleted:

                            await action(@event);

                            resourceVersion = @event.Value.ResourceVersion();

                            break;

                        default:

                            throw new KubernetesException();
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // This normal: we'll see this when the watcher is disposed.

                logger?.LogInformationEx("Disposing");
            }
            catch (OperationCanceledException)
            {
                // This normal: we'll see this when the watcher is disposed.

                logger?.LogInformationEx("Disposing");
            }
        }

        /// <summary>
        /// Used to validate a given resource version by sending a simple request to the Kubernetes API server.
        /// </summary>
        /// <param name="resourceVersion">The resource version.</param>
        /// <param name="namespaceParameter">The object namespace, or null for cluster objects</param>
        /// <param name="fieldSelector">Optional field selector</param>
        /// <param name="labelSelector">Optional label selector.</param>
        /// <param name="timeoutSeconds">Optional timeout.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        private async Task ValidateResourceVersionAsync(
            string  resourceVersion,
            string  namespaceParameter   = null,
            string  fieldSelector        = null,
            string  labelSelector        = null,
            int?    timeoutSeconds       = null)
        {
            await SyncContext.Clear;

            try
            {
                if (string.IsNullOrEmpty(namespaceParameter))
                {
                    var result = await k8s.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync<T>(
                        fieldSelector:        fieldSelector,
                        labelSelector:        labelSelector,
                        limit:                1,
                        resourceVersion:      resourceVersion,
                        resourceVersionMatch: "NotOlderThan",
                        timeoutSeconds:       timeoutSeconds,
                        watch:                false);

                    result.Dispose();
                }
                else
                {
                    var result = await k8s.CustomObjects.ListNamespacedCustomObjectWithHttpMessagesAsync<T>(
                        namespaceParameter,
                        fieldSelector:        fieldSelector,
                        labelSelector:        labelSelector,
                        limit:                1,
                        resourceVersion:      resourceVersion,
                        resourceVersionMatch: "NotOlderThan",
                        timeoutSeconds:       timeoutSeconds,
                        watch:                false);

                    result.Dispose();
                }
            }
            catch (KubernetesException kubernetesException)
            {
                logger?.LogErrorEx(kubernetesException);

                // Deal with this non-recoverable condition "too old resource version"

                if (string.Equals(kubernetesException.Status.Reason, "Expired", StringComparison.Ordinal))
                {
                    throw;
                }
            }
            catch (HttpOperationException httpOperationException)
            {
                logger?.LogErrorEx(httpOperationException);
                if (httpOperationException.Response.StatusCode == HttpStatusCode.Gone)
                {
                    this.resourceVersion = "0";
                }
            }
            catch (Exception e)
            {
                logger?.LogErrorEx(e);
            }
        }
    }
}
