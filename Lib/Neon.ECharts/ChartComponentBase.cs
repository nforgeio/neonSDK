// -----------------------------------------------------------------------------
// FILE:	    ChartComponentBase.cs
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
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.JSInterop;

using Neon.ECharts.Options;
using Neon.ECharts.Options.Enum;
using Neon.Tasks;

namespace Neon.ECharts
{
    /// <summary>
    /// Base class for the chart component.
    /// </summary>
    /// <typeparam name="T">The type of the chart option.</typeparam>
    public class ChartComponentBase<T> : ComponentBase, IAsyncDisposable where T : class
    {
        /// <summary>
        /// The unique identifier for the chart component.
        /// </summary>
        public readonly string Id = "echarts_" + Guid.NewGuid().ToString("N");
        private DotNetObjectReference<ChartComponentBase<T>> _objectReference;
        private string _theme;

#pragma warning disable BL0007
        [Parameter]
        public string Theme
        {
            get
            {
                return _theme;
            }
            set
            {
                _theme = value;

                _ = JsInterop.DisposeChart(Id);
            }
        }
#pragma warning restore BL0007

        [Parameter]
        public EChartsOption<T> Option { get; set; }
        [Parameter]
        public string OptionRaw { get; set; }
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public bool AutoRender { get; set; } = true;

        [Parameter]
        public bool ISResize { get; set; } = true;
        protected bool RequireRender { get; set; }
        [Inject]
        public JsInterop JsInterop { get; set; }
        [Parameter]
        public Func<object, Task> OnRenderCompleted { get; set; }

        [Parameter]
        public List<EventType> EventTypes { get; set; } = new List<EventType>();

        [Parameter]
        public EventCallback<EchartsEventArgs> OnEventCallback { get; set; }

        [Parameter]
        public EventCallback OnResizeEventCallback { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        private EventInvokeHelper _eventInvokeHelper;

        private bool hasBindEvent;

        [Parameter]
        public bool NotMerge { get; set; } = false;

        private bool IsPrerenderPhase { get; set; } = true;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _eventInvokeHelper = new EventInvokeHelper(async echartsParams =>
            {
                if (EventTypes.Count > 0 && OnEventCallback.HasDelegate)
                    await OnEventCallback.InvokeAsync(echartsParams);
            });
            _objectReference = DotNetObjectReference.Create(this);
        }

        /// <summary>
        /// Marks the chart component as requiring render.
        /// </summary>
        public void MarkAsRequireRender()
        {
            RequireRender = true;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            RequireRender = false;
        }

        protected override bool ShouldRender()
        {
            base.ShouldRender();

            return RequireRender;
        }

        protected override async Task OnParametersSetAsync()
        {
            await SyncContext.Clear;

            await base.OnParametersSetAsync();

            if (IsPrerenderPhase) return;

            await SetupChartAsync();
        }

