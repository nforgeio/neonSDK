//-----------------------------------------------------------------------------
// FILE:	    LogTagNames.cs
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

namespace Neon.Diagnostics
{
    /// <summary>
    /// Defines tag names used to emit log records.
    /// </summary>
    public class LogTagNames
    {
        //---------------------------------------------------------------------
        // These are the names we use for exporting the built-in LogRecord properties.

        /// <summary>
        /// Identifies the message body. 
        /// </summary>
        public const string Body = "{body}";

        /// <summary>
        /// Identifies the event category name.
        /// </summary>
        public const string CategoryName = "categoryName";

        /// <summary>
        /// Identifies a related exception.
        /// </summary>
        public const string Exception = "exception";

        /// <summary>
        /// Identifies the event labels (also know as tags or attributes).
        /// </summary>
        public const string Labels = "labels";

        /// <summary>
        /// Identifies resources related to the event.
        /// </summary>
        public const string Resources = "resources";

        /// <summary>
        /// Identifies the event severity by OpenTelemetry secerity name.
        /// </summary>
        public const string Severity = "severity";

        /// <summary>
        /// Identifies the event severity by OpenTelemetry severity number.
        /// </summary>
        public const string SeverityNumber = "severityNumber";

        /// <summary>
        /// Identifies the current span.
        /// </summary>
        public const string SpanId = "spanid";

        /// <summary>
        /// Identifies the event timetamp formatted as Unix Epoc nanoseconds.
        /// </summary>
        public const string TsNs = "tsNs";

        /// <summary>
        /// Identifies the current trace.
        /// </summary>
        public const string TraceId = "traceid";

        //---------------------------------------------------------------------
        // These are tag names reserved by NeonSDK and other Neon related repositories.

        /// <summary>
        /// <b>bool:</b> Indicates that the log event is related to a transient error.
        /// </summary>
        public const string NeonTransient = "neon.transient";
    }
}
