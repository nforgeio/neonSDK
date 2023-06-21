//-----------------------------------------------------------------------------
// FILE:        Test_Logging.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Neon.Common;
using Neon.Diagnostics;
using Neon.Xunit;

using Xunit;
using Neon.IO;
using Microsoft.AspNetCore.Mvc;

namespace TestCommon
{
    [Trait(TestTrait.Category, TestArea.NeonCommon)]
    public partial class Test_Logging
    {
        [Fact]
        public void JsonConsoleExporter_WithStockLogger()
        {
            // Verify that the stock MSFT [ILogger] implementation is compatible with
            // our JSON exporter.  We're going to do this by intercepting the log events
            // before they are emitted and also intercepting the text written to the
            // output streams.
            //
            // We're going to use lists to capture these things while logging and then
            // examine what we intercepted afterwards to verify that things worked as
            // expected.

            const string serviceName    = "my-service";
            const string serviceVersion = "1.2.3";
            const string categoryName   = "my-category";

            var interceptedEvents = new List<LogEvent>();
            var interceptedStdOut = new List<string>();
            var interceptedStdErr = new List<string>();
            var utcNow            = DateTime.UtcNow;

            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource(serviceName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: serviceVersion))
                .Build();

            using var loggerFactory = LoggerFactory.Create(
                builder =>
                {
                    builder
                        .SetMinimumLevel(LogLevel.Debug)
                        .AddOpenTelemetry(
                            options =>
                            {
                                options.ParseStateValues        = true;
                                options.IncludeFormattedMessage = true;
                                options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: serviceVersion));
                                options.AddConsoleJsonExporter(
                                    options =>
                                    {
                                        options.SingleLine           = true;
                                        options.Emit                 = false;
                                        options.ExceptionStackTraces = true;
                                        options.StandardErrorLevel   = LogLevel.Warning;
                                        options.LogEventInterceptor  = logEvent => interceptedEvents.Add(logEvent.Clone());
                                        options.StdErrInterceptor    = text => interceptedStdErr.Add(text);
                                        options.StdOutInterceptor    = text => interceptedStdOut.Add(text);
                                    });
                            });
                });

            var logger = loggerFactory.CreateLogger(categoryName);

            using var activityListener = new ActivityListener()
            {
                ShouldListenTo      = s => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
                Sample              = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
            };

            ActivitySource.AddActivityListener(activityListener);

            using var activitySource = new ActivitySource("");

            //-----------------------------------------------------------------
            // Local method to clear intercepted stuff and other locals to
            // prepare for another test.

            void Clear()
            {
                utcNow = DateTime.UtcNow;

                interceptedEvents.Clear();
                interceptedStdErr.Clear();
                interceptedStdOut.Clear();
            }

            //-----------------------------------------------------------------
            // Verify that we can use the stock logger to log an unformatted message.

            Clear();

            logger.LogInformation("test message");

            Assert.Single(interceptedEvents);
            Assert.Single(interceptedStdOut);
            Assert.Empty(interceptedStdErr);

            var logEvent = interceptedEvents.Single();

            Assert.True(utcNow <= NeonHelper.UnixEpochNanosecondsToDateTimeUtc(logEvent.TsNs));
            Assert.Equal("test message", logEvent.Body);
            Assert.Equal(categoryName, logEvent.CategoryName);
            Assert.Equal("Information", logEvent.Severity);
            Assert.Equal((int)SeverityNumber.SEVERITY_NUMBER_INFO, logEvent.SeverityNumber);
            Assert.Null(logEvent.SpanId);
            Assert.Null(logEvent.TraceId);
            Assert.NotNull(logEvent.Attributes);
            Assert.NotNull(logEvent.Resources);
            Assert.Equal(serviceName, logEvent.Resources.Single(item => item.Key == "service.name").Value);
            Assert.Equal(serviceVersion, logEvent.Resources.Single(item => item.Key == "service.version").Value);
            Assert.NotEmpty((string)logEvent.Resources.Single(item => item.Key == "service.instance.id").Value);

            //-----------------------------------------------------------------
            // Verify that we can use the stock logger to log a formatted message.

            Clear();

            logger.LogInformation("test message: {foo}", "bar");

            Assert.Single(interceptedEvents);
            Assert.Single(interceptedStdOut);
            Assert.Empty(interceptedStdErr);

            logEvent = interceptedEvents.Single();

            Assert.True(utcNow <= NeonHelper.UnixEpochNanosecondsToDateTimeUtc(logEvent.TsNs));
            Assert.Equal("test message: bar", logEvent.Body);
            Assert.Equal(categoryName, logEvent.CategoryName);
            Assert.Equal("Information", logEvent.Severity);
            Assert.Equal((int)SeverityNumber.SEVERITY_NUMBER_INFO, logEvent.SeverityNumber);
            Assert.Null(logEvent.SpanId);
            Assert.Null(logEvent.TraceId);
            Assert.NotNull(logEvent.Resources);
            Assert.Equal(serviceName, logEvent.Resources.Single(item => item.Key == "service.name").Value);
            Assert.Equal(serviceVersion, logEvent.Resources.Single(item => item.Key == "service.version").Value);
            Assert.NotEmpty((string)logEvent.Resources.Single(item => item.Key == "service.instance.id").Value);

            Assert.NotNull(logEvent.Attributes);
            Assert.Equal("bar", logEvent.Attributes["foo"]);

            //-----------------------------------------------------------------
            // Verify that a single line of JSON is emitted and that when parsed, it
            // matches the intercepted event.

            Clear();

            logger.LogCritical("test message");

            Assert.Single(interceptedEvents);
            Assert.Empty(interceptedStdOut);
            Assert.Single(interceptedStdErr);

            var jsonText    = interceptedStdErr.Single();
            var parsedEvent = JsonConvert.DeserializeObject<LogEvent>(jsonText);

            Assert.Equal(1, interceptedStdErr.Single().Count(ch => ch == '\n'));
            Assert.Equal(JsonConvert.SerializeObject(interceptedEvents.Single(), Formatting.None), JsonConvert.SerializeObject(parsedEvent, Formatting.None)); 

            //-----------------------------------------------------------------
            // Verify that events logged with levels higher than Information are emitted.

            Clear();

            logger.LogCritical("test message");

            Assert.Single(interceptedEvents);
            Assert.Empty(interceptedStdOut);
            Assert.Single(interceptedStdErr);

            logEvent = interceptedEvents.Single();

            Assert.True(utcNow <= NeonHelper.UnixEpochNanosecondsToDateTimeUtc(logEvent.TsNs));
            Assert.Equal("test message", logEvent.Body);
            Assert.Equal(categoryName, logEvent.CategoryName);
            Assert.Equal("Critical", logEvent.Severity);
            Assert.Equal((int)SeverityNumber.SEVERITY_NUMBER_FATAL, logEvent.SeverityNumber);
            Assert.Null(logEvent.SpanId);
            Assert.Null(logEvent.TraceId);
            Assert.NotNull(logEvent.Attributes);
            Assert.NotNull(logEvent.Resources);
            Assert.Equal(serviceName, logEvent.Resources.Single(item => item.Key == "service.name").Value);
            Assert.Equal(serviceVersion, logEvent.Resources.Single(item => item.Key == "service.version").Value);
            Assert.NotEmpty((string)logEvent.Resources.Single(item => item.Key == "service.instance.id").Value);

            //-----------------------------------------------------------------
            // Verify that events logged with levels lower than Debug are NOT emitted.

            Clear();

            logger.LogTrace("test message");

            Assert.Empty(interceptedEvents);
            Assert.Empty(interceptedStdOut);
            Assert.Empty(interceptedStdErr);

            //-----------------------------------------------------------------
            // We configured the exporter to send Information and lower events
            // (but higher than Trace) to STDTOUT and events higher than Information
            // to STDERR.  Let's verify that this is working.

            Clear();

            logger.LogWarning("warning");
            logger.LogInformation("information");
            logger.LogDebug("debug");

            Assert.NotEmpty(interceptedEvents);
            Assert.NotEmpty(interceptedStdOut);
            Assert.NotEmpty(interceptedStdErr);

            var stdOutText0  = interceptedStdOut.ElementAtOrDefault(0);
            var stdOutEvent0 = JsonConvert.DeserializeObject<LogEvent>(stdOutText0);

            Assert.NotNull(stdOutEvent0);
            Assert.Equal("information", stdOutEvent0.Body);

            var stdOutText1  = interceptedStdOut.ElementAtOrDefault(1);
            var stdOutEvent1 = JsonConvert.DeserializeObject<LogEvent>(stdOutText1);

            Assert.NotNull(stdOutEvent1);
            Assert.Equal("debug", stdOutEvent1.Body);

            var stdErrText0  = interceptedStdErr.ElementAtOrDefault(0);
            var stdErrEvent0 = JsonConvert.DeserializeObject<LogEvent>(stdErrText0);

            Assert.NotNull(stdErrEvent0);
            Assert.Equal("warning", stdErrEvent0.Body);

            // [AggregeteException] with multiple inner exceptions.

            Clear();

            try
            {
                Exception e0;
                Exception e1;

                try
                {
                    throw new Exception("exception-0");
                }
                catch (Exception e)
                {
                    e0 = e;
                }

                try
                {
                    throw new FormatException("exception-1");
                }
                catch (Exception e)
                {
                    e1 = e;
                }

                var aggregeteException = new AggregateException("There be an exception!", new Exception[] { e0, e1 });

                throw aggregeteException;
            }
            catch (AggregateException e)
            {
                logger.LogError(e, "There be an exception!");

                var exceptionEventText = interceptedStdErr.ElementAtOrDefault(0);
                var exceptionEvent = JsonConvert.DeserializeObject<LogEvent>(exceptionEventText);

                Assert.Equal("There be an exception!", exceptionEvent.Body);

                Assert.Equal(typeof(AggregateException).FullName, exceptionEvent.Attributes["exception.type"]);
                Assert.NotNull(exceptionEvent.Attributes["exception.stacktrace"]);
                Assert.StartsWith("at ", (string)exceptionEvent.Attributes["exception.stacktrace"]);
            }
            catch (Exception e)
            {
                Assert.True(false, $"Expected an AggregateException, not a [{e.GetType().FullName}]");
            }

            //-----------------------------------------------------------------
            // Verify that we include the trace and span IDs when logging within
            // a span.

            Clear();

            var activity = activitySource.StartActivity();

            Assert.NotNull(activity);

            using (activity)
            {
                logger.LogInformationEx("information");
            }

            Assert.NotEmpty(interceptedEvents);

            logEvent = interceptedEvents.Single();

            Assert.NotEmpty(logEvent.TraceId);
            Assert.NotEmpty(logEvent.SpanId);
        }

        [Fact]
        public void JsonConsoleExporter_WithNeonLogger()
        {
            // Verify that our extended MSFT [ILogger] implementation is compatible with
            // our JSON exporter.  We're going to do this by intercepting the log events
            // before they are emitted and also intercepting the text written to the
            // output streams.
            //
            // We're going to use lists to capture these things while logging and then
            // examine what we intercepted afterwards to verify that things worked as
            // expected.

            const string serviceName    = "my-service";
            const string serviceVersion = "1.2.3";
            const string categoryName   = "my-category";

            var interceptedEvents = new List<LogEvent>();
            var interceptedStdOut = new List<string>();
            var interceptedStdErr = new List<string>();
            var utcNow            = DateTime.UtcNow;

            using var loggerFactory = LoggerFactory.Create(
                builder =>
                {
                    builder
                        .SetMinimumLevel(LogLevel.Debug)
                        .AddOpenTelemetry(
                            options =>
                            {
                                options.ParseStateValues        = true;
                                options.IncludeFormattedMessage = true;
                                options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: serviceVersion));
                                options.AddConsoleJsonExporter(
                                    options =>
                                    {
                                        options.SingleLine           = true;
                                        options.Emit                 = false;
                                        options.ExceptionStackTraces = true;
                                        options.StandardErrorLevel   = LogLevel.Warning;
                                        options.LogEventInterceptor  = logEvent => interceptedEvents.Add(logEvent.Clone());
                                        options.StdErrInterceptor    = text => interceptedStdErr.Add(text);
                                        options.StdOutInterceptor    = text => interceptedStdOut.Add(text);
                                    });
                            });
                });

            var logger = loggerFactory.CreateLogger(categoryName);

            using var activityListener = new ActivityListener()
            {
                ShouldListenTo      = s => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
                Sample              = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
            };

            ActivitySource.AddActivityListener(activityListener);

            using var activitySource = new ActivitySource("");

            //-----------------------------------------------------------------
            // Local method to clear intercepted stuff and other locals to
            // prepare for another test.

            void Clear()
            {
                utcNow = DateTime.UtcNow;

                interceptedEvents.Clear();
                interceptedStdErr.Clear();
                interceptedStdOut.Clear();
            }

            //-----------------------------------------------------------------
            // Verify that we can use the stock logger to log an unformatted message.

            Clear();

            logger.LogInformationEx("test message", attributes => attributes.Add("foo", "bar"));

            Assert.Single(interceptedEvents);
            Assert.Single(interceptedStdOut);
            Assert.Empty(interceptedStdErr);

            var logEvent = interceptedEvents.Single();

            Assert.True(utcNow <= NeonHelper.UnixEpochNanosecondsToDateTimeUtc(logEvent.TsNs));
            Assert.Equal("test message", logEvent.Body);
            Assert.Equal(categoryName, logEvent.CategoryName);
            Assert.Equal("Information", logEvent.Severity);
            Assert.Equal((int)SeverityNumber.SEVERITY_NUMBER_INFO, logEvent.SeverityNumber);
            Assert.Null(logEvent.SpanId);
            Assert.Null(logEvent.TraceId);
            Assert.NotNull(logEvent.Attributes);
            Assert.NotNull(logEvent.Resources);
            Assert.Equal(serviceName, logEvent.Resources.Single(item => item.Key == "service.name").Value);
            Assert.Equal(serviceVersion, logEvent.Resources.Single(item => item.Key == "service.version").Value);
            Assert.NotEmpty((string)logEvent.Resources.Single(item => item.Key == "service.instance.id").Value);

            //-----------------------------------------------------------------
            // Verify that we can use the stock logger to log a formatted message.

            Clear();

            logger.LogInformationEx("test message", attributes => attributes.Add("foo", "bar"));

            Assert.Single(interceptedEvents);
            Assert.Single(interceptedStdOut);
            Assert.Empty(interceptedStdErr);

            logEvent = interceptedEvents.Single();

            Assert.True(utcNow <= NeonHelper.UnixEpochNanosecondsToDateTimeUtc(logEvent.TsNs));
            Assert.Equal("test message", logEvent.Body);
            Assert.Equal(categoryName, logEvent.CategoryName);
            Assert.Equal("Information", logEvent.Severity);
            Assert.Equal((int)SeverityNumber.SEVERITY_NUMBER_INFO, logEvent.SeverityNumber);
            Assert.Null(logEvent.SpanId);
            Assert.Null(logEvent.TraceId);
            Assert.NotNull(logEvent.Resources);
            Assert.Equal(serviceName, logEvent.Resources.Single(item => item.Key == "service.name").Value);
            Assert.Equal(serviceVersion, logEvent.Resources.Single(item => item.Key == "service.version").Value);
            Assert.NotEmpty((string)logEvent.Resources.Single(item => item.Key == "service.instance.id").Value);

            Assert.NotNull(logEvent.Attributes);
            Assert.Equal("bar", logEvent.Attributes["foo"]);

            //-----------------------------------------------------------------
            // Verify that a single line of JSON is emitted and that when parsed, it
            // matches the intercepted event.

            Clear();

            logger.LogCriticalEx("test message");

            Assert.Single(interceptedEvents);
            Assert.Empty(interceptedStdOut);
            Assert.Single(interceptedStdErr);

            var jsonText    = interceptedStdErr.Single();
            var parsedEvent = JsonConvert.DeserializeObject<LogEvent>(jsonText);

            Assert.Equal(1, interceptedStdErr.Single().Count(ch => ch == '\n'));
            Assert.Equal(JsonConvert.SerializeObject(interceptedEvents.Single(), Formatting.None), JsonConvert.SerializeObject(parsedEvent, Formatting.None)); 

            //-----------------------------------------------------------------
            // Verify that events logged with levels higher than Information are emitted.

            Clear();

            logger.LogCriticalEx("test message");

            Assert.Single(interceptedEvents);
            Assert.Empty(interceptedStdOut);
            Assert.Single(interceptedStdErr);

            logEvent = interceptedEvents.Single();

            Assert.True(utcNow <= NeonHelper.UnixEpochNanosecondsToDateTimeUtc(logEvent.TsNs));
            Assert.Equal("test message", logEvent.Body);
            Assert.Equal(categoryName, logEvent.CategoryName);
            Assert.Equal("Critical", logEvent.Severity);
            Assert.Equal((int)SeverityNumber.SEVERITY_NUMBER_FATAL, logEvent.SeverityNumber);
            Assert.Null(logEvent.SpanId);
            Assert.Null(logEvent.TraceId);
            Assert.NotNull(logEvent.Attributes);
            Assert.Equal(categoryName, logEvent.Attributes[LogAttributeNames.CategoryName]);
            Assert.NotNull(logEvent.Resources);
            Assert.Equal(serviceName, logEvent.Resources.Single(item => item.Key == "service.name").Value);
            Assert.Equal(serviceVersion, logEvent.Resources.Single(item => item.Key == "service.version").Value);
            Assert.NotEmpty((string)logEvent.Resources.Single(item => item.Key == "service.instance.id").Value);

            //-----------------------------------------------------------------
            // Verify that events logged with levels lower than Debug are NOT emitted.

            Clear();

            logger.LogTraceEx("test message");

            Assert.Empty(interceptedEvents);
            Assert.Empty(interceptedStdOut);
            Assert.Empty(interceptedStdErr);

            //-----------------------------------------------------------------
            // We configured the exporter to send Information and lower events
            // (but higher than Trace) to STDTOUT and events higher than Information
            // to STDERR.  Let's verify that this is working.

            Clear();

            logger.LogWarningEx("warning");
            logger.LogInformationEx("information");
            logger.LogDebugEx("debug");

            Assert.NotEmpty(interceptedEvents);
            Assert.NotEmpty(interceptedStdOut);
            Assert.NotEmpty(interceptedStdErr);

            var stdOutText0  = interceptedStdOut.ElementAtOrDefault(0);
            var stdOutEvent0 = JsonConvert.DeserializeObject<LogEvent>(stdOutText0);

            Assert.NotNull(stdOutEvent0);
            Assert.Equal("information", stdOutEvent0.Body);

            var stdOutText1  = interceptedStdOut.ElementAtOrDefault(1);
            var stdOutEvent1 = JsonConvert.DeserializeObject<LogEvent>(stdOutText1);

            Assert.NotNull(stdOutEvent1);
            Assert.Equal("debug", stdOutEvent1.Body);

            var stdErrText0  = interceptedStdErr.ElementAtOrDefault(0);
            var stdErrEvent0 = JsonConvert.DeserializeObject<LogEvent>(stdErrText0);

            Assert.NotNull(stdErrEvent0);
            Assert.Equal("warning", stdErrEvent0.Body);

            //-----------------------------------------------------------------
            // Verify that exception logging works.

            // Without an explict message.

            Clear();

            try
            {
                ThrowException("Test-Exception");
            }
            catch (Exception e)
            {
                logger.LogErrorEx(e);

                var exceptionEventText = interceptedStdErr.ElementAtOrDefault(0);
                var exceptionEvent     = JsonConvert.DeserializeObject<LogEvent>(exceptionEventText);

                Assert.Equal(NeonHelper.ExceptionError(e), exceptionEvent.Body);

                Assert.Equal(typeof(Exception).FullName, exceptionEvent.Attributes["exception.type"]);
                Assert.NotNull(exceptionEvent.Attributes["exception.stacktrace"]);
                Assert.StartsWith("at ", (string)exceptionEvent.Attributes["exception.stacktrace"]);
            }

            // With an explict message.

            Clear();

            try
            {
                ThrowException("Test-Exception");
            }
            catch (Exception e)
            {
                logger.LogErrorEx(e, "There be an exception!");

                var exceptionEventText = interceptedStdErr.ElementAtOrDefault(0);
                var exceptionEvent     = JsonConvert.DeserializeObject<LogEvent>(exceptionEventText);

                Assert.Equal("There be an exception!", exceptionEvent.Body);

                Assert.Equal(typeof(Exception).FullName, exceptionEvent.Attributes["exception.type"]);
                Assert.NotNull(exceptionEvent.Attributes["exception.stacktrace"]);
                Assert.StartsWith("at ", (string)exceptionEvent.Attributes["exception.stacktrace"]);
            }

            // [AggregeteException] with multiple inner exceptions.

            Clear();

            try
            {
                Exception e0;
                Exception e1;

                try
                {
                    throw new Exception("exception-0");
                }
                catch (Exception e)
                {
                    e0 = e;
                }

                try
                {
                    throw new FormatException("exception-1");
                }
                catch (Exception e)
                {
                    e1 = e;
                }

                var aggregeteException = new AggregateException("There be an exception!", new Exception[] { e0, e1 });

                throw aggregeteException;
            }
            catch (AggregateException e)
            {
                logger.LogErrorEx(e, "There be an exception!");

                var exceptionEventText = interceptedStdErr.ElementAtOrDefault(0);
                var exceptionEvent     = JsonConvert.DeserializeObject<LogEvent>(exceptionEventText);

                Assert.Equal("There be an exception!", exceptionEvent.Body);

                Assert.Equal(typeof(AggregateException).FullName, exceptionEvent.Attributes["exception.type"]);
                Assert.NotNull(exceptionEvent.Attributes["exception.stacktrace"]);
                Assert.StartsWith("at ", (string)exceptionEvent.Attributes["exception.stacktrace"]);
            }
            catch (Exception e)
            {
                Assert.True(false, $"Expected an AggregateException, not a [{e.GetType().FullName}]");
            }

            //-----------------------------------------------------------------
            // Verify that we include the trace and span IDs when logging within
            // a span.

            Clear();

            var activity = activitySource.StartActivity();

            Assert.NotNull(activity);

            using (activity)
            {
                logger.LogInformationEx("information");
            }

            Assert.NotEmpty(interceptedEvents);

            logEvent = interceptedEvents.Single();

            Assert.NotEmpty(logEvent.TraceId);
            Assert.NotEmpty(logEvent.SpanId);
        }

        [Fact]
        public void LoggerWithAttributes()
        {
            // Verify that [LoggerWithAttributes] actually adds tags to logged events.

            const string serviceName    = "my-service";
            const string serviceVersion = "1.2.3";
            const string categoryName   = "my-category";

            var interceptedEvents = new List<LogEvent>();
            var interceptedStdOut = new List<string>();
            var interceptedStdErr = new List<string>();
            var utcNow            = DateTime.UtcNow;

            using var loggerFactory = LoggerFactory.Create(
                builder =>
                {
                    builder
                        .SetMinimumLevel(LogLevel.Debug)
                        .AddOpenTelemetry(
                            options =>
                            {
                                options.ParseStateValues        = true;
                                options.IncludeFormattedMessage = true;
                                options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: serviceVersion));
                                options.AddConsoleJsonExporter(
                                    options =>
                                    {
                                        options.SingleLine           = true;
                                        options.Emit                 = false;
                                        options.ExceptionStackTraces = true;
                                        options.StandardErrorLevel   = LogLevel.Warning;
                                        options.LogEventInterceptor  = logEvent => interceptedEvents.Add(logEvent.Clone());
                                        options.StdErrInterceptor    = text => interceptedStdErr.Add(text);
                                        options.StdOutInterceptor    = text => interceptedStdOut.Add(text);
                                    });
                            });
                });

            var logger = loggerFactory.CreateLogger(categoryName)
                .AddAttributes(
                    attributes =>
                    {
                        attributes.Add("default-0", "0");
                        attributes.Add("default-1", "1");
                    });

            using var activityListener = new ActivityListener()
            {
                ShouldListenTo      = s => true,
                SampleUsingParentId = (ref ActivityCreationOptions<string> activityOptions) => ActivitySamplingResult.AllData,
                Sample              = (ref ActivityCreationOptions<ActivityContext> activityOptions) => ActivitySamplingResult.AllData
            };

            ActivitySource.AddActivityListener(activityListener);

            using var activitySource = new ActivitySource("");

            //-----------------------------------------------------------------
            // Local method to clear intercepted stuff and other locals to
            // prepare for another test.

            void Clear()
            {
                utcNow = DateTime.UtcNow;

                interceptedEvents.Clear();
                interceptedStdErr.Clear();
                interceptedStdOut.Clear();
            }

            //-----------------------------------------------------------------
            // Verify that logged events include the default tags added to the logger.

            Clear();

            logger.LogInformationEx("test message", attributes => attributes.Add("foo", "bar"));

            Assert.Single(interceptedEvents);
            Assert.Single(interceptedStdOut);
            Assert.Empty(interceptedStdErr);

            var logEvent = interceptedEvents.Single();

            Assert.True(utcNow <= NeonHelper.UnixEpochNanosecondsToDateTimeUtc(logEvent.TsNs));
            Assert.Equal("test message", logEvent.Body);
            Assert.Equal(categoryName, logEvent.CategoryName);
            Assert.Equal("Information", logEvent.Severity);
            Assert.Equal("0", logEvent.Attributes["default-0"]);
            Assert.Equal("1", logEvent.Attributes["default-1"]);
            Assert.Equal("bar", logEvent.Attributes["foo"]);

            //-----------------------------------------------------------------
            // Verify that tags explicitly included in logged events override any
            // tags held by the [ILogger].

            Clear();

            logger.LogInformationEx("test message", 
                attributes =>
                    {
                        attributes.Add("foo", "bar");
                        attributes.Add("default-0", "OVERRIDDEN!");
                    });

            Assert.Single(interceptedEvents);
            Assert.Single(interceptedStdOut);
            Assert.Empty(interceptedStdErr);

            logEvent = interceptedEvents.Single();

            Assert.True(utcNow <= NeonHelper.UnixEpochNanosecondsToDateTimeUtc(logEvent.TsNs));
            Assert.Equal("test message", logEvent.Body);
            Assert.Equal(categoryName, logEvent.CategoryName);
            Assert.Equal("Information", logEvent.Severity);
            Assert.Equal("OVERRIDDEN!", logEvent.Attributes["default-0"]);
            Assert.Equal("1", logEvent.Attributes["default-1"]);
            Assert.Equal("bar", logEvent.Attributes["foo"]);
        }

        [Fact]
        public void FileLogExporter_Human()
        {
            // Confirm that the FileLogExporter works for our bespoke Human format.

            const string serviceName    = "my-service";
            const string serviceVersion = "1.2.3";
            const string categoryName   = "my-category";
            const string logFileName    = "test.log";

            var utcNow            = DateTime.UtcNow;
            var interceptedEvents = new List<LogEvent>();

            using (var tempFolder = new TempFolder())
            {
                var logFilePath = Path.Combine(tempFolder.Path, logFileName);

                using var loggerFactory = LoggerFactory.Create(
                    builder =>
                    {
                        builder
                            .SetMinimumLevel(LogLevel.Debug)
                            .AddOpenTelemetry(
                                options =>
                                {
                                    options.ParseStateValues        = true;
                                    options.IncludeFormattedMessage = true;
                                    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: serviceVersion));
                                    options.AddFileExporter(
                                        options =>
                                        {
                                            options.Format              = FileLogExporterFormat.Human;
                                            options.LogFolder           = tempFolder.Path;
                                            options.LogFileName         = logFileName;
                                            options.LogEventInterceptor = logEvent => interceptedEvents.Add(logEvent.Clone());
                                        });
                                });
                    });

                var logger = loggerFactory.CreateLogger(categoryName)
                    .AddAttributes(
                        attributes =>
                        {
                            attributes.Add("default-0", "0");
                            attributes.Add("default-1", "1");
                        });

                Exception testException = null;

                try
                {
                    ThrowException("Test-Exception");
                }
                catch (Exception e)
                {
                    testException = e;
                }

                // Log two events.

                logger.LogInformationEx("event #0",
                    attributes =>
                    {
                        attributes.Add("tag-1", "ONE");
                        attributes.Add("tag-2", "TWO");
                    });

                logger.LogErrorEx(testException, "event #1",
                    attributes =>
                    {
                        attributes.Add("tag-1", "ONE");
                        attributes.Add("tag-2", "TWO");
                    });

                // Ensure that we intercepted the events that we logged.

                Assert.Equal(2, interceptedEvents.Count);

                Assert.Equal("event #0", interceptedEvents[0].Body);
                Assert.Equal("event #1", interceptedEvents[1].Body);

                // Ensure that that event times are reasonable.

                foreach (var @event in interceptedEvents)
                {
                    Assert.True(NeonHelper.UnixEpochNanosecondsToDateTimeUtc(@event.TsNs) >= utcNow);
                }

                // $todo(jefflill):
                //
                // I've manually confirmed that the logs look OK but we're not doing to
                // try parsing and verifying the human readable format at this time.
            }
        }

        [Fact]
        public void FileLogExporter_Json()
        {
            // Confirm that the FileLogExporter works for the JSON format.

            const string serviceName    = "my-service";
            const string serviceVersion = "1.2.3";
            const string categoryName   = "my-category";
            const string logFileName    = "test.log";

            var utcNow            = DateTime.UtcNow;
            var interceptedEvents = new List<LogEvent>();

            using (var tempFolder = new TempFolder())
            {
                var logFilePath = Path.Combine(tempFolder.Path, logFileName);

                using var loggerFactory = LoggerFactory.Create(
                    builder =>
                    {
                        builder
                            .SetMinimumLevel(LogLevel.Debug)
                            .AddOpenTelemetry(
                                options =>
                                {
                                    options.ParseStateValues        = true;
                                    options.IncludeFormattedMessage = true;
                                    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: serviceVersion));
                                    options.AddFileExporter(
                                        options =>
                                        {
                                            options.Format              = FileLogExporterFormat.Json;
                                            options.LogFolder           = tempFolder.Path;
                                            options.LogFileName         = logFileName;
                                            options.LogEventInterceptor = logEvent => interceptedEvents.Add(logEvent.Clone());
                                        });
                                });
                    });

                var logger = loggerFactory.CreateLogger(categoryName)
                    .AddAttributes(
                        attributes =>
                        {
                            attributes.Add("default-0", "0");
                            attributes.Add("default-1", "1");
                        });

                Exception testException = null;

                try
                {
                    ThrowException("Test-Exception");
                }
                catch (Exception e)
                {
                    testException = e;
                }

                // Log two events.

                logger.LogInformationEx("event #0",
                    attributes =>
                    {
                        attributes.Add("tag-1", "ONE");
                        attributes.Add("tag-2", "TWO");
                    });

                logger.LogErrorEx(testException, "event #1",
                    attributes =>
                    {
                        attributes.Add("tag-1", "ONE");
                        attributes.Add("tag-2", "TWO");
                    });

                // Ensure that we intercepted the events that we logged.

                Assert.Equal(2, interceptedEvents.Count);

                Assert.Equal("event #0", interceptedEvents[0].Body);
                Assert.Equal("event #1", interceptedEvents[1].Body);

                // Ensure that that event times are reasonable.

                foreach (var @event in interceptedEvents)
                {
                    Assert.True(NeonHelper.UnixEpochNanosecondsToDateTimeUtc(@event.TsNs) >= utcNow);
                }

                // Open the log file and confirm that the events were logged as JSON.

                Assert.True(File.Exists(logFilePath));

                using (var logStream = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = new StreamReader(logStream))
                    {
                        for (int i = 0; i < interceptedEvents.Count; i++)
                        {
                            LogEvent interceptedEvent;
                            LogEvent fileEvent;

                            var line = reader.ReadLine();

                            Assert.NotNull(line);
                            Assert.NotEmpty(line);

                            interceptedEvent = interceptedEvents[i];
                            fileEvent        = NeonHelper.JsonDeserialize<LogEvent>(line);

                            Assert.Equal(NeonHelper.JsonSerialize(interceptedEvent), NeonHelper.JsonSerialize(fileEvent));
                        }
                    }
                }
            }
        }

        [Fact]
        public void FileLogExporter_Rotation()
        {
            // Verify that file log rotation works.
            //
            // We're going to do this by reducing the max log file size to 10 KiB
            // and setting the maximum number of retained log files to 10 and then
            // writing log events while watching and confirming the rotation behavior.

            // Confirm that the FileLogExporter works for the JSON format.

            const string serviceName    = "my-service";
            const string serviceVersion = "1.2.3";
            const string categoryName   = "my-category";
            const string logFileName    = "test.log";

            var utcNow            = DateTime.UtcNow;
            var interceptedEvents = new List<LogEvent>();

            using (var tempFolder = new TempFolder())
            {
                var logFilePath = Path.Combine(tempFolder.Path, logFileName);

                using var loggerFactory = LoggerFactory.Create(
                    builder =>
                    {
                        builder
                            .SetMinimumLevel(LogLevel.Debug)
                            .AddOpenTelemetry(
                                options =>
                                {
                                    options.ParseStateValues        = true;
                                    options.IncludeFormattedMessage = true;
                                    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName, serviceVersion: serviceVersion));
                                    options.AddFileExporter(
                                        options =>
                                        {
                                            options.Format              = FileLogExporterFormat.Json;
                                            options.LogFolder           = tempFolder.Path;
                                            options.LogFileName         = logFileName;
                                            options.FileLimit           = (long)(10 * ByteUnits.KibiBytes);
                                            options.MaxLogFiles         = 10;
                                            options.LogEventInterceptor = logEvent => interceptedEvents.Add(logEvent.Clone());
                                        });
                                });
                    });

                var logger = loggerFactory.CreateLogger(categoryName)
                    .AddAttributes(
                        attributes =>
                        {
                            attributes.Add("default-0", "0");
                            attributes.Add("default-1", "1");
                        });

                // Write 150 log entries with 1 KiB character messages.  This should result in 
                // 10 log files, but no more.  We'll confirm this as well as confirm that
                // all log files besides the current one are at least 10 KiB in size.

                var message = new string('x', 1024);

                for (int i = 0; i < 150; i++)
                {
                    logger.LogInformationEx(message);
                }

                // Verify that the current log file exists.

                Assert.True(File.Exists(logFilePath));

                // Verify that we have 9 rotated files and that they're
                // all at or over the size limit.

                var rotatedPaths = Directory.GetFiles(tempFolder.Path, "test-*.log", SearchOption.TopDirectoryOnly);

                Assert.Equal(9, rotatedPaths.Length);

                foreach (var rotatedPath in rotatedPaths)
                {
                    Assert.True(File.Exists(rotatedPath));
                    Assert.True(new FileInfo(rotatedPath).Length >= (long)(10 * ByteUnits.KibiBytes));
                }
            }
        }

        /// <summary>
        /// Used to throw an exception with multiple frames in the stack trace
        /// to test how these are formatted.
        /// </summary>
        /// <param name="message">The exception message.</param>
        private void ThrowException(string message)
        {
            throw new Exception(message);
        }
    }
}
