using System;
using System.Threading.Tasks;

using Microsoft.JSInterop;

namespace Neon.Blazor
{
    public class MobileDetector : IDisposable
    {

        private IJSRuntime JS { get; set; }

        public MobileDetector(IJSRuntime js)
        {
            JS = js;
        }

        private IJSObjectReference jsModule;

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        public async Task<bool?> IsMobileAsync()
        {
            if (jsModule == null)
            {
                jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Blazor/interop.js");
            }
            return await jsModule.InvokeAsync<bool>("isMobile");
        }
    }
}
