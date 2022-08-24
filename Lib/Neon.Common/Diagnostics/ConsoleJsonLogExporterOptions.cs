//-----------------------------------------------------------------------------
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
    /// Specifies options used to configure a <see cref="ConsoleJsonLogExporter"/>.
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
        /// <para>
        /// Specifies whether thw log event JSON written to the console should be formatted as single 
        /// lines of JSON (the default) or render these as indented multi-line JSON separated by
        /// a blank line.
        /// </para>
        /// <note>
        /// This may be useful for debugging but should probably never be used for production.
        /// </note>
        /// </summary>
        public bool SingleLine { get; set; } = true;
    }
}
