//-----------------------------------------------------------------------------
// FILE:	    TextLogger.cs
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
using System.Diagnostics.Contracts;
using System.IO;

using Microsoft.Extensions.Logging;
using Neon.Common;
using Prometheus;

namespace Neon.Diagnostics
{
    /// <summary>
    /// A general purpose implementation of <see cref="INeonLogger"/> and <see cref="ILogger"/> that
    /// logs to STDERR by default, which is typical for container and Kubernetes applications.  The
    /// output can also be directed to a custom <see cref="TextWriter"/>.
    /// </summary>
    public class TextLogger : INeonLogger, ILogger
    {
        //---------------------------------------------------------------------
        // Static members

        private static readonly Counter LogEventCountByLevel = Metrics.CreateCounter(NeonHelper.NeonMetricsPrefix + "log_events_total", "Number of logged events.", "level");

        //---------------------------------------------------------------------
        // Instance members

        private string                  categoryName;
        private bool                    infoAsDebug;
        private TextWriter              writer;
        private LogTags                 tags;
        private Func<LogEvent, bool>    logFilter;
        private Func<bool>              isLogEnabledFunc;

        /// <inheritdoc/>
        public bool IsLogTraceEnabled => TelemetryHub.Default.LogLevel <= LogLevel.Trace;

        /// <inheritdoc/>
        public bool IsLogDebugEnabled => TelemetryHub.Default.LogLevel <= LogLevel.Debug;

        /// <inheritdoc/>
        public bool IsLogErrorEnabled => TelemetryHub.Default.LogLevel <= LogLevel.Error;

        /// <inheritdoc/>
        public bool IsLogCriticalEnabled => TelemetryHub.Default.LogLevel <= LogLevel.Critical;

        /// <inheritdoc/>
        public bool IsLogInformationEnabled => TelemetryHub.Default.LogLevel <= LogLevel.Information;

        /// <inheritdoc/>
        public bool IsLogWarningEnabled => TelemetryHub.Default.LogLevel <= LogLevel.Warning;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="categoryName">
        /// Optionally identifies the event source category.  This is typically used 
        /// for identifying the event source.
        /// </param>
        /// <param name="tags">
        /// Optionally specifies tags to be included in every event logged by the logger returned.  This may
        /// be passed as <c>null</c>.
        /// </param>
        /// <param name="logFilter">
        /// Optionally specifies a filter predicate to be used for filtering log entries.  This examines
        /// the <see cref="LogEvent"/> and returns <c>true</c> if the event should be logged or <c>false</c>
        /// when it is to be ignored.  All events will be logged when this is <c>null</c>.
        /// </param>
        /// <param name="isLogEnabledFunc">
        /// Optionally specifies a function that will be called at runtime to
        /// determine whether to event logging is actually enabled.  This defaults
        /// to <c>null</c> which will always log events.
        /// </param>
        public TextLogger(
            string                  categoryName     = null,
            LogTags                 tags             = null,
            Func<LogEvent, bool>    logFilter        = null,
            Func<bool>              isLogEnabledFunc = null)
        {
            this.categoryName     = categoryName;
            this.writer           = writer ?? Console.Error;
            this.tags             = tags;
            this.logFilter        = logFilter;
            this.isLogEnabledFunc = isLogEnabledFunc;

            // $hack(jefflill):
            //
            // ASP.NET is super noisy, logging three or four <b>INFO</b> events per request.  There
            // doesn't appear to an easy way to change this behavior, I'd really like to recategorize
            // these as <b>DEBUG</b> to reduce pressure on the logs.
            //
            // We're going to assume that ASP.NET related loggers are always
            // prefixed by: [Microsoft.AspNetCore]

            this.infoAsDebug = categoryName != null && categoryName.StartsWith("Microsoft.AspNetCore.");
        }

