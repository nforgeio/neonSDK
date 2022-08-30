//-----------------------------------------------------------------------------
// FILE:	    LoggerExtensions.cs
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
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Neon.Common;
using OpenTelemetry.Trace;
using YamlDotNet.Core.Tokens;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Extends <see cref="ILogger"/> with additional handy logging methods.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// This wraps the logger passed with another logger that adds a colection of tags
        /// to every logged event.
        /// </summary>
        /// <param name="logger">The logger being wrapped.</param>
        /// <param name="tagSetter">Action used to add tags to the logger.</param>
        /// <returns>An <see cref="ILogger"/> that will include the tags in every event it logs.</returns>
        /// <remarks>
        /// This method returns a new logger that includes the tags added by the
        /// <paramref name="tagSetter"/> action.
        /// </remarks>
        public static ILogger AddTags(this ILogger logger, Action<LogTags> tagSetter)
        {
            Covenant.Requires<ArgumentNullException>(logger != null, nameof(logger));

            var tags = new LogTags();

            tagSetter?.Invoke(tags);

            if (logger is LoggerWithTags loggerWithTags)
            {
                // The logger passed is a [LoggerWithTags] so we'll create a new logger that
                // combines the existing tags with the new ones.  Note that we're adding the
                // tags passed to the existing tags so new tags with the same names will override
                // the existing tags.

                var newTags = new LogTags(loggerWithTags.Tags);

                foreach (var tag in tags.Tags)
                {
                    newTags.Add(tag.Key, tag.Value);
                }

                return new LoggerWithTags(logger, newTags);
            }
            else
            {
                return new LoggerWithTags(logger, tags);
            }
        }

        /// <summary>
        /// Actually emits logs, trying to do something reasonable various combinations of arguments.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="logLevel">Specifies the log level.</param>
        /// <param name="exception">Optionally specifies an exception.</param>
        /// <param name="message">Optionally specifies a message text.</param>
        /// <param name="messageFunc">Optionally specifies a function used to retrieve the message text.</param>
        /// <param name="tagSetter">Optionally specifies a function that can add tags to the event.</param>
        private static void LogInternal(this ILogger logger, LogLevel logLevel, Exception exception = null, string message = null, Func<string> messageFunc = null, Action<LogTags> tagSetter = null)
        {
            if (!logger.IsEnabled(logLevel))
            {
                return;
            }

            // $note(jefflill):
            //
            // The [Microsoft.Extensions.Logger.ILogger] extension methods handling of tags is a bit
            // odd.  Instead of passing an [KeyValuePair<string, object>] array or something, they
            // have you pass a message string with the key names encoded as "{Name}" with the values
            // passed as a params array of objects.
            //
            // This really bothered me for a while because it looked like these tag names in the
            // message would be replaced with the values and that this would be the message body
            // that users would see in their logs.  This can be included as a "formatted message",
            // but that functionality is disabled by default.  I'm not entirely sure why MSFT
            // implemented logging like this, but I suspect that this may be just old code or
            // perhaps this reduces the number of memory allocations required for logging.
            //
            // We're going to handle this by decoupling our concept of message text from MSFT's
            // concept by persisting the user's message passed as an explicit [body] tag and
            // then constructing a separate message used internally only for specifing tag names.
            //
            // The MSFT logger implementation tries pretty hard to reduce the number of memory
            // allocations via pooling, etc.  We're going to try to do the same by pooling
            // [LogTags] collections and [StringBuilder]s.  We're going to try to do the same
            // by using the [DiagnosticPools] to cache and reuse various objects:
            // 
            //      LogTags         - Uused for user tag-setter functions
            //      StringBuilder   - Used for generating the formatted messages holding the tag
            //                        names we'll be passing to the underlying [ILogger].
            //      TagArgs         - Used for passing tag values to the underlying [ILoger].

            // $hack(jefflill)
            //
            // The [TagArgs] items depends on the MSFT logger implementation not requiring that
            // the number of items in the argument array passed match the number of tag names
            // in the low-level message we'll be passing to the underlying [ILogger] implementation.
            //
            // I've tested this and MSFT seems to tolerate any mismatch lengths, so I'm going
            // to go ahead with this otherwise the C# compiler will allocate [params] arrays for
            // every call.

            LogTags logTags = null;

            try
            {
                logTags = DiagnosticPools.GetLogTags();

                // Process the event message.

                if (message == null && messageFunc != null)
                {
                    message = messageFunc();
                }

                // Append any tags held by [LoggerWithTags] loggers.

                var loggerWithTags = logger as LoggerWithTags;
                var loggerTagCount = loggerWithTags == null ? 0 : loggerWithTags.Tags.Count;

                if (loggerTagCount > 0)
                {
                    foreach (var tag in loggerWithTags.Tags.Tags)
                    {
                        logTags.Add(tag.Key, tag.Value);
                    }
                }

                // Generate a log message from an exception when the user didn't
                // specify a message.

                if (exception != null && message == null)
                {
                    message = NeonHelper.ExceptionError(exception);
                }

                // Temporarily persist the message as a tag.

                logTags.Add(LogTagNames.InternalBody, message);

                if (tagSetter != null)
                {
                    tagSetter.Invoke(logTags);
                }

                // Use stock [ILogger] to log the event.

                logger.Log(logLevel, default(EventId), logTags.Tags, exception,
                    (state, exception) =>
                    {
                        return message;
                    });
            }
            finally
            {
                if (logTags != null)
                {
                    DiagnosticPools.ReturnLogTags(logTags);
                }
            }
        }

        //---------------------------------------------------------------------
        // CRITICAL:

        /// <summary>
        /// Logs a critical message.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="message">Specifies the message.</param>
        /// <param name="tagSetter">Specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogCriticalEx(this ILogger logger, string message, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Critical, message: message, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs a critical message retrieved via a message function.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogCriticalEx(this ILogger logger, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Critical, messageFunc: messageFunc, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs a critical exception.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies the exception.</param>
        /// <param name="message">Optionally specifies the event message.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogCriticalEx(this ILogger logger, Exception exception, string message = null, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Critical, exception: exception, message: message, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs a critical exception with a message returned by a custom message function..
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageFunc">Specifies the message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogCriticalEx(this ILogger logger, Exception exception, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Critical, exception: exception, messageFunc: messageFunc, tagSetter: tagSetter);
        }

        //---------------------------------------------------------------------
        // ERROR:

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="message">Specifies message.</param>
        /// <param name="tagSetter">Specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogErrorEx(this ILogger logger, string message, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Error, message: message, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs an error message retrieved via a message function.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogErrorEx(this ILogger logger, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Error, messageFunc: messageFunc, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs an error exception.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="message">Optionally specifies the event message.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogErrorEx(this ILogger logger, Exception exception, string message = null, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Error, exception: exception, message: message, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs an error exception with a message returned by a custom message function..
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogErrorEx(this ILogger logger, Exception exception, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Error, exception: exception, messageFunc: messageFunc, tagSetter: tagSetter);
        }

        //---------------------------------------------------------------------
        // WARNING:

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="message">Specifies message.</param>
        /// <param name="tagSetter">Specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWarningEx(this ILogger logger, string message, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Warning, message: message, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs a warning message retrieved via a message function.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWarningEx(this ILogger logger, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Warning, messageFunc: messageFunc, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs a warning exception.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="message">Optionally specifies the event message.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWarningEx(this ILogger logger, Exception exception, string message = null, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Warning, exception: exception, message: message, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs a warning exception with a message returned by a custom message function..
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWarningEx(this ILogger logger, Exception exception, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Warning, exception: exception, messageFunc: messageFunc, tagSetter: tagSetter);
        }

        //---------------------------------------------------------------------
        // INFORMATION:

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="message">Specifies message.</param>
        /// <param name="tagSetter">Specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogInformationEx(this ILogger logger, string message, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Information, message: message, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs an information message retrieved via a message function.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogInformationEx(this ILogger logger, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Information, messageFunc: messageFunc, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs an information exception.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="message">Optionally specifies the event message.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogInformationEx(this ILogger logger, Exception exception, string message = null, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Information, exception: exception, message: message, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs a information exception with a message returned by a custom message function..
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogInformationEx(this ILogger logger, Exception exception, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Information, exception: exception, messageFunc: messageFunc, tagSetter: tagSetter);
        }

        //---------------------------------------------------------------------
        // DEBUG:

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="message">Specifies message.</param>
        /// <param name="tagSetter">Specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogDebugEx(this ILogger logger, string message, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Debug, message: message, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs a debug message retrieved via a message function.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogDebugEx(this ILogger logger, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Debug, messageFunc: messageFunc, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs a debug exception.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="message">Optionally specifies the event message.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogDebugEx(this ILogger logger, Exception exception, string message = null, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Debug, exception: exception, message: message, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs a debug exception with a message returned by a custom message function..
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogDebugEx(this ILogger logger, Exception exception, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Debug, exception: exception, messageFunc: messageFunc, tagSetter: tagSetter);
        }

        //---------------------------------------------------------------------
        // TRACE:

        /// <summary>
        /// Logs a trace message.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="message">Specifies message.</param>
        /// <param name="tagSetter">Specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogTraceEx(this ILogger logger, string message, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Trace, message: message, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs a trace message retrieved via a message function.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogTraceEx(this ILogger logger, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Trace, messageFunc: messageFunc, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs a trace exception.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="message">Optionally specifies the event message.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogTraceEx(this ILogger logger, Exception exception, string message = null, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Trace, exception: exception, message: message, tagSetter: tagSetter);
        }

        /// <summary>
        /// Logs a trace exception with a message returned by a custom message function..
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogTraceEx(this ILogger logger, Exception exception, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            logger.LogInternal(LogLevel.Trace, exception: exception, messageFunc: messageFunc, tagSetter: tagSetter);
        }
    }
}
