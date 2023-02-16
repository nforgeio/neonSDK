//-----------------------------------------------------------------------------
// FILE:	    OtlpCollectorChecker.cs
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
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Cryptography;
using Neon.Diagnostics;
using Neon.IO;
using Neon.Net;
using Neon.Retry;
using Neon.Tasks;
using Neon.Time;

using DnsClient;
using Prometheus;

using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Neon.Service
{
    /// <summary>
    /// <para>
    /// Used to check for the presence of a DNS host specified by a URI.  This is used
    /// by services to determine whether an OTEL Collector relay service exists in the
    /// Kubernetes namespace where the service is running.
    /// </para>
    /// <para>
    /// The idea is for the service call <see cref="Start(NeonService, Uri, TimeSpan)"/> 
    /// to start the checker and then set the trace sampler to <see cref="Sampler"/> so that 
    /// tracing will be enabled when the collector relay service exists and disable tracing 
    /// when it doesn't.
    /// </para>
    /// </summary>
    public static class OtlpCollectorChecker
    {
        //---------------------------------------------------------------------
        // Private types

        /// <summary>
        /// Implements a sampler that filters out all traces when <see cref="OtlpCollectorChecker.Ready"/>
        /// is <c>false</c>.
        /// </summary>
        private class ReadySampler : Sampler
        {
            private SamplingResult enabled  = new SamplingResult(SamplingDecision.RecordAndSample);
            private SamplingResult disabled = new SamplingResult(SamplingDecision.Drop);

            public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
            {
                return OtlpCollectorChecker.Ready ? enabled : disabled;
            }
        }

        //---------------------------------------------------------------------
        // Implementation

        private static LookupClient     dns;
        private static Uri              collectorUri;
        private static string           collectorHostName;
        private static TimeSpan         checkInterval;
        private static NeonService      service;
        private static Counter          unavailableCount;

        /// <summary>
        /// Returns <c>true</c> when a collector service appears to be present in the same
        /// namespace where the <see cref="NeonService"/> is running.
        /// </summary>
        public static bool Ready { get; private set; } = false;

        /// <summary>
        /// Returns an OpenTelemetry sampler that allows all traces when <see cref="Ready"/> is
        /// <c>true</c> and ignores all traces when that is <c>false</c>.
        /// </summary>
        public static Sampler Sampler { get; private set; } = new ReadySampler();

        /// <summary>
        /// Starts the loop that checks for the presence of a OpenTelemetry Collector service
        /// in the current Kubernetes namespace.  The <see cref="Ready"/> property will be set
        /// to <c>true</c> and the <see cref="Sampler"/> will be configured to record all traces
        /// when the collector service appears to be available.
        /// </summary>
        /// <param name="service">Passed as the <see cref="NeonService"/> instance.</param>
        /// <param name="collectorUri">
        /// Optionally specifies the target collector URI.  When this is passed as <c>null</c>,
        /// No checking checking will be performed and <see cref="Ready"/> property will always
        /// be <c>false</c> and the <see cref="Sampler"/> will always be configured to drop all 
        /// traces.
        /// </param>
        /// <param name="checkInterval">
        /// Optionally specifies the  the interval at which the presence of the collector service
        /// service.  This defaults to <b>60 seconds</b> but may be customized.
        /// </param>
        public static void Start(NeonService service, Uri collectorUri = null, TimeSpan checkInterval = default)
        {
            Covenant.Requires<ArgumentNullException>(service != null, nameof(service));
            Covenant.Requires<ArgumentException>(checkInterval >= TimeSpan.Zero, nameof(checkInterval));

            OtlpCollectorChecker.service          = service;
            OtlpCollectorChecker.collectorUri     = collectorUri;
            OtlpCollectorChecker.unavailableCount = Metrics.CreateCounter($"{NeonHelper.NeonMetricsPrefix}_otlp_collector_unavailable_total", "Number of times the OTLP Collector service has transitioned to being unavailable.");

            dns = new LookupClient(new LookupClientOptions()
            {
                UseCache            = false
            });

            if (checkInterval == TimeSpan.Zero)
            {
                OtlpCollectorChecker.checkInterval = TimeSpan.FromSeconds(60);
            }
            else
            {
                OtlpCollectorChecker.checkInterval = checkInterval;
            }

            if (collectorUri != null)
            {
                collectorHostName = collectorUri.Host;

                // Configure the DNS client and then do an immediate check for the
                // OpenTelemetry Collector service before starting the status
                // polling loop.

                try
                {
                    CheckForCollectorAsync().Wait();
                }
                catch (Exception e)
                {
                    service.Logger.LogErrorEx(e.Message);
                }

                if (!Ready)
                {
                    service.Logger.LogWarningEx(() => $"DNS lookup failed for [{collectorHostName}].  Tracing is disabled.");
                }

                // Start the collector checker loop.  Note that we're not awaiting
                // anything here.  This will run continuously until the application
                // terminates.

                _ = Task.Run(() => CheckerLoopAsync());
            }
            else
            {
                // We're going to drop all traces.

                Ready = false;
            }
        }

        /// <summary>
        /// Used to periodically check for the presence of a OpenTelemetry Collector service
        /// in the current namespace.  This sets the <see cref="Ready"/> property to <c>true</c>
        /// when this service is present or <c>false</c> when it's not present.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        private static async Task CheckForCollectorAsync()
        {
            var lookup = await dns.QueryAsync(collectorHostName, QueryType.A);
            
            Ready = !(lookup.HasError || lookup.Answers.IsEmpty());
        }

        /// <summary>
        /// Loops in the background, checking for the presence of a reachable
        /// OpenTelemetry Collector service.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        private static async Task CheckerLoopAsync()
        {
            while (true)
            {
                await Task.Delay(checkInterval);

                try
                {
                    var orgReady = Ready;

                    await CheckForCollectorAsync();

                    // Track OTLP collector availability transisions.

                    if (orgReady != Ready)
                    {
                        if (Ready)
                        {
                            service.Logger.LogInformationEx(() => $"DNS lookup succeeded for [{collectorUri}].  Tracing is enabled.");
                        }
                        else
                        {
                            service.Logger.LogWarningEx(() => $"DNS lookup failed for [{collectorUri}].  Tracing is disabled.");
                            unavailableCount.Inc();
                        }
                    }
                }
                catch
                {
                    // Ignoring all exceptions.
                }
            }
        }
    }
}
