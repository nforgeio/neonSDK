// -----------------------------------------------------------------------------
// FILE:	    DnsCache.cs
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

using System.Collections.Generic;

namespace Neon.SignalR
{

    /// <summary>
    /// Provides a DNS cache for SignalR.
    /// </summary>
    public interface IDnsCache
    {

        /// <summary>
        /// List of hosts to cache.
        /// </summary>
        public HashSet<string> Hosts { get; set; }

        /// <summary>
        /// Checks to see if the cache contains the specified key.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ContainsKey(string value);

        /// <summary>
        /// Returns the current address of this server.
        /// </summary>
        public string GetSelfAddress();

        /// <summary>
        /// Sets the current address of this server.
        /// </summary>
        public void SetSelfAddress(string address);

    }

    /// <inheritdoc/>
    public class DnsCache : IDnsCache
    {
        /// <inheritdoc/>
        public HashSet<string> Hosts { get; set; } = new HashSet<string>();

        /// <inheritdoc/>
        public string GetSelfAddress() { return SelfAddress; }

        /// <inheritdoc/>
        public void SetSelfAddress(string address)
        {
            this.SelfAddress = address;
        }

        /// <inheritdoc/>
        public bool ContainsKey(string value)
        {
            return Hosts.Contains(value);
        }

        private string SelfAddress { get; set; }
    }
}
