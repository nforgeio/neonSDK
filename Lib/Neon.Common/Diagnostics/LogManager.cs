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
using YamlDotNet.Serialization;

namespace Neon.Diagnostics
{
    /// <summary>
    /// A reasonable default implementation of an application log manager.  See
    /// <see cref="ITelemetryHub"/> for a description of how log managers work.
    /// </summary>
    public class TelemetryHub : ITelemetryHub
    {
        //---------------------------------------------------------------------
        // Static members

        /// <summary>
        /// Returns the <see cref="Regex"/> used for lite validation of program version strings.
        /// </summary>
        public static Regex VersionRegex { get; private set; } = new Regex(@"[0-9a-zA-Z\.-_/]+");

        /// <summary>
        /// <para>
        /// The default <see cref="ITelemetryHub"/> that can be used by applications that don't
        /// use dependency injection.  This defaults to an instance of <see cref="TelemetryHub"/>
        /// but can be set to something else for unit tests or early in application startup.
        /// </para>
        /// <para>
        /// Applications that do use dependency injection can obtain this by default via
        /// <see cref="NeonHelper.ServiceContainer"/>.
        /// </para>
        /// </summary>
        public static ITelemetryHub Default
        {
            get { return NeonHelper.ServiceContainer.GetService<ITelemetryHub>(); }

            set
            {
                // Ensure that updates to the default manager will also be reflected in 
                // the dependency services so users won't be surprised.

                NeonHelper.ServiceContainer.AddSingleton<ITelemetryHub>(value);
                NeonHelper.ServiceContainer.AddSingleton<ILoggerProvider>(value);
            }
        }

        /// <summary>
        /// Returns a <b>log-nothing</b> log manager.
        /// </summary>
        public static ITelemetryHub Disabled { get; private set; }
        
        /// <summary>
        /// Static constructor.
        /// </summary>
        static TelemetryHub()
        {
            Default  = new TelemetryHub(name: "DEFAULT");
            Disabled = new TelemetryHub(parseLogLevel: false)
            {
                LogLevel = NeonLogLevel.None
            };
        }

        //---------------------------------------------------------------------
        // Instance members

        // $todo(jefflill):
        //
        // Using [syncRoot] to implement thread safety via a [Monitor] may introduce
        // some performance overhead for ASP.NET sites with lots of traffic.  It
        // may be worth investigating whether a [SpinLock] might be better.

        private readonly object                             syncRoot         = new object();
        private readonly Dictionary<string, INeonLogger>    categoryToLogger = new Dictionary<string, INeonLogger>();
        private readonly TextWriter                         writer           = null;
        private NeonLogLevel                                logLevel         = NeonLogLevel.Information;
        private long                                        emitCount;
        private LoggerCreatorDelegate                       loggerCreator;
        private Func<LogEvent, bool>                        logFilter;
        private Func<bool>                                  isLogEnabledFunc;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="parseLogLevel">Indicates that the <b>LOG-LEVEL</b> environment variable should be parsed (defaults to <c>true</c>).</param>
        /// <param name="version">
        /// Optionally specifies the semantic version of the current program.  This can be a somewhat arbitrary
        /// string that matches this regex: <b>[0-9a-zA-Z\.-_/]+</b>.  This defaults to <c>null</c>.
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
        /// <param name="name">
        /// Optionally specifies the log manager's name.  This can be useful when debugging the
        /// log manager itself.
        /// </param>
        /// <param name="writer">Optionally specifies the output writer.  This defaults to <see cref="Console.Error"/>.</param>
        public TelemetryHub(bool parseLogLevel = true, string version = null, Func<LogEvent, bool> logFilter = null, Func<bool> isLogEnabledFunc = null, TextWriter writer = null, string name = null)
        {
            this.Name = name;

            if (parseLogLevel && !Enum.TryParse<NeonLogLevel>(Environment.GetEnvironmentVariable("LOG_LEVEL"), true, out logLevel))
            {
                logLevel = NeonLogLevel.Information;
            }

            if (!string.IsNullOrEmpty(version) && VersionRegex.IsMatch(version))
            {
                this.Version = version;
            }
            else
            {
                this.Version = "unknown";
            }

            this.logFilter        = logFilter;
            this.isLogEnabledFunc = isLogEnabledFunc;
            this.writer           = writer;
            this.emitCount        = 0;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            lock (syncRoot)
            {
                LoggerCreator    = null;
                LogLevel         = NeonLogLevel.Information;
                logFilter        = null;
                isLogEnabledFunc = null;
                EmitIndex        = true;
                emitCount        = 0;

                categoryToLogger.Clear();
            }
        }

