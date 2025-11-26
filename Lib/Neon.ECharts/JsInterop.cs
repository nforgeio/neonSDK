// -----------------------------------------------------------------------------
// FILE:	    JsInterop.cs
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
using System.Threading.Tasks;

using Microsoft.JSInterop;

using Neon.ECharts.Options;
using Neon.ECharts.Options.Enum;

namespace Neon.ECharts
{
    public class JsInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> echartsTask;
        private readonly Lazy<Task<IJSObjectReference>> echartsGlTask;
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public IJSObjectReference EchartsModule;
        public IJSObjectReference EchartsGlModule;
        private IJSObjectReference module;
        public IJSObjectReference Module
        {
            get
            {
                if (module == null)
                {
                    InitAsync().GetAwaiter().GetResult();
                }

                return module;
            }
        }

        public JsInterop(IJSRuntime jsRuntime)
        {
            echartsTask = new Lazy<Task<IJSObjectReference>>(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/Neon.ECharts/echarts.min.js").AsTask());
            echartsGlTask = new Lazy<Task<IJSObjectReference>>(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/Neon.ECharts/echarts-gl.min.js").AsTask());
            moduleTask = new Lazy<Task<IJSObjectReference>>(() => jsRuntime.InvokeAsync<IJSObjectReference>(
               "import", "./_content/Neon.ECharts/core.js").AsTask());
        }

        public async ValueTask InitAsync()
        {
            module          = await moduleTask.Value.ConfigureAwait(false);
            EchartsModule   = await echartsTask.Value.ConfigureAwait(false);
            EchartsGlModule = await echartsGlTask.Value.ConfigureAwait(false);
        }

        public async ValueTask<string> Prompt(string message)
        {
            await InitAsync();

            return await Module.InvokeAsync<string>("showPrompt", message);
        }

        public async ValueTask<IJSObjectReference> InitChart(string id, string theme = "light")
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts_id_");
            await InitAsync();
            return await Module.InvokeAsync<IJSObjectReference>("echartsFunctions.initChart", id, theme);
        }
        public async Task RegisterMap(string name, string svg)
        {
            await InitAsync();
            await Module.InvokeVoidAsync("echartsFunctions.registerMap", name, svg);
        }

        public async Task SetupChart<T>(string id, string theme, EChartsOption<T> option, bool notMerge = false)
        {
            await SetupChart(id, theme, option.ToString(), notMerge);
        }

        public async Task SetupChart(string id, string theme, string option, bool notMerge = false)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts_id_");
            if (option == null) throw new ArgumentNullException(nameof(option), "echarts");
            if (string.IsNullOrWhiteSpace(theme)) theme = "light";
            await InitAsync();
            try
            {
                await Module.InvokeVoidAsync("echartsFunctions.setupChart", id, theme, option, notMerge);
            }
            catch
            {
                Console.WriteLine("id:" + id);
                Console.WriteLine("theme:" + theme);
                Console.WriteLine("option:" + option);
                Console.WriteLine("notMerge:" + notMerge);
            }
        }

        public async Task Resize(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts_id_");
            await InitAsync();
            await Module.InvokeVoidAsync("echartsFunctions.resize", id);
        }

        public async Task ChartOn(string id, EventType eventType, DotNetObjectReference<EventInvokeHelper> objectReference)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts_id_");
            await InitAsync();
            await Module.InvokeVoidAsync("echartsFunctions.on", id, eventType.ToString(), objectReference);
        }

        public async Task ChartShowLoading(string id, string type = "default", LoadingOption opts = null)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts_id_");
            if (opts == null) opts = new LoadingOption();
            await InitAsync();
            await Module.InvokeVoidAsync("echartsFunctions.showLoading", id, type, opts.ToString());
        }

        public async Task ChartHideLoading(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts_id_");
            await InitAsync();
            await Module.InvokeVoidAsync("echartsFunctions.hideLoading", id);
        }

        public async Task DispatchAction(string id, DispatchActionOption option)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts_id_");
            await InitAsync();
            await Module.InvokeVoidAsync("echartsFunctions.dispatchAction", id, option.ToString());
        }

        public async ValueTask<T> ConvertToPixel<T>(string id, string finder, object value)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts_id_");
            await InitAsync();
            return await Module.InvokeAsync<T>("echartsFunctions.convertToPixel", id, finder, value);
        }

        public async ValueTask<T> ConvertFromPixel<T>(string id, string finder, object value)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts_id_");
            await InitAsync();
            return await Module.InvokeAsync<T>("echartsFunctions.convertFromPixel", id, finder, value);
        }

        public async Task DisposeChart(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts_id_");
            await InitAsync();
            await Module.InvokeVoidAsync("echartsFunctions.dispose", id);
        }

        public async Task ClearChart(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id), "echarts_id_");
            await InitAsync();
            await Module.InvokeVoidAsync("echartsFunctions.clear", id);
        }
#nullable enable

        public async ValueTask InvokeVoidAsync(string identifier, params object?[] args)
        {
            await InitAsync();
            await Module.InvokeVoidAsync(identifier, args);
        }
#nullable disable

        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                await InitAsync();
                await Module.DisposeAsync();
            }
        }
    }
}