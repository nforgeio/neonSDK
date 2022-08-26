using System.Diagnostics;

using Neon.Common;
using Neon.Diagnostics;

using Microsoft.Extensions.Logging;

using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;

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
                            options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: ServiceName, serviceVersion: ServiceVersion));
                            options.AddLogAsTraceProcessor(options => options.LogLevel = LogLevel.Information);
                            options.AddConsoleJsonExporter(options => options.SingleLine = false);
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
                //logger.LogError(e, "My Exception");
            }

            using (var span = tracer.StartSpan("test"))
            {
                span.AddEvent("test-event");

                logger.LogInformationEx("Hello World!", tagSetter: tags => tags.Add("foo", "bar"));


                //logger.LogInformationEx("{tag-0}{tag-1}", "0", "1", "2", "3");
            }
        }
    }
}
