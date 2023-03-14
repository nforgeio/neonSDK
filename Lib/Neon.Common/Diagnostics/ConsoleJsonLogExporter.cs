//-----------------------------------------------------------------------------
// FILE:	    ConsoleJsonLogExporter.cs
// CONTRIBUTOR: Jeff Lill
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
        private Dictionary<string, object>      tags      = new Dictionary<string, object>();
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
                //-------------------------------------------------------------
                // Convert the LogRecord into a LogEvent which we'll use to render
                // the LogRecord into JSON.  Note that we're using some preallocated
                // objects here to reduce GC pressure.

                DiagnosticsHelper.SetLogEvent(this, options.ExceptionStackTraces, record, tags, resources, logEvent);

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

            Console.Out.Flush();
            Console.Error.Flush();

            return ExportResult.Success;
        }
    }
}
