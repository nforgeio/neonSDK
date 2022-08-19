using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;

namespace Neon.Blazor
{
    public class MobileDetector : IDisposable
    {

        private IJSRuntime JS { get; set; }

        private IHttpContextAccessor HttpContextAccessor { get; set; }

        public MobileDetector(IJSRuntime js,
            IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
            JS = js;
        }

        private IJSObjectReference jsModule;

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        public async Task<bool?> IsMobileAsync()
        {
            if (!HttpContextAccessor.HttpContext.WebSockets.IsWebSocketRequest)
            {
                return null;
            }
            if (jsModule == null)
            {
                jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Blazor/interop.js");
            }
            return await jsModule.InvokeAsync<bool>("isMobile");
        }
    }
}
