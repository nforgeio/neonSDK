//-----------------------------------------------------------------------------
// FILE:	    Test_Logging_ConsoleJsonLogExporter.cs
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
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenTelemetry.Resources;

using Neon.Common;
using Neon.Diagnostics;
using Neon.Xunit;

using Xunit;

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
                                        options.InnerExceptions      = true;
                                        options.LogEventInterceptor  = logEvent => interceptedEvents.Add(logEvent);
                                        options.StdErrInterceptor    = text => interceptedStdErr.Add(text);
                                        options.StdOutInterceptor    = text => interceptedStdOut.Add(text);
                                    });
                            });
                });

            var logger = loggerFactory.CreateLogger(categoryName);

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
            Assert.Null(logEvent.Exception);
            Assert.Null(logEvent.Labels);
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
            Assert.Null(logEvent.Exception);
            Assert.NotNull(logEvent.Resources);
            Assert.Equal(serviceName, logEvent.Resources.Single(item => item.Key == "service.name").Value);
            Assert.Equal(serviceVersion, logEvent.Resources.Single(item => item.Key == "service.version").Value);
            Assert.NotEmpty((string)logEvent.Resources.Single(item => item.Key == "service.instance.id").Value);

            Assert.NotNull(logEvent.Labels);
            Assert.Equal("bar", logEvent.Labels["foo"]);

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
            Assert.Equal("Fatal", logEvent.Severity);
            Assert.Equal((int)SeverityNumber.SEVERITY_NUMBER_FATAL, logEvent.SeverityNumber);
            Assert.Null(logEvent.SpanId);
            Assert.Null(logEvent.TraceId);
            Assert.Null(logEvent.Exception);
            Assert.Null(logEvent.Labels);
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
        }

        [Fact]
        public void JsonConsoleExporter_WithExtendedLogger()
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
                                        options.InnerExceptions      = true;
                                        options.LogEventInterceptor  = logEvent => interceptedEvents.Add(logEvent);
                                        options.StdErrInterceptor    = text => interceptedStdErr.Add(text);
                                        options.StdOutInterceptor    = text => interceptedStdOut.Add(text);
                                    });
                            });
                });

            var logger = loggerFactory.CreateLogger(categoryName);

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

            logger.LogInformationEx("test message", tags => tags.Add("foo", "bar"));

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
            Assert.Null(logEvent.Exception);
            Assert.NotNull(logEvent.Labels);
            Assert.NotNull(logEvent.Resources);
            Assert.Equal(serviceName, logEvent.Resources.Single(item => item.Key == "service.name").Value);
            Assert.Equal(serviceVersion, logEvent.Resources.Single(item => item.Key == "service.version").Value);
            Assert.NotEmpty((string)logEvent.Resources.Single(item => item.Key == "service.instance.id").Value);

            //-----------------------------------------------------------------
            // Verify that we can use the stock logger to log a formatted message.

            Clear();

            logger.LogInformationEx("test message", tags => tags.Add("foo", "bar"));

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
            Assert.Null(logEvent.Exception);
            Assert.NotNull(logEvent.Resources);
            Assert.Equal(serviceName, logEvent.Resources.Single(item => item.Key == "service.name").Value);
            Assert.Equal(serviceVersion, logEvent.Resources.Single(item => item.Key == "service.version").Value);
            Assert.NotEmpty((string)logEvent.Resources.Single(item => item.Key == "service.instance.id").Value);

            Assert.NotNull(logEvent.Labels);
            Assert.Equal("bar", logEvent.Labels["foo"]);

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
            Assert.Equal("Fatal", logEvent.Severity);
            Assert.Equal((int)SeverityNumber.SEVERITY_NUMBER_FATAL, logEvent.SeverityNumber);
            Assert.Null(logEvent.SpanId);
            Assert.Null(logEvent.TraceId);
            Assert.Null(logEvent.Exception);
            Assert.Null(logEvent.Labels);
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
        }
    }
}
