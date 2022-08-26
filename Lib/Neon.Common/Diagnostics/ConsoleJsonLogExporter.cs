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
    ///                 options.AddProcessor(new LogAsTraceProcessor());
    ///                 options.AddConsoleJsonExporter(options => options.SingleLine = false);
    ///             });
    ///     });
    /// </code>
    /// </remarks>
    public class ConsoleJsonLogExporter : BaseExporter<LogRecord>
    {
        //---------------------------------------------------------------------
        // Private types

        /// <summary>
        /// Used for serializing the log records to JSON.
        /// </summary>
        private class JsonEvent
        {
            [JsonProperty(PropertyName = "tsNs")]
            public long TsNs { get; set; }

            [JsonProperty(PropertyName = "severity", DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(null)]
            public string Severity { get; set; }

            [JsonProperty(PropertyName = "body", DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(null)]
            public string Body { get; set; }

            [JsonProperty(PropertyName = "categoryName", DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(null)]
            public string CategoryName { get; set; }

            [JsonProperty(PropertyName = "severityNumber", DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(-1)]
            public int SeverityNumber { get; set; }

            [JsonProperty(PropertyName = "labels", DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(null)]
            public Dictionary<string, object> Labels { get; set; } = new Dictionary<string, object>();

            [JsonProperty(PropertyName = "resources", DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(null)]
            public Dictionary<string, object> Resources { get; set; } = new Dictionary<string, object>();

            [JsonProperty(PropertyName = "spanId", DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(null)]
            public string SpanId { get; set; }

            [JsonProperty(PropertyName = "traceId", DefaultValueHandling = DefaultValueHandling.Ignore)]
            [DefaultValue(null)]
            public string TraceId { get; set; }
        }

        //---------------------------------------------------------------------
        // Implementation

        private ConsoleJsonLogExporterOptions   options;
        private JsonEvent                       jsonRecord = new JsonEvent();
        private Dictionary<string, object>      labels     = new Dictionary<string, object>();
        private Dictionary<string, object>      resources  = new Dictionary<string, object>();

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

                jsonRecord.CategoryName   = record.CategoryName;
                jsonRecord.Severity       = severityInfo.Name;
                jsonRecord.SeverityNumber = severityInfo.Number;
                jsonRecord.SpanId         = record.SpanId == default ? null : record.SpanId.ToHexString();
                jsonRecord.TraceId        = record.TraceId == default ? null : record.TraceId.ToHexString();
                jsonRecord.TsNs           = record.Timestamp.ToUnixEpochNanoseconds();

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
                    jsonRecord.Resources = resources;
                }
                else
                {
                    jsonRecord.Resources = null;
                }

                labels.Clear();

                if (record.StateValues != null && record.StateValues.Count > 0)
                {
                    // We're going to special case the situation where there is no "body"
                    // property in the state and use the "{OriginalFormat}" value as the
                    // body instead if it's present.  We'll see this when user use the
                    // standard MSFT ILogger extensions.

                    var originalFormat = (object)null;

                    foreach (var item in record.StateValues)
                    {
                        // We're going to ignore the "{OriginalFormat}" tag because that's only
                        // going to include the weird tag names and we explicitly set the event
                        // body as the "body" property.

                        if (item.Key == "{OriginalFormat}")
                        {
                            originalFormat = item.Value;
                            continue;
                        }

                        // Special case the "_body_" property.

                        if (item.Key == LogTagNames.Body)
                        {
                            jsonRecord.Body = item.Value as string;
                            continue;
                        }

                        // Add all other tags to the property.

                        labels[item.Key] = item.Value;
                    }

                    if (string.IsNullOrEmpty(jsonRecord.Body) && originalFormat != null)
                    {
                        jsonRecord.Body = originalFormat as string;
                    }

                    jsonRecord.Labels = labels.Count > 0 ? labels : null;
                }
                else
                {
                    jsonRecord.Labels = null;
                }

                // Write the JSON formatted record on a single line to stdout or stderr depending
                // on the event's log level and the exporter options.

                if (options.SingleLine)
                {
                    var jsonText = JsonConvert.SerializeObject(jsonRecord, Formatting.None);

                    if ((int)record.LogLevel >= (int)options.StandardErrorLevel)
                    {
                        Console.Error.WriteLine(jsonText);
                    }
                    else
                    {
                        Console.Out.WriteLine(jsonText);
                    }
                }
                else
                {
                    var jsonText = JsonConvert.SerializeObject(jsonRecord, Formatting.Indented);

                    if ((int)record.LogLevel >= (int)options.StandardErrorLevel)
                    {
                        Console.Error.WriteLine(jsonText);
                        Console.Error.WriteLine();
                    }
                    else
                    {
                        Console.Out.WriteLine(jsonText);
                        Console.Out.WriteLine();
                    }
                }
            }

            return ExportResult.Success;
        }
    }
}
