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
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Extends <see cref="ILogger"/> with additional handy logging methods.
    /// </summary>
    public static class LoggerExtensions
    {
        //---------------------------------------------------------------------
        // CRITICAL

        /// <summary>
        /// Logs a critical message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="tagSetter">Specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogCritical(this ILogger logger, string message, Action<LogTags> tagSetter)
        {
            if (logger.IsEnabled(LogLevel.Critical))
            {
                // $todo(jefflill): Implement this
            }
        }

        /// <summary>
        /// Logs a critical message retrieved via a message function.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogCritical(this ILogger logger, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            if (logger.IsEnabled(LogLevel.Critical))
            {
                // $todo(jefflill): Implement this
            }
        }

        /// <summary>
        /// Logs a critical exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The exception.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogCritical(this ILogger logger, Exception e, Action<LogTags> tagSetter = null)
        {
            if (logger.IsEnabled(LogLevel.Critical))
            {
                // $todo(jefflill): Implement this
            }
        }

        //---------------------------------------------------------------------
        // ERROR

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="tagSetter">Specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogError(this ILogger logger, string message, Action<LogTags> tagSetter)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                // $todo(jefflill): Implement this
            }
        }

        /// <summary>
        /// Logs an error message retrieved via a message function.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogError(this ILogger logger, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                // $todo(jefflill): Implement this
            }
        }

        /// <summary>
        /// Logs an error exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The exception.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogError(this ILogger logger, Exception e, Action<LogTags> tagSetter = null)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                // $todo(jefflill): Implement this
            }
        }

        //---------------------------------------------------------------------
        // WARNING

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="tagSetter">Specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWarning(this ILogger logger, string message, Action<LogTags> tagSetter)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                // $todo(jefflill): Implement this
            }
        }

        /// <summary>
        /// Logs a warning message retrieved via a message function.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWarning(this ILogger logger, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                // $todo(jefflill): Implement this
            }
        }

        /// <summary>
        /// Logs a warning exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The exception.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWarning(this ILogger logger, Exception e, Action<LogTags> tagSetter = null)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                // $todo(jefflill): Implement this
            }
        }

        //---------------------------------------------------------------------
        // INFORMATION

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="tagSetter">Specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogInformation(this ILogger logger, string message, Action<LogTags> tagSetter)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                // $todo(jefflill): Implement this
            }
        }

        /// <summary>
        /// Logs an information message retrieved via a message function.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogInformation(this ILogger logger, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                // $todo(jefflill): Implement this
            }
        }

        /// <summary>
        /// Logs an information exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The exception.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogInformation(this ILogger logger, Exception e, Action<LogTags> tagSetter = null)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                // $todo(jefflill): Implement this
            }
        }

        //---------------------------------------------------------------------
        // DEBUG

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="tagSetter">Specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogDebug(this ILogger logger, string message, Action<LogTags> tagSetter)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                // $todo(jefflill): Implement this
            }
        }

        /// <summary>
        /// Logs a debug message retrieved via a message function.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogDebug(this ILogger logger, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                // $todo(jefflill): Implement this
            }
        }

        /// <summary>
        /// Logs a debug exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The exception.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogDebug(this ILogger logger, Exception e, Action<LogTags> tagSetter = null)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                // $todo(jefflill): Implement this
            }
        }

        //---------------------------------------------------------------------
        // TRACE

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="message">The message.</param>
        /// <param name="tagSetter">Specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogTrace(this ILogger logger, string message, Action<LogTags> tagSetter)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                // $todo(jefflill): Implement this
            }
        }

        /// <summary>
        /// Logs a debug message retrieved via a message function.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and tags
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogTrace(this ILogger logger, Func<string> messageFunc, Action<LogTags> tagSetter = null)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                // $todo(jefflill): Implement this
            }
        }

        /// <summary>
        /// Logs a debug exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The exception.</param>
        /// <param name="tagSetter">Optionally specifies an action that can be used to add tags to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing tags when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogTrace(this ILogger logger, Exception e, Action<LogTags> tagSetter = null)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                // $todo(jefflill): Implement this
            }
        }
    }
}
