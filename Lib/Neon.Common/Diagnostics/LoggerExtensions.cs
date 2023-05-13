//-----------------------------------------------------------------------------
// FILE:	    LoggerExtensions.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
    /// Extends <see cref="ILogger"/> with additional handy logging methods.  We recommend that users
    /// standardize on calling these logger extensions as opposed to using Microsoft's standard
    /// <see cref="Microsoft.Extensions.Logging.LoggerExtensions"/>.
    /// </summary>
    /// <remarks>
    /// <para><b>EVENT LOGGING:</b></para>
    /// <para>
    /// This class extends <see cref="ILogger"/> with methods intended to be somewhat easier to
    /// use than Microsoft's extensions.  Our logging method names end with <b>"Ex"</b> and we
    /// provide methods for logging critical, error, warning, information, debug and trace events.
    /// We have overrides for each log level that can be used for different purposes.  We'll discuss
    /// the information methods below.  The methods for the other log levels folow the same pattern.
    /// </para>
    /// <para>
    /// The first thing to note, is that all logging methods include an optional <b>attributeSetter</b>
    /// parameter.  This can be set to an <see cref="Action"/> that adds arbitrary tags to the event
    /// when logged.  This is an easy and clean way to specify attributes, much cleaner than specifying
    /// a message format string as required by the <see cref="Microsoft.Extensions.Logging.LoggerExtensions"/>
    /// (e.g. there's no way to include an attribute without having it appear in the event message).
    /// Here's how this works:
    /// </para>
    /// <code language="C#">
    /// logger.LogInformationEx("Test message",
    ///     attributes => {
    ///         attributes.Add("my-attr-0", "test-0");
    ///         attributes.Add("my-attr-1", "test-1");
    ///     });
    /// </code>
    /// <note>
    /// The attributes lambda function is only called when the event is actually going to be logged, based on
    /// the current log level.
    /// </note>
    /// <para>
    /// Some of our extensions accept the message as a string and others accept a lambda function
    /// that returns the message string.  The general rule is that you should pass constant strings
    /// directly to the logging methods but strings generated at runtime via interpolation or other
    /// mechanisms should be specified by passing a message lambda function.
    /// </para>
    /// <note>
    /// The message lambda function is only called when the event is actually going to be logged, based on
    /// the current log level.
    /// </note>
    /// <para>
    /// The latter recomendation will improve performance because the lambda function won't be called
    /// when the event won't actually be logged due to the current log level, avoiding the overhead
    /// of generating the string.  Imagine if your program logged a lot of TRACE events with dynamically
    /// generated messages.  This means that when running at the INFORMATION log level, all of those
    /// trace messages would be created at runtime and then be immediately discarded, resulting in
    /// wasted CPU used to generate the message as well as extra heap allocations (all for nothing).
    /// </para>
    /// <code language="C#">
    /// // Log a static message:
    /// 
    /// logger.LogInformationEx("Hello World!");
    /// 
    /// // Log a dynamic message:
    /// 
    /// var name = "Sally";
    /// 
    /// logger.LogInformation(() => $"Hello: {name}");
    /// 
    /// // YOU DON'T WANT TO DO THIS because the message string will always be generated at runtime,
    /// // even when the event won't be logged due to the current log level:
    /// 
    /// logger.LogInformation($"Hello: {name}");
    /// </code>
    /// <para>
    /// <see cref="LogInformationEx(ILogger, string, Action{LogAttributes})"/>: Used for logging a
    /// constant message string.  Avoid calling this for dynamically generated messages.
    /// </para>
    /// <para>
    /// <see cref="LogInformationEx(ILogger, Func{string}, Action{LogAttributes})"/>: Used for
    /// logging a dynamically generated message.  This will be much more efficient when the event
    /// isn't going to be logged due to the current log level setting.
    /// </para>
    /// <para>
    /// <see cref="LogInformationEx(ILogger, Exception, string, Action{LogAttributes})"/>: Used for
    /// logging an exception with a constant or <c>null</c> message.  When message is passed as empty
    /// or <c>null</c>, a message generated from exception will be used.
    /// </para>
    /// <para>
    /// <see cref="LogInformationEx(ILogger, Exception, Func{string}, Action{LogAttributes})"/>: Used
    /// for logging an exception with a dynamic message.  When the message function is passed <c>null</c>,
    /// a message generated from exception will be used.
    /// </para>
    /// <para><b>LOGGER ATTRIBUTES</b></para>
    /// <para>
    /// Use the <see cref="LoggerExtensions.AddAttributes(ILogger, Action{LogAttributes})"/> method 
    /// to create a new <see cref="ILogger"/> with new attributes such that these attributes will
    /// be included in subsequent events emitted by the logger.  Note that attributes logged with
    /// the event will override logger attributes with the same name.
    /// </para>
    /// <para>
    /// Here's how this works:
    /// </para>
    /// <code language="C#">
    /// var logger     = TelemetryHub.CreateLogger("my-logger");
    /// var attributes = new LogAttributes();
    /// 
    /// attributes.Add("my-attr-0", "test-0");
    /// attributes.Add("my-attr-1", "test-1");
    /// 
    /// logger = logger.AddAttributes(attributes);  // Creates a new logger including the attributes passed.
    /// 
    /// logger.LogInformationEx("Test message");    // This event will include the new attributes
    /// 
    /// // This example overrides the logger's "test-1" attribute with the "OVERRIDE" value:
    /// 
    /// logger.LogInformationEx("Test message", attributes => attributes.Add("test-1", "OVERRIDE"));
    /// </code>
    /// <note>
    /// <b>IMPORTANT:</b> Any additional attributes added to the logger returned will only
    /// be recognized by the neondSDK logger extensions <see cref="LoggerExtensions"/> with
    /// logging method names ending in <b>"Ex"</b>, like: <see cref="LogInformationEx(ILogger, Func{string}, Action{LogAttributes})"/>.
    /// The standard Microsoft logger extension methods implemented by <see cref="Microsoft.Extensions.Logging.LoggerExtensions"/>
    /// will ignore these logger attributes.
    /// </note>
    /// </remarks>
    public static class LoggerExtensions
    {
        /// <summary>
        /// <para>
        /// This wraps the logger passed with another logger that adds a colection of attributes
        /// to every logged event.
        /// </para>
        /// <note>
        /// <b>IMPORTANT:</b> Any additional attributes added to the logger returned will only
        /// be recognized by the neondSDK logger extensions <see cref="LoggerExtensions"/> with
        /// logging method names ending in <b>"Ex"</b>, like: <see cref="LogInformationEx(ILogger, Func{string}, Action{LogAttributes})"/>.
        /// The standard Microsoft logger extension methods implemented by <see cref="Microsoft.Extensions.Logging.LoggerExtensions"/>
        /// will ignore these logger attributes.
        /// </note>
        /// </summary>
        /// <param name="logger">The logger being wrapped.</param>
        /// <param name="attributeSetter">Action used to add attributes to the logger.</param>
        /// <returns>An <see cref="ILogger"/> that will include the attributes in every event it logs.</returns>
        /// <remarks>
        /// This method returns a new logger that includes the attributes added by the
        /// <paramref name="attributeSetter"/> action.
        /// </remarks>
        public static ILogger AddAttributes(this ILogger logger, Action<LogAttributes> attributeSetter)
        {
            Covenant.Requires<ArgumentNullException>(logger != null, nameof(logger));

            var attributes = new LogAttributes();

            attributeSetter?.Invoke(attributes);

            if (logger is AttributeLogger attributeLogger)
            {
                // The logger passed is a [AttributeLogger] so we'll create a new logger that
                // combines the existing attributes with the new ones.  Note that we're adding the
                // attributes passed to the existing attributes so new attributes with the same names
                // will override the existing attributes.

                var newAttributes = new LogAttributes(attributeLogger.Attributes);

                foreach (var attribute in attributes.Attributes)
                {
                    newAttributes.Add(attribute.Key, attribute.Value);
                }

                return new AttributeLogger(logger, newAttributes);
            }
            else
            {
                return new AttributeLogger(logger, attributes);
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
        /// <param name="attributeSetter">Optionally specifies a function that can add attributes to the event.</param>
        private static void LogInternal(this ILogger logger, LogLevel logLevel, Exception exception = null, string message = null, Func<string> messageFunc = null, Action<LogAttributes> attributeSetter = null)
        {
            if (!logger.IsEnabled(logLevel))
            {
                return;
            }

            // $note(jefflill):
            //
            // The [Microsoft.Extensions.Logger.ILogger] extension methods handling of attributes is a bit
            // odd.  Instead of passing an [KeyValuePair<string, object>] array or something, they
            // have you pass a message string with the key names encoded as "{Name}" with the values
            // passed as a params array of objects.
            //
            // This really bothered me for a while because it looked like these attribute names in the
            // message would be replaced with the values and that this would be the message body
            // that users would see in their logs.  This can be included as a "formatted message",
            // but that functionality is disabled by default.  I'm not entirely sure why MSFT
            // implemented logging like this, but I suspect that this may be just old code or
            // perhaps this reduces the number of memory allocations required for logging.
            //
            // We're going to handle this by decoupling our concept of message text from MSFT's
            // concept by persisting the user's message passed as an explicit [body] attribute and
            // then constructing a separate message used internally only for specifing attribute names.

            var logAttributes = new LogAttributes();    // $todo(jefflill): It would be really nice to be able to pool these: https://github.com/nforgeio/TEMPKUBE/issues/1668#issuecomment-1235696464

            // Process the event message.

            if (message == null && messageFunc != null)
            {
                message = messageFunc();
            }

            // Add the index attribute.

            logAttributes.Add(LogAttributeNames.NeonIndex, TelemetryHub.GetNextIndex());
            
            // Add the trace attributes

            if (Activity.Current != null)
            {
                if (exception != null)
                {
                    Activity.Current?.RecordException(exception);
                }

                var context = Activity.Current.Context;

                logAttributes.Add(LogAttributeNames.TraceId, context.TraceId.ToString());
                logAttributes.Add(LogAttributeNames.SpanId, context.SpanId.ToString());
            }

            // Append any attributes held by [AttributeLogger] loggers.

            var attributeLogger      = logger as AttributeLogger;
            var loggerAttributeCount = attributeLogger == null ? 0 : attributeLogger.Attributes.Count;

            if (loggerAttributeCount > 0)
            {
                foreach (var attribute in attributeLogger.Attributes.Attributes)
                {
                    logAttributes.Add(attribute.Key, attribute.Value);
                }
            }

            // Generate a log message from an exception when the user didn't
            // specify a message.

            if (exception != null && message == null)
            {
                message = $"[{exception.GetType().FullName}]: {DiagnosticsHelper.CleanExceptionMessage(exception)}";
            }

            // Temporarily persist the message as an attribute.

            logAttributes.Add(LogAttributeNames.InternalBody, message);

            if (attributeSetter != null)
            {
                attributeSetter.Invoke(logAttributes);
            }

            // Use stock [ILogger] to log the event.

            logger.Log(logLevel, default(EventId), logAttributes.Attributes, exception,
                (state, exception) =>
                {
                    return message;
                });
        }

        //---------------------------------------------------------------------
        // Log with specified level:

        /// <summary>
        /// Logs a message with the specified <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="logLevel">Specifies the <see cref="LogLevel"/>.</param>
        /// <param name="message">Specifies the message.</param>
        /// <param name="attributeSetter">Specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWithLevelEx(this ILogger logger, LogLevel logLevel, string message, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(logLevel, message: message, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs a message retrieved via a message function with the specified <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="logLevel">Specifies the <see cref="LogLevel"/>.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWithLevelEx(this ILogger logger, LogLevel logLevel, Func<string> messageFunc, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(logLevel, messageFunc: messageFunc, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs an exception retrieved via a message function with the specified <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="logLevel">Specifies the <see cref="LogLevel"/>.</param>
        /// <param name="exception">Specifies the exception.</param>
        /// <param name="message">Optionally specifies the event message.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWithLevelEx(this ILogger logger, LogLevel logLevel, Exception exception, string message = null, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(logLevel, exception: exception, message: message, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs an exception with a message returned by a custom message function with the specified <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="logLevel">Specifies the <see cref="LogLevel"/>.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageFunc">Specifies the message function.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWithLevelEx(this ILogger logger, LogLevel logLevel, Exception exception, Func<string> messageFunc, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(logLevel, exception: exception, messageFunc: messageFunc, attributeSetter: attributeSetter);
        }

        //---------------------------------------------------------------------
        // CRITICAL:

        /// <summary>
        /// Logs a critical message.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="message">Specifies the message.</param>
        /// <param name="attributeSetter">Specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogCriticalEx(this ILogger logger, string message, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Critical, message: message, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs a critical message retrieved via a message function.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogCriticalEx(this ILogger logger, Func<string> messageFunc, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Critical, messageFunc: messageFunc, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs a critical exception.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies the exception.</param>
        /// <param name="message">Optionally specifies the event message.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogCriticalEx(this ILogger logger, Exception exception, string message = null, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Critical, exception: exception, message: message, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs a critical exception with a message returned by a custom message function.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="messageFunc">Specifies the message function.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogCriticalEx(this ILogger logger, Exception exception, Func<string> messageFunc, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Critical, exception: exception, messageFunc: messageFunc, attributeSetter: attributeSetter);
        }

        //---------------------------------------------------------------------
        // ERROR:

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="message">Specifies message.</param>
        /// <param name="attributeSetter">Specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogErrorEx(this ILogger logger, string message, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Error, message: message, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs an error message retrieved via a message function.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogErrorEx(this ILogger logger, Func<string> messageFunc, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Error, messageFunc: messageFunc, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs an error exception.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="message">Optionally specifies the event message.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogErrorEx(this ILogger logger, Exception exception, string message = null, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Error, exception: exception, message: message, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs an error exception with a message returned by a custom message function..
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogErrorEx(this ILogger logger, Exception exception, Func<string> messageFunc, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Error, exception: exception, messageFunc: messageFunc, attributeSetter: attributeSetter);
        }

        //---------------------------------------------------------------------
        // WARNING:

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="message">Specifies message.</param>
        /// <param name="attributeSetter">Specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWarningEx(this ILogger logger, string message, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Warning, message: message, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs a warning message retrieved via a message function.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWarningEx(this ILogger logger, Func<string> messageFunc, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Warning, messageFunc: messageFunc, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs a warning exception.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="message">Optionally specifies the event message.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWarningEx(this ILogger logger, Exception exception, string message = null, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Warning, exception: exception, message: message, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs a warning exception with a message returned by a custom message function..
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWarningEx(this ILogger logger, Exception exception, Func<string> messageFunc, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Warning, exception: exception, messageFunc: messageFunc, attributeSetter: attributeSetter);
        }

        //---------------------------------------------------------------------
        // INFORMATION:

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="message">Specifies message.</param>
        /// <param name="attributeSetter">Specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogInformationEx(this ILogger logger, string message, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Information, message: message, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs an information message retrieved via a message function.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogInformationEx(this ILogger logger, Func<string> messageFunc, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Information, messageFunc: messageFunc, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs an information exception.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="message">Optionally specifies the event message.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogInformationEx(this ILogger logger, Exception exception, string message = null, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Information, exception: exception, message: message, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs a information exception with a message returned by a custom message function..
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogInformationEx(this ILogger logger, Exception exception, Func<string> messageFunc, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Information, exception: exception, messageFunc: messageFunc, attributeSetter: attributeSetter);
        }

        //---------------------------------------------------------------------
        // DEBUG:

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="message">Specifies message.</param>
        /// <param name="attributeSetter">Specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogDebugEx(this ILogger logger, string message, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Debug, message: message, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs a debug message retrieved via a message function.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogDebugEx(this ILogger logger, Func<string> messageFunc, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Debug, messageFunc: messageFunc, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs a debug exception.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="message">Optionally specifies the event message.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogDebugEx(this ILogger logger, Exception exception, string message = null, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Debug, exception: exception, message: message, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs a debug exception with a message returned by a custom message function..
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogDebugEx(this ILogger logger, Exception exception, Func<string> messageFunc, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Debug, exception: exception, messageFunc: messageFunc, attributeSetter: attributeSetter);
        }

        //---------------------------------------------------------------------
        // TRACE:

        /// <summary>
        /// Logs a trace message.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="message">Specifies message.</param>
        /// <param name="attributeSetter">Specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogTraceEx(this ILogger logger, string message, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Trace, message: message, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs a trace message retrieved via a message function.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogTraceEx(this ILogger logger, Func<string> messageFunc, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Trace, messageFunc: messageFunc, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs a trace exception.
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="message">Optionally specifies the event message.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogTraceEx(this ILogger logger, Exception exception, string message = null, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Trace, exception: exception, message: message, attributeSetter: attributeSetter);
        }

        /// <summary>
        /// Logs a trace exception with a message returned by a custom message function..
        /// </summary>
        /// <param name="logger">Specifies the logger.</param>
        /// <param name="exception">Specifies exception.</param>
        /// <param name="messageFunc">Specifies message function.</param>
        /// <param name="attributeSetter">Optionally specifies an action that can be used to add attributes to the event being logged.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogTraceEx(this ILogger logger, Exception exception, Func<string> messageFunc, Action<LogAttributes> attributeSetter = null)
        {
            logger.LogInternal(LogLevel.Trace, exception: exception, messageFunc: messageFunc, attributeSetter: attributeSetter);
        }
    }
}
