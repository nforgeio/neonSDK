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
using System.Linq;
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
    /// This exporter currently supports writing logs as JSON or a bespoke human readable 
    /// format (the default).  You can customize this by using options with the 
    /// <see cref="FileLogExporterOptions.Format"/> to one of the <see cref="FileLogExporterFormat"/>
    /// values (as shown below).
    /// </para>
    /// <para>
    /// <b>IMPORTANT:</b> To enable the inclusion of log tags in the output, you must
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
    /// This exporter is currently somewhat limited.  It overwrites any existing log file
    /// when the exporter is instantiated and then just writes logs to the file without 
    /// limiting the file size ordoing any file rotation.  We'll likely implement rotation
    /// and size limits in the future.
    /// </note>
    /// </remarks>
    public class FileLogExporter : BaseExporter<LogRecord>
    {
        private readonly FileLogExporterOptions     options;
        private readonly FileStream                 logStream;
        private readonly StreamWriter               logWriter;
        private readonly LogEvent                   logEvent  = new LogEvent();
        private readonly Dictionary<string, object> tags      = new Dictionary<string, object>();
        private readonly Dictionary<string, object> resources = new Dictionary<string, object>();

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
                logStream = new FileStream(Path.Combine(options.LogFolder, options.LogFileName), FileMode.Create, FileAccess.Write, FileShare.Read);
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

            const string indent1 = "    ";
            const string indent2 = indent1 + indent1;

            LogEvent logEvent                     = new LogEvent();
            Dictionary<string, object>  tags      = new Dictionary<string, object>();
            Dictionary<string, object>  resources = new Dictionary<string, object>();

            foreach (var record in batch)
            {
                //-------------------------------------------------------------
                // Convert the LogRecord into a LogEvent which we'll use to render
                // the LogRecord into JSON.  Note that we're using some preallocated
                // objects here to reduce GC pressure.

                DiagnosticsHelper.SetLogEvent(this, includeStackTrace: true, record, tags, resources, logEvent);

                //-------------------------------------------------------------
                // Give any interceptor a chance to see and/or modify the event.

                options.LogEventInterceptor?.Invoke(logEvent);

                //-------------------------------------------------------------
                // Write the log record.

                try
                {
                    switch (options.Format)
                    {
                        case FileLogExporterFormat.Human:

                            logWriter.Write("[");
                            logWriter.Write(NeonHelper.UnixEpochNanosecondsToDateTimeUtc(logEvent.TsNs).ToString(NeonHelper.DateFormatTZ));
                            logWriter.Write("]: ");
                            logWriter.WriteLine(logEvent.Body);

                            logWriter.Write($"{indent1}SEVERITY: ");
                            logWriter.WriteLine(logEvent.Severity);

                            if (!string.IsNullOrEmpty(logEvent.CategoryName))
                            {
                                logWriter.Write($"{indent1}CATEGORY: ");
                                logWriter.WriteLine(logEvent.CategoryName);
                            }

                            if (!string.IsNullOrEmpty(logEvent.TraceId))
                            {
                                logWriter.Write($"{indent1}TRACE-ID: ");
                                logWriter.WriteLine(logEvent.TraceId);
                            }

                            if (!string.IsNullOrEmpty(logEvent.SpanId))
                            {
                                logWriter.Write($"{indent1}SPAN-ID: ");
                                logWriter.WriteLine(logEvent.SpanId);
                            }

                            if (logEvent.Resources?.Count > 0)
                            {
                                logWriter.WriteLine($"{indent1}RESOURCES:");

                                foreach (var item in logEvent.Resources)
                                {
                                    logWriter.Write(indent2);
                                    logWriter.Write(item.Key);
                                    logWriter.Write(": ");
                                    logWriter.WriteLine(item.Value?.ToString());
                                }
                            }

                            if (logEvent.Attributes?.Count > 0)
                            {
                                logWriter.WriteLine($"{indent1}ATTRIBUTES:");

                                foreach (var item in logEvent.Attributes)
                                {
                                    // We're going to ignore any exception related attributes
                                    // here because we call those out below.

                                    if (item.Key.StartsWith("exception."))
                                    {
                                        continue;
                                    }

                                    logWriter.Write(indent2);
                                    logWriter.Write(item.Key);
                                    logWriter.Write(": ");
                                    logWriter.WriteLine(item.Value?.ToString());
                                }
                            }

                            // Write any exception information indented under the log line,
                            // when enabled.

                            if (record.Exception != null)
                            {
                                var exception = record.Exception;

                                logWriter.Write($"{indent1}EXCEPTION.TYPE: ");
                                logWriter.WriteLine(exception.GetType().FullName);
                                logWriter.Write($"{indent1}EXCEPTION.MESSAGE: ");
                                logWriter.WriteLine(exception.Message);
                                logWriter.WriteLine($"{indent1}EXCEPTION.STACKTRACE:");

                                using (var reader = new StringReader(exception.StackTrace))
                                {
                                    foreach (var line in reader.Lines())
                                    {
                                        logWriter.Write(indent1);
                                        logWriter.WriteLine(line);
                                    }
                                }
                            }

                            logWriter.WriteLine();
                            break;

                        case FileLogExporterFormat.Json:

                            // Write the JSON formatted record as a single line.

                            logWriter.WriteLine(JsonConvert.SerializeObject(logEvent, Formatting.None));
                            break;

                        default:

                            throw new NotImplementedException();
                    }
                }
                catch
                {
                    // It's possible that we can't write to the log file for some reason.

                    return ExportResult.Failure;
                }
            }

            if (options.FlushAgressively)
            {
                logWriter.Flush();
            }

            return ExportResult.Success;
        }
    }
}