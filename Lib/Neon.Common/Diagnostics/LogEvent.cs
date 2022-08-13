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
    public struct LogEvent
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="module">Optionally identifies the source module.</param>
        /// <param name="index">
        /// Specifies the one-based position of the event in the stream of events
        /// logged by the log manager.
        /// </param>
        /// <param name="timeUtc">Time (UTC) when the event was logged.</param>
        /// <param name="logLevel">The event log level.</param>
        /// <param name="message">Optionally specifies the event message.</param>
        /// <param name="e">Optionally specifies the exception being logged.</param>
        public LogEvent(
            string      module,
            long        index,
            DateTime    timeUtc,
            LogLevel    logLevel,
            string      message,
            Exception   e)
        {
            this.Module    = module;
            this.Index     = index;
            this.TimeUtc   = timeUtc;
            this.LogLevel  = logLevel;
            this.Message   = message;
            this.Exception = e;
        }

        /// <summary>
        /// Optionally identifies the source module.
        /// </summary>
        public string Module { get; private set; }

        /// <summary>
        /// Specifies the one-based position of the event in the stream of events
        /// logged by the log manager.
        /// </summary>
        public long Index { get; internal set; }

        /// <summary>
        /// Time (UTC) when the event was logged.
        /// </summary>
        public DateTime TimeUtc { get; private set; }

        /// <summary>
        /// The event log level.
        /// </summary>
        public LogLevel LogLevel { get; private set; }

        /// <summary>
        /// The event message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Optionally specifies the event exception.
        /// </summary>
        public Exception Exception { get; private set; }
    }
}
