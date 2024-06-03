//-----------------------------------------------------------------------------
// FILE:        WebsocketMetrics.cs
// CONTRIBUTOR: Marcus Bowyer
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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

using Prometheus;

using Yarp.Telemetry.Consumption;

namespace NeonSignalRProxy
{
    /// <summary>
    /// Collects Websocket metrics.
    /// </summary>
    public class WebsocketMetrics
    {
        private static readonly string[] LabelNames = { "host", "upstream_host" };

        /// <summary>
        /// The total number of websocket connections established.
        /// </summary>
        public static readonly Counter ConnectionsEstablished = Metrics.CreateCounter(
            "neonsignalrproxy_websockets_connections_established_total",
            "Number of websocket requests inititated.",
            labelNames: LabelNames);

        /// <summary>
        /// The current number of open websocket connections.
        /// </summary>
        public static readonly Gauge CurrentConnections = Metrics.CreateGauge(
            "neonsignalrproxy_websockets_current_connections",
            "Number of active websocket connections that have are connected.",
            labelNames: LabelNames);
    }
}
