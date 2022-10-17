//-----------------------------------------------------------------------------
// FILE:	    DiagnosticsHelper.cs
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
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Neon.Common;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Internal helpers.
    /// </summary>
    internal static class DiagnosticsHelper
    {
        //---------------------------------------------------------------------
        // Local types

        /// <summary>
        /// Used to map .NET log levels to OpenTelemetry severity names and numbers.
        /// </summary>
        public class SeverityInfo
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="name">The standard OpenTelemetry severity name.</param>
            /// <param name="number">The standard OpenTelemetry severity number.</param>
            internal SeverityInfo(string name, int number)
            {
                this.Name    = name;
                this.Number = number;
            }

            /// <summary>
            /// Returns the standard OpenTelemetry severity name.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Returns the standard OpenTelementry severity number.
            /// </summary>
            public int Number { get; private set; }
        }

        //---------------------------------------------------------------------
        // Implementation

        private static SeverityInfo[] logLevelToSeverityInfo;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static DiagnosticsHelper()
        {
            // Initialize the map.

            logLevelToSeverityInfo = new SeverityInfo[(int)LogLevel.None + 1];

            logLevelToSeverityInfo[(int)LogLevel.Critical]    = new SeverityInfo("Fatal", (int)SeverityNumber.SEVERITY_NUMBER_FATAL);
            logLevelToSeverityInfo[(int)LogLevel.Error]       = new SeverityInfo("Error", (int)SeverityNumber.SEVERITY_NUMBER_ERROR);
            logLevelToSeverityInfo[(int)LogLevel.Warning]     = new SeverityInfo("Warning", (int)SeverityNumber.SEVERITY_NUMBER_WARN);
            logLevelToSeverityInfo[(int)LogLevel.Information] = new SeverityInfo("Information", (int)SeverityNumber.SEVERITY_NUMBER_INFO);
            logLevelToSeverityInfo[(int)LogLevel.Debug]       = new SeverityInfo("Debug", (int)SeverityNumber.SEVERITY_NUMBER_DEBUG);
            logLevelToSeverityInfo[(int)LogLevel.Trace]       = new SeverityInfo("Trace", (int)SeverityNumber.SEVERITY_NUMBER_TRACE);
            logLevelToSeverityInfo[(int)LogLevel.None]        = new SeverityInfo("Unspecified", (int)SeverityNumber.SEVERITY_NUMBER_UNSPECIFIED);
        }

        /// <summary>
        /// Maps a .NET log level to information about the related standard OpenTelemetry
        /// severity name and number.
        /// </summary>
        /// <param name="logLevel">The .NET log level.</param>
        /// <returns>A <see cref="SeverityInfo"/> with the OpenTelemetry information.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SeverityInfo GetSeverityInfo(LogLevel logLevel)
        {
            return logLevelToSeverityInfo[(int)logLevel];
        }

        /// <summary>
        /// Converts a <see cref="LogRecord"/> into a <see cref="LogEvent"/>.  This is used
        /// internally by our custom log exporters.  This method is designed to be called
        /// within a batch export loop and reuses a <see cref="LogEvent"/> instances as well
        /// as tag and resource dictionaries to reduce GC pressure.
        /// </summary>
        /// <param name="exporter">The exporter.</param>
        /// <param name="includeStackTrace">Controls whether exception stack traces are include.</param>
        /// <param name="record">The input record.</param>
        /// <param name="resources">Passed as an allocated temporary dictionary used for rendering resources.</param>
        /// <param name="tags">Passed as an allocated temporary dictionary used for rendering tags.</param>
        /// <param name="logEvent">Returns as initialized from <paramref name="record"/>.</param>
        internal static void SetLogEvent(BaseExporter<LogRecord> exporter, bool includeStackTrace, LogRecord record, Dictionary<string, object> tags, Dictionary<string, object> resources, LogEvent logEvent)
        {
            Covenant.Requires<ArgumentNullException>(exporter != null, nameof(exporter));
            Covenant.Requires<ArgumentNullException>(record != null, nameof(record));
            Covenant.Requires<ArgumentNullException>(tags != null, nameof(tags));
            Covenant.Requires<ArgumentNullException>(resources != null, nameof(resources));
            Covenant.Requires<ArgumentNullException>(logEvent != null, nameof(logEvent));

            var severityInfo = DiagnosticsHelper.GetSeverityInfo(record.LogLevel);

            logEvent.CategoryName   = record.CategoryName;
            logEvent.Severity       = severityInfo.Name;
            logEvent.SeverityNumber = severityInfo.Number;
            logEvent.SpanId         = record.SpanId == default ? null : record.SpanId.ToHexString();
            logEvent.TraceId        = record.TraceId == default ? null : record.TraceId.ToHexString();
            logEvent.TsNs           = record.Timestamp.ToUnixEpochNanoseconds();

            //-------------------------------------------------------------
            // Exception information

            var exceptionInfo = new ExceptionInfo();

            if (record.Exception != null)
            {
                var exception = record.Exception;

                exceptionInfo.Type    = exception.GetType().FullName;
                exceptionInfo.Message = exception.Message;
                exceptionInfo.Stack   = exception.StackTrace.TrimStart();
            }

            //-------------------------------------------------------------
            // Resource information

            resources.Clear();

            var resource = exporter.ParentProvider.GetResource();

            if (resource != Resource.Empty)
            {
                foreach (var attribute in resource.Attributes)
                {
                    resources[attribute.Key] = attribute.Value;
                }
            }

            if (resources.Count > 0)
            {
                logEvent.Resources = resources;
            }
            else
            {
                logEvent.Resources = null;
            }

            //-------------------------------------------------------------
            // Tags

            tags.Clear();

            logEvent.CategoryName = record.CategoryName;

            if (!string.IsNullOrEmpty(record.CategoryName))
            {
                tags.Add(LogAttributeNames.CategoryName, logEvent.CategoryName);
            }

            if ((record.StateValues != null && record.StateValues.Count > 0) || record.Exception != null)
            {
                if (record.StateValues != null)
                {
                    foreach (var item in record.StateValues)
                    {
                        // Ignore tags without a name to be safe.

                        if (string.IsNullOrEmpty(item.Key))
                        {
                            continue;
                        }

                        // We need to special case the [Body] property.  Our [ILogger] extensions persist
                        // the log message in as the [LogTagNames.InternalBody] whereas the stock MSFT
                        // logger methods set [FormattedMessage] to the formatted message when enabled 
                        // and also save the non-formatted message as the "{OriginalFormat}" attribute.
                        //
                        // We're going to honor [FormattedMessage], if present, so that events logged with 
                        // the MSFT logger extensions will continue to work as always and when this is
                        // not present and our [LogTags.Body] attribute is present, we're going to use
                        // our body as the message.
                        //
                        // I don't believe it makes a lot of sense to ever include the "{OriginalFormat}"
                        // as an attribute so we're not doing to emit it.  I guess there could be a
                        // scenerio where the user has disabled message formatting when using the MSFT
                        // extensions but still wants to see the "{OriginalFormat}", but I think MSFT
                        // wouldn't have named the attribute like that if they intended it to be public.
                        //
                        // We're going take care of this by ignoring attributes with names starting with "{"
                        // in case there are other situations where internal attributes are generated.

                        if (string.IsNullOrEmpty(record.FormattedMessage))
                        {
                            if (item.Key == LogAttributeNames.InternalBody)
                            {
                                logEvent.Body = item.Value as string;
                                continue;
                            }
                        }

                        if (item.Key.StartsWith("{"))
                        {
                            continue;
                        }

                        tags[item.Key] = item.Value;
                    }
                }

                // Use the formatted message if present.  This will be set when the user uses 
                // the base MSFT logger extensions.

                if (record.FormattedMessage != null)
                {
                    logEvent.Body = record.FormattedMessage;
                }

                // Add any exception information.

                if (record.Exception != null)
                {
                    tags.Add("exception.type", exceptionInfo.Type);
                    tags.Add("exception.message", exceptionInfo.Message);

                    if (includeStackTrace)
                    {
                        tags.Add("exception.stacktrace", exceptionInfo.Stack);
                    }
                }

                logEvent.Attributes = tags.Count > 0 ? tags : null;
            }
            else
            {
                logEvent.Attributes = null;
            }
        }
    }
}
