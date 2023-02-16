//-----------------------------------------------------------------------------
// FILE:	    DependencyInjectionExtensions.cs
// CONTRIBUTOR: Marcus Bowyer
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Neon.Diagnostics;

using OpenTelemetry.Trace;

namespace Neon.Web
{
    /// <summary>
    /// Neon.Web Tracing Instrumentation.
    /// </summary>
    public static class TracerProviderBuilderExtensions
    {
        /// <summary>
        /// The assembly name.
        /// </summary>
        internal static readonly AssemblyName AssemblyName = typeof(TracerProviderBuilderExtensions).Assembly.GetName();

        /// <summary>
        /// The activity source name.
        /// </summary>
        internal static readonly string ActivitySourceName = AssemblyName.Name;

        /// <summary>
        /// The version.
        /// </summary>
        internal static readonly Version Version = AssemblyName.Version;

        /// <summary>
        /// Adds Neon.Web to the tracing pipeline.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static TracerProviderBuilder AddNeonWeb(
            this TracerProviderBuilder builder)
        {
            if (TelemetryHub.ActivitySource == null)
            {
                TelemetryHub.ActivitySource = new ActivitySource(ActivitySourceName, Version.ToString());

                builder.AddSource(ActivitySourceName);
            }

            return builder;
        }
    }
}
