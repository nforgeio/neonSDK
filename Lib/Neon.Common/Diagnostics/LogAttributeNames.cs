//-----------------------------------------------------------------------------
// FILE:        LogAttributeNames.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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
    /// Defines attributes names used when emitting log records.
    /// </summary>
    public class LogAttributeNames
    {
        //---------------------------------------------------------------------
        // These are the names we use for exporting the built-in LogRecord properties.

        /// <summary>
        /// Identifies our internal message body. 
        /// </summary>
        public const string InternalBody = "{Body}";

        /// <summary>
        /// Identifies the MSFT logger implementation's attribute that holds the message format string.
        /// </summary>
        public const string InternalOriginalFormat = "{OriginalFormat}";

        /// <summary>
        /// Identifies the MSFT logger implementation's attribute that holds the category name.
        /// </summary>
        public const string CategoryName = "dotnet.ilogger.category";

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
        // These are attribute names reserved by NeonSDK and other Neon related repositories.

        /// <summary>
        /// <b>bool:</b> Indicates that the log event is related to a transient error.
        /// </summary>
        public const string NeonTransient = "neon.transient";

        /// <summary>
        /// <para>
        /// Indicates the position of the log event in the stream of logs emitted by the 
        /// application.  The first event emitted by the application will have a zero
        /// index, and then this is incremented after every logged event.
        /// </para>
        /// <para>
        /// This attribute is useful for listing events in the order they were actually
        /// logged.  Timestamps often don't have enough resolution to distinguish between
        /// to events logged logged very quickly in sequence.
        /// </para>
        /// </summary>
        public const string NeonIndex = "neon.index";
    }
}