        /// <inheritdoc/>
        public string Name { get; private set; } = null;

        /// <inheritdoc/>
        public string Version { get; set; } = null;

        /// <inheritdoc/>
        public NeonLogLevel LogLevel
        {
            get => this.logLevel;
            set => this.logLevel = value;
        }

        /// <inheritdoc/>
        public void SetLogLevel(string level)
        {
            level = level ?? "INFO";

            switch (level.ToUpperInvariant())
            {
                case "CRITICAL":

                    LogLevel = NeonLogLevel.Critical;
                    break;

                case "SERROR":

                    LogLevel = NeonLogLevel.SecurityError;
                    break;

                case "ERROR":

                    LogLevel = NeonLogLevel.Error;
                    break;

                case "WARN":
                case "WARNING":

                    LogLevel = NeonLogLevel.Warning;
                    break;

                default:
                case "INFO":
                case "INFORMATION":

                    LogLevel = NeonLogLevel.Information;
                    break;

                case "SINFO":

                    LogLevel = NeonLogLevel.SecurityInformation;
                    break;

                case "TRANSIENT":

                    LogLevel = NeonLogLevel.Transient;
                    break;

                case "DEBUG":

                    LogLevel = NeonLogLevel.Debug;
                    break;

                case "TRACE":

                    LogLevel = NeonLogLevel.Trace;
                    break;

                case "NONE":

                    LogLevel = NeonLogLevel.None;
                    break;
            }
        }

        /// <inheritdoc/>
        public bool EmitTimestamp { get; set; } = true;

        /// <inheritdoc/>
        public bool EmitIndex { get; set; } = true;

        /// <inheritdoc/>
        public long GetNextEventIndex()
        {
            if (EmitIndex)
            {
                return Interlocked.Increment(ref emitCount);
            }
            else
            {
                return -1;
            }
        }

        /// <inheritdoc/>
        public LoggerCreatorDelegate LoggerCreator
        {
            get => this.loggerCreator;

            set
            {
                // We're going to clear any cached loggers so they will be recreated
                // using the new create function as necessary.

                lock (syncRoot)
                {
                    categoryToLogger.Clear();

                    this.loggerCreator = value;
                }
            }
        }

        /// <summary>
        /// Uses the <see cref="LoggerCreator"/> function to construct a logger for a specific 
        /// source category name.
        /// </summary>
        /// <param name="categoryName">
        /// Optionally identifies the event source category.  This is typically used 
        /// for identifying the event source.
        /// </param>
        /// <param name="writer">Optionally specifies a target <see cref="TextWriter"/>.</param>
        /// <param name="attributes">
        /// Specifies attributes to be included in every event logged by the logger returned.  This may
        /// be passed as <c>null</c>.
        /// </param>
        /// <param name="logFilter">
        /// Optionally overrides the manager's log filter predicate.  This examines the <see cref="LogEvent"/>
        /// and returns <c>true</c> if the event should be logged or <c>false</c> when it is to be ignored.  
        /// All events will be logged when this is and the managers filter is <c>null</c>.
        /// </param>
        /// <param name="isLogEnabledFunc">
        /// Optionally specifies a function that will be called at runtime to determine whether to event
        /// logging is actually enabled.  This overrides the parent <see cref="ITelemetryHub"/> function
        /// if any.  Events will be logged for <c>null</c> functions.
        /// </param>
        /// <returns>The <see cref="INeonLogger"/> instance.</returns>
        private INeonLogger CreateLogger(string categoryName, TextWriter writer, IEnumerable<KeyValuePair<string, string>> attributes, Func<LogEvent, bool> logFilter, Func<bool> isLogEnabledFunc)
        {
            logFilter        ??= this.logFilter;
            isLogEnabledFunc ??= this.isLogEnabledFunc;

            if (LoggerCreator == null)
            {
                return new TextLogger(this, categoryName, writer: writer, logFilter: logFilter, isLogEnabledFunc: isLogEnabledFunc);
            }
            else
            {
                return loggerCreator(this, categoryName, writer: writer, logFilter: logFilter, isLogEnabledFunc: isLogEnabledFunc);
            }
        }

