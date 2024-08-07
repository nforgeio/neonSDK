//-----------------------------------------------------------------------------
// FILE:        NullLogger.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Implements a <b>do-nothing</b> <see cref="ILogger"/>.
    /// </summary>
    public class NullLogger : ILogger
    {
        //---------------------------------------------------------------------
        // Private types

        private sealed class NullScope : IDisposable
        {
            public void Dispose()
            {
            }
        }

        //---------------------------------------------------------------------
        // Static members.

        /// <summary>
        /// Returns a <see cref="NullLogger"/> instance.
        /// </summary>
        public static ILogger Instance { get; private set; } = new NullLogger();

        //---------------------------------------------------------------------
        // Implementation

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            return new NullScope();
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel) => false;

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }
    }
}
