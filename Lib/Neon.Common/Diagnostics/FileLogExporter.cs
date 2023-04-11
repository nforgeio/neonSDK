//-----------------------------------------------------------------------------
// FILE:	    FileLogExporter.cs
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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Neon.Common;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Exports log records to a file as specified by <see cref="FileLogExporterOptions"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exporter currently supports writing logs as JSON or a bespoke human readable 
    /// format (the default).  You can customize this by using options with the 
    /// <see cref="FileLogExporterOptions.Format"/> to one of the <see cref="FileLogExporterFormat"/>
    /// values (as shown below).
    /// </para>
    /// <para>
    /// <b>IMPORTANT:</b> To enable the inclusion of log tags in the output, you must
    /// set <see cref="OpenTelemetryLoggerOptions.ParseStateValues"/><c>=true</c> when
    /// configuring your OpenTelemetry options.  This is is <c>false</c> by default.
    /// </para>
    /// <code language="C#">
    /// var loggerFactory = LoggerFactory.Create(
    ///     builder =>
    ///     {
    ///         builder.AddOpenTelemetry(
    ///             options =>
    ///             {
    ///                 options.ParseStateValues = true;    // &lt;--- SET THIS TO TRUE
    ///                 
    ///                 options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: ServiceName, serviceVersion: ServiceVersion));
    ///                 options.AddFileExporter(
    ///                     options => 
    ///                     {
    ///                         options.Format      = FileLogExporterFormat.Human;
    ///                         options.LogFolder   = NeonHelper.GetBaseDirtectory();
    ///                         options.LogFileName = "MyProgram.log";
    ///                     });
    ///             });
    ///     });
    /// </code>
    /// <para>
    /// This exporter supports log file rotation.  This is controlled by the <see cref="FileLogExporterOptions.FileLimit"/>
    /// and <see cref="FileLogExporterOptions.MaxLogFiles"/> options.  When the current log file's size equals or exceeds
    /// <see cref="FileLogExporterOptions.FileLimit"/> after writing a log event, the exporter will close and rename the
    /// current file by appending a timestamp and start logging to a new file named <see cref="FileLogExporterOptions.LogFileName"/>.
    /// <see cref="FileLogExporterOptions.FileLimit"/> defaults to <b>10 MiB</b> and <see cref="FileLogExporterOptions.MaxLogFiles"/>
    /// defaults to retain <b>10</b> log files.
    /// </para>
    /// <para>
    /// The rotated files will be named like "LOGFILENAME-YYYY-MM-ddTHH-mm-ss.fffZ.EXT", where <b>LOGFILENAME</b> is the
    /// filename part of <see cref="FileLogExporterOptions.LogFileName"/> and <b>EXT</b> is the extension.
    /// </para>
    /// <para>
    /// <see cref="FileLogExporterOptions.MaxLogFiles"/> controls how many log files will be retained.
    /// </para>
    /// </remarks>
    public class FileLogExporter : BaseExporter<LogRecord>
    {
        private readonly FileLogExporterOptions         options;
        private readonly string                         logSeparator = new string('=', 80);
        private readonly LogEvent                       logEvent     = new LogEvent();
        private readonly Dictionary<string, object>     tags         = new Dictionary<string, object>();
        private readonly Dictionary<string, object>     resources    = new Dictionary<string, object>();
        private FileStream                              logStream;
        private StreamWriter                            logWriter;

        /// <summary>
        /// Constructs a log exporter that writes log records to standard output and/or
        /// standard error as single line JSON objects.
        /// </summary>
        /// <param name="options">Optionally specifies the exporter options.</param>
        public FileLogExporter(FileLogExporterOptions options = null)
        {
            this.options = options ?? new FileLogExporterOptions();

            if (string.IsNullOrEmpty(options.LogFolder))
            {
                throw new ArgumentException($"[{nameof(FileLogExporterOptions)}.{nameof(FileLogExporterOptions.LogFolder)}] must be specified.");
            }

            if (string.IsNullOrEmpty(options.LogFileName))
            {
                throw new ArgumentException($"[{nameof(FileLogExporterOptions)}.{nameof(FileLogExporterOptions.LogFileName)}] must be specified.");
            }

            // Ensure that the log folder exists.

            Directory.CreateDirectory(options.LogFolder);

            // Create or open the log output stream.

            try
            {
                logStream = new FileStream(Path.Combine(options.LogFolder, options.LogFileName), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                logWriter = new StreamWriter(logStream, Encoding.UTF8, bufferSize: 8192, leaveOpen: true);
            }
            catch (IOException)
            {
                logStream = null;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            logWriter?.Flush();
            logWriter = null;
            
            logStream?.Dispose();
            logStream = null;
        }

        /// <inheritdoc/>
        protected override bool OnForceFlush(int timeoutMilliseconds)
        {
            try
            {
                logWriter?.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        protected override bool OnShutdown(int timeoutMilliseconds)
        {
            try
            {
                logWriter?.Flush();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override ExportResult Export(in Batch<LogRecord> batch)
        {
            // It's possible that we couldn't create the log stream.

            if (logStream == null)
            {
                return ExportResult.Failure;
            }

            const string indent = "  ";

            LogEvent logEvent                     = new LogEvent();
            Dictionary<string, object>  tags      = new Dictionary<string, object>();
            Dictionary<string, object>  resources = new Dictionary<string, object>();

            foreach (var record in batch)
            {
                //-------------------------------------------------------------
                // Convert the LogRecord into a LogEvent which we'll use to render
                // the LogRecord into JSON.  Note that we're using some preallocated
                // objects here to reduce GC pressure.

                DiagnosticsHelper.SetLogEvent(this, includeStackTrace: true, record, tags, resources, logEvent);

                //-------------------------------------------------------------
                // Give any interceptor a chance to see and/or modify the event.

                options.LogEventInterceptor?.Invoke(logEvent);

                //-------------------------------------------------------------
                // Write the log record.

                try
                {
                    switch (options.Format)
                    {
                        case FileLogExporterFormat.Human:

                            logWriter?.WriteLine(logSeparator);
                            logWriter?.Write("[");
                            logWriter?.Write(NeonHelper.UnixEpochNanosecondsToDateTimeUtc(logEvent.TsNs).ToString(NeonHelper.DateFormatTZ));
                            logWriter?.Write("]: ");
                            logWriter?.WriteLine(logEvent.Body);

                            logWriter?.Write($"SEVERITY: ");
                            logWriter?.WriteLine(logEvent.Severity);

                            if (!string.IsNullOrEmpty(logEvent.CategoryName))
                            {
                                logWriter?.Write($"CATEGORY: ");
                                logWriter?.WriteLine(logEvent.CategoryName);
                            }

                            if (!string.IsNullOrEmpty(logEvent.TraceId))
                            {
                                logWriter?.Write($"TRACE-ID: ");
                                logWriter?.WriteLine(logEvent.TraceId);
                            }

                            if (!string.IsNullOrEmpty(logEvent.SpanId))
                            {
                                logWriter?.Write($"SPAN-ID: ");
                                logWriter?.WriteLine(logEvent.SpanId);
                            }

                            if (logEvent.Resources?.Count > 0)
                            {
                                logWriter?.WriteLine($"RESOURCES:");

                                foreach (var item in logEvent.Resources)
                                {
                                    logWriter?.Write(indent);
                                    logWriter?.Write(item.Key);
                                    logWriter?.Write(": ");
                                    logWriter?.WriteLine(item.Value?.ToString());
                                }
                            }

                            if (logEvent.Attributes?.Count > 0)
                            {
                                logWriter?.WriteLine($"ATTRIBUTES:");

                                foreach (var item in logEvent.Attributes)
                                {
                                    // We're going to ignore any exception related attributes
                                    // here because we call those out below.

                                    if (item.Key.StartsWith("exception."))
                                    {
                                        continue;
                                    }

                                    logWriter?.Write(indent);
                                    logWriter?.Write(item.Key);
                                    logWriter?.Write(": ");
                                    logWriter?.WriteLine(item.Value?.ToString());
                                }
                            }

                            // Write any exception information indented under the log line,
                            // when enabled.

                            if (record.Exception != null)
                            {
                                var exception = record.Exception;

                                logWriter?.Write($"EXCEPTION.TYPE: ");
                                logWriter?.WriteLine(exception.GetType().FullName);
                                logWriter?.Write($"EXCEPTION.MESSAGE: ");
                                logWriter?.WriteLine(DiagnosticsHelper.CleanExceptionMessage(exception));
                                logWriter?.WriteLine($"EXCEPTION.STACKTRACE:");

                                using (var reader = new StringReader(exception.StackTrace))
                                {
                                    foreach (var line in reader.Lines())
                                    {
                                        logWriter?.Write(indent);
                                        logWriter?.WriteLine(line.TrimStart());
                                    }
                                }
                            }

                            logWriter?.WriteLine();
                            break;

                        case FileLogExporterFormat.Json:

                            // Write the JSON formatted record as a single line.

                            logWriter?.WriteLine(JsonConvert.SerializeObject(logEvent, Formatting.None));
                            break;

                        default:

                            throw new NotImplementedException();
                    }
                }
                catch
                {
                    // It's possible that we can't write to the log file for some reason.

                    return ExportResult.Failure;
                }
            }

            if (options.FlushAgressively)
            {
                logWriter?.Flush();
            }

            //-----------------------------------------------------------------
            // Handle log file rotation:
            //
            //      1. Check to see if the current file size equals or exceeds the limit.
            //      2. Just return when we're below the threshold
            //      3. Flush the current file and then create a new rotated log file by
            //         adding a timestamp to the log file name.
            //      4. Copy the contents of the current file to the rotated one.
            //      5. Clear the current log file.

            // $note(jefflill):
            //
            // This approach never closes the current log file so anything tailing the
            // file will continue to work.  The cost is having to do the file copy instead
            // of just renaming the current file and creating a new one.
            //
            // Note also that the file size computation may be lower that what will actually
            // be written when agressive flusing is disabled.  This means it may take more
            // than one event to be logged after the limit is exceeded before we'll actually
            // detect it.  This isn't really a big deal, especially since agressive flushing
            // is enabled by default.

            if (logStream.Length >= options.FileLimit)
            {
                // Flush current writer, leaving the output stream alone.  We're not going
                // to dispose the writer though, because that closes the underlying stream.

                logWriter?.Flush();

                // Create a new rotated log file using a timestamp and then copy the contents
                // of the current log file to the rotated file.

                var logFileWithoutExtension = Path.GetFileNameWithoutExtension(options.LogFileName);
                var extension               = Path.GetExtension(options.LogFileName);

                // Generate the rotated file name including a timestamp, converting colons
                // in the timestamp to dashes to be compatiable with the Windows filesystem.

                var rotatedFileName = $"{logFileWithoutExtension}-{DateTime.UtcNow.ToString(NeonHelper.DateFormatTZ)}{extension}";

                rotatedFileName = rotatedFileName.Replace(':', '-');

                // Rotate the log file.

                using (var rotatedStream = File.Create(Path.Combine(options.LogFolder, rotatedFileName)))
                {
                    logStream.Position = 0;
                    logStream.CopyTo(rotatedStream);

                    // Clear the current log file and recreate the output stream.

                    logStream.SetLength(0);
                    logWriter = new StreamWriter(logStream);

                    // Count the number of log files present and delete the oldest files
                    // to get back to the limit, when necessary.

                    var logFilePaths = new List<string>();

                    logFilePaths.Add(Path.Combine(options.LogFolder, options.LogFileName));

                    foreach (var logPath in Directory.GetFiles(options.LogFolder, $"{logFileWithoutExtension}-*{extension}", SearchOption.TopDirectoryOnly)
                        .OrderByDescending(path => path))
                    {
                        logFilePaths.Add(logPath);
                    }

                    // $note(jefflill):
                    //
                    // The list will hold the names of all of the log files from the current log file
                    // to the oldest one, in that order.  We'll remove files from the end of the list
                    // until we get back to the limit.

                    if (logFilePaths.Count > options.MaxLogFiles)
                    {
                        logFilePaths.Reverse();

                        for (int i = 0; i < logFilePaths.Count - options.MaxLogFiles; i++)
                        {
                            NeonHelper.DeleteFile(logFilePaths[i]);
                        }
                    }
                }
            }

            return ExportResult.Success;
        }
    }
}