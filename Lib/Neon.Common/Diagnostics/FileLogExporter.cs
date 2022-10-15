//-----------------------------------------------------------------------------
// FILE:	    FileLogExporter.cs
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
using System.IO;
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
    /// Exports log records to a file as specified by <see cref="FileLogExporterOptions"/>.
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
    ///                 options.AddFileExporter(
    ///                     options => 
    ///                     {
    ///                         options.Format      = FileLogExporterFormat.Human;
    ///                         options.LogFolder   = NeonHelper.GetBaseDirtectory();
    ///                         options.LogFileName = "MyProgram.log";
    ///                     });
    ///             });
    ///     });
    /// </code>
    /// <note>
    /// This exporter is currently somewhat limited.  It deletes any existing log file and
    /// then just writes logs to the file without limiting the log file size or rotating
    /// the log files.  We'll probably implement that functionality in the future.
    /// </note>
    /// </remarks>
    public class FileLogExporter : BaseExporter<LogRecord>
    {
        private FileLogExporterOptions      options;
        private FileStream                  logStream;
        private StreamWriter                logWriter;
        private LogEvent                    logEvent  = new LogEvent();
        private Dictionary<string, object>  tags      = new Dictionary<string, object>();
        private Dictionary<string, object>  resources = new Dictionary<string, object>();
        private StringBuilder               sb        = new StringBuilder();

        /// <summary>
        /// Constructs a log exporter that writes log records to standard output and/or
        /// standard error as single line JSON objects.
        /// </summary>
        /// <param name="options">Optionally specifies the exporter options.</param>
        public FileLogExporter(FileLogExporterOptions options = null)
        {
            this.options = options ?? new FileLogExporterOptions();

            if (string.IsNullOrEmpty(options.LogFolder))
            {
                throw new ArgumentException($"[{nameof(FileLogExporterOptions)}.{nameof(FileLogExporterOptions.LogFolder)}] must be specified.");
            }

            if (string.IsNullOrEmpty(options.LogFileName))
            {
                throw new ArgumentException($"[{nameof(FileLogExporterOptions)}.{nameof(FileLogExporterOptions.LogFileName)}] must be specified.");
            }

            // Create the log output stream.  This will overwrite any existing file.

            try
            {
                logStream = File.Create(Path.Combine(options.LogFolder, options.LogFileName));
                logWriter = new StreamWriter(logStream);
            }
            catch (IOException)
            {
                logStream = null;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            logWriter?.Flush();
            logStream?.Dispose();
        }

        /// <inheritdoc/>
        protected override bool OnForceFlush(int timeoutMilliseconds)
        {
            try
            {
                logWriter.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(int timeoutMilliseconds)
        {
            try
            {
                logWriter.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override ExportResult Export(in Batch<LogRecord> batch)
        {
            // It's possible that we couldn't create the log stream.

            if (logStream == null)
            {
                return ExportResult.Failure;
            }

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

                var resource = this.ParentProvider.GetResource();

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
                            // We're going to honor [FormattedMessage] if present so that events logged with 
                            // the MSFT logger extensions will continue to work as always and when this is
                            // not present and our [LogTags.Body] attribute is present, we're going to use
                            // our body as the message.
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

                    // Add any exception information for non-Human formats.

                    if (options.Format != FileLogExporterFormat.Human && record.Exception != null)
                    {
                        tags.Add("exception.type", exceptionInfo.Type);
                        tags.Add("exception.message", exceptionInfo.Message);
                        tags.Add("exception.stacktrace", exceptionInfo.Stack);
                    }

                    logEvent.Attributes = tags.Count > 0 ? tags : null;
                }
                else
                {
                    logEvent.Attributes = null;
                }

                //-------------------------------------------------------------
                // Give any interceptor a chance to see and/or modify the event.

                options.LogEventInterceptor?.Invoke(logEvent);

                //-------------------------------------------------------------
                // Output the log record.

                switch (options.Format)
                {
                    case FileLogExporterFormat.Human:

                        sb.Clear();
                        sb.AppendWithSeparator(new DateTime(logEvent.TsNs / 100).ToString(NeonHelper.DateFormatMicroTZ));
                        sb.AppendWithSeparator(logEvent.Severity);

                        if (!string.IsNullOrEmpty(logEvent.CategoryName))
                        {
                            sb.AppendWithSeparator($"[category={logEvent.CategoryName}]");
                        }

                        sb.AppendWithSeparator($"[body={logEvent.Body}]");

                        if (!string.IsNullOrEmpty(logEvent.TraceId))
                        {
                            sb.AppendWithSeparator($"[traceId={logEvent.TraceId}]");
                        }

                        if (!string.IsNullOrEmpty(logEvent.SpanId))
                        {
                            sb.AppendWithSeparator($"[spanId={logEvent.SpanId}]");
                        }

                        if (logEvent.Resources?.Count > 0)
                        {
                            sb.Append("[resources=");

                            foreach (var item in logEvent.Resources)
                            {
                                sb.AppendWithSeparator($"({item.Key}={item.Value})");
                            }

                            sb.Append(']');
                        }

                        // We're going to write any exception information on indented lines.

                        if (logEvent.Attributes?.Count > 0)
                        {
                            sb.Append("[attributes=");

                            foreach (var item in logEvent.Attributes)
                            {
                                sb.AppendWithSeparator($"({item.Key}={item.Value})");
                            }

                            sb.Append(']');
                        }

                        try
                        {
                            logWriter.WriteLine(sb);

                            // Write any exception information indented under the log line.

                            if (record.Exception != null)
                            {
                                const string indent1 = "    ";
                                const string indent2 = indent1 + indent1;

                                var exception = record.Exception;

                                logWriter.WriteLine($"{indent1}TYPE={exception.GetType().FullName} MESSAGE={exception.Message}");
                                logWriter.WriteLine($"{indent1}STACKTRACE:");

                                using (var reader = new StringReader(exception.StackTrace))
                                {
                                    foreach (var line in reader.Lines())
                                    {
                                        logWriter.WriteLine($"{indent2}{line}");
                                    }
                                }
                            }
                        }
                        catch
                        {
                            return ExportResult.Failure;
                        }
                        break;

                    case FileLogExporterFormat.Json:

                        // Write the JSON formatted record as a single line.

                        try
                        {
                            logWriter.WriteLine(JsonConvert.SerializeObject(logEvent, Formatting.None));
                        }
                        catch
                        {
                            return ExportResult.Failure;
                        }
                        break;

                    default:

                        throw new NotImplementedException();
                }
            }

            return ExportResult.Success;
        }
    }
}