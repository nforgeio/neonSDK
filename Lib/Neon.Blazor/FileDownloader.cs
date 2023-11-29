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
    public class FileDownloader : IDisposable
    {
        private IJSRuntime JS { get; set; }

        private IJSObjectReference jsModule;

        public FileDownloader(IJSRuntime js)
        {
            JS = js;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        public async Task DownloadFileFromUrlAsync(string fileUrl, string fileName = null)
        {
            if (jsModule == null)
            {
                jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Blazor/interop.js");
            }

            await jsModule.InvokeVoidAsync("triggerFileDownload", fileUrl, fileName);
        }
    }
}
