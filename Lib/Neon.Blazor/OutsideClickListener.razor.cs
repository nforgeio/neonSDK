// -----------------------------------------------------------------------------
// FILE:	    OutsideClickListener.cs
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
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Neon.Blazor
{
    public partial class OutsideClickListener : IAsyncDisposable
    {
        [Inject]
        private IJSRuntime JsRuntime { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public EventCallback OnClickOutside { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        [Parameter]
        public bool Capture { get; set; }

        private IJSObjectReference jsModule;
        private DotNetObjectReference<OutsideClickListener> listenerRef;
        private ElementReference HtmlElementRef { get; set; }

        [JSInvokable]
        public async Task OnClickOutsideAsync()
        {
            await OnClickOutside.InvokeAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            await ConnectToJavaScript();
        }

        private async Task ConnectToJavaScript()
        {
            listenerRef = DotNetObjectReference.Create(this);

            jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "/_content/Neon.Blazor/outsideClickListener.js");

            await AttachAsync();
        }

        private async Task AttachAsync()
        {
            await jsModule.InvokeVoidAsync(
                identifier: "addWindowClickEvent",
                args:
                [
                    HtmlElementRef,
                    listenerRef,
                    Capture
                ]);
        }

        private async Task DetachAsync()
        {
            await jsModule!.InvokeVoidAsync(
                identifier: "removeWindowClickEvent",
                args:
                [
                    HtmlElementRef
                ]);
        }

        public async ValueTask DisposeAsync()
        {
            await DetachAsync();

            if (jsModule != null)
            {
                await jsModule.DisposeAsync();
            }

            listenerRef?.Dispose();
        }
    }
}
