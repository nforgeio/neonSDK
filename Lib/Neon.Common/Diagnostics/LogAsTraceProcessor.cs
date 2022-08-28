//-----------------------------------------------------------------------------
// FILE:	    LogAsTraceProcessor.cs
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
    /// This OpenTelemetry processor submits any logged events that satisfy
    /// a log level as trace events to the current trace span (if any).  This
    /// is an easy way to converge logging any tracing while we're waiting for
    /// the OpenTelemetry folks support this natively.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is very easy to use.  Simply call <see cref="ConfigExtensions.AddLogAsTraceProcessor(OpenTelemetryLoggerOptions, Action{LogAsTraceProcessorOptions})"/>,
    /// optionally passing options that specify the log level for events to be
    /// added as trace events.
    /// </para>
    /// </remarks>
    public class LogAsTraceProcessor : BaseProcessor<LogRecord>
    {
        //---------------------------------------------------------------------
        // Static members

        private static int[]        levelToSeverityNumber;
        private static string[]     levelToSeverityName;

        /// <summary>
        /// Constructor.
        /// </summary>
        static LogAsTraceProcessor()
        {
            // $hack(jefflill): This is a bit of a hack to improve performance.

            levelToSeverityNumber                            = new int[(int)LogLevel.None + 1];
            levelToSeverityNumber[(int)LogLevel.Trace]       = (int)SeverityNumber.SEVERITY_NUMBER_TRACE;
            levelToSeverityNumber[(int)LogLevel.Debug]       = (int)SeverityNumber.SEVERITY_NUMBER_DEBUG;
            levelToSeverityNumber[(int)LogLevel.Information] = (int)SeverityNumber.SEVERITY_NUMBER_INFO;
            levelToSeverityNumber[(int)LogLevel.Warning]     = (int)SeverityNumber.SEVERITY_NUMBER_WARN;
            levelToSeverityNumber[(int)LogLevel.Error]       = (int)SeverityNumber.SEVERITY_NUMBER_ERROR;
            levelToSeverityNumber[(int)LogLevel.Critical]    = (int)SeverityNumber.SEVERITY_NUMBER_FATAL;
            levelToSeverityNumber[(int)LogLevel.None]        = (int)SeverityNumber.SEVERITY_NUMBER_UNSPECIFIED;

            levelToSeverityName                              = new string[(int)LogLevel.None + 1];
            levelToSeverityName[(int)LogLevel.Trace]        = "Trace";
            levelToSeverityName[(int)LogLevel.Debug]        = "Debug";
            levelToSeverityName[(int)LogLevel.Information]  = "Information";
            levelToSeverityName[(int)LogLevel.Warning]      = "Warning";
            levelToSeverityName[(int)LogLevel.Error]        = "Error";
            levelToSeverityName[(int)LogLevel.Critical]     = "Fatal";
            levelToSeverityName[(int)LogLevel.None]         = "Unspecified";
        }

        //---------------------------------------------------------------------
        // Instance members

        private LogAsTraceProcessorOptions   options;
        private LogLevel                    logLevel;

        /// <summary>
        /// Constructs a processor that forwards logged events to the current
        /// trace as trace events.
        /// </summary>
        /// <param name="options">
        /// Optionally specifies the processor options.  This is used to filter the
        /// events logged to as trace events by <see cref="LogLevel"/>.  This defaults
        /// to forwarding events with log levels greater than or equal to 
        /// <see cref="LogLevel.Information"/>.
        /// </param>
        /// <remarks>
        /// <para>
        /// All log events added as events in the current span will have their name
        /// set to <see cref="TelemetrySpanEventNames.Log"/> and will also include
        /// the log event tags but with their names prefixed by "neon.log." to avoid
        /// conflicts with unrelated tags.
        /// </para>
        /// </remarks>
        public LogAsTraceProcessor(LogAsTraceProcessorOptions options)
        {
            this.options  = options ?? new LogAsTraceProcessorOptions();
            this.logLevel = options.LogLevel;
        }

        /// <summary>
        /// Handles the event forwarding.
        /// </summary>
        /// <param name="logRecord">The log record.</param>
        public override void OnEnd(LogRecord logRecord)
        {
            Covenant.Requires<ArgumentNullException>(logRecord != null, nameof(logRecord));

            var activity = Activity.Current;

            if (activity != null && logRecord.LogLevel >= logLevel)
            {
                // Compute the number of attributes we'll be adding to the span so we can
                // construct the array.

                var spanTagCount = 3;       // All events will include severity and severity-number tags
                var message      = logRecord.FormattedMessage;

                if (!string.IsNullOrEmpty(message))
                {
                    spanTagCount++;
                }

                if (logRecord.Exception != null)
                {
                    spanTagCount++;
                }

                var tags       = new KeyValuePair<string, object>[spanTagCount + (logRecord.StateValues?.Count ?? 0)];
                var nextIndex  = 0;

                // Add the required attributes.

                tags[nextIndex++] = new KeyValuePair<string, object>("category-name", logRecord.CategoryName);
                tags[nextIndex++] = new KeyValuePair<string, object>("severity", levelToSeverityName[(int)logLevel]);
                tags[nextIndex++] = new KeyValuePair<string, object>("severity-number", levelToSeverityName[(int)logLevel]);

                // Add additional log record properties as attributes if present.

                if (!string.IsNullOrEmpty(message))
                {
                    tags[nextIndex++] = new KeyValuePair<string, object>(LogTagNames.InternalBody, message);
                }

                if (logRecord.Exception != null)
                {
                    tags[nextIndex++] = new KeyValuePair<string, object>(LogTagNames.Exception, logRecord.Exception);
                }

                // Add the the log record attributes.

                if (logRecord.StateValues != null)
                {
                    foreach (var item in logRecord.StateValues)
                    {
                        tags[nextIndex++] = item;
                    }
                }

                if (logRecord.StateValues != null && logRecord.StateValues.Count > 0)
                {
                    var activityTags = DiagnosticPools.GetActivityTags();

                    try
                    {
                        foreach (var tag in logRecord.StateValues)
                        {
                            activityTags.Add(tag);
                        }

                        activity.AddEvent(new ActivityEvent(logRecord.FormattedMessage, logRecord.Timestamp, activityTags));
                    }
                    finally
                    {
                        DiagnosticPools.ReturnActivityTags(activityTags);
                    }
                }
                else
                {
                    activity.AddEvent(new ActivityEvent(logRecord.FormattedMessage, logRecord.Timestamp));
                }
            }

            base.OnEnd(logRecord);
        }
    }
}