        /// <inheritdoc/>
        public bool IsLogLevelEnabled(LogLevel logLevel)
        {
            // Verify that logging isn't temporarily disabled.

            if (isLogEnabledFunc != null && !isLogEnabledFunc())
            {
                return false;
            }

            // Map into Neon log levels.

            switch (logLevel)
            {
                case LogLevel.None:

                    return false;

                case LogLevel.Critical:

                    return IsLogCriticalEnabled;

                case LogLevel.Error:

                    return IsLogErrorEnabled;

                case LogLevel.Warning:

                    return IsLogWarningEnabled;

                case LogLevel.Information:

                    return IsLogInformationEnabled;

                case LogLevel.Debug:

                    return IsLogDebugEnabled;

                case LogLevel.Trace:

                    return IsLogTraceEnabled;

                default:

                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Normalizes a log message by escaping any backslashes and replacing any line
        /// endings with "\n".  This converts multi-line message to a single line.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The normalized message.</returns>
        private static string Normalize(string message)
        {
            if (message == null)
            {
                return string.Empty;
            }

            message = message.Replace("\\", "\\\\");
            message = message.Replace("\r\n", "\\n");
            message = message.Replace("\n", "\\n");

            return message;
        }

        /// <summary>
        /// Logs an event.
        /// </summary>
        /// <param name="logLevel">The event level.</param>
        /// <param name="message">The event message.</param>
        /// <param name="e">Optionally passed as a related exception.</param>
        /// <param name="tags">Optionally specifies tags to be added to the event.</param>
        /// <param name="tagGetter">Optionally specifies a function that returns tags to be added to the event.</param>
        private void Log(LogLevel logLevel, string message, Exception e = null, LogTags tags = null, Func<LogTags> tagGetter = null)
        {
            // Increment the metrics counter for the event type.  Note that we're
            // going to increment the count even when logging for the level is
            // disabled.  This will help devops know when there might be issues
            // they may need to investigate by changing the log level.

            switch (logLevel)
            {
                case LogLevel.Critical:

                    LogEventCountByLevel.WithLabels("critical").Inc();
                    break;

                case LogLevel.Debug:

                    LogEventCountByLevel.WithLabels("debug").Inc();
                    break;

                case LogLevel.Error:

                    LogEventCountByLevel.WithLabels("error").Inc();
                    break;

                case LogLevel.Information:

                    LogEventCountByLevel.WithLabels("info").Inc();
                    break;

                case LogLevel.None:

                    break;

                case LogLevel.Trace:

                    LogEventCountByLevel.WithLabels("trace").Inc();
                    break;

                case LogLevel.Warning:

                    LogEventCountByLevel.WithLabels("warn").Inc();
                    break;

                default:

                    throw new NotImplementedException();
            }

            if (infoAsDebug && logLevel == LogLevel.Information)
            {
                if (!IsLogDebugEnabled)
                {
                    return;
                }

                logLevel = LogLevel.Debug;
            }

            if (logFilter != null)
            {
                // $todo(jefflill): Replace this with LogRecord???

                var logEvent =
                    new LogEvent(
                        categoryName: this.categoryName,
                        index:        0,                  // We don't set this when filtering
                        timestamp:    DateTime.UtcNow,
                        logLevel:     logLevel,
                        body:         message,
                        tags:         null,
                        e:            e);

                if (!logFilter(logEvent))
                {
                    // Ignore filtered events.

                    return;
                }
            }

            var level = NeonHelper.EnumToString(logLevel);

            message = Normalize(message);

            var version = $" [version:{TelemetryHub.Default.ActivitySource.Version ?? String.Empty}]";

            var categoryName = string.Empty;

            if (!string.IsNullOrEmpty(this.categoryName))
            {
                categoryName = $" [categoryName:{this.categoryName}]";
            }

            var index = string.Empty;

            if (TelemetryHub.Default.EmitIndex)
            {
                index = $" [index:{TelemetryHub.Default.GetNextEventIndex()}]";
            }

            if (TelemetryHub.Default.EmitTimestamp)
            {
                var timestamp = DateTime.UtcNow.ToString(NeonHelper.DateFormatTZ);

                writer.WriteLine($"[{timestamp}] [{level}]{version}{categoryName}{index} {message}");
            }
            else
            {
                writer.WriteLine($"[{level}]{version}{categoryName}{index} {message}");
            }
        }

        /// <inheritdoc/>
        public void LogDebug(string message, LogTags tags = null, Func<LogTags> tagGetter = null)
        {
            if (IsLogDebugEnabled)
            {
                try
                {
                    Log(LogLevel.Debug, message, tags: tags, tagGetter: tagGetter);
                }
                catch
                {
                    // Doesn't make sense to handle this.
                }
            }
        }

        /// <inheritdoc/>
        public void LogDebug(string message, Exception e, LogTags tags = null, Func<LogTags> tagGetter = null)
        {
            Covenant.Requires<ArgumentNullException>(e != null, nameof(e));

            if (IsLogDebugEnabled)
            {
                try
                {
                    if (message != null)
                    {
                        Log(LogLevel.Debug, $"{message} {NeonHelper.ExceptionError(e, stackTrace: true)}", e: e, tags: tags, tagGetter: tagGetter);
                    }
                    else
                    {
                        Log(LogLevel.Debug, $"{NeonHelper.ExceptionError(e, stackTrace: true)}", e: e, tags: tags, tagGetter: tagGetter);
                    }
                }
                catch
                {
                    // Doesn't make sense to handle this.
                }
            }
        }

        /// <inheritdoc/>
        public void LogTrace(string message, LogTags tags = null, Func<LogTags> tagGetter = null)
        {
            if (IsLogDebugEnabled)
            {
                try
                {
                    Log(LogLevel.Trace, message, tags: tags, tagGetter: tagGetter);
                }
                catch
                {
                    // Doesn't make sense to handle this.
                }
            }
        }

        /// <inheritdoc/>
        public void LogTrace(string message, Exception e, LogTags tags = null, Func<LogTags> tagGetter = null)
        {
            Covenant.Requires<ArgumentNullException>(e != null, nameof(e));

            if (IsLogDebugEnabled)
            {
                try
                {
                    if (message != null)
                    {
                        Log(LogLevel.Trace, $"{message} {NeonHelper.ExceptionError(e, stackTrace: true)}", e: e, tags: tags, tagGetter: tagGetter);
                    }
                    else
                    {
                        Log(LogLevel.Trace, $"{NeonHelper.ExceptionError(e, stackTrace: true)}", e: e, tags: tags, tagGetter: tagGetter);
                    }
                }
                catch
                {
                    // Doesn't make sense to handle this.
                }
            }
        }

        /// <inheritdoc/>
        public void LogError(string message, LogTags tags = null, Func<LogTags> tagGetter = null)
        {
            if (IsLogErrorEnabled)
            {
                try
                {
                    Log(LogLevel.Error, message, tags: tags, tagGetter: tagGetter);
                }
                catch
                {
                    // Doesn't make sense to handle this.
                }
            }
        }

        /// <inheritdoc/>
        public void LogError(string message, Exception e, LogTags tags = null, Func<LogTags> tagGetter = null)
        {
            Covenant.Requires<ArgumentNullException>(e != null, nameof(e));

            if (IsLogErrorEnabled)
            {
                try
                {
                    if (message != null)
                    {
                        Log(LogLevel.Error, $"{message} {NeonHelper.ExceptionError(e, stackTrace: true)}", e: e, tags: tags, tagGetter: tagGetter);
                    }
                    else
                    {
                        Log(LogLevel.Error, $"{NeonHelper.ExceptionError(e, stackTrace: true)}", e: e, tags: tags, tagGetter: tagGetter);
                    }
                }
                catch
                {
                    // Doesn't make sense to handle this.
                }
            }
        }

        /// <inheritdoc/>
        public void LogCritical(string message, LogTags tags = null, Func<LogTags> tagGetter = null)
        {
            if (IsLogCriticalEnabled)
            {
                try
                {
                    Log(LogLevel.Critical, message, tags: tags, tagGetter: tagGetter);
                }
                catch
                {
                    // Doesn't make sense to handle this.
                }
            }
        }

        /// <inheritdoc/>
        public void LogCritical(string message, Exception e, LogTags tags = null, Func<LogTags> tagGetter = null)
        {
            Covenant.Requires<ArgumentNullException>(e != null, nameof(e));

            if (IsLogCriticalEnabled)
            {
                try
                {
                    if (message != null)
                    {
                        Log(LogLevel.Critical, $"{message} {NeonHelper.ExceptionError(e, stackTrace: true)}", e: e, tags: tags, tagGetter: tagGetter);
                    }
                    else
                    {
                        Log(LogLevel.Critical, $"{NeonHelper.ExceptionError(e, stackTrace: true)}", e: e, tags: tags, tagGetter: tagGetter);
                    }
                }
                catch
                {
                    // Doesn't make sense to handle this.
                }
            }
        }

        /// <inheritdoc/>
        public void LogInformation(string message, LogTags tags = null, Func<LogTags> tagGetter = null)
        {
            if (IsLogInformationEnabled)
            {
                try
                {
                    Log(LogLevel.Information, message, tags: tags, tagGetter: tagGetter);
                }
                catch
                {
                    // Doesn't make sense to handle this.
                }
            }
        }

        /// <inheritdoc/>
        public void LogInformation(string message, Exception e, LogTags tags = null, Func<LogTags> tagGetter = null)
        {
            Covenant.Requires<ArgumentNullException>(e != null, nameof(e));

            if (IsLogInformationEnabled)
            {
                try
                {
                    if (message != null)
                    {
                        Log(LogLevel.Information, $"{message} {NeonHelper.ExceptionError(e, stackTrace: true)}", e: e, tags: tags, tagGetter: tagGetter);
                    }
                    else
                    {
                        Log(LogLevel.Information, $"{NeonHelper.ExceptionError(e, stackTrace: true)}", e: e, tags: tags, tagGetter: tagGetter);
                    }
                }
                catch
                {
                    // Doesn't make sense to handle this.
                }
            }
        }

        /// <inheritdoc/>
        public void LogWarning(string message, LogTags tags = null, Func<LogTags> tagGetter = null)
        {
            if (IsLogWarningEnabled)
            {
                try
                {
                    Log(LogLevel.Warning, message, tags: tags, tagGetter: tagGetter);
                }
                catch
                {
                    // Doesn't make sense to handle this.
                }
            }
        }

        /// <inheritdoc/>
        public void LogWarning(string message, Exception e, LogTags tags = null, Func<LogTags> tagGetter = null)
        {
            Covenant.Requires<ArgumentNullException>(e != null, nameof(e));

            if (IsLogWarningEnabled)
            {
                try
                {
                    if (message != null)
                    {
                        Log(LogLevel.Warning, $"{message} {NeonHelper.ExceptionError(e, stackTrace: true)}", e: e, tags: tags, tagGetter: tagGetter);
                    }
                    else
                    {
                        Log(LogLevel.Warning, $"{NeonHelper.ExceptionError(e, stackTrace: true)}", e: e, tags: tags, tagGetter: tagGetter);
                    }
                }
                catch
                {
                    // Doesn't make sense to handle this.
                }
            }
        }

        //---------------------------------------------------------------------
        // ILogger implementation
        //
        // We're implementing this so that Neon logging will be compatible with 
        // non-Neon components.

        /// <summary>
        /// Do-nothing disposable returned by <see cref="BeginScope{TState}(TState)"/>.
        /// </summary>
        public sealed class Scope : IDisposable
        {
            /// <summary>
            /// Internal connstructor.
            /// </summary>
            internal Scope()
            {
            }

            /// <inheritdoc/>
            public void Dispose()
            {
            }
        }

        private static Scope scopeGlobal = new Scope(); // This can be static because it doesn't actually do anything.

        /// <summary>
        /// Converts a Microsoft log level into the corresponding Neon level.
        /// </summary>
        /// <param name="logLevel">The Microsoft log level.</param>
        /// <returns>The Neon <see cref="LogLevel"/>.</returns>
        private static LogLevel ToNeonLevel(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            switch (logLevel)
            {
                default:
                case Microsoft.Extensions.Logging.LogLevel.None:

                    return LogLevel.None;

                case Microsoft.Extensions.Logging.LogLevel.Debug:
                case Microsoft.Extensions.Logging.LogLevel.Trace:

                    return LogLevel.Debug;

                case Microsoft.Extensions.Logging.LogLevel.Information:

                    return LogLevel.Information;

                case Microsoft.Extensions.Logging.LogLevel.Warning:

                    return LogLevel.Warning;

                case Microsoft.Extensions.Logging.LogLevel.Error:

                    return LogLevel.Error;

                case Microsoft.Extensions.Logging.LogLevel.Critical:

                    return LogLevel.Critical;
            }
        }

        /// <inheritdoc/>
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state, Exception e, Func<TState, Exception, string> formatter)
        {
            // It appears that formatters are not supposed to generate anything for
            // exceptions, so we don't have to do anything special.
            //
            //      https://github.com/aspnet/Logging/issues/442

            var message = formatter(state, null) ?? string.Empty;

            switch (ToNeonLevel(logLevel))
            {
                case LogLevel.Critical:

                    if (e == null)
                    {
                        LogCritical(message);
                    }
                    else
                    {
                        LogCritical(message, e);
                    }
                    break;

                case LogLevel.Debug:

                    if (e == null)
                    {
                        LogDebug(message);
                    }
                    else
                    {
                        LogDebug(message, e);
                    }
                    break;

                case LogLevel.Trace:

                    if (e == null)
                    {
                        LogTrace(message);
                    }
                    else
                    {
                        LogTrace(message, e);
                    }
                    break;

                case LogLevel.Error:

                    if (e == null)
                    {
                        LogError(message);
                    }
                    else
                    {
                        LogError(message, e);
                    }
                    break;

                case LogLevel.Information:

                    if (e == null)
                    {
                        LogInformation(message);
                    }
                    else
                    {
                        LogInformation(message, e);
                    }
                    break;

                case LogLevel.Warning:

                    if (e == null)
                    {
                        LogWarning(message);
                    }
                    else
                    {
                        LogWarning(message, e);
                    }
                    break;
            }
        }

        /// <inheritdoc/>
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return IsLogLevelEnabled(ToNeonLevel(logLevel));
        }

        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            // We're not doing anything special for this right now.

            return scopeGlobal;
        }
    }
}
