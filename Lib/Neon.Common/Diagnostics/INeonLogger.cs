//-----------------------------------------------------------------------------
// FILE:	    INeonLogger.cs
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
    /// Defines the methods and properties for a diagnostics logger. 
    /// </summary>
    public interface INeonLogger : ILogger
    {
        /// <summary>
        /// Returns <c>true</c> if <see cref="LogLevel.Trace"/> logging is enabled.
        /// </summary>
        public bool IsLogTraceEnabled { get; }

        /// <summary>
        /// Returns <c>true</c> if <see cref="LogLevel.Debug"/> logging is enabled.
        /// </summary>
        bool IsLogDebugEnabled { get; }

        /// <summary>
        /// Returns <c>true</c> if <see cref="LogLevel.Warning"/>> logging is enabled.
        /// </summary>
        bool IsLogWarningEnabled { get; }

        /// <summary>
        /// Returns <c>true</c> if <see cref="LogLevel.Information"/>> logging is enabled.
        /// </summary>
        bool IsLogInformationEnabled { get; }

        /// <summary>
        /// Returns <c>true</c> if <see cref="LogLevel.Error"/> logging is enabled.
        /// </summary>
        bool IsLogErrorEnabled { get; }

        /// <summary>
        /// Returns <c>true</c> if <see cref="LogLevel.Critical"/> logging is enabled.
        /// </summary>
        bool IsLogCriticalEnabled { get; }

        /// <summary>
        /// Indicates whether logging is enabled for a specific log level.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <returns><c>true</c> if logging is enabled for <paramref name="logLevel"/>.</returns>
        bool IsLogLevelEnabled(LogLevel logLevel);

        /// <summary>
        /// Logs a <b>debug</b> event.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="tags">Optionally specifies tags to be added to the logged event.</param>
        /// <param name="tagGetter">Optionally specifies a function that returns tags to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// tags returned by <paramref name="tagGetter"/> will override tags passed via <paramref name="tags"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogDebug(string message, LogTags tags = null, Func<LogTags> tagGetter = null);

        /// <summary>
        /// Logs an <b>information</b> event.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="tags">Optionally specifies tags to be added to the logged event.</param>
        /// <param name="tagGetter">Optionally specifies a function that returns tags to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// tags returned by <paramref name="tagGetter"/> will override tags passed via <paramref name="tags"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogInformation(string message, LogTags tags = null, Func<LogTags> tagGetter = null);

        /// <summary>
        /// Logs a <b>warning</b> event.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="tags">Optionally specifies tags to be added to the logged event.</param>
        /// <param name="tagGetter">Optionally specifies a function that returns tags to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// tags returned by <paramref name="tagGetter"/> will override tags passed via <paramref name="tags"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogWarning(string message, LogTags tags = null, Func<LogTags> tagGetter = null);

        /// <summary>
        /// Logs an <b>error</b> event.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="tags">Optionally specifies tags to be added to the logged event.</param>
        /// <param name="tagGetter">Optionally specifies a function that returns tags to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// tags returned by <paramref name="tagGetter"/> will override tags passed via <paramref name="tags"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogError(string message, LogTags tags = null, Func<LogTags> tagGetter = null);

        /// <summary>
        /// Logs a <b>critical</b> event.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="tags">Optionally specifies tags to be added to the logged event.</param>
        /// <param name="tagGetter">Optionally specifies a function that returns tags to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// tags returned by <paramref name="tagGetter"/> will override tags passed via <paramref name="tags"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogCritical(string message, LogTags tags = null, Func<LogTags> tagGetter = null);

        /// <summary>
        /// Logs a <b>debug</b> event along with exception information.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="e">The exception.</param>
        /// <param name="tags">Optionally specifies tags to be added to the logged event.</param>
        /// <param name="tagGetter">Optionally specifies a function that returns tags to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// tags returned by <paramref name="tagGetter"/> will override tags passed via <paramref name="tags"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogDebug(string message, Exception e, LogTags tags = null, Func<LogTags> tagGetter = null);

        /// <summary>
        /// Logs an <b>information</b> event along with exception information.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="e">The exception.</param>
        /// <param name="tags">Optionally specifies tags to be added to the logged event.</param>
        /// <param name="tagGetter">Optionally specifies a function that returns tags to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// tags returned by <paramref name="tagGetter"/> will override tags passed via <paramref name="tags"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogInformation(string message, Exception e, LogTags tags = null, Func<LogTags> tagGetter = null);

        /// <summary>
        /// Logs a <b>warning</b> event along with exception information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="e">The exception.</param>
        /// <param name="tags">Optionally specifies tags to be added to the logged event.</param>
        /// <param name="tagGetter">Optionally specifies a function that returns tags to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// tags returned by <paramref name="tagGetter"/> will override tags passed via <paramref name="tags"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogWarning(string message, Exception e, LogTags tags = null, Func<LogTags> tagGetter = null);

        /// <summary>
        /// Logs an <b>error</b> event along with exception information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="e">The exception.</param>
        /// <param name="tags">Optionally specifies tags to be added to the logged event.</param>
        /// <param name="tagGetter">Optionally specifies a function that returns tags to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// tags returned by <paramref name="tagGetter"/> will override tags passed via <paramref name="tags"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogError(string message, Exception e, LogTags tags = null, Func<LogTags> tagGetter = null);

        /// <summary>
        /// Logs a <b>critical</b> event along with exception information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="e">The exception.</param>
        /// <param name="tags">Optionally specifies tags to be added to the logged event.</param>
        /// <param name="tagGetter">Optionally specifies a function that returns tags to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// tags returned by <paramref name="tagGetter"/> will override tags passed via <paramref name="tags"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogCritical(string message, Exception e, LogTags tags = null, Func<LogTags> tagGetter = null);
    }
}
