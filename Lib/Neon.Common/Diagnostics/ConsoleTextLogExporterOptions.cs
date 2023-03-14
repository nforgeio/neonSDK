//-----------------------------------------------------------------------------
// FILE:	    ConsoleTextLogExporterOptions.cs
// CONTRIBUTOR: Marcus Bowyer
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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Neon.Common;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Specifies the options used to configure a <see cref="ConsoleJsonLogExporter"/>.
    /// </summary>
    public class ConsoleTextLogExporterOptions
    {
        /// <summary>
        /// Constructs an instance with reasonable settings.
        /// </summary>
        public ConsoleTextLogExporterOptions()
        {
        }

        /// <summary>
        /// <para>
        /// Used to disable writing events to the output stream.  This can be useful for
        /// unit testing.
        /// </para>
        /// <note>
        /// Any configured <see cref="LogEventInterceptor"/> actions will still be called
        /// event when <see cref="Emit"/> is configured as <c>false</c>.
        /// </note>
        /// </summary>
        public bool Emit { get; set; } = true;

        /// <summary>
        /// Used to direct log output for events to <b>standard error</b> based on the
        /// event log level.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By default, <see cref="ConsoleJsonLogExporter"/> writes logs to <b>standard output</b>
        /// because this property defaults to <see cref="LogLevel.None"/>.  You may override this
        /// by setting this to another level; then events with log levels &gt;= this property will 
        /// be written to <b>standard error</b> instead.
        /// </para>
        /// <para>
        /// Set this to <see cref="LogLevel.Critical"/> to send all logs to <b>standard output</b>.
        /// </para>
        /// </remarks>
        public LogLevel StandardErrorLevel { get; set; } = LogLevel.None;

        /// <summary>
        /// Specifies whether exception stack traces should be included in logged events.
        /// This defaults to <c>true</c>.
        /// </summary>
        public bool ExceptionStackTraces { get; set; } = true;

        /// <summary>
        /// <para>
        /// Used to intercept log events just before they are emitted by the exporter.  You can
        /// use this for implementing logging related unit tests or modifying other event properties 
        /// like the timestamp, labels, tags, etc.
        /// </para>
        /// <note>
        /// <b>IMPORTANT:</b> <see cref="LogEvent"/> record instances are reused by the Neon telemetry
        /// code, so you'll need to call <see cref="LogEvent.Clone()"/> when you're using the interceptor
        /// to collected logged events for later analysis (i.e. when unit testing).
        /// </note>
        /// </summary>
        public LogEventInterceptor LogEventInterceptor { get; set; } = null;

        /// <summary>
        /// Used internally by unit tests to intercept JSON records emitted to <b>standard output</b>.
        /// </summary>
        internal Action<string> StdOutInterceptor { get; set; } = null;

        /// <summary>
        /// Used internally by unit tests to intercept JSON records emitted to <b>standard error</b>.
        /// </summary>
        internal Action<string> StdErrInterceptor { get; set; } = null;

        /// <summary>
        /// Used to set the log format.
        /// </summary>
        public Func<LogRecord, string> Format { get; set; } = (record) => $"[{record.LogLevel}] {record.FormattedMessage}";
    }
}
