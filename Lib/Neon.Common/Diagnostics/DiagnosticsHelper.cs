//-----------------------------------------------------------------------------
// FILE:	    DiagnosticsHelper.cs
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Neon.Common;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Internal helpers.
    /// </summary>
    internal static class DiagnosticsHelper
    {
        //---------------------------------------------------------------------
        // Local types

        /// <summary>
        /// Used to map .NET log levels to OpenTelemetry severity names and numbers.
        /// </summary>
        public class SeverityInfo
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="name">The standard OpenTelemetry severity name.</param>
            /// <param name="number">The standard OpenTelemetry severity number.</param>
            internal SeverityInfo(string name, int number)
            {
                this.Name    = name;
                this.Number = number;
            }

            /// <summary>
            /// Returns the standard OpenTelemetry severity name.
            /// </summary>
            public string Name { get; private set; }

            /// <summary>
            /// Returns the standard OpenTelementry severity number.
            /// </summary>
            public int Number { get; private set; }
        }

        //---------------------------------------------------------------------
        // Implementation

        private static SeverityInfo[] logLevelToSeverityInfo;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static DiagnosticsHelper()
        {
            // Initialize the map.

            logLevelToSeverityInfo = new SeverityInfo[(int)LogLevel.None + 1];

            logLevelToSeverityInfo[(int)LogLevel.Critical]    = new SeverityInfo("Fatal", (int)SeverityNumber.SEVERITY_NUMBER_FATAL);
            logLevelToSeverityInfo[(int)LogLevel.Error]       = new SeverityInfo("Error", (int)SeverityNumber.SEVERITY_NUMBER_ERROR);
            logLevelToSeverityInfo[(int)LogLevel.Warning]     = new SeverityInfo("Warning", (int)SeverityNumber.SEVERITY_NUMBER_WARN);
            logLevelToSeverityInfo[(int)LogLevel.Information] = new SeverityInfo("Information", (int)SeverityNumber.SEVERITY_NUMBER_INFO);
            logLevelToSeverityInfo[(int)LogLevel.Debug]       = new SeverityInfo("Debug", (int)SeverityNumber.SEVERITY_NUMBER_DEBUG);
            logLevelToSeverityInfo[(int)LogLevel.Trace]       = new SeverityInfo("Trace", (int)SeverityNumber.SEVERITY_NUMBER_TRACE);
            logLevelToSeverityInfo[(int)LogLevel.None]        = new SeverityInfo("Unspecified", (int)SeverityNumber.SEVERITY_NUMBER_UNSPECIFIED);
        }

        /// <summary>
        /// Maps a .NET log level to information about the related standard OpenTelemetry
        /// severity name and number.
        /// </summary>
        /// <param name="logLevel">The .NET log level.</param>
        /// <returns>A <see cref="SeverityInfo"/> with the OpenTelemetry information.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SeverityInfo GetSeverityInfo(LogLevel logLevel)
        {
            return logLevelToSeverityInfo[(int)logLevel];
        }
    }
}
