//-----------------------------------------------------------------------------
// FILE:        OtelConfigExtensions.cs
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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Neon.Common;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;

namespace Neon.Service
{
    /// <summary>
    /// Implements extension methods used for configuring <see cref="Neon.Diagnostics"/> related 
    /// exporters and processors.
    /// </summary>
    public static class OtelConfigExtensions
    {
        /// <summary>
        /// Adds a <see cref="LogMetricsProcessor"/> to a <see cref="OpenTelemetryLoggerOptions"/> instance
        /// that counts the number of logged events by <see cref="LogLevel"/> using the <b>neonsdk_log_events_total</b>
        /// metrics counter.
        /// </summary>
        /// <param name="loggerOptions">The <see cref="OpenTelemetryLoggerOptions"/> options to where the exporter will be added.</param>
        /// <returns>The <paramref name="loggerOptions"/> to enable fluent style programming.</returns>
        public static OpenTelemetryLoggerOptions AddLogMetricsProcessor(this OpenTelemetryLoggerOptions loggerOptions)
        {
            Covenant.Requires<ArgumentNullException>(loggerOptions != null, nameof(loggerOptions));

            return loggerOptions.AddProcessor(new LogMetricsProcessor());
        }
    }
}
