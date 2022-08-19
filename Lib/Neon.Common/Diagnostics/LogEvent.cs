//-----------------------------------------------------------------------------
// FILE:	    LogEvent.cs
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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Used by or capturing logged events in memory.
    /// </summary>
    public class LogEvent
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="categoryName">Optionally identifies the log event category name.</param>
        /// <param name="index">
        /// Specifies the one-based position of the event in the stream of events
        /// logged by the log manager.
        /// </param>
        /// <param name="timestamp">Time (UTC) when the event was logged.</param>
        /// <param name="logLevel">The event log level.</param>
        /// <param name="body">Specifies the event body or <c>null</c>.</param>
        /// <param name="attributes">Specifies any arrtibutes to be logged with the event or <c>null</c>.</param>
        /// <param name="e">Optionally specifies the exception being logged.</param>
        public LogEvent(
            string                                      categoryName,
            long                                        index,
            DateTime                                    timestamp,
            NeonLogLevel                                    logLevel,
            string                                      body,
            IEnumerable<KeyValuePair<string, string>>   attributes,
            Exception                                   e)
        {
            this.CategoryName = categoryName;
            this.Index        = index;
            this.Timestamp    = timestamp;
            this.LogLevel     = logLevel;
            this.Body         = body ?? string.Empty;
            this.Attributes   = attributes;
            this.Exception    = e;
        }

        /// <summary>
        /// Returns the log event category name or <c>null</c>.  This is often the name of
        /// the class or parent class that's responsible for the operation being logged.
        /// </summary>
        public string CategoryName { get; private set; }

        /// <summary>
        /// Returns the one-based position of the event in the stream of events
        /// logged by the log manager.
        /// </summary>
        public long Index { get; internal set; }

        /// <summary>
        /// Returns the time (UTC) when the event was logged.
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// Returns event log level.
        /// </summary>
        public NeonLogLevel LogLevel { get; private set; }

        /// <summary>
        /// Returns event body text.
        /// </summary>
        public string Body { get; private set; }

        /// <summary>
        /// Returns and event attributes.  This may be <c>null</c>.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Attributes { get; private set; }

        /// <summary>
        /// Returns any exception associated with the event.
        /// </summary>
        public Exception Exception { get; private set; }
    }
}