        private async Task SetupChartAsync()
        {
            if (Option == null && string.IsNullOrWhiteSpace(OptionRaw) && ChildContent == null) return;

            if (ChildContent != null)
            {
                var sb = new StringBuilder();
                var rtb = new RenderTreeBuilder();
                ChildContent.Invoke(rtb);
#pragma warning disable BL0006 // Do not use RenderTree types
                foreach (var frame in rtb.GetFrames().Array)
                {
                    if (frame.FrameType == RenderTreeFrameType.Markup)
                    {
                        sb.AppendLine(frame.MarkupContent);
                    }
                }
#pragma warning restore BL0006 // Do not use RenderTree types
                var output = sb.ToString().Trim();

                if (!string.IsNullOrWhiteSpace(output))
                    await JsInterop.SetupChart(Id, Theme, output, NotMerge);
            }
            else if (!string.IsNullOrWhiteSpace(OptionRaw))
                await JsInterop.SetupChart(Id, Theme, OptionRaw, NotMerge);
            else
                await JsInterop.SetupChart(Id, Theme, Option, NotMerge);

            if (EventTypes.Count > 0 && OnEventCallback.HasDelegate && !hasBindEvent)
            {
                foreach (var eventType in EventTypes)
                {
                    await JsInterop.ChartOn(Id, eventType, DotNetObjectReference.Create(_eventInvokeHelper));
                }
                hasBindEvent = true;
            }
            if (ISResize)
            {
                await AddResizeListener();
                await ResizeAsync();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await SyncContext.Clear;

            await base.OnAfterRenderAsync(firstRender);

            if (AutoRender == false) return;
            if (firstRender)
            {
                IsPrerenderPhase = false;

                await SetupChartAsync();

                if (OnRenderCompleted != null)
                {
                    await OnRenderCompleted(this);
                }
            }
            RequireRender = false;
        }

        /// <summary>
        /// Sets up the chart component with the specified option.
        /// </summary>
        /// <param name="opt">The option to set up the chart component with.</param>
        public async Task SetupOptionAsync(string opt)
        {
            await JsInterop.SetupChart(Id, Theme, opt, NotMerge);
        }

        /// <summary>
        /// Sets up the chart component with the specified option.
        /// </summary>
        /// <param name="opt">The option to set up the chart component with.</param>
        public async Task SetupOptionAsync(EChartsOption<T> opt)
        {
            await JsInterop.SetupChart(Id, Theme, opt, NotMerge);
        }

        /// <summary>
        /// Resizes the chart component.
        /// </summary>
        public async Task ResizeAsync()
        {
            await JsInterop.Resize(Id);
        }

        [JSInvokable("OnResize")]
        public void OnResize()
        {
            if (ISResize)
            {
                _ = ResizeAsync();
                if (OnResizeEventCallback.HasDelegate)
                {
                    _ = OnResizeEventCallback.InvokeAsync();
                }
            }
        }

        private async Task AddResizeListener()
        {
            await JsInterop.InvokeVoidAsync("echartsFunctions.addResizeListener", _objectReference);
        }

        private async Task RemoveResizeListener()
        {
            await JsInterop.InvokeVoidAsync("echartsFunctions.removeResizeListener", _objectReference);
        }

        /// <summary>
        /// Refreshes the chart component.
        /// </summary>
        public void Refresh()
        {
            StateHasChanged();
        }

        /// <summary>
        /// Assigns a DotNetObjectReference to the chart component.
        /// </summary>
        /// <typeparam name="TD">The type of the DotNetObjectReference.</typeparam>
        /// <param name="dotNetObject">The DotNetObjectReference to assign.</param>
        public async Task AssignDotNetHelper<TD>(DotNetObjectReference<TD> dotNetObject) where TD : class
        {
            await JsInterop.InvokeVoidAsync("echartsFunctions.assignDotNetHelper", Id, dotNetObject);
        }

        /// <summary>
        /// Shows the loading animation on the chart component.
        /// </summary>
        /// <param name="option">The loading option.</param>
        public void ShowLoading(LoadingOption option = null)
        {
            _ = JsInterop.ChartShowLoading(Id, opts: option);
        }

        /// <summary>
        /// Hides the loading animation on the chart component.
        /// </summary>
        public void HideLoading()
        {
            _ = JsInterop.ChartHideLoading(Id);
        }

        /// <summary>
        /// Clears the chart component.
        /// </summary>
        public void Clear()
        {
            _ = JsInterop.ClearChart(Id);
        }

        /// <summary>
        /// Dispatches an action to the chart component.
        /// </summary>
        /// <param name="option">The dispatch action option.</param>
        public void DispatchAction(DispatchActionOption option)
        {
            _ = JsInterop.DispatchAction(Id, option);
        }

        /// <summary>
        /// Converts a value to pixel coordinates.
        /// </summary>
        /// <typeparam name="TN">The type of the pixel coordinates.</typeparam>
        /// <param name="finder">The finder expression.</param>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted pixel coordinates.</returns>
        public async ValueTask<TN> ConvertToPixel<TN>(string finder, object value)
        {
            return await JsInterop.ConvertToPixel<TN>(Id, finder, value);
        }

        /// <summary>
        /// Converts pixel coordinates to a value.
        /// </summary>
        /// <typeparam name="TN">The type of the value.</typeparam>
        /// <param name="finder">The finder expression.</param>
        /// <param name="value">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public async ValueTask<TN> ConvertFromPixel<TN>(string finder, object value)
        {
            return await JsInterop.ConvertFromPixel<TN>(Id, finder, value);
        }

        public async ValueTask DisposeAsync()
        {
            if (IsPrerenderPhase) return;
            await RemoveResizeListener();
            _objectReference?.Dispose();
        }
    }
}
