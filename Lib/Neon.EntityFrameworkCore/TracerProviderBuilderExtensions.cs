//-----------------------------------------------------------------------------
// FILE:        TracerProviderBuilderExtensions.cs
// CONTRIBUTOR: Marcus Bowyer
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
using System.Diagnostics;
using System.Reflection;

using OpenTelemetry.Trace;

namespace Neon.EntityFrameworkCore
{
    /// <summary>
    /// <b>Neon.EntityFrameworkCore</b> OpenTelemetry tracing instrumentation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This library publishes its activities via a single <see cref="ActivitySource"/> named
    /// after the assembly (<b>Neon.EntityFrameworkCore</b>).  OpenTelemetry ignores activity
    /// sources that haven't been explicitly subscribed to, so nothing is collected until you
    /// call <see cref="AddNeonEntityFrameworkCore(TracerProviderBuilder)"/> when configuring
    /// your tracer provider.
    /// </para>
    /// <para>
    /// Because the instrumentation is built on <see cref="ActivitySource"/> rather than on the
    /// OpenTelemetry SDK, the cost when nothing is listening is a single null check per call:
    /// no <see cref="Activity"/> is allocated and no tags are computed.  It is safe to leave
    /// the library referenced in projects that don't use OpenTelemetry at all.
    /// </para>
    /// <example>
    /// <code language="csharp">
    /// builder.Services.AddOpenTelemetry()
    ///     .WithTracing(tracing =>
    ///     {
    ///         tracing.AddNeonEntityFrameworkCore()
    ///                .AddNpgsql()                   // Npgsql client spans (optional)
    ///                .AddOtlpExporter();
    ///     });
    /// </code>
    /// </example>
    /// <para>
    /// The activities emitted by this library are documented in the
    /// <a href="https://github.com/nforgeio/neonSDK/blob/master/Lib/Neon.EntityFrameworkCore/README.md">README</a>.
    /// </para>
    /// </remarks>
    public static class TracerProviderBuilderExtensions
    {
        /// <summary>
        /// The assembly name.
        /// </summary>
        internal static readonly AssemblyName AssemblyName = TraceContext.AssemblyName;

        /// <summary>
        /// The name of the <see cref="ActivitySource"/> used by this library.  This is the value
        /// you'd pass to <c>TracerProviderBuilder.AddSource()</c> if you'd rather not reference
        /// <see cref="AddNeonEntityFrameworkCore(TracerProviderBuilder)"/>.
        /// </summary>
        public static readonly string ActivitySourceName = TraceContext.ActivitySourceName;

        /// <summary>
        /// The version reported by the <see cref="ActivitySource"/>.
        /// </summary>
        internal static readonly Version Version = TraceContext.Version;

        /// <summary>
        /// Adds the <b>Neon.EntityFrameworkCore</b> activity source to the tracing pipeline so that
        /// the activities emitted by this library are collected and exported.
        /// </summary>
        /// <param name="builder">The <see cref="TracerProviderBuilder"/> being configured.</param>
        /// <returns>The <paramref name="builder"/> to allow fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
        /// <remarks>
        /// <note>
        /// This subscribes only to the activities produced by <b>this</b> library.  It does not
        /// enable Entity Framework Core's own instrumentation, nor the database driver's.  For a
        /// complete picture of a request you'll typically also want <c>AddEntityFrameworkCoreInstrumentation()</c>
        /// (from <b>OpenTelemetry.Instrumentation.EntityFrameworkCore</b>) and/or <c>AddNpgsql()</c>
        /// (from <b>Npgsql.OpenTelemetry</b>).
        /// </note>
        /// </remarks>
        public static TracerProviderBuilder AddNeonEntityFrameworkCore(
            this TracerProviderBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));

            builder.AddSource(ActivitySourceName);

            return builder;
        }
    }
}