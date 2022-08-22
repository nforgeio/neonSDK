//-----------------------------------------------------------------------------
// FILE:	    LoggerWithTags.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright (c) 2005-2022 by neonFORGE LLC.  All rights reserved.
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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

using Neon.Common;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Used for holding a collection of tags to be included in all logged events.
    /// This is a very convienient way to ensure that all events logged will share
    /// a common set of tags.  This comes with at a performance cost (see the remarks).
    /// </summary>
    /// <remarks>
    /// <para>
    /// This functionally potentially comes at the cost of extra memory allocations
    /// which could bog down applications that do a lot of logging.  This problem
    /// surfaces when combining tags associated with a <see cref="LoggerWithTags"/>
    /// instance with event tags.  This won't be an issue if either the logger or
    /// logged events don't have any tags.
    /// </para>
    /// <para>
    /// When tags need to be merged and a <b>null formatter</b> is passed, then this method
    /// will perform one additional allocation for the combined tag list.
    /// </para>
    /// <para>
    /// When tags need to be merged and a <b>non-null formatter</b> is passed, then this method
    /// will perform three additional allocations: one for the combined tag list, one
    /// one for a wrapped formatter function, and one for a closure within thew wrapped
    /// function.
    /// </para>
    /// <para>
    /// I'd bet that custom formatters are pretty rare in the wild, so this means that
    /// combining tags will generally require only a single additional allocation, which
    /// isn't too bad.
    /// </para>
    /// </remarks>
    internal class LoggerWithTags : ILogger
    {
        private ILogger     logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Specifies the logger being wrapped.</param>
        /// <param name="tags">Specifies to tags to be included in events logged to the instance.</param>
        public LoggerWithTags(ILogger logger, LogTags tags)
        {
            Covenant.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Covenant.Requires<ArgumentNullException>(tags != null, nameof(tags));

            this.logger     = logger;
            this.LoggerTags = tags;
        }

        /// <summary>
        /// Returns the tags associated with the logger..
        /// </summary>
        internal LogTags LoggerTags { get; private set; }

        //---------------------------------------------------------------------
        // ILogger implementation

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state) => logger.BeginScope(state);

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => logger.IsEnabled(logLevel);

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (LoggerTags.Count == 0)
            {
                // There are no logger tags, so we can drop thru to the base implementation immediately.

                Log(logLevel, eventId, state, exception, formatter);
            }
            else
            {
                // $hack(jefflill):
                //
                // After examining the OpenTelemetry source code, I've determined that TState
                // can be one of these two types (at this time):
                //
                //      IReadOnlyList<KeyValuePair<string, object>>
                //      IEnumerable<KeyValuePair<string, object>> stateValues
                //
                // We're going to handle these individually so we can call any formatter passed.
                // Note that we'll ignore the formatter passed for any other TState types as a
                // fallback, in case this changes in the future.

                if (state is IReadOnlyList<KeyValuePair<string, object>> stateList)
                {
                    if (stateList != null && stateList.Count > 0)
                    {
                        var combinedTags = new LogTags(LoggerTags);

                        foreach (var tag in stateList)
                        {
                            combinedTags.AddInternal(tag);
                        }

                        Log(logLevel, eventId, (IReadOnlyList<KeyValuePair<string, object>>)combinedTags, exception, WrapFormatter<TState, IReadOnlyList<KeyValuePair<string, object>>>(state, formatter));
                    }
                    else
                    {
                        Log(logLevel, eventId, (IReadOnlyList<KeyValuePair<string, object>>)LoggerTags, exception, WrapFormatter<TState, IReadOnlyList<KeyValuePair<string, object>>>(state, formatter));
                    }
                }
                else if (state is IEnumerable<KeyValuePair<string, object>> stateValues)
                {
                    if (stateValues != null && stateValues.Count() > 0)
                    {
                        var combinedTags = new LogTags(LoggerTags);

                        foreach (var tag in stateValues)
                        {
                            combinedTags.AddInternal(tag);
                        }

                        Log(logLevel, eventId, (IEnumerable<KeyValuePair<string, object>>)combinedTags, exception, WrapFormatter<TState, IEnumerable<KeyValuePair<string, object>>>(state, formatter));
                    }
                    else
                    {
                        Log(logLevel, eventId, (IEnumerable<KeyValuePair<string, object>>)LoggerTags, exception, WrapFormatter<TState, IEnumerable <KeyValuePair<string, object>>> (state, formatter));
                    }
                }
                else
                {
                    // [TState] doesn't match one of the expected types which means that we
                    // don't know how to merge the logger tags with the event tags.
                    //
                    // We're just going to ignore any logger tags in this situation as the
                    // best possible fallback if/when OpenTelemetry changes there .NET
                    // implementation so at least the user will still see some logs.

                    Log(logLevel, eventId, state, exception, formatter);
                }
            }
        }

        /// <summary>
        /// This method wraps non-null formatter functions that accepts a <typeparamref name="TState"/> state parameter
        /// with new function that accepts a <typeparamref name="TWrapped"/> state parameter.
        /// </summary>
        /// <typeparam name="TState">Identifies the state type for the formatter passed.</typeparam>
        /// <typeparam name="TWrapped">Identifies the state type for the wrapped formatter returned.</typeparam>
        /// <param name="state"></param>
        /// <param name="formatter"></param>
        /// <returns>The wrapped formatter function or <c>null</c> when the formatter is <c>null</c>.</returns>
        /// <remarks>
        /// <para>
        /// This is necessary because even though the values for the <typeparamref name="TState"/> and <typeparamref name="TWrapped"/>
        /// types will be the same, the C# complier doesn't know that and because <typeparamref name="TState"/> is not
        /// restricted to being a class, we can't cast the tags in our <see cref="Log{TState}(LogLevel, EventId, TState, Exception, Func{TState, Exception, string})"/>
        /// method above to to the <typeparamref name="TState"/> so we can call the original formatter directly.
        /// </para>
        /// <para>
        /// This method does the job, but at the cost of two memory allocations: one for the wrapper function
        /// itself and the other for the captured outer variable (or closure), which is unfortunate.
        /// </para>
        /// <para>
        /// We're going to do the best we can to mitegate this by not wrapping <c>null</c> formatter functions
        /// above and then crossing our fingers that custom formatter usage will be rare in the real world.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Func<TWrapped, Exception, string> WrapFormatter<TState, TWrapped>(TState state, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                return null;
            }

            return new Func<TWrapped, Exception, string>(
                (wrappedState, exception) =>
                {
                    return formatter(state, exception);
                });
        }
    }
}
