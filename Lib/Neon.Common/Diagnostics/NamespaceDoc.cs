//-----------------------------------------------------------------------------
// FILE:	    NamespaceDoc.cs
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
using System.Diagnostics.Contracts;
using System.Reflection;

using Microsoft.Extensions.Logging;

using Neon.Common;

namespace Neon.Diagnostics
{
    /// <summary>
    /// This namespace includes the common logging code used throughout Neon applications and libraries.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Neon.Common</b> nuget releases versions 3.0+ are now fully integrated with OpenTelemetry
    /// and the standard Microsoft logging extensions.  Before v3.0, we included a custom logging 
    /// solution that was lightly based on <see cref="ILogger"/> and was configured in a completely
    /// non-standard way.
    /// </para>
    /// <para>
    /// We now support OpenTelemetry for logging as well as for tracing and metrics and this can be
    /// configured using the standard <b>OpenTelemetry</b> and <b>Microsoft.Logging.Extensions</b> APIs.
    /// </para>
    /// <para>
    /// We've also included some useful types and extension methods.
    /// </para>
    /// <list type="table">
    /// <item>
    ///     <term><see cref="TelemetryHub"/></term>
    ///     <description>
    ///     <para>
    ///     This <c>static</c> class can be used to hold global state such as the 
    ///     <see cref="TelemetryHub.ActivitySource"/> and <see cref="TelemetryHub.LoggerFactory"/>
    ///     and also provides easy-to-use methods for obtaining an <see cref="ILogger"/> or
    ///     parsing loglevel strings in a backwards compatible way.
    ///     </para>
    ///     <note>
    ///     <b>IMPORTANT:</b> <b>NeonSDK</b> and other Neon libraries won't emit logs or traces unless the
    ///     <see cref="TelemetryHub.LoggerFactory"/> and <see cref="TelemetryHub.ActivitySource"/>
    ///     properties are initialized.  Your applications will need call <see cref="TelemetryHub.Initialize(ILoggerFactory, System.Diagnostics.ActivitySource)"/>
    ///     immediately after configuring telemetry using the <b>OpenTelemetry</b> and <b>Microsoft.Extensions.Logging</b> APIs
    ///     to enable logging from Neon libraries.
    ///     </note>
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b></b></term>
    ///     <description>
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b></b></term>
    ///     <description>
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b></b></term>
    ///     <description>
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b></b></term>
    ///     <description>
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b></b></term>
    ///     <description>
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b></b></term>
    ///     <description>
    ///     </description>
    /// </item>
    /// </list>
    /// </remarks>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
}
