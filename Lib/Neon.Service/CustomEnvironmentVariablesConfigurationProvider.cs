// -----------------------------------------------------------------------------
// FILE:	    CustomEnvironmentVariablesConfigurationProvider.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration;

namespace Neon.Service
{
    /// <summary>
    /// This fixes broken behavior in containers.
    /// https://github.com/dotnet/runtime/issues/87130#issuecomment-1583859511
    /// </summary>
    internal class CustomEnvironmentVariablesConfigurationProvider : EnvironmentVariablesConfigurationProvider
    {
        internal const string DefaultDotReplacement = ":_";
        private string dotReplacement;
        public CustomEnvironmentVariablesConfigurationProvider(string dotReplacement = DefaultDotReplacement) : base()
        {
            this.dotReplacement = dotReplacement ?? DefaultDotReplacement;
        }

        public CustomEnvironmentVariablesConfigurationProvider(string prefix, string dotReplacment = DefaultDotReplacement) : base(prefix)
        {
            dotReplacement = dotReplacment ?? DefaultDotReplacement;
        }

        public override void Load()
        {
            base.Load();

            Dictionary<string, string> data = new Dictionary<string, string>();

            foreach (KeyValuePair<string, string> kvp in Data)
            {
                if (kvp.Key.Contains(dotReplacement))
                {
                    data.Add(kvp.Key.Replace(dotReplacement, ".", StringComparison.OrdinalIgnoreCase), kvp.Value);
                }
                else
                {
                    data.Add(kvp.Key, kvp.Value);
                }
            }

            Data = data;
        }
    }

    internal class CustomEnvironmentVariablesConfigurationSource : IConfigurationSource
    {
        public string Prefix { get; set; }
        public string DotReplacement { get; set; }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new CustomEnvironmentVariablesConfigurationProvider(Prefix, DotReplacement);
        }
    }

    internal static class CustomEnvironmentVariablesExtensions
    {
        public static IConfigurationBuilder AddCustomEnvironmentVariables(this IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Add(new CustomEnvironmentVariablesConfigurationSource());
            return configurationBuilder;
        }

        public static IConfigurationBuilder AddCustomEnvironmentVariables(this IConfigurationBuilder configurationBuilder,
            string prefix,
            string dotReplacement = CustomEnvironmentVariablesConfigurationProvider.DefaultDotReplacement)
        {
            configurationBuilder.Add(new CustomEnvironmentVariablesConfigurationSource { Prefix = prefix, DotReplacement = dotReplacement });
            return configurationBuilder;
        }

        public static IConfigurationBuilder AddCustomEnvironmentVariables(this IConfigurationBuilder builder,
            Action<CustomEnvironmentVariablesConfigurationSource> configureSource) => builder.Add(configureSource);
    }
}
