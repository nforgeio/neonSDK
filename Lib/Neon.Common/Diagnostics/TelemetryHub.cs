//-----------------------------------------------------------------------------
// FILE:	    TelemetryHub.cs
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
    /// to enable logging and tracing by Neon libraries will need to call <see cref="Initialize(ILoggerFactory, ActivitySource)"/>
    /// immediately after configuring telemetry using the <b>OpenTelemetry</b> and <b>Microsoft.Extensions.Logging</b>
    /// APIs.
    /// </para>
    /// <note>
    /// The <b>Neon.Service.NeonService</b> class initializes these properties by default when
    /// used by programs.
    /// </note>
    /// <para>
    /// <see cref="CreateLogger{T}(LogTags, bool)"/> and <see cref="CreateLogger(string, LogTags, bool)"/>
    /// are helper methods for obtaining loggers.
    /// </para>
    /// <para>
    /// The <see cref="ParseLogLevel(string, LogLevel)"/> utility can be used to parse a log level
    /// string obtained from an environment variable or elsewhere.
    /// </para>
    /// </summary>
    public static class TelemetryHub
    {
        private static bool isInitialized = false;

        /// <summary>
        /// Holds the global activity source used by Neon and perhaps other libraries for emitting
        /// traces.  This defaults to <c>null</c> which means that libraries won't emit any
        /// traces by default.  Programs should set this after configuring tracing.
        /// </summary>
        public static ActivitySource ActivitySource { get; private set; } = null;

        /// <summary>
        /// Holds the global <see cref="ILoggerFactory"/> used by the Neon and perhaps other libraries
        /// for emitting logs.  This defaults to <c>null</c> which means that libraries won't emit any
        /// logs by default.  Programs should set this after configuring logging.
        /// </summary>
        public static ILoggerFactory LoggerFactory { get; private set; } = null;

        /// <summary>
        /// <para>
        /// Initializes <see cref="TelemetryHub"/> by setting the <see cref="ActivitySource"/> and/or <see cref="LoggerFactory"/>
        /// properties.  These enable tracing and logging for neonSDK and other Neon related libraries.  You should call this 
        /// immediately after configuring telemetry using the <b>OpenTelemetry</b> and <b>Microsoft.Extensions.Logging</b> APIs
        /// to enable logging from Neon libraries.
        /// </para>
        /// <note>
        /// You may only call this once per application process.
        /// </note>
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="activitySource"></param>
        /// <exception cref="InvalidOperationException">Thrown when this is call multiple times for a process.</exception>
        public static void Initialize(ILoggerFactory loggerFactory = null, ActivitySource activitySource = null)
        {
            if (isInitialized)
            {
                throw new InvalidOperationException($"[{nameof(TelemetryHub)}.{nameof(Initialize)}] cannot be called more than once per process.");
            }

            isInitialized = true;

            LoggerFactory  = loggerFactory;
            ActivitySource = activitySource;
        }

        /// <summary>
        /// Returns an <see cref="ILogger"/> using the fully qualified name of the <typeparamref name="T"/>
        /// type as the logger's category name.
        /// </summary>
        /// <typeparam name="T">Identifies the type whose fully-qualified name is to be used as the logger's category name.</typeparam>
        /// <param name="tags">Optionally specifies tags to be included in every event logged.</param>
        /// <param name="nullLogger">Optionally specifies that a do-nothing logger should be returned.  This defaults to <c>false</c>.</param>
        /// <returns>The <see cref="ILogger"/>.</returns>
        public static ILogger CreateLogger<T>(LogTags tags = null, bool nullLogger = false)
        {
            return CreateLogger(typeof(T).FullName, tags, nullLogger);
        }

        /// <summary>
        /// Returns an <see cref="ILogger"/> using the category name passed.
        /// </summary>
        /// <param name="categoryName">Specifies the logger's category name.</param>
        /// <param name="tags">Optionally specifies tags to be included in every event logged.</param>
        /// <param name="nullLogger">Optionally specifies that a do-nothing logger should be returned.  This defaults to <c>false</c>.</param>
        /// <returns>The <see cref="ILogger"/>.</returns>
        public static ILogger CreateLogger(string categoryName, LogTags tags = null, bool nullLogger = false)
        {
            if (nullLogger)
            {
                return new NullLogger();
            }

            categoryName ??= "DEFAULT";

            if (LoggerFactory == null)
            {
                return new NullLogger();
            }
            else
            {
                var logger = LoggerFactory.CreateLogger(categoryName); ;

                if (tags != null && tags.Count > 0)
                {
                    logger = logger.CreateLoggerWithTags(tags);
                }

                return logger;
            }
        }

        /// <summary>
        /// Parses a <see cref="LogLevel"/> from a string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="default">The default value to return when <paramref name="input"/> is <c>null</c> or invalid.</param>
        /// <returns></returns>
        public static LogLevel ParseLogLevel(string input, LogLevel @default = LogLevel.Information)
        {
            if (input == null)
            {
                return @default;
            }

            switch (input.ToUpperInvariant())
            {
                case "CRITICAL":

                    return LogLevel.Critical;

                case "ERROR":

                    return LogLevel.Error;

                case "WARN":    // Backwards compatibility
                case "WARNING":

                    return LogLevel.Warning;

                case "INFO":    // Backwards compatibility
                case "INFORMATION":

                    return LogLevel.Information; ;

                case "DEBUG":

                    return LogLevel.Debug;

                case "TRACE":

                    return LogLevel.Trace;

                case "NONE":

                    return LogLevel.None;

                default:

                    return @default;
            }
        }
    }
}
