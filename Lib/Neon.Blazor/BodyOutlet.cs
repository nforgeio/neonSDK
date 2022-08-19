using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;


namespace Neon.Blazor
{
    public class BodyOutlet : IDisposable
    {
        private HashSet<string> Class { get; set; } = new HashSet<string>();

        private IJSRuntime JS { get; set; }

        private IHttpContextAccessor HttpContextAccessor { get; set; }

        public BodyOutlet(IJSRuntime js,
            IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
            JS = js;
        }

        private IJSObjectReference jsModule;

        /// <inheritdoc/>
        private async Task InitializeAsync()
        {
            if (HttpContextAccessor.HttpContext.WebSockets.IsWebSocketRequest)
            {
                jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Blazor/interop.js");
            }
        }

        public HashSet<string> GetProperties()
        {
            return Class;
        }

        public async Task AddPropertyAsync(string property)
        {
            await InitializeAsync();
            Class.Add(property);
            await OnPropertyChangedAsync();
        }

        public async Task AddPropertiesAsync(string[] properties)
        {
            await InitializeAsync();
            foreach (var property in properties)
            {
                Class.Add(property);
            }
            await OnPropertyChangedAsync();
        }

        public async Task AddPropertiesAsync(string properties)
        {
            await InitializeAsync();
            foreach (var property in properties.Split())
            {
                Class.Add(property);
            }
            await OnPropertyChangedAsync();
        }

        public async Task RemovePropertyAsync(string property)
        {
            await InitializeAsync();
            if (Class.TryGetValue(property, out var val))
            {
                Class.Remove(property);
            }
            await OnPropertyChangedAsync();
        }

        public async Task RemovePropertiesAsync(string properties)
        {
            await InitializeAsync();
            foreach (var property in properties.Split())
            {
                if (Class.TryGetValue(property, out var val))
                {
                    Class.Remove(property);
                }
            }
            await OnPropertyChangedAsync();
        }

        public async Task RemovePropertiesAsync(string[] properties)
        {
            await InitializeAsync();
            foreach (var property in properties)
            {
                if (Class.TryGetValue(property, out var val))
                {
                    Class.Remove(property);
                }
            }
            await OnPropertyChangedAsync();
        }

        /// <inheritdoc/>
        private async Task OnPropertyChangedAsync()
        {
            string cssClass;
            if (Class != null)
            {
                var sb = new StringBuilder();

                foreach (var property in Class)
                {
                    sb.Append($"{property} ");
                }

                cssClass = sb.ToString().Trim();
            }
            else
            {
                cssClass = string.Empty;
            }

            await jsModule.InvokeAsync<string>("setBodyClass", cssClass);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}