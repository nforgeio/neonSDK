// -----------------------------------------------------------------------------
// FILE:	    CallbackAction.cs
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
using System.Threading.Tasks;

using Microsoft.JSInterop;

namespace Neon.Mapbox
{
    public class CallbackAction 
    {
        private readonly IJSObjectReference Runtime;
        private readonly string             EventType;
        private readonly Delegate           Delegate;
        private readonly Type               Type;

        public CallbackAction(IJSObjectReference runtime, string eventType, Delegate @delegate, Type type)
        {
            Runtime   = runtime;
            EventType = eventType;
            Delegate  = @delegate;
            Type      = type;
        }

        public CallbackAction(IJSObjectReference runtime, string eventType, Delegate @delegate)
        {
            Runtime   = runtime;
            EventType = eventType;
            Delegate  = @delegate;
        }

        public async Task Remove()
        {
            // TODO: Need to determine if it is a popup or map event to remove. 
            //await Runtime.InvokeVoidAsync("Mapbox.off", EventType);

            await Task.CompletedTask;
        }

        [JSInvokable]
        public void Invoke(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
            {
                Delegate.DynamicInvoke();
                return;
            }

            var response = JsonSerializer.Deserialize(args, Type);

            Delegate.DynamicInvoke(response);
        }

        [JSInvokable]
        public void InvokeWithoutArgs()
        {
            Delegate.DynamicInvoke();
        }
    }
}
