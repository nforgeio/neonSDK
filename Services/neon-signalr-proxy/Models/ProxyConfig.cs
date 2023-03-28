//-----------------------------------------------------------------------------
// FILE:	    ProxyConfig.cs
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

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

using Neon.Common;

using Newtonsoft.Json;

using YamlDotNet.Serialization;

namespace NeonSignalRProxy
{
    /// <summary>
    /// Describes the proxy configuration.
    /// </summary>
    public class ProxyConfig
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ProxyConfig()
        {
        }

        /// <summary>
        /// Describes the upstream SignalR backend.
        /// </summary>
        [JsonProperty(PropertyName = "Backends", Required = Required.Always, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "backends", ApplyNamingConventions = false)]
        [DefaultValue(null)]
        public List<Backend> Backends { get; set; } = null;

        /// <summary>
        /// The port for the SignalR Proxy to listen on. Defaults to 80.
        /// </summary>
        [JsonProperty(PropertyName = "Port", Required = Required.Always, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "port", ApplyNamingConventions = false)]
        [DefaultValue(80)]
        public int Port { get; set; } = 80;

        /// <summary>
        /// Where to store session information.
        /// </summary>
        [JsonProperty(PropertyName = "SessionStore", Required = Required.Always, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "sessionStore", ApplyNamingConventions = false)]
        [DefaultValue(SessionStoreType.Cookie)]
        public SessionStoreType SessionStore { get; set; } = SessionStoreType.Cookie;

        /// <summary>
        /// Defines cache settings. InMemory is supported for single instances, but Redis is recommended for High Availability deployments.
        /// </summary>
        [JsonProperty(PropertyName = "Cache", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "cache", ApplyNamingConventions = false)]
        [DefaultValue(null)]
        public Cache Cache { get; set; } = new Cache();

        /// <summary>
        /// Optional DNS settings.
        /// </summary>
        [JsonProperty(PropertyName = "Dns", Required = Required.Default, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        [YamlMember(Alias = "dns", ApplyNamingConventions = false)]
        [DefaultValue(null)]
        public DnsConfig Dns { get; set; } = new DnsConfig();

        /// <summary>
        /// <para>
        /// Helper to read a <see cref="ProxyConfig"/> from a file. It also validates the config.
        /// </para>
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task<ProxyConfig> FromFileAsync(string file)
        {
            using (StreamReader reader = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read)))
            {
                var config = NeonHelper.YamlDeserializeViaJson<ProxyConfig>(await reader.ReadToEndAsync());

                config.Validate();

                return config;
            }
        }

        /// <summary>
        /// Validates the <see cref="ProxyConfig"/>.
        /// </summary>
        public void Validate()
        {
            this.Cache = Cache ?? new Cache();
            this.Dns   = Dns   ?? new DnsConfig();
        }
    }
}