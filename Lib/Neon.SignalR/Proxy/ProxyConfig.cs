// -----------------------------------------------------------------------------
// FILE:	    ProxyConfig.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
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

namespace Neon.SignalR
{
    /// <summary>
    /// SignalR proxy configuration.
    /// </summary>
    public class ProxyConfig
    {
        /// <summary>
        /// The address of this service. Must have valid SRV records.
        /// </summary>
        public string PeerAddress { get; set; }

        /// <summary>
        /// The service port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The hostname of the 
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// The interval to probe DNS for changes.
        /// </summary>
        public TimeSpan DnsProbeInterval { get; set; } = TimeSpan.FromSeconds(10);
    }
}
