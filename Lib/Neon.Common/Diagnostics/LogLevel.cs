//-----------------------------------------------------------------------------
// FILE:	    LogLevel.cs
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
using System.Text;
using System.Threading.Tasks;

namespace Neon.Diagnostics
{
    /// <summary>
    /// <para>
    /// Enumerates the possible log levels.  Note that the relative ordinal values of these
    /// definitions are used when deciding to log an event when a specific <see cref="LogLevel"/>
    /// is set.  Only events with log levels greater than or equal to the current level will be logged.
    /// </para>
    /// <para>
    /// The ordinal values map to the OpenTelemetry defined severity numbers as described 
    /// <a href="https://opentelemetry.io/docs/reference/specification/logs/data-model/#displaying-severity">here</a>.
    /// This gives OpenTelemetry a fighting chance to convert our log level values into something
    /// that makes sense for other logging systems.
    /// </para>
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Logging is disabled.
        /// </summary>
        None = 0,

        /// <summary>
        /// A critical or fatal error has been detected.  These errors indicate that
        /// a very serious failure has occurred that may have crashed the program or
        /// at least seriousoly impacts its functioning.
        /// </summary>
        Critical = 21,  // SeverityNumber.FATAL

        /// <summary>
        /// A security related error has occurred.  Errors indicate a problem that may be
        /// transient, be recovered from, or are perhaps more serious.
        /// </summary>
        SError = 18,    // SeverityNumber.ERROR2

        /// <summary>
        /// An error has been detected.  Errors indicate a problem that may be
        /// transient, be recovered from, or are perhaps more serious.
        /// </summary>
        Error = 17,     // SeverityNumber.ERROR

        /// <summary>
        /// An unusual condition has been detected that may ultimately lead to an error.
        /// </summary>
        Warn = 13,      // SeverityNumber.WARN

        /// <summary>
        /// Describes a non-error security operation or condition, such as a 
        /// a successful login or authentication.
        /// </summary>
        SInfo = 10,     // SeverityNumber.INFO2

        /// <summary>
        /// Describes a normal operation or condition.
        /// </summary>
        Info = 9,       // SeverityNumber.INFO

        /// <summary>
        /// Describes a transient error, typically logged by a <see cref="Neon.Retry.IRetryPolicy"/>
        /// implementations.
        /// </summary>
        Transient = 6,  // SeverityNumber.DEBUG2

        /// <summary>
        /// Describes detailed debug or diagnostic information.
        /// </summary>
        Debug = 5,      // SeverityNumber.DEBUG

        /// <summary>
        /// Describes <b>very</b> detailed debug or diagnostic information.
        /// </summary>
        Trace = 1       // SeverityNumber.TRACE
    }
}
