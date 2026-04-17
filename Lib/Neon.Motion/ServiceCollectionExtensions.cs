// -----------------------------------------------------------------------------
// FILE:        ServiceCollectionExtensions.cs
// CONTRIBUTOR: NEONFORGE Team
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

using Neon.Motion;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for configuring Neon.Motion services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Neon.Motion services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <remarks>
        /// Call this from your <c>Program.cs</c> or service registration:
        /// <code>
        /// builder.Services.AddMotion();
        /// </code>
        /// Then add the following to your <c>_Imports.razor</c>:
        /// <code>
        /// @using Neon.Motion.Components
        /// </code>
        /// </remarks>
        public static IServiceCollection AddMotion(this IServiceCollection services)
        {
            services.AddScoped<JsInterop>();

            return services;
        }
    }
}
