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
    public class Utils : IDisposable
    {
        private IJSRuntime JS { get; set; }

        public Utils(IJSRuntime js)
        {
            JS = js;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        public async Task DownloadFileFromURL(string fileUrl, string fileName = null)
        {
            await JS.InvokeVoidAsync("triggerFileDownload", fileName, fileUrl);
        }
    }
}
