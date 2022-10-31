//-----------------------------------------------------------------------------
// FILE:	    LogMetricsProcessor.cs
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

namespace Neon.Diagnostics
{
    /// <summary>
    /// <para>
    /// This OpenTelemetry processor maintains the labeled <b>neon_log_events</b> metrics
    /// counter by incrementing the counter whenever events are logged, using the event's 
    /// <see cref="LogLevel"/> as the counter label.
    /// </para>
    /// <note>
    /// <b>NeonService</b> based services configure this processor by default.
    /// </note>
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is useful for screening for services that may be logging too many warnings or
    /// errors, requiring further investigation.
    /// </para>
    /// <para>
    /// This is very easy to use.  Simply call <see cref="ConfigExtensions.AddLogMetricsProcessor(OpenTelemetryLoggerOptions, string)"/>.
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

            levelToSeverityName                              = new string[(int)LogLevel.None + 1];
            levelToSeverityName[(int)LogLevel.Trace]        = "Trace";
            levelToSeverityName[(int)LogLevel.Debug]        = "Debug";
            levelToSeverityName[(int)LogLevel.Information]  = "Information";
            levelToSeverityName[(int)LogLevel.Warning]      = "Warning";
            levelToSeverityName[(int)LogLevel.Error]        = "Error";
            levelToSeverityName[(int)LogLevel.Critical]     = "Critical";
            levelToSeverityName[(int)LogLevel.None]         = "None";
        }

        //---------------------------------------------------------------------
        // Instance members

        private Counter     logCounter;

        /// <summary>
        /// Constructs a processor that counts logged events by <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="metricsPrefix">Optionally specifies a prefix to be prepended to to the counter name.</param>
        public LogMetricsProcessor(string metricsPrefix = null)
        {
            var counterName = "neon_log_events";

            if (!string.IsNullOrEmpty(metricsPrefix))
            {
                counterName = $"{metricsPrefix}_{counterName}";
            }

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
            logCounter.WithLabels(levelToSeverityName[(int)logRecord.LogLevel]).Inc();
        }
    }
}
