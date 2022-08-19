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

namespace Neon.Diagnostics
{
    /// <summary>
    /// Defines the methods and properties for a diagnostics logger. 
    /// </summary>
    public interface INeonLogger
    {
        /// <summary>
        /// Returns <c>true</c> if <b>trace</b> logging is enabled.
        /// </summary>
        public bool IsLogTraceEnabled { get; }

        /// <summary>
        /// Returns <c>true</c> if <b>debug</b> logging is enabled.
        /// </summary>
        bool IsLogDebugEnabled { get; }

        /// <summary>
        /// Returns <c>true</c> if <b>transient</b> logging is enabled.
        /// </summary>
        bool IsLogTransientEnabled { get; }

        /// <summary>
        /// Returns <c>true</c> if <b>sinfo</b> logging is enabled.
        /// </summary>
        bool IsLogSecurityInformationEnabled { get; }

        /// <summary>
        /// Returns <c>true</c> if <b>info</b> logging is enabled.
        /// </summary>
        bool IsLogInformationEnabled { get; }

        /// <summary>
        /// Returns <c>true</c> if <b>warn</b> logging is enabled.
        /// </summary>
        bool IsLogWarningEnabled { get; }

        /// <summary>
        /// Returns <c>true</c> if <b>error</b> logging is enabled.
        /// </summary>
        bool IsLogErrorEnabled { get; }

        /// <summary>
        /// Returns <c>true</c> if <b>serror</b> logging is enabled.
        /// </summary>
        bool IsLogSecurityErrorEnabled { get; }

        /// <summary>
        /// Returns <c>true</c> if <b>critical</b> logging is enabled.
        /// </summary>
        bool IsLogCriticalEnabled { get; }

        /// <summary>
        /// Indicates whether logging is enabled for a specific log level.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <returns><c>true</c> if logging is enabled for <paramref name="logLevel"/>.</returns>
        bool IsLogLevelEnabled(NeonLogLevel logLevel);

        /// <summary>
        /// Logs a <b>debug</b> event.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogDebug(string message, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs a <b>transient</b> event.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogTransient(string message, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs a <b>security information</b> event.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogSecurityInformation(string message, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs an <b>information</b> event.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogInformation(string message, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs a <b>warning</b> event.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogWarning(string message, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs a <b>security error</b> event.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogSecurityError(string message, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs an <b>error</b> event.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogError(string message, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs a <b>critical</b> event.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogCritical(string message, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs a <b>debug</b> event along with exception information.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="e">The exception.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogDebug(string message, Exception e, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs a <b>transient</b> ewvent along with exception information.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="e">The exception.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogTransient(string message, Exception e, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs a <b>security information</b> event along with exception information.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="e">The exception.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogSecurityInformation(string message, Exception e, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs an <b>information</b> event along with exception information.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="e">The exception.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogInformation(string message, Exception e, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs a <b>warning</b> event along with exception information.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="e">The exception.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogWarning(string message, Exception e, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs an <b>error</b> event along with exception information.
        /// </summary>
        /// <param name="body">The message.</param>
        /// <param name="e">The exception.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogError(string body, Exception e, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs a <b>security error</b> event along with exception information.
        /// </summary>
        /// <param name="body">The message.</param>
        /// <param name="e">The exception.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogSecurityError(string body, Exception e, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);

        /// <summary>
        /// Logs a <b>critical</b> event along with exception information.
        /// </summary>
        /// <param name="body">The message.</param>
        /// <param name="e">The exception.</param>
        /// <param name="attributes">Optionally specifies attributes to be added to the logged event.</param>
        /// <param name="attributeGetter">Optionally specifies a function that returns attributes to be added to the logged event.</param>
        /// <remarks>
        /// <note>
        /// Attributes returned by <paramref name="attributeGetter"/> will override attributes passed via <paramref name="attributes"/>
        /// when there are any conflicts.
        /// </note>
        /// </remarks>
        void LogCritical(string body, Exception e, LogAttributes attributes = null, Func<LogAttributes> attributeGetter = null);
    }
}
