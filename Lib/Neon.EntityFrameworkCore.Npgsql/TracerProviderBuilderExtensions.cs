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

namespace Neon.EntityFrameworkCore.Npgsql
{
    /// <summary>
    /// <b>Neon.EntityFrameworkCore.Npgsql</b> OpenTelemetry tracing instrumentation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This package publishes its activities via its own <see cref="ActivitySource"/>, separate from
    /// the one used by <b>Neon.EntityFrameworkCore</b>, following the OpenTelemetry convention of one
    /// source per instrumentation library.  That means you subscribe to each package you actually
    /// use, and each reports its own version to the backend.
    /// </para>
    /// <example>
    /// <code language="csharp">
    /// builder.Services.AddOpenTelemetry()
    ///     .WithTracing(tracing =>
    ///     {
    ///         tracing.AddNeonEntityFrameworkCore()          // Neon.EntityFrameworkCore
    ///                .AddNeonEntityFrameworkCoreNpgsql()    // this package
    ///                .AddNpgsql()                           // Npgsql client spans (optional)
    ///                .AddOtlpExporter();
    ///     });
    /// </code>
    /// </example>
    /// <para>
    /// Because migrations normally run in a short lived job rather than in your service, remember to
    /// configure the tracer provider in the migrator too — otherwise the lock acquisition spans this
    /// package exists to give you are never collected.  Short lived processes should also make sure
    /// the provider is disposed (or flushed) before exit so the final batch is exported.
    /// </para>
    /// </remarks>
    public static class TracerProviderBuilderExtensions
    {
        /// <summary>
        /// The assembly name.
        /// </summary>
        internal static readonly AssemblyName AssemblyName = TraceContext.AssemblyName;

        /// <summary>
        /// The name of the <see cref="ActivitySource"/> used by this package.  This is the value
        /// you'd pass to <c>TracerProviderBuilder.AddSource()</c> if you'd rather not reference
        /// <see cref="AddNeonEntityFrameworkCoreNpgsql(TracerProviderBuilder)"/>.
        /// </summary>
        public static readonly string ActivitySourceName = TraceContext.ActivitySourceName;

        /// <summary>
        /// The version reported by the <see cref="ActivitySource"/>.
        /// </summary>
        internal static readonly Version Version = TraceContext.Version;

        /// <summary>
        /// Adds the <b>Neon.EntityFrameworkCore.Npgsql</b> activity source to the tracing pipeline so
        /// that the activities emitted by this package are collected and exported.
        /// </summary>
        /// <param name="builder">The <see cref="TracerProviderBuilder"/> being configured.</param>
        /// <returns>The <paramref name="builder"/> to allow fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
        public static TracerProviderBuilder AddNeonEntityFrameworkCoreNpgsql(
            this TracerProviderBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));

            builder.AddSource(ActivitySourceName);

            return builder;
        }
    }
}