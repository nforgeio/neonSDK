//-----------------------------------------------------------------------------
// FILE:	    SeverityNumber.cs
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
using System.Runtime.Serialization;

using Microsoft.Extensions.Logging;

namespace Neon.Diagnostics
{
    /// <summary>
    /// <para>
    /// Adapted from the OpenTelemetry standard severity number enumeration to bring
    /// those definitions into <b>Neon.Common</b> and also to add attributes used to
    /// render severity values into their corresponding strings.
    /// </para>
    /// <note>
    /// <see cref="INeonLogger"/> extends the log levels recorded by <see cref="ILogger"/>
    /// by adding additional log levels that map to severity numbers not emitted by 
    /// <see cref="ILogger"/>.  The attributes specified below will render these extended
    /// log levels to the new mappings.
    /// </note>
    /// </summary>
    internal enum SeverityNumber
    {
        [EnumMember(Value="None")]
        SEVERITY_NUMBER_UNSPECIFIED = 0,

        [EnumMember(Value="Trace")]
        SEVERITY_NUMBER_TRACE = 1,

        [EnumMember(Value="Trace2")]
        SEVERITY_NUMBER_TRACE2 = 2,

        [EnumMember(Value="Trace3")]
        SEVERITY_NUMBER_TRACE3 = 3,

        [EnumMember(Value="Trace4")]
        SEVERITY_NUMBER_TRACE4 = 4,

        [EnumMember(Value="Debug")]
        SEVERITY_NUMBER_DEBUG = 5,

        [EnumMember(Value="Transient")]
        SEVERITY_NUMBER_DEBUG2 = 6,

        [EnumMember(Value="Debug3")]
        SEVERITY_NUMBER_DEBUG3 = 7,

        [EnumMember(Value="Debug4")]
        SEVERITY_NUMBER_DEBUG4 = 8,

        [EnumMember(Value="Information")]
        SEVERITY_NUMBER_INFO = 9,

        [EnumMember(Value="Infomation2")]
        SEVERITY_NUMBER_INFO2 = 10,

        [EnumMember(Value="Infomation3")]
        SEVERITY_NUMBER_INFO3 = 11,

        [EnumMember(Value="Infomation4")]
        SEVERITY_NUMBER_INFO4 = 12,

        [EnumMember(Value="Warning")]
        SEVERITY_NUMBER_WARN = 13,

        [EnumMember(Value="Warning2")]
        SEVERITY_NUMBER_WARN2 = 14,

        [EnumMember(Value="Warning3")]
        SEVERITY_NUMBER_WARN3 = 15,

        [EnumMember(Value="Warning4")]
        SEVERITY_NUMBER_WARN4 = 16,

        [EnumMember(Value="Error")]
        SEVERITY_NUMBER_ERROR = 17,

        [EnumMember(Value= "SecurityError")]
        SEVERITY_NUMBER_ERROR2 = 18,

        [EnumMember(Value="Error3")]
        SEVERITY_NUMBER_ERROR3 = 19,

        [EnumMember(Value="Error4")]
        SEVERITY_NUMBER_ERROR4 = 20,

        [EnumMember(Value="Critical")]
        SEVERITY_NUMBER_FATAL = 21,

        [EnumMember(Value="Critical2")]
        SEVERITY_NUMBER_FATAL2 = 22,

        [EnumMember(Value="Critical3")]
        SEVERITY_NUMBER_FATAL3 = 23,

        [EnumMember(Value="Critical4")]
        SEVERITY_NUMBER_FATAL4 = 24,
    }
}