//-----------------------------------------------------------------------------
// FILE:        TelemetryHub.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Neon.Common;

namespace Neon.Diagnostics
{
    /// <summary>
    /// <para>
    /// Provides a standard global place where libraries and applications can gain access to
    /// the application's <see cref="ActivitySource"/> and <see cref="LoggerFactory"/> for 
    /// recording traces and logs.  Applications that enable tracing and logging and want 
    /// to enable logging and tracing by Neon libraries set <see cref="LoggerFactory"/>
    /// and <see cref="ActivitySource"/> immediately after configuring telemetry using 
    /// the <b>OpenTelemetry</b> and <b>Microsoft.Extensions.Logging</b> APIs.
    /// </para>
    /// <note>
    /// The <b>Neon.Service.NeonService</b> class initializes these properties by default when
    /// used by applications based on this class.
    /// </note>
    /// <para>
    /// <see cref="CreateLogger{T}(LogAttributes, bool, bool)"/>, <see cref="CreateLogger(Type, LogAttributes, bool, bool)"/>,
    /// or <see cref="CreateLogger(string, LogAttributes, bool, bool)"/> are helper methods for obtaining loggers.
    /// </para>
    /// <para>
    /// You can also set the <see cref="Diagnostics.LogAttributes"/> property to attributes you'd like to include
    /// in the loggers returned by the <c>CreateLogger()</c> methods.  This is a handy way to include
    /// a common set of attributes with all logged events.
    /// </para>
    /// <para>
    /// The <see cref="ParseLogLevel(string, LogLevel)"/> utility can be used to parse a log level
    /// string obtained from an environment variable or elsewhere.  This returns the parsed log level
    /// and also sets the <b>Logging__LogLevel__Microsoft</b> environment variable which will be
    /// honored by any created loggers.
    /// </para>
    /// </summary>
    public static class TelemetryHub
    {
        private static long index = 0;

        /// <summary>
        /// Holds the global activity source used by Neon and perhaps other libraries for emitting
        /// traces.  This defaults to <c>null</c> which means that libraries won't emit any
        /// traces by default.  Programs should set this after configuring tracing.
        /// </summary>
        public static ActivitySource ActivitySource { get; set; } = null;

        /// <summary>
        /// Holds the global <see cref="ILoggerFactory"/> used by the Neon and perhaps other libraries
        /// for emitting logs.  This defaults to <c>null</c> which means that libraries won't emit any
        /// logs by default.  Programs should set this after configuring logging.
        /// </summary>
        public static ILoggerFactory LoggerFactory { get; set; } = null;

        /// <summary>
        /// Optionally holds any <see cref="Diagnostics.LogAttributes"/> that will be added to <see cref="ILogger"/>
        /// instances returned by <see cref="CreateLogger(string, LogAttributes, bool, bool)"/>, 
        /// <see cref="CreateLogger(Type, LogAttributes, bool, bool)"/>, or <see cref="CreateLogger{T}(LogAttributes, bool, bool)"/>.
        /// </summary>
        public static LogAttributes LogAttributes { get; set; } = null;

        /// <summary>
        /// Returns an <see cref="ILogger"/> using the fully qualified name of the <typeparamref name="T"/>
        /// type as the logger's category name.
        /// </summary>
        /// <typeparam name="T">Identifies the type whose fully-qualified name is to be used as the logger's category name.</typeparam>
        /// <param name="attributes">Optionally specifies attributes to be included in every event logged.</param>
        /// <param name="noAttributes">Optionally indicates that the <see cref="Diagnostics.LogAttributes"/> <b>should not</b> be added to the logger returned.</param>
        /// <param name="nullLogger">Optionally specifies that a do-nothing logger should be returned.  This defaults to <c>false</c>.</param>
        /// <returns>The <see cref="ILogger"/>.</returns>
        public static ILogger CreateLogger<T>(LogAttributes attributes = null, bool noAttributes = false, bool nullLogger = false)
        {
            return CreateLogger(typeof(T).FullName, attributes, noAttributes, nullLogger);
        }

        /// <summary>
        /// Returns an <see cref="ILogger"/> using the fully qualified name from <paramref name="type"/>.
        /// type as the logger's category name.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attributes">Optionally specifies attributes to be included in every event logged.</param>
        /// <param name="noAttributes">Optionally indicates that the <see cref="Diagnostics.LogAttributes"/> <b>should not</b> be added to the logger returned.</param>
        /// <param name="nullLogger">Optionally specifies that a do-nothing logger should be returned.  This defaults to <c>false</c>.</param>
        /// <returns>The <see cref="ILogger"/>.</returns>
        public static ILogger CreateLogger(Type type, LogAttributes attributes = null, bool noAttributes = false, bool nullLogger = false)
        {
            Covenant.Requires<ArgumentNullException>(type != null, nameof(type));

            return CreateLogger(type.FullName, attributes, noAttributes, nullLogger);
        }

        /// <summary>
        /// Returns an <see cref="ILogger"/> using the category name passed.
        /// </summary>
        /// <param name="categoryName">Specifies the logger's category name.</param>
        /// <param name="attributes">Optionally specifies attributes to be included in every event logged.</param>
        /// <param name="noAttributes">Optionally indicates that the <see cref="Diagnostics.LogAttributes"/> <b>should not</b> be added to the logger returned.</param>
        /// <param name="nullLogger">Optionally specifies that a do-nothing logger should be returned.  This defaults to <c>false</c>.</param>
        /// <returns>The <see cref="ILogger"/>.</returns>
        public static ILogger CreateLogger(string categoryName, LogAttributes attributes = null, bool noAttributes = false, bool nullLogger = false)
        {
            if (nullLogger)
            {
                return NullLogger.Instance;
            }

            categoryName ??= "DEFAULT";

            if (LoggerFactory == null)
            {
                return NullLogger.Instance;
            }
            else
            {
                var logger = LoggerFactory.CreateLogger(categoryName); ;

                if (!noAttributes && attributes != null && attributes.Count > 0)
                {
                    logger = logger.AddAttributes(
                        _attributes =>
                        {
                            foreach (var attributes in _attributes.Attributes)
                            {
                                _attributes.Add(attributes.Key, attributes.Value);
                            }
                        });
                }

                return logger;
            }
        }

        /// <summary>
        /// Parses a <see cref="LogLevel"/> from a string and also sets the 
        /// <c>Logging__LogLevel__Microsoft</c> environment variable which
        /// is honored by any created <see cref="ILogger"/> instances.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="default">The default value to return when <paramref name="input"/> is <c>null</c> or invalid.</param>
        /// <returns>The parsed <see cref="LogLevel"/>.</returns>
        public static LogLevel ParseLogLevel(string input, LogLevel @default = LogLevel.Information)
        {
            var logLevel = @default;

            if (input != null)
            {
                switch (input.ToUpperInvariant())
                {
                    case "CRITICAL":
                    case "FATAL":

                        logLevel = LogLevel.Critical;
                        break;

                    case "ERROR":

                        logLevel = LogLevel.Error;
                        break;

                    case "WARN":    // Backwards compatibility
                    case "WARNING":

                        logLevel = LogLevel.Warning;
                        break;

                    case "INFO":    // Backwards compatibility
                    case "INFORMATION":

                        logLevel = LogLevel.Information;
                        break;

                    case "DEBUG":

                        logLevel = LogLevel.Debug;
                        break;

                    case "TRACE":

                        logLevel = LogLevel.Trace;
                        break;

                    case "NONE":
                    case "UNSPECIFIED":

                        logLevel = LogLevel.None;
                        break;
                }
            }

            Environment.SetEnvironmentVariable("Logging__LogLevel__Microsoft", logLevel.ToString());

            return logLevel;
        }

        /// <summary>
        /// Returns the next logged event index.  This value will be included as the <see cref="LogAttributeNames.NeonIndex"/>
        /// attribute for all events logged by the <see cref="LoggerExtensions"/> methods.
        /// </summary>
        /// <returns>The next index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long GetNextIndex()
        {
            return Interlocked.Increment(ref index);
        }
    }
}
