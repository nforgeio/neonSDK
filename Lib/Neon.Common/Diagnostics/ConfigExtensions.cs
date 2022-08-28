//-----------------------------------------------------------------------------
// FILE:	    ConfigExtensions.cs
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

using Microsoft.Extensions.Logging;

using Neon.Common;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Implements extension methods used for configuring <see cref="Neon.Diagnostics"/> related 
    /// exporters and processors.
    /// </summary>
    public static class ConfigExtensions
    {
        /// <summary>
        /// Adds a <see cref="ConsoleJsonLogExporter"/> to a <see cref="OpenTelemetryLoggerOptions"/> instance
        /// when configuring a OpenTelemetry pipeline.
        /// </summary>
        /// <param name="loggerOptions">The <see cref="OpenTelemetryLoggerOptions"/> options to where the exporter will be added.</param>
        /// <param name="configure">Exporter configuration options.</param>
        /// <returns>The <paramref name="loggerOptions"/> to enable fluent style programming.</returns>
        public static OpenTelemetryLoggerOptions AddConsoleJsonExporter(this OpenTelemetryLoggerOptions loggerOptions, Action<ConsoleJsonLogExporterOptions> configure = null)
        {
            Covenant.Requires<ArgumentNullException>(loggerOptions != null, nameof(loggerOptions));

            var options = new ConsoleJsonLogExporterOptions();

            configure?.Invoke(options);

            return loggerOptions.AddProcessor(new SimpleLogRecordExportProcessor(new ConsoleJsonLogExporter(options)));
        }

        /// <summary>
        /// Adds a <see cref="ConsoleJsonLogExporter"/> to a <see cref="OpenTelemetryLoggerOptions"/> instance
        /// when configuring a OpenTelemetry pipeline.
        /// </summary>
        /// <param name="loggerOptions">The <see cref="OpenTelemetryLoggerOptions"/> options to where the exporter will be added.</param>
        /// <param name="configure">Exporter configuration options.</param>
        /// <returns>The <paramref name="loggerOptions"/> to enable fluent style programming.</returns>
        public static OpenTelemetryLoggerOptions AddLogAsTraceProcessor(this OpenTelemetryLoggerOptions loggerOptions, Action<LogAsTraceProcessorOptions> configure = null)
        {
            Covenant.Requires<ArgumentNullException>(loggerOptions != null, nameof(loggerOptions));

            var options = new LogAsTraceProcessorOptions();

            configure?.Invoke(options);

            return loggerOptions.AddProcessor(new LogAsTraceProcessor(options));
        }

        /// <summary>
        /// Adds a <see cref="LogInterceptProcessor"/> to a <see cref="OpenTelemetryLoggerOptions"/> instance
        /// when configuring a OpenTelemetry pipeline.
        /// </summary>
        /// <param name="loggerOptions">The <see cref="OpenTelemetryLoggerOptions"/> options to where the exporter will be added.</param>
        /// <param name="interceptor">The interceptor delegate that will be called for processed log records.</param>
        /// <returns>The <paramref name="loggerOptions"/> to enable fluent style programming.</returns>
        public static OpenTelemetryLoggerOptions AddLogInterceptProcessor(this OpenTelemetryLoggerOptions loggerOptions, LogRecordInterceptor interceptor)
        {
            Covenant.Requires<ArgumentNullException>(interceptor != null, nameof(interceptor));

            return loggerOptions.AddProcessor(new LogInterceptProcessor(interceptor));
        }
    }
}
