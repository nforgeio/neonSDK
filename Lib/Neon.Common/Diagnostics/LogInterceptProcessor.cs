//-----------------------------------------------------------------------------
// FILE:	    LogInterceptProcessor.cs
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
    /// Typically used for unit testing log output of components.  This simply calls
    /// a delegate for ever <see cref="LogRecord"/> received by the processor.
    /// </summary>
    public class LogInterceptProcessor : BaseProcessor<LogRecord>
    {
        private LogRecordInterceptor interceptor;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="interceptor">Specifies the delegate to be called for each log record received.</param>
        public LogInterceptProcessor(LogRecordInterceptor interceptor)
        {
            Covenant.Requires<ArgumentNullException>(interceptor != null, nameof(interceptor));

            this.interceptor = interceptor;
        }

        /// <inheritdoc/>
        public override void OnEnd(LogRecord logRecord)
        {
            Covenant.Requires<ArgumentNullException>(logRecord != null, nameof(logRecord));

            interceptor.Invoke(logRecord);

            base.OnEnd(logRecord);
        }
    }
}
