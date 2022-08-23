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

            this.logger = logger;
            this.Tags   = tags;
        }

        /// <summary>
        /// Returns the tags associated with the logger..
        /// </summary>
        internal LogTags Tags { get; private set; }

        //---------------------------------------------------------------------
        // ILogger implementation

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state) => logger.BeginScope(state);

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => logger.IsEnabled(logLevel);

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            logger.Log<TState>(logLevel, eventId, state, exception, formatter);
        }
    }
}
