﻿//-----------------------------------------------------------------------------
// FILE:	    ConsoleJsonLogExporterOptions.cs
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
    public class ConsoleJsonLogExporterOptions
    {
        /// <summary>
        /// Constructs an instance with reasonable settings.
        /// </summary>
        public ConsoleJsonLogExporterOptions()
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
        /// <para>
        /// Specifies whether the log event JSON written to the console should be formatted as single 
        /// lines of JSON (the default) or render these as indented multi-line JSON separated by
        /// a blank line.
        /// </para>
        /// <note>
        /// This may be useful for debugging but should probably never be used for production.  This
        /// defaults to <c>true</c>.
        /// </note>
        /// </summary>
        public bool EmitSingleLine { get; set; } = true;

        /// <summary>
        /// <para>
        /// Specifies whether the OpenTelemetry <b>severityNumber</b> attribute should be included the the emitted
        /// logs.  OpenTelemetry defines a standard set of [severity numbers](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/logs/data-model.md#field-severitynumber)
        /// that are more expansive (0..24) than the ordinal values assigned to the <see cref="LogLevel"/> enum values (0..6).
        /// </para>
        /// <para>
        /// This defaults to <c>false</c>.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// We're logging the OpenTelemetry severity number in the hope that OpenTelemetry eventually becomes
        /// the de-facto logging standard so querying logs based on severity number would make sense.
        /// </para>
        /// <para>
        /// Here's the mapping between <see cref="LogLevel"/> values and OpenTelemetry severity numbers:
        /// </para>
        /// <list type="table">
        /// <item>
        ///     <term><see cref="LogLevel.None"/></term>
        ///     <description><b>0</b> (UNSPECIFIED)</description>
        /// </item>
        /// <item>
        ///     <term><see cref="LogLevel.Trace"/></term>
        ///     <description><b>1</b> (TRACE)</description>
        /// </item>
        /// <item>
        ///     <term><see cref="LogLevel.Debug"/></term>
        ///     <description><b>5</b> (DEBUG)</description>
        /// </item>
        /// <item>
        ///     <term><see cref="LogLevel.Information"/></term>
        ///     <description><b>9</b> (INFORMATION)</description>
        /// </item>
        /// <item>
        ///     <term><see cref="LogLevel.Critical"/></term>
        ///     <description><b>13</b> (WARN)</description>
        /// </item>
        /// <item>
        ///     <term><see cref="LogLevel.Error"/></term>
        ///     <description><b>17</b> (ERROR)</description>
        /// </item>
        /// <item>
        ///     <term><see cref="LogLevel.Critical"/></term>
        ///     <description><b>21</b> (FATAL)</description>
        /// </item>
        /// </list>
        /// </remarks>
        public bool EmitSeverityNumber { get; set; } = true;

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
        /// Used to intercept log events just before they are emitted by the exporter.  You can
        /// use this for implementing logging related unit tests or modifying other event properties 
        /// like the timestamp, labels, tags, etc.
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
    }
}
