//-----------------------------------------------------------------------------
// FILE:	    AttributeLogger.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
    /// <para>
    /// Used for holding a collection of attributes to be included in all logged events.
    /// This is a very convienient way to ensure that all events logged will share
    /// a common set of attributes.  This comes with at a performance cost (see the remarks).
    /// </para>
    /// <note>
    /// <para>
    /// <b>IMPORTANT:</b> You must use the extended <see cref="ILogger"/> logging extensions
    /// like <see cref="LoggerExtensions.LogInformationEx(ILogger, Func{string}, Action{LogAttributes})"/>
    /// for any attributes added to a <see cref="AttributeLogger"/> instance to be included in the
    /// log output
    /// </para>
    /// <para>
    /// We recommend that developers consider switch to using our extended logging methods
    /// from the stock .NET extensions <see cref="Microsoft.Extensions.Logging.LoggerExtensions"/>.
    /// Not only do the NeonSDK <see cref="Neon.Diagnostics.LoggerExtensions"/> interoperate
    /// with the <see cref="AttributeLogger"/>, we believe our extensions are easier to use,
    /// especially when specifying attributes.  We also have overrides that make it efficient
    /// to use string interpolation for generating log messages.
    /// </para>
    /// </note>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This functionally potentially comes at the cost of extra memory allocations
    /// which could bog down applications that do a lot of logging.  This problem
    /// surfaces when combining attributes associated with a <see cref="AttributeLogger"/>
    /// instance with event attributes.  This won't be an issue if either the logger or
    /// logged events don't have any attributes.
    /// </para>
    /// <para>
    /// When attributes need to be merged and a <b>null formatter</b> is passed, then this method
    /// will perform one additional allocation for the combined attribute list.
    /// </para>
    /// <para>
    /// When attributes need to be merged and a <b>non-null formatter</b> is passed, then this method
    /// will perform three additional allocations: one for the combined attribute list, one
    /// one for a wrapped formatter function, and one for a closure within thew wrapped
    /// function.
    /// </para>
    /// <para>
    /// I'd bet that custom formatters are pretty rare in the wild, so this means that
    /// combining attributes will generally require only a single additional allocation,
    /// which isn't too bad.
    /// </para>
    /// </remarks>
    internal class AttributeLogger : ILogger
    {
        private ILogger     logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Specifies the logger being wrapped.</param>
        /// <param name="attributes">Specifies to attributes to be included in events logged to the instance.</param>
        /// <remarks>
        /// <note>
        /// We do not support wrapping another <see cref="AttributeLogger"/> instance.
        /// </note>
        /// </remarks>
        public AttributeLogger(ILogger logger, LogAttributes attributes)
        {
            Covenant.Requires<ArgumentNullException>(logger != null, nameof(logger));
            Covenant.Requires<ArgumentException>(!(logger is AttributeLogger), nameof(logger), $"An [{nameof(AttributeLogger)}] cannot wrap another [{nameof(AttributeLogger)}] instance.");
            Covenant.Requires<ArgumentNullException>(attributes != null, nameof(attributes));

            this.logger     = logger;
            this.Attributes = attributes;
        }

        /// <summary>
        /// Returns the attributes associated with the logger..
        /// </summary>
        internal LogAttributes Attributes { get; private set; }

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
