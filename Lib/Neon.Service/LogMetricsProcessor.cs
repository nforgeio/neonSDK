//-----------------------------------------------------------------------------
// FILE:        LogMetricsProcessor.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Neon.Common;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;

using Prometheus;

namespace Neon.Service
{
    /// <summary>
    /// <para>
    /// This OpenTelemetry processor maintains the labeled <b>neonsdk_log_events_total</b> metrics
    /// counter by incrementing the counter whenever events are logged, using the event's 
    /// <see cref="LogLevel"/> as the counter label.
    /// </para>
    /// <note>
    /// <see cref="NeonService"/> based applications configure this processor by default.
    /// </note>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is useful for screening for services that may be logging too many warnings or
    /// errors, requiring further investigation.
    /// </para>
    /// <para>
    /// This is very easy to use.  Simply call <see cref="OtelConfigExtensions.AddLogMetricsProcessor(OpenTelemetryLoggerOptions)"/>.
    /// </para>
    /// </remarks>
    public class LogMetricsProcessor : BaseProcessor<LogRecord>
    {
        //---------------------------------------------------------------------
        // Static members

        private static string[]     levelToSeverityName;

        /// <summary>
        /// Constructor.
        /// </summary>
        static LogMetricsProcessor()
        {
            // $hack(jefflill): This is a bit of a hack to improve performance.

            levelToSeverityName = new string[(int)LogRecordSeverity.Fatal4 + 1];

            levelToSeverityName[(int)LogRecordSeverity.Unspecified] = "None";
            levelToSeverityName[(int)LogRecordSeverity.Trace]       = "Trace";
            levelToSeverityName[(int)LogRecordSeverity.Trace2]      = "Trace";
            levelToSeverityName[(int)LogRecordSeverity.Trace3]      = "Trace";
            levelToSeverityName[(int)LogRecordSeverity.Trace4]      = "Trace";
            levelToSeverityName[(int)LogRecordSeverity.Debug]       = "Debug";
            levelToSeverityName[(int)LogRecordSeverity.Debug2]      = "Debug";
            levelToSeverityName[(int)LogRecordSeverity.Debug3]      = "Debug";
            levelToSeverityName[(int)LogRecordSeverity.Debug4]      = "Debug";
            levelToSeverityName[(int)LogRecordSeverity.Info]        = "Info";
            levelToSeverityName[(int)LogRecordSeverity.Info2]       = "Info";
            levelToSeverityName[(int)LogRecordSeverity.Info3]       = "Info";
            levelToSeverityName[(int)LogRecordSeverity.Info4]       = "Info";
            levelToSeverityName[(int)LogRecordSeverity.Warn]        = "Warn";
            levelToSeverityName[(int)LogRecordSeverity.Warn2]       = "Warn";
            levelToSeverityName[(int)LogRecordSeverity.Warn3]       = "Warn";
            levelToSeverityName[(int)LogRecordSeverity.Warn4]       = "Warn";
            levelToSeverityName[(int)LogRecordSeverity.Error]       = "Error";
            levelToSeverityName[(int)LogRecordSeverity.Error2]      = "Error";
            levelToSeverityName[(int)LogRecordSeverity.Error3]      = "Error";
            levelToSeverityName[(int)LogRecordSeverity.Error4]      = "Error";
            levelToSeverityName[(int)LogRecordSeverity.Fatal]       = "Fatal";
            levelToSeverityName[(int)LogRecordSeverity.Fatal2]      = "Fatal";
            levelToSeverityName[(int)LogRecordSeverity.Fatal3]      = "Fatal";
            levelToSeverityName[(int)LogRecordSeverity.Fatal4]      = "Fatal";
        }

        //---------------------------------------------------------------------
        // Instance members

        private Counter     logCounter;

        /// <summary>
        /// Constructs a processor that counts logged events by <see cref="LogLevel"/>.
        /// </summary>
        public LogMetricsProcessor()
        {
            var counterName = $"{NeonHelper.NeonMetricsPrefix}_log_events_total";

            logCounter = Metrics.CreateCounter(counterName, "Logged event count.", "level" );

            // Emit a ZERO count for all counted log levels so that we'll start
            // out with values for all loglevel labels.

            foreach (var level in Enum.GetValues(typeof(LogLevel)))
            {
                logCounter.WithLabels(levelToSeverityName[(int)level]).IncTo(0);
            }
        }

        /// <summary>
        /// Handles the event counting.
        /// </summary>
        /// <param name="logRecord">The log record.</param>
        public override void OnEnd(LogRecord logRecord)
        {
            logCounter.WithLabels(levelToSeverityName[(int)logRecord.Severity]).Inc();
        }
    }
}
