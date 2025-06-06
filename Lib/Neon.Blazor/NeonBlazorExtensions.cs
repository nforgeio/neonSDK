//-----------------------------------------------------------------------------
// FILE:        NeonBlazorExtensions.cs
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

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

using Microsoft.Extensions.DependencyInjection;

namespace Neon.Blazor
{
    /// <summary>
    /// A set of extension methods for Neon Blazor.
    /// </summary>
    public static class NeonBlazorExtensions
    {
        /// <summary>
        /// Adds Neon Blazor services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns></returns>
        public static IServiceCollection AddNeonBlazor(this IServiceCollection services)
        {
            if (RuntimeInformation.IsOSPlatform(PlatformExtensions.BrowserPlatform))
            {
                services.AddBrowserComponents();
            }
            else
            {
#pragma warning disable CA1416 // Validate platform compatibility
                services.AddServerComponents();
#pragma warning restore CA1416 // Validate platform compatibility
            }

            services
                .AddScoped<BodyOutlet>()
                .AddScoped<MobileDetector>()
                .AddScoped<FileDownloader>()
                .AddScoped<TimeProvider, BrowserTimeProvider>();

            return services;
        }

        private static IServiceCollection AddBrowserComponents(this IServiceCollection services)
        {
            return services.AddScoped<IRenderContext, ClientRenderContext>();
        }

        [UnsupportedOSPlatform("browser")]
        private static IServiceCollection AddServerComponents(this IServiceCollection services)
        {
            return services
                .AddHttpContextAccessor()
                .AddScoped<IRenderContext, ServerRenderContext>();
        }
    }
}