        /// <summary>
        /// Manages the creation and caching of an <see cref="INeonLogger"/> based on its category name, if any.
        /// There's no reason to create new loggers for the same category name.
        /// </summary>
        /// <param name="categoryName">
        /// Optionally identifies the event source category.  This is typically used 
        /// for identifying the event source.
        /// </param>
        /// <param name="writer">Optionally specifies a target <see cref="TextWriter"/>.</param>
        /// <param name="attributes">
        /// Optionally specifies attributes to be included in every event logged by the logger returned.  This may
        /// be passed as <c>null</c>.
        /// </param>
        /// <param name="logFilter">
        /// Optionally overrides the manager's log filter predicate.  This examines the <see cref="LogEvent"/>
        /// and returns <c>true</c> if the event should be logged or <c>false</c> when it is to be ignored.  
        /// All events will be logged when this is and the managers filter is <c>null</c>.
        /// </param>
        /// <param name="isLogEnabledFunc">
        /// Optionally specifies a function that will be called at runtime to determine whether to event
        /// logging is actually enabled.  This overrides the parent <see cref="ITelemetryHub"/> function
        /// if any.  Events will be logged for <c>null</c> functions.
        /// </param>
        /// <returns>The <see cref="INeonLogger"/> instance.</returns>
        private INeonLogger InternalGetLogger(string categoryName, TextWriter writer = null, IEnumerable<KeyValuePair<string, string>> attributes = null, Func<LogEvent, bool> logFilter = null, Func<bool> isLogEnabledFunc = null)
        {
            categoryName = categoryName ?? string.Empty;

            lock (syncRoot)
            {
                if (!categoryToLogger.TryGetValue((string)categoryName, out var logger))
                {
                    logger = CreateLogger((string)categoryName, writer: writer, attributes:attributes, logFilter: logFilter, isLogEnabledFunc: isLogEnabledFunc);

                    categoryToLogger.Add((string)categoryName, logger);
                }

                return logger;
            }
        }

        /// <inheritdoc/>
        public INeonLogger GetLogger(string categoryName = null, IEnumerable<KeyValuePair<string, string>> attributes = null, Func<LogEvent, bool> logFilter = null, Func<bool> isLogEnabledFunc = null)
        {
            return InternalGetLogger(categoryName, writer, attributes, logFilter, isLogEnabledFunc);
        }

        /// <inheritdoc/>
        public INeonLogger GetLogger(Type type, IEnumerable<KeyValuePair<string, string>> attributes = null, Func<LogEvent, bool> logFilter = null, Func<bool> isLogEnabledFunc = null)
        {
            return InternalGetLogger(type.FullName, writer, attributes, logFilter, isLogEnabledFunc);
        }

        /// <inheritdoc/>
        public INeonLogger GetLogger<T>(IEnumerable<KeyValuePair<string, string>> attributes = null, Func<LogEvent, bool> logFilter = null, Func<bool> isLogEnabledFunc = null)
        {
            return InternalGetLogger(typeof(T).FullName, writer, attributes, logFilter, isLogEnabledFunc);
        }

        //---------------------------------------------------------------------
        // ILoggerProvider implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Releases all associated resources.
        /// </summary>
        /// <param name="disposing">Pass <c>true</c> if we're disposing, <c>false</c> if we're finalizing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Creates a logger.
        /// </summary>
        /// <param name="categoryName">
        /// Optionally identifies the event source category.  This is typically used 
        /// for identifying the event source.
        /// </param>
        /// <returns>The created <see cref="ILogger"/>.</returns>
        public ILogger CreateLogger(string categoryName)
        {

            return (ILogger)GetLogger(categoryName: categoryName);
        }
    }
}
