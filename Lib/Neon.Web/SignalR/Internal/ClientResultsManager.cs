//-----------------------------------------------------------------------------
// FILE:        ClientResultsManager.cs
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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace Neon.Web.SignalR
{
    /// <summary>
    /// Common type used by our HubLifetimeManager implementations to manage client results.
    /// Handles cancellation, cleanup, and completion, so any bugs or improvements can be made in a single place
    /// </summary>
    internal sealed class ClientResultsManager : IInvocationBinder
    {
        private readonly ConcurrentDictionary<string, (Type Type, string ConnectionId, object Tcs, Action<object, CompletionMessage> Complete)> pendingInvocations = new();

        public Task<T> AddInvocation<T>(string connectionId, string invocationId, CancellationToken cancellationToken)
        {
            var tcs    = new TaskCompletionSourceWithCancellation<T>(this, connectionId, invocationId, cancellationToken);
            var result = pendingInvocations.TryAdd(invocationId, (typeof(T), connectionId, tcs, 
                static (state, completionMessage) =>
                {
                    var tcs = (TaskCompletionSourceWithCancellation<T>)state;

                    if (completionMessage.HasResult)
                    {
                        tcs.SetResult((T)completionMessage.Result);
                    }
                    else
                    {
                        tcs.SetException(new Exception(completionMessage.Error));
                    }
                }
                ));

            Debug.Assert(result);

            tcs.RegisterCancellation();

            return tcs.Task;
        }

        public void AddInvocation(string invocationId, (Type Type, string ConnectionId, object Tcs, Action<object, CompletionMessage> Complete) invocationInfo)
        {
            var result = pendingInvocations.TryAdd(invocationId, invocationInfo);

            Debug.Assert(result);
        }

        public void TryCompleteResult(string connectionId, CompletionMessage message)
        {
            if (pendingInvocations.TryGetValue(message.InvocationId!, out var item))
            {
                if (item.ConnectionId != connectionId)
                {
                    throw new InvalidOperationException($"Connection ID '{connectionId}' is not valid for invocation ID '{message.InvocationId}'.");
                }

                // if false the connection disconnected right after the above TryGetValue
                // or someone else completed the invocation (likely a bad client)
                // we'll ignore both cases.

                if (pendingInvocations.Remove(message.InvocationId!, out _))
                {
                    item.Complete(item.Tcs, message);
                }
            }
            else
            {
                // connection was disconnected or someone else completed the invocation
            }
        }

        public (Type Type, string ConnectionId, object Tcs, Action<object, CompletionMessage> Completion)? RemoveInvocation(string invocationId)
        {
            pendingInvocations.TryRemove(invocationId, out var item);

            return item;
        }

        public bool TryGetType(string invocationId, [NotNullWhen(true)] out Type type)
        {
            if (pendingInvocations.TryGetValue(invocationId, out var item))
            {
                type = item.Type;
                return true;
            }

            type = null;
            return false;
        }

        public Type GetReturnType(string invocationId)
        {
            if (TryGetType(invocationId, out var type))
            {
                return type;
            }

            throw new InvalidOperationException($"Invocation ID '{invocationId}' is not associated with a pending client result.");
        }

        // UNUSED: Here to honor the IInvocationBinder interface but should never be called
        public IReadOnlyList<Type> GetParameterTypes(string methodName)
        {
            throw new NotImplementedException();
        }

        // UNUSED: Here to honor the IInvocationBinder interface but should never be called
        public Type GetStreamItemType(string streamId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Custom TCS type to avoid the extra allocation that would be introduced if we managed the cancellation separately
        /// Also makes it easier to keep track of the CancellationTokenRegistration for disposal
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal sealed class TaskCompletionSourceWithCancellation<T> : TaskCompletionSource<T>
        {
            private readonly ClientResultsManager   clientResultsManager;
            private readonly string                 connectionId;
            private readonly string                 invocationId;
            private readonly CancellationToken      token;
            private CancellationTokenRegistration   tokenRegistration;

            public TaskCompletionSourceWithCancellation(ClientResultsManager clientResultsManager, string connectionId, string invocationId, CancellationToken cancellationToken)
                : base(TaskCreationOptions.RunContinuationsAsynchronously)
            {
                this.clientResultsManager = clientResultsManager;
                this.connectionId         = connectionId;
                this.invocationId         = invocationId;
                this.token                = cancellationToken;
            }

            // Needs to be called after adding the completion to the dictionary in order to avoid synchronous completions of the token registration
            // not canceling when the dictionary hasn't been updated yet.
            public void RegisterCancellation()
            {
                if (token.CanBeCanceled)
                {
                    tokenRegistration = token.UnsafeRegister(static o =>
                    {
                        var tcs = (TaskCompletionSourceWithCancellation<T>)o!;

                        tcs.SetCanceled();
                    }, this);
                }
            }

            public new void SetCanceled()
            {
                // TODO: RedisHubLifetimeManager will want to notify the other server (if there is one) about the cancellation
                // so it can clean up state and potentially forward that info to the connection

                clientResultsManager.TryCompleteResult(connectionId, CompletionMessage.WithError(invocationId, "Canceled"));
            }

            public new void SetResult(T result)
            {
                tokenRegistration.Dispose();
                base.SetResult(result);
            }

            public new void SetException(Exception exception)
            {
                tokenRegistration.Dispose();
                base.SetException(exception);
            }

#pragma warning disable IDE0060     // Remove unused parameter

            // Just making sure we don't accidentally call one of these without knowing

#pragma warning disable CS0109      // Does not hide an accessible member
            public static new void SetCanceled(CancellationToken cancellationToken) => Debug.Assert(false);
#pragma warning restore CS0109
            public static new void SetException(IEnumerable<Exception> exceptions) => Debug.Assert(false);
            public static new bool TrySetCanceled()
            {
                Debug.Assert(false);
                return false;
            }
            public static new bool TrySetCanceled(CancellationToken cancellationToken)
            {
                Debug.Assert(false);
                return false;
            }
            public static new bool TrySetException(IEnumerable<Exception> exceptions)
            {
                Debug.Assert(false);
                return false;
            }
            public static new bool TrySetException(Exception exception)
            {
                Debug.Assert(false);
                return false;
            }
            public static new bool TrySetResult(T result)
            {
                Debug.Assert(false);
                return false;
            }
#pragma warning restore IDE0060 // Remove unused parameter
        }
    }
}
