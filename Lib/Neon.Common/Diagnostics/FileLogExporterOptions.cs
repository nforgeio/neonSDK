//-----------------------------------------------------------------------------
// FILE:	    FileLogExporterOptions.cs
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
    /// Specifies the options used to configure a <see cref="ConsoleJsonLogExporter"/>.
    /// </summary>
    public class FileLogExporterOptions
    {
        /// <summary>
        /// Constructs an instance with reasonable settings.
        /// </summary>
        public FileLogExporterOptions()
        {
        }

        /// <summary>
        /// Specifies the folder where the log file will be written.  This must be specified.
        /// </summary>
        public string LogFolder { get; set; }

        /// <summary>
        /// Specifies the name to use for the log file.  This must be specified.
        /// </summary>
        public string LogFileName { get; set; }

        /// <summary>
        /// Specifies the export format.  This defaults to <see cref="FileLogExporterFormat.Human"/>.
        /// </summary>
        public FileLogExporterFormat Format { get; set; } = FileLogExporterFormat.Human;

        /// <summary>
        /// Used to intercept log events just before they are emitted by the exporter.  You can
        /// use this for implementing logging related unit tests or modifying other event properties 
        /// like the timestamp, labels, tags, etc.
        /// </summary>
        public LogEventInterceptor LogEventInterceptor { get; set; } = null;
    }
}
