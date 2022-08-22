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
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

using Neon.Common;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Used for holding a collection of tags to be included in all logged events.
    /// </summary>
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

            this.logger = logger;
            this.Tags   = tags;
        }

        /// <summary>
        /// Returns the collection of tags to be included in events logged to the instance.
        /// </summary>
        public LogTags Tags { get; private set; }

        //---------------------------------------------------------------------
        // ILogger implementation

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state) => logger.BeginScope(state);

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => logger.IsEnabled(logLevel);

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (Tags.Count == 0)
            {
                // There are no logger tags, so we can drop thru to the base implementation immediately.
                
                Log(logLevel, eventId, state, exception, formatter);
            }
            else
            {
                var stateTags = GetTagsFromState(state);

                // $node(jefflill):
                //
                // We're ignorning any custom formatter here.  I'm not entirely sure that
                // formatters make sense here.  I could probably make this work if anybody
                // cares, by converting the tag collection into something compatible with
                // [TState], but I suspect that custom message formatting is going to be
                // used pretty rarely.

                if (stateTags == null || stateTags.Count == 0)
                {
                    // There are no tags in the state, so we'll just pass the logger tags.

                    Log<IEnumerable<KeyValuePair<string, object>>>(logLevel, eventId, Tags.Tags, exception, formatter: null);
                }
                else
                {
                    // We need to combine the logger tags with the state tags, where the state
                    // tags will take precedence over the logger tags.

                    var combinedTags = new LogTags(Tags.Tags);

                    foreach (var tag in stateTags)
                    {
                        combinedTags.Add(tag.Key, tag.Value);
                    }

                    Log<IEnumerable<KeyValuePair<string, object>>>(logLevel, eventId, combinedTags.Tags, exception, formatter: null);
                }
            }
        }

        /// <summary>
        /// Attempts to extract log tags from a <typeparamref name="TState"/> instance.
        /// </summary>
        /// <typeparam name="TState">Specifies the state type.</typeparam>
        /// <param name="state">The state instance.</param>
        /// <returns>A read-only list of log tags or <c>null</c> when there are no tags or <typeparamref name="TState"/> was not recognized.</returns>
        private static IReadOnlyList<KeyValuePair<string, object>> GetTagsFromState<TState>(TState state)
        {
            if (state is IReadOnlyList<KeyValuePair<string, object>> stateList)
            {
                return stateList;
            }
            else if (state is IEnumerable<KeyValuePair<string, object>> stateValues)
            {
                return new List<KeyValuePair<string, object>>(stateValues).AsReadOnly();
            }
            else
            {
                return null;
            }
        }
    }
}
