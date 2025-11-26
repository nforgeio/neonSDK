// -----------------------------------------------------------------------------
// FILE:	    EventInvokeHelper.cs
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

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.JSInterop;

using Neon.ECharts.Options;

namespace Neon.ECharts
{
    public class EventInvokeHelper
    {
        private readonly Action<EchartsEventArgs> _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventInvokeHelper"/> class.
        /// </summary>
        /// <param name="action">The action to be invoked.</param>
        public EventInvokeHelper(Action<EchartsEventArgs> action)
        {
            _action = action;
        }

        [JSInvokable]
        public void EventCaller(string args)
        {
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters =
                    {
                        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                    }
            };
            _action.Invoke(JsonSerializer.Deserialize<EchartsEventArgs>(args, jsonSerializerOptions));
        }
    }
}
