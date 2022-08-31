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
    ///     <term><see cref="LoggerExtensions"/></term>
    ///     <description>
    ///     <para>
    ///     This namespace defines several <b><i>extended</i></b> <see cref="ILogger"/> extension methods
    ///     whose names end with <b>"Ex"</b>, like <see cref="LoggerExtensions.LogInformationEx(ILogger, Exception, string, Action{LogAttributes})"/>.
    ///     </para>
    ///     <para>
    ///     We recommend that developers consider switch to using our extended logging methods
    ///     from the stock .NET extensions <see cref="Microsoft.Extensions.Logging.LoggerExtensions"/>.
    ///     Not only do the NeonSDK <see cref="Neon.Diagnostics.LoggerExtensions"/> interoperate
    ///     with the <see cref="AttributeLogger"/>, we believe our extensions are easier to use,
    ///     especially when specifying attributes.  We also have overrides that make it efficient
    ///     to use string interpolation for generating log messages.
    ///     </para>
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b><see cref="AttributeLogger"/></b></term>
    ///     <description>
    ///     This is an extended <see cref="ILogger"/> that may include tags that will be automatically
    ///     added to all events submitted to the logger.  Typically, you'll use the <see cref="LoggerExtensions.AddAttributes(ILogger, Action{LogAttributes})"/>
    ///     method to construct one of these that wraps another logger.
    ///     </description>
    /// </item>
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
    ///     properties are initialized.  Your applications will need set <see cref="TelemetryHub.LoggerFactory"/>
    ///     and <see cref="TelemetryHub.ActivitySource"/> immediately after configuring telemetry using the 
    ///     <b>OpenTelemetry</b> and <b>Microsoft.Extensions.Logging</b> APIs to enable logging by Neon libraries.
    ///     </note>
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><see cref="LogAsTraceProcessor"/></term>
    ///     <description>
    ///     This processor can be used to forward logged events to the current trace span, if there is
    ///     one.  I understand that OpenTelemetry may convere log and trace events at somepoint in the
    ///     future, but in the meantime, adding a <see cref="LogAsTraceProcessor"/> to your OpenTelemetry
    ///     log pipeline is a reasonable alternative.
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><see cref="Covenant"/></term>
    ///     <description>
    ///     This is basically a clone of <see cref="System.Diagnostics.Contracts"/>.  We started using
    ///     <see cref="System.Diagnostics.Contracts"/> years ago and liked it, but unforunately I ran
    ///     into trouble (with building projects, I believe).  So I cloned the API and put it in the
    ///     <see cref="System.Diagnostics"/> namespace so it would be easy access and easier to revert.
    ///     </description>
    /// </item>
    /// </list>
    /// </remarks>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
}
