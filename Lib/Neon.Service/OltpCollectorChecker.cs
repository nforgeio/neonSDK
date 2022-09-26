//-----------------------------------------------------------------------------
// FILE:	    OltpCollectorChecker.cs
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
    /// Used to check for the presence of a Kubernetes service listening at <see cref="NeonHelper.NeonKubeOtelCollectorUri"/> 
    /// (<b>http://neon-otel-collector</b>) in the same namespace as the <see cref="NeonService"/> to control whether to
    /// disable tracing when there is no collector service present.
    /// </summary>
    public static class OltpCollectorChecker
    {
        //---------------------------------------------------------------------
        // Private types

        /// <summary>
        /// Implements a sampler that filters out all traces when <see cref="OltpCollectorChecker.Ready"/>
        /// is <c>false</c>.
        /// </summary>
        private class ReadySampler : Sampler
        {
            private SamplingResult enabled  = new SamplingResult(SamplingDecision.RecordOnly);
            private SamplingResult disabled = new SamplingResult(SamplingDecision.Drop);

            public override SamplingResult ShouldSample(in SamplingParameters samplingParameters)
            {
                return OltpCollectorChecker.Ready ? enabled : disabled;
            }
        }

        //---------------------------------------------------------------------
        // Implementation

        private static string           oltpCollectorHost = new Uri(NeonHelper.NeonKubeOtelCollectorUri).Host;
        private static TimeSpan         checkInterval     = TimeSpan.FromSeconds(120);
        private static LookupClient     dns               = new LookupClient();

        /// <summary>
        /// Returns <c>true</c> when a collector service appears to be present in the same
        /// namespace where the <see cref="NeonService"/> is running.
        /// </summary>
        public static bool Ready { get; private set; } = false;

        /// <summary>
        /// Returns an OpenTelemetry sampler that allows all traces when <see cref="Ready"/> is
        /// <c>true</c> and ignores all traces when that is <c>false</c>.
        /// </summary>
        public static Sampler Sampler { get; private set; }

        /// <summary>
        /// Starts the loop that checks for the presence of a <b>neon-otel-collector</b> service
        /// in the current Kubernetes namespace.
        /// </summary>
        /// <param name="checkInterval">
        /// Optionally specifies the  the interval at which the presence of <b>neon-otel-collector</b>
        /// service.  This defaults to <b>120 seconds</b> but may be customized.
        /// </param>
        public static void Start(TimeSpan checkInterval = default)
        {
            Covenant.Requires<ArgumentException>(checkInterval > TimeSpan.Zero, nameof(checkInterval));

            if (checkInterval == TimeSpan.Zero)
            {
                OltpCollectorChecker.checkInterval = TimeSpan.FromSeconds(120);
            }
            else
            {
                OltpCollectorChecker.checkInterval = checkInterval;
            }

            oltpCollectorHost = new Uri(NeonHelper.NeonKubeOtelCollectorUri).Host;

            // Configure the DNS client and then do an immediate check for the
            // <b>neon-otel-collector</b> service before starting the status
            // polling loop.

            CheckForCollectorAsync().Wait();

            // Start the collector checker loop.  We're not awaiting anything here.
            // This will run continuously until the application terminates.

            _ = Task.Run(() => CollectorCheckerLoopAsync());
        }

        /// <summary>
        /// Used to periodically check for the presence of a <b>neon-otel-collector</b> service
        /// in the current namespace.  This sets the <see cref="ready"/> field to <c>true</c>
        /// when this service is present or <c>false</c> when it's not present.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        private static async Task CheckForCollectorAsync()
        {
            var hostEntry = await dns.GetHostEntryAsync(oltpCollectorHost);

            Ready = hostEntry.AddressList.Length > 0;
        }

        /// <summary>
        /// Loops in the background, checking for the presence of a reachable
        /// a <b>neon-otel-collector</b> service.
        /// </summary>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        private static async Task CollectorCheckerLoopAsync()
        {
            while (true)
            {
                await Task.Delay(checkInterval);

                try
                {
                    await CheckForCollectorAsync();
                }
                catch
                {
                    // Ignoring all exceptions.
                }
            }
        }
    }
}
