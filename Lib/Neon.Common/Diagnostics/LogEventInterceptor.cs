//-----------------------------------------------------------------------------
// FILE:	    LogEventInterceptor.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
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

namespace Neon.Diagnostics
{
    /// <summary>
    /// Use this delegate for filtering, inspecting, or modifying log events immediately
    /// before they are emitted by <see cref="ConsoleJsonLogExporter"/> or one of the
    /// other log exporters in the <see cref="Neon.Diagnostics"/> namespace.
    /// </summary>
    /// <param name="logEvent">The log event.</param>
    /// <remarks>
    /// <note>
    /// <b>IMPORTANT:</b> <see cref="LogEvent"/> record instances are reused by the Neon telemetry
    /// code, so you'll need to call <see cref="LogEvent.Clone()"/> when you're using the interceptor
    /// to collected logged events for later analysis (i.e. when unit testing).
    /// </note>
    /// <para>
    /// You can configure an interceptor in <see cref="ConsoleJsonLogExporterOptions"/> or
    /// the options for other log exporters in the <see cref="Neon.Diagnostics"/> namespace.
    /// This is useful for unit tests that verify that code is logging events correctly.
    /// </para>
    /// <para>
    /// You can also use this to modify the event in other ways like modifing the body,
    /// timestamp, labels, resources, etc. before the event is emitted.
    /// </para>
    /// </remarks>
    public delegate void LogEventInterceptor(LogEvent logEvent);
}
