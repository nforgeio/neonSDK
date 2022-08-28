//-----------------------------------------------------------------------------
// FILE:	    ConsoleJsonLogExporter.cs
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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Neon.Common;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Neon.Diagnostics
{
    /// <summary>
    /// <para>
    /// Exports log records to the console where each record will be written
    /// as a line of JSON text to standard output and/or standard error when
    /// configured.
    /// </para>
    /// <para>
    /// This is suitable for production environments like Kubernetes, Docker,
    /// etc. where logs are captured from the program output.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>IMPORTANT:</b> To enable the inclusion of log tags in the output JSON, you must
    /// set <see cref="OpenTelemetryLoggerOptions.ParseStateValues"/><c>=true</c> when
    /// configuring your OpenTelemetry options.  This is is <c>false</c> by default.
    /// </para>
    /// <code language="C#">
    /// var loggerFactory = LoggerFactory.Create(
    ///     builder =>
    ///     {
    ///         builder.AddOpenTelemetry(
    ///             options =>
    ///             {
    ///                 options.ParseStateValues = true;    // &lt;--- SET THIS TO TRUE
    ///                 
    ///                 options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: ServiceName, serviceVersion: ServiceVersion));
    ///                 options.AddLogAsTraceProcessor(options => options.LogLevel = LogLevel.Warning);
    ///                 options.AddConsoleJsonExporter();
    ///             });
    ///     });
    /// </code>
    /// </remarks>
    public class ConsoleJsonLogExporter : BaseExporter<LogRecord>
    {
        private ConsoleJsonLogExporterOptions   options;
        private LogEvent                        logEvent  = new LogEvent();
        private Dictionary<string, object>      labels    = new Dictionary<string, object>();
        private Dictionary<string, object>      resources = new Dictionary<string, object>();

        /// <summary>
        /// Constructs a log exporter that writes log records to standard output and/or
        /// standard error as single line JSON objects.
        /// </summary>
        /// <param name="options">Optionally specifies the exporter options.</param>
        public ConsoleJsonLogExporter(ConsoleJsonLogExporterOptions options = null)
        {
            this.options = options ?? new ConsoleJsonLogExporterOptions();
        }

        /// <inheritdoc/>
        public override ExportResult Export(in Batch<LogRecord> batch)
        {
            foreach (var record in batch)
            {
                var severityInfo = DiagnosticsHelper.GetSeverityInfo(record.LogLevel);

                logEvent.CategoryName   = record.CategoryName;
                logEvent.Severity       = severityInfo.Name;
                logEvent.SeverityNumber = severityInfo.Number;
                logEvent.SpanId         = record.SpanId == default ? null : record.SpanId.ToHexString();
                logEvent.TraceId        = record.TraceId == default ? null : record.TraceId.ToHexString();
                logEvent.TsNs           = record.Timestamp.ToUnixEpochNanoseconds();

                //-------------------------------------------------------------
                // Exception information

                ExceptionInfo       exceptionInfo      = null;
                ExceptionInfo       innerExceptionInfo = null;
                List<ExceptionInfo> innerExceptionList = null;

                if (record.Exception != null)
                {
                    var exception = record.Exception;
                    
                    exceptionInfo       = DiagnosticPools.GetExceptionInfo();
                    exceptionInfo.Name  = exception.GetType().FullName;
                    exceptionInfo.Stack = exception.StackTrace;

                    if (options.InnerExceptions && exception != null)
                    {
                        if (exception is AggregateException aggregateException && aggregateException.InnerExceptions.Count > 0)
                        {
                            innerExceptionList = new List<ExceptionInfo>();

                            foreach (var innerException in aggregateException.InnerExceptions)
                            {
                                var inner = new ExceptionInfo();

                                inner.Name  = innerException.GetType().FullName;
                                inner.Stack = innerException.StackTrace;

                                innerExceptionList.Add(inner);
                            }
                        }
                        else if (exception.InnerException != null)
                        {
                            var innerException     = exception.InnerException;
                            
                            innerExceptionInfo = new ExceptionInfo();

                            innerExceptionInfo.Name  = innerException.GetType().FullName;
                            innerExceptionInfo.Stack = innerException.StackTrace;

                            innerExceptionList.Add(innerExceptionInfo);
                        }

                        exceptionInfo.InnerExceptions = innerExceptionList;
                    }
                }

                //-------------------------------------------------------------
                // Resource information

                resources.Clear();

                var resource = this.ParentProvider.GetResource();

                if (resource != Resource.Empty)
                {
                    foreach (var tag in resource.Attributes)
                    {
                        resources[tag.Key] = tag.Value;
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
                // Labels

                labels.Clear();

                if ((record.StateValues != null && record.StateValues.Count > 0) || exceptionInfo != null)
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
                            // and also save the non-formatted message as the "{OriginalFormat}" tag.
                            //
                            // We're going to honor [FormattedMessage] if present so that events logged with 
                            // the MSFT logger extensions will continue to work as always and when this is
                            // not present and our [LogTags.Body] tag is present, we're going to use our body
                            // as the message.
                            //
                            // I don't believe it makes a lot of sense to ever include the "{OriginalFormat}"
                            // as a label so we're not doing to emit it as a label.  I guess there could be
                            // a scenerio where the user has disabled message formatting when using the MSFT
                            // extensions but still wants to see the "{OriginalFormat}", but I think MSFT
                            // wouldn't have named the label like that if they intended it to be public.
                            //
                            // We're going take care of this by ignoring tags with names starting with "{"
                            // in case there are other situations where internal tags are generated.

                            if (string.IsNullOrEmpty(record.FormattedMessage))
                            {
                                if (item.Key == LogTagNames.InternalBody)
                                {
                                    logEvent.Body = item.Value as string;
                                    continue;
                                }
                            }

                            if (item.Key.StartsWith("{"))
                            {
                                continue;
                            }

                            labels[item.Key] = item.Value;
                        }
                    }

                    // Use the formatted message if present.  This will be set when the user uses 
                    // the base MSFT logger extensions.

                    if (record.FormattedMessage != null)
                    {
                        logEvent.Body = record.FormattedMessage;
                    }

                    // Add any exception information.

                    if (exceptionInfo != null)
                    {
                        labels.Add("exception.name", exceptionInfo.Name);

                        if (options.ExceptionStackTraces)
                        {
                            labels.Add("exception.stack", exceptionInfo.Stack);
                        }

                        if (innerExceptionList.Count > 0 && options.InnerExceptions)
                        {
                            labels.Add("exception.inner", innerExceptionList);
                        }
                    }

                    logEvent.Labels = labels.Count > 0 ? labels : null;
                }
                else
                {
                    logEvent.Labels = null;
                }

                //-------------------------------------------------------------
                // We can return this to its source pools now.

                if (exceptionInfo != null)
                {
                    DiagnosticPools.ReturnExceptionInfo(exceptionInfo);
                    exceptionInfo = null;
                }

                //-------------------------------------------------------------
                // Give any interceptor a chance to see and/or modify the event.

                options.LogEventInterceptor?.Invoke(logEvent);

                //-------------------------------------------------------------
                // Write the JSON formatted record on a single line to STDOUT or STDERR
                // depending on the event's log level and the exporter options.

                if (options.SingleLine)
                {
                    var jsonText = JsonConvert.SerializeObject(logEvent, Formatting.None);

                    if ((int)record.LogLevel >= (int)options.StandardErrorLevel)
                    {
                        options.StdErrInterceptor?.Invoke(jsonText + Environment.NewLine);

                        if (options.Emit)
                        {
                            Console.Error.WriteLine(jsonText);
                        }
                    }
                    else
                    {
                        options.StdOutInterceptor?.Invoke(jsonText + Environment.NewLine);

                        if (options.Emit)
                        {
                            Console.Out.WriteLine(jsonText);
                        }
                    }
                }
                else
                {
                    var jsonText = JsonConvert.SerializeObject(logEvent, Formatting.Indented);

                    if ((int)record.LogLevel >= (int)options.StandardErrorLevel)
                    {
                        options.StdErrInterceptor?.Invoke(jsonText + Environment.NewLine + Environment.NewLine);

                        if (options.Emit)
                        {
                            Console.Error.WriteLine(jsonText);
                            Console.Error.WriteLine();
                        }
                    }
                    else
                    {
                        options.StdOutInterceptor?.Invoke(jsonText + Environment.NewLine + Environment.NewLine);

                        if (options.Emit)
                        {
                            Console.Out.WriteLine(jsonText);
                            Console.Out.WriteLine();
                        }
                    }
                }
            }

            return ExportResult.Success;
        }
    }
}
