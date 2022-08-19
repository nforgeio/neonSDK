//-----------------------------------------------------------------------------
// FILE:	    LoggerCreatorDelegate.cs
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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Neon.Common;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Defines the <see cref="ITelemetryHub.LoggerCreator"/> function used to return custom
    /// logger implementations.
    /// </summary>
    /// <param name="telemetryHub">The parent telemetry hub.</param>
    /// <param name="categoryName">
    /// Optionally identifies the event source category.  This is typically used 
    /// for identifying the event source.
    /// </param>
    /// <param name="writer">
    /// Optionally specifies a target <see cref="TextWriter"/>.  This can be useful for 
    /// redirecting the ourput of a text logger to a file or somewhere else.  This parameter
    /// may be ignored for non-text based loggers or for other logger specific reasons.
    /// </param>
    /// <param name="logFilter">
    /// Optionally specifies a filter predicate that overrides the parent <see cref="ITelemetryHub"/> filter
    /// (if any) used for filtering log entries.  This examines the <see cref="LogEvent"/> and returns <c>true</c>
    /// if the event should be logged or <c>false</c> when it is to be ignored.  All events will be logged when
    /// this is <c>null</c>.
    /// </param>
    /// <param name="isLogEnabledFunc">
    /// Optionally specifies a function that will be called at runtime to determine whether event
    /// logging is actually enabled.  This overrides the parent <see cref="ITelemetryHub"/> function
    /// if any.  Events will be logged for <c>null</c> functions.
    /// </param>
    /// <returns>The <see cref="INeonLogger"/> instance.</returns>
    public delegate INeonLogger LoggerCreatorDelegate(TelemetryHub telemetryHub, string categoryName, TextWriter writer, Func<LogEvent, bool> logFilter, Func<bool> isLogEnabledFunc);
}