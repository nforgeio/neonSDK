//-----------------------------------------------------------------------------
// FILE:        LogEvent.cs
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
    /// Used for serializing the log records.
    /// </summary>
    public class LogEvent
    {
        /// <summary>
        /// The event timestamp expressed as Unix Epoch nanoseconds.
        /// </summary>
        [JsonProperty(PropertyName = "tsNs")]
        public long TsNs { get; set; }

        /// <summary>
        /// The human readable event severity level. 
        /// </summary>
        [JsonProperty(PropertyName = "severity", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(null)]
        public string Severity { get; internal set; }

        /// <summary>
        /// The event message.
        /// </summary>
        [JsonProperty(PropertyName = "body", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(null)]
        public string Body { get; set; }

        /// <summary>
        /// The event source category name.
        /// </summary>
        [JsonProperty(PropertyName = "categoryName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(null)]
        public string CategoryName { get; set; }

        /// <summary>
        /// The standard OpenTelemetry event severity number.
        /// </summary>
        [JsonProperty(PropertyName = "severityNumber", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(-1)]
        public int SeverityNumber { get; internal set; }

        /// <summary>
        /// The event tags.
        /// </summary>
        [JsonProperty(PropertyName = "attributes", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(null)]
        public IReadOnlyDictionary<string, object> Attributes { get; internal set; }

        /// <summary>
        /// The related event resources.
        /// </summary>
        [JsonProperty(PropertyName = "resources", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(null)]
        public IReadOnlyDictionary<string, object> Resources { get; internal set; }

        /// <summary>
        /// The associated trace span ID.
        /// </summary>
        [JsonProperty(PropertyName = "spanId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(null)]
        public string SpanId { get; set; }

        /// <summary>
        /// The associated trace ID.
        /// </summary>
        [JsonProperty(PropertyName = "traceId", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [DefaultValue(null)]
        public string TraceId { get; set; }

        /// <summary>
        /// Clones the current instance.  This is used internally when passing instances to a
        /// <see cref="LogEventInterceptor"/> because our logging code reused <see cref="LogEvent"/>
        /// instances to reduce GC pressure.
        /// </summary>
        /// <returns>The cloned instance.</returns>
        public LogEvent Clone()
        {
            Dictionary<string, object> clonedAttributes = null;
            Dictionary<string, object> clonedResources  = null;

            if (this.Attributes != null)
            {
                clonedAttributes = new Dictionary<string, object>();

                foreach (var item in this.Attributes)
                {
                    clonedAttributes.Add(item.Key, item.Value);
                }
            }

            if (this.Resources != null)
            {
                clonedResources = new Dictionary<string, object>();

                foreach (var item in this.Resources)
                {
                    clonedResources.Add(item.Key, item.Value);
                }
            }

            return new LogEvent()
            {
                TsNs           = this.TsNs,
                Severity       = this.Severity,
                Body           = this.Body,
                CategoryName   = this.CategoryName,
                SeverityNumber = this.SeverityNumber,
                Attributes     = clonedAttributes,
                Resources      = clonedResources,
                SpanId         = this.SpanId,
                TraceId        = this.TraceId
            };
        }
    }
}
