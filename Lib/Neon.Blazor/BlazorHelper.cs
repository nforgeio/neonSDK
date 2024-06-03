// -----------------------------------------------------------------------------
// FILE:	    BlazorHelper.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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
using System.Reflection;
using System.Text;

using Microsoft.AspNetCore.Components;

namespace Neon.Blazor
{
    public static class BlazorHelper
    {
        /// <summary>
        /// Generates a Blazor component path from the component type and the parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string GeneratePath<T>(params object[] values)
            where T : IComponent
        {
            var route = typeof(T).GetCustomAttribute<RouteAttribute>();

            if (route == null)
            {
                throw new ArgumentException($"{nameof(T)} does not have a {nameof(RouteAttribute)}");
            }

            var sb = new StringBuilder();
            sb.Append("/");

            int index = 0;
            foreach (var part in route.Template.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (part.StartsWith('{') && part.EndsWith('}'))
                {
                    sb.Append(values[index]);
                    index++;
                }
                else
                {
                    sb.Append(part);
                }

                sb.Append("/");
            }

            return sb.ToString().TrimEnd('/');
        }
    }
}
