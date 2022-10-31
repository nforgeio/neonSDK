//-----------------------------------------------------------------------------
// FILE:	    WorkflowLogger.cs
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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Neon.Cadence;
using Neon.Cadence.Internal;
using Neon.Common;
using Neon.Diagnostics;
using Neon.Tasks;
using static Neon.Cadence.WorkflowBase;

namespace Neon.Cadence
{
    /// <summary>
    /// Custom <see cref="ILogger"/> implementation for a <see cref="Workflow"/>.  This handles
    /// special cases like not logging replayed workflow actions.
    /// </summary>
    internal class WorkflowLogger : ILogger
    {
        private ILogger     baseLogger;
        private Workflow    workflow;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseLogger">Passed as the <see cref="ILogger"/> to use when events are actually logged.</param>
        /// <param name="workflow">The associated workflow.</param>
        public WorkflowLogger(ILogger baseLogger, Workflow workflow)
        {
            Covenant.Requires<ArgumentNullException>(baseLogger != null, nameof(baseLogger));
            Covenant.Requires<ArgumentNullException>(workflow != null, nameof(workflow));

            this.baseLogger = baseLogger;
            this.workflow   = workflow;
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            return baseLogger.BeginScope<TState>(state);
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return baseLogger.IsEnabled(logLevel);
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // Don't log anything when we're not within the context of a workflow execution or we're
            // replaying an action and logging is not enabled for that.

            if (CallContext.Value == WorkflowCallContext.None || (workflow.IsReplaying && !workflow.Client.Settings.LogDuringReplay))
            {
                return;
            }

            // Log the event.

            baseLogger.Log<TState>(logLevel, eventId, state, exception, formatter);
        }
    }
}
