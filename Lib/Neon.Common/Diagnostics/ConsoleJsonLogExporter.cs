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

// $todo(jefflill):
//
// This exporter uses NewtonSoft to render the log lines and we're allocating a ton of
// objects to do this.  We should consider rolling our own JSON serialization code to
// optimize this.  We could probably implement this with nearly zero allocations.
//
//      https://github.com/nforgeio/neonSDK/issues/23

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
    public class ConsoleJsonLogExporter : BaseExporter<LogRecord>
    {
        private ConsoleJsonLogExporterOptions   options;
        private JObject                         logObject;
        private JProperty                       bodyProperty;
        private JProperty                       categoryNameProperty;
        private JProperty                       labelsProperty;
        private JProperty                       resourcesProperty;
        private JProperty                       severityProperty;
        private JProperty                       severityNumberProperty;
        private JProperty                       spanIdProperty;
        private JProperty                       traceIdProperty;
        private JProperty                       tsNsProperty;

        /// <summary>
        /// Constructs a log exporter that writes log records to standard output and/or
        /// standard error as single line JSON objects.
        /// </summary>
        /// <param name="options">Optionally specifies the exporter options.</param>
        public ConsoleJsonLogExporter(ConsoleJsonLogExporterOptions options = null)
        {
            this.options = options ?? new ConsoleJsonLogExporterOptions();

            // We're going to reuse a [JObject] for rendering the log record as JSON
            // to avoid some memory allocations.  We're going to initialze this with
            // the top-level properties that we'll always emit and save references to
            // these, so they'll be easy to set in Export().

            logObject = new JObject();

            bodyProperty = new JProperty(LogTagNames.Body, new JValue(String.Empty));
            logObject.Add(bodyProperty);

            categoryNameProperty = new JProperty(LogTagNames.CategoryName, new JValue(String.Empty));
            logObject.Add(categoryNameProperty);

            labelsProperty = new JProperty(LogTagNames.Labels, new JObject());
            logObject.Add(labelsProperty);

            resourcesProperty = new JProperty(LogTagNames.Resources, new JObject());
            logObject.Add(resourcesProperty);

            severityProperty = new JProperty(LogTagNames.Severity, new JValue(String.Empty));
            logObject.Add(severityProperty);

            severityNumberProperty = new JProperty(LogTagNames.SeverityNumber, new JValue(0));
            logObject.Add(severityNumberProperty);

            spanIdProperty = new JProperty(LogTagNames.SpanId, new JValue(String.Empty));
            logObject.Add(spanIdProperty);

            traceIdProperty = new JProperty(LogTagNames.TraceId, new JValue(String.Empty));
            logObject.Add(traceIdProperty);

            tsNsProperty = new JProperty(LogTagNames.TsNs, new JValue(0L));
            logObject.Add(tsNsProperty);
        }

        /// <inheritdoc/>
        public override ExportResult Export(in Batch<LogRecord> batch)
        {
            foreach (var record in batch)
            {
                var severityInfo = DiagnosticsHelper.GetSeverityInfo(record.LogLevel);

                categoryNameProperty.Value   = new JValue(record.CategoryName);
                severityProperty.Value       = new JValue(severityInfo.Name);
                severityNumberProperty.Value = new JValue(severityInfo.Number);
                spanIdProperty.Value         = record.SpanId != default ? new JValue(record.SpanId.ToHexString()) : new JValue((object)null);
                traceIdProperty.Value        = record.TraceId != default ? new JValue(record.TraceId.ToHexString()) : new JValue((object)null);
                tsNsProperty.Value           = new JValue(record.Timestamp.ToUnixEpochNanoseconds());

                // Clear and then set the resource properties.

                resourcesProperty.Value = new JObject();

                // Clear and then set the label propeties.

                labelsProperty.Value = new JObject();

                // Make sure we clear the body property before potentially extracting the 
                // body from the state.
                //
                // We're going to special case the situation where there is no "body"
                // property in the state and use the "{OriginalFormat}" value as the
                // body instead if it's present.  We'll see this when user use the
                // standard MSFT ILogger extensions.

                bodyProperty.Value = (JToken)null;

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

                    // Special case the "body" property.

                    if (item.Key == LogTagNames.Body)
                    {
                        bodyProperty.Value = (JToken)item.Value;
                        continue;
                    }

                    // Add all other tags to the property.

                    ((JObject)labelsProperty.Value).Add(item.Key, JToken.FromObject(item.Value));

                    // Handle the related resource information.

                    resourcesProperty.Value = new JObject();

                    var resource = this.ParentProvider.GetResource();

                    if (resource != Resource.Empty)
                    {
                        foreach (var tag in resource.Attributes)
                        {
                            ((JObject)resourcesProperty.Value).Add(tag.Key, JToken.FromObject(tag.Value));
                        }
                    }
                }

                if (bodyProperty.Value.Type == JTokenType.Null && originalFormat != null)
                {
                    bodyProperty.Value = new JValue(originalFormat);
                }

                // Write the JSON formatted record on a single line to stdout or stderr depending
                // on the event's log level and the exporter options.

                if (options.SingleLine)
                {
                    var jsonText = logObject.ToString(Formatting.None);

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
                    var jsonText = logObject.ToString(Formatting.Indented);

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
