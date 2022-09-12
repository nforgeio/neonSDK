using System.Diagnostics;

using Neon.Common;
using Neon.Diagnostics;

using Microsoft.Extensions.Logging;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TelemetryTest
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            const string ServiceName    = "Test-Service";
            const string ServiceVersion = "1.0.0";

            using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddSource(ServiceName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: ServiceName, serviceVersion: ServiceVersion))
                //.AddConsoleExporter()
                .Build();

            var loggerFactory = LoggerFactory.Create(
                builder =>
                {
                    builder.AddOpenTelemetry(
                        options =>
                        {
                            options.ParseStateValues = true;
                            options.IncludeFormattedMessage = true;
                            options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: ServiceName, serviceVersion: ServiceVersion));
                            options.AddLogAsTraceProcessor(
                                options =>
                                {
                                    options.LogLevel = LogLevel.Information;
                                });
                            options.AddConsoleJsonExporter(
                                options =>
                                {
                                    options.SingleLine = true;
                                });
                        });
                });

            var logger = loggerFactory.CreateLogger("TEST-LOGGER");
            var tracer = tracerProvider.GetTracer(ServiceName, ServiceVersion);

            try
            {
                throw new Exception("Help Me!");
            }
            catch (Exception e)
            {
                logger.LogError(e, "My Exception");
            }

            Exception inner1;
            Exception inner2;

            try
            {
                try
                {
                    throw new ArgumentException("bad-1");
                }
                catch (Exception e)
                {
                    inner1 = e;
                }

                try
                {
                    throw new ArgumentException("bad-2");
                }
                catch (Exception e)
                {
                    inner2 = e;
                }

                throw new AggregateException("Help Me Aggregate!", inner1, inner2);
            }
            catch (Exception e)
            {
                logger.LogError(e, "My Exception");
            }

            var activitySource = new ActivitySource(ServiceName, ServiceVersion);

            using (activitySource.CreateActivity("my-activity", ActivityKind.Internal))
            {
            }

            using (var span = tracer.StartSpan("test"))
            {
                span.AddEvent("test-event");

                logger.LogInformationEx("Hello World!", attributeSetter: attributes => attributes.Add("foo", "bar"));
            }
        }
    }
}
