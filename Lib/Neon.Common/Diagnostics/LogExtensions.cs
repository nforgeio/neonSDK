//-----------------------------------------------------------------------------
// FILE:	    LogExtensions.cs
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

namespace Neon.Diagnostics
{
    /// <summary>
    /// Extends <see cref="INeonLogger"/>.
    /// </summary>
    public static class LogExtensions
    {
        /// <summary>
        /// Logs a debug message retrieved via a message function.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogDebug(this INeonLogger logger, Func<string> messageFunc, Func<LogAttributes> attributeGetter = null)
        {
            if (logger.IsLogDebugEnabled)
            {
                logger.LogDebug(messageFunc(), attributeGetter: attributeGetter);
            }
        }

        /// <summary>
        /// Logs a transient message retrieved via a message function.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogTransient(this INeonLogger logger, Func<string> messageFunc, Func<LogAttributes> attributeGetter = null)
        {
            if (logger.IsLogDebugEnabled)
            {
                logger.LogTransient(messageFunc(), attributeGetter: attributeGetter);
            }
        }

        /// <summary>
        /// Logs a <b>security information</b> event.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogSecurityInformation(this INeonLogger logger, Func<string> messageFunc, Func<LogAttributes> attributeGetter = null)
        {
            if (logger.IsLogSecurityErrorEnabled)
            {
                logger.LogSecurityInformation(messageFunc(), attributeGetter: attributeGetter);
            }
        }

        /// <summary>
        /// Logs an informational message retrieved via a message function.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogInformation(this INeonLogger logger, Func<string> messageFunc, Func<LogAttributes> attributeGetter = null)
        {
            if (logger.IsLogInformationEnabled)
            {
                logger.LogInformation(messageFunc(), attributeGetter: attributeGetter);
            }
        }

        /// <summary>
        /// Logs a warning message retrieved via a message function.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWarning(this INeonLogger logger, Func<string> messageFunc, Func<LogAttributes> attributeGetter = null)
        {
            if (logger.IsLogWarningEnabled)
            {
                logger.LogWarning(messageFunc(), attributeGetter: attributeGetter);
            }
        }

        /// <summary>
        /// Logs a <b>security error</b> event.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogSecurityError(this INeonLogger logger, Func<string> messageFunc, Func<LogAttributes> attributeGetter = null)
        {
            if (logger.IsLogSecurityErrorEnabled)
            {
                logger.LogSecurityError(messageFunc(), attributeGetter: attributeGetter);
            }
        }

        /// <summary>
        /// Logs an error message retrieved via a message function.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogError(this INeonLogger logger, Func<string> messageFunc, Func<LogAttributes> attributeGetter = null)
        {
            if (logger.IsLogErrorEnabled)
            {
                logger.LogError(messageFunc(), attributeGetter: attributeGetter);
            }
        }

        /// <summary>
        /// Logs a critical message retrieved via a message function.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="messageFunc">The message function.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing interpolated strings and attributes
        /// when the current log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogCritical(this INeonLogger logger, Func<string> messageFunc, Func<LogAttributes> attributeGetter = null)
        {
            if (logger.IsLogCriticalEnabled)
            {
                logger.LogCritical(messageFunc(), attributeGetter: attributeGetter);
            }
        }

        /// <summary>
        /// Logs a debug exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The exception.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogDebug(this INeonLogger logger, Exception e, Func<LogAttributes> attributeGetter = null)
        {
            if (logger.IsLogDebugEnabled)
            {
                logger.LogDebug(null, e, attributeGetter: attributeGetter);
            }
        }

        /// <summary>
        /// Logs a transient exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The exception.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogTransient(this INeonLogger logger, Exception e, Func<LogAttributes> attributeGetter = null)
        {
            if (logger.IsLogTransientEnabled)
            {
                logger.LogTransient(null, e, attributeGetter: attributeGetter);
            }
        }

        /// <summary>
        /// Logs an info exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The exception.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogInformation(this INeonLogger logger, Exception e, Func<LogAttributes> attributeGetter = null)
        {
            if (logger.IsLogInformationEnabled)
            {
                logger.LogInformation(null, e, attributeGetter: attributeGetter);
            }
        }

        /// <summary>
        /// Logs a warning exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The exception.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogWarning(this INeonLogger logger, Exception e, Func<LogAttributes> attributeGetter = null)
        {
            if (logger.IsLogWarningEnabled)
            {
                logger.LogWarning(null, e, attributeGetter: attributeGetter);
            }
        }

        /// <summary>
        /// Logs an error exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The exception.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogError(this INeonLogger logger, Exception e, Func<LogAttributes> attributeGetter = null)
        {
            if (logger.IsLogErrorEnabled)
            {
                logger.LogError(null, e, attributeGetter: attributeGetter);
            }
        }

        /// <summary>
        /// Logs a critical exception.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="e">The exception.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// This method is intended mostly to avoid processing attributes when the current 
        /// log level prevents any log from being emitted, for better performance.
        /// </remarks>
        public static void LogCritical(this INeonLogger logger, Exception e, Func<LogAttributes> attributeGetter = null)
        {
            if (logger.IsLogCriticalEnabled)
            {
                logger.LogCritical(null, e, attributeGetter: attributeGetter);
            }
        }
    }
}
