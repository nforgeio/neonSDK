//-----------------------------------------------------------------------------
// FILE:	    KestrelMetrics.cs
// CONTRIBUTOR: Marcus Bowyer
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

using Prometheus;

using Yarp.Telemetry.Consumption;

namespace NeonSignalRProxy
{
    /// <inheritdoc/>
    public sealed class KestrelMetrics : IMetricsConsumer<Yarp.Telemetry.Consumption.KestrelMetrics>
    {
        private static readonly Counter Connections = Metrics.CreateCounter(
            "neonsignalrproxy_kestrel_connections_total",
            "Number of incomming connections opened");

        private static readonly Counter TlsHandshakes = Metrics.CreateCounter(
            "neonsignalrproxy_kestrel_tls_Handshakes_total",
            "Numer of TLS handshakes started");

        private static readonly Gauge _currentTlsHandshakes = Metrics.CreateGauge(
            "neonsignalrproxy_kestrel_current_tls_handshakes",
            "Number of active TLS handshakes that have started but not yet completed or failed");

        private static readonly Counter _failedTlsHandshakes = Metrics.CreateCounter(
            "neonsignalrproxy_kestrel_failed_tls_handshakes_total",
            "Number of TLS handshakes that failed");

        private static readonly Gauge _currentConnections = Metrics.CreateGauge(
            "neonsignalrproxy_kestrel_current_connections",
            "Number of currently open incomming connections");

        private static readonly Gauge _connectionQueueLength = Metrics.CreateGauge(
            "neonsignalrproxy_kestrel_connection_queue_length",
            "Number of connections on the queue.");

        private static readonly Gauge _requestQueueLength = Metrics.CreateGauge(
            "neonsignalrproxy_kestrel_request_queue_length",
            "Number of requests on the queue");

        /// <inheritdoc/>
        public void OnMetrics(
            Yarp.Telemetry.Consumption.KestrelMetrics previous, 
            Yarp.Telemetry.Consumption.KestrelMetrics current)
        {
            Connections.IncTo(current.TotalConnections);
            TlsHandshakes.IncTo(current.TotalTlsHandshakes);
            _currentTlsHandshakes.Set(current.CurrentTlsHandshakes);
            _failedTlsHandshakes.IncTo(current.FailedTlsHandshakes);
            _currentConnections.Set(current.CurrentConnections);
            _connectionQueueLength.Set(current.ConnectionQueueLength);
            _requestQueueLength.Set(current.RequestQueueLength);
        }
    }
}
