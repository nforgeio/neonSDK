//-----------------------------------------------------------------------------
// FILE:        Portal.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using Neon.Tasks;

namespace Neon.Tailwind
{
    /// <summary>
    /// Portals provide a first-class way to render children into a DOM node that 
    /// exists outside the DOM hierarchy of the parent c
    /// </summary>
    public class Portal : ComponentBase
    {
        /// <summary>
        /// The injected <see cref="IPortalBinder"/>.
        /// </summary>
        [Inject] 
        public IPortalBinder PortalBinder { get; set; }

        /// <summary>
        /// The name of the Portal.
        /// </summary>
        [Parameter] 
        public string Name { get; set; } = "root";

        /// <summary>
        /// Additional attributes.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        [Parameter]
        public string Class { get; set; }

        private HashSet<string> classes = new HashSet<string>();

        public string Id { get; set; } = GenerateId();

        private RenderFragment content;

        /// <inheritdoc/>
        protected override void OnInitialized()
        {
            PortalBinder?.RegisterPortal(Name, this);
        }

        /// <inheritdoc/>
        protected override void OnParametersSet()
        {
            if (Attributes != null && Attributes.TryGetValue("class", out var @class))
            {
                foreach (var c in ((string)@class).Split(' '))
                {
                    classes.Add(c);
                }
            }
        }

        /// <summary>
        /// Renders the <see cref="RenderFragment"/>.
        /// </summary>
        /// <param name="content"></param>
        public void RenderContent(RenderFragment content)
        {
            this.content = content;
            StateHasChanged();
        }

        /// <summary>
        /// Renders the <see cref="RenderFragment"/>.
        /// </summary>
        /// <param name="content"></param>
        public async Task RenderContentAsync(RenderFragment content)
        {
            await SyncContext.Clear;

            Id = GenerateId();

            this.content = content;

            await InvokeAsync(StateHasChanged);
        }


        public Task<TComponent> RenderComponentAsync<TComponent>()
            where TComponent : IComponent
        {
            return RenderComponentAsync<TComponent>(parameters: null);
        }

        public Task<TComponent> RenderComponentAsync<TComponent>(Action<Dictionary<string, object>> parameterSetter = null)
            where TComponent : IComponent
        {
            var parameters = new Dictionary<string, object>();
            parameterSetter.Invoke(parameters);

            return RenderComponentAsync<TComponent>(parameters);
        }

        public async Task<TComponent> RenderComponentAsync<TComponent>(Dictionary<string, object> parameters = null)
            where TComponent : IComponent
        {
            await SyncContext.Clear;

            Id = GenerateId();

            var hasRendered = new TaskCompletionSource<bool>();
            TComponent component = default;

            var componentId = GenerateId();

            this.content = builder =>
            {
                builder.OpenComponent<TComponent>(0);

                if (parameters != null)
                {
                    builder.AddMultipleAttributes(1, parameters);
                }

                builder.AddComponentReferenceCapture(2, c =>
                {
                    if (component == null)
                    {
                        component = (TComponent)c;
                        hasRendered.TrySetResult(true);
                    }
                });

                builder.SetKey(componentId);
                builder.CloseComponent();
            };

            await InvokeAsync(StateHasChanged);

            await hasRendered.Task;

            return component;
        }

        public void Close()
        {
            this.content = null;
            StateHasChanged();
        }

        public async Task CloseAsync()
        {
            await SyncContext.Clear;

            this.content = null;

            await InvokeAsync(StateHasChanged);
        }

        /// <inheritdoc/>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "id", Name);
            builder.AddMultipleAttributes(2, Attributes.Where(a => a.Key != "class").Select(a => new KeyValuePair<string, object>(a.Key, a.Value)));
            builder.AddAttribute(3, "class", string.Join(" ", classes));
            builder.AddContent(4, content);
            builder.SetKey(Id);
            builder.CloseElement();
        }

        public static string GenerateId() => Guid.NewGuid().ToString("N");
    }
}
