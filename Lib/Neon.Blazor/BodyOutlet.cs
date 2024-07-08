using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.JSInterop;

using Neon.Tasks;


namespace Neon.Blazor
{
    /// <summary>
    /// A class for managing the class of the body element.
    /// </summary>
    public class BodyOutlet : IDisposable
    {
        private HashSet<string> Class { get; set; } = new HashSet<string>();

        private IJSRuntime JS { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="js"></param>
        public BodyOutlet(IJSRuntime js)
        {
            JS = js;
        }

        private IJSObjectReference jsModule;

        /// <inheritdoc/>
        private async Task InitializeAsync()
        {
            await SyncContext.Clear;

            if (jsModule != null)
            {
                return;
            }

            jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Blazor/interop.js");
        }

        /// <summary>
        /// Gets the current properties of the body element.
        /// </summary>
        /// <returns></returns>
        public HashSet<string> GetProperties()
        {
            return Class;
        }

        /// <summary>
        /// Adds a property to the body element.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public async Task AddPropertyAsync(string property)
        {
            await SyncContext.Clear;

            await InitializeAsync();

            Class.Add(property);

            await OnPropertyChangedAsync();
        }

        /// <summary>
        /// Adds properties to the body element.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public async Task AddPropertiesAsync(params string[] properties)
        {
            await SyncContext.Clear;

            await InitializeAsync();

            foreach (var property in properties)
            {
                Class.Add(property);
            }

            await OnPropertyChangedAsync();
        }

        /// <summary>
        /// Adds properties to the body element.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public async Task AddPropertiesAsync(string properties)
        {
            await SyncContext.Clear;

            await InitializeAsync();

            foreach (var property in properties.Split())
            {
                Class.Add(property);
            }

            await OnPropertyChangedAsync();
        }

        /// <summary>
        /// Removes a property from the body element.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public async Task RemovePropertyAsync(string property)
        {
            await SyncContext.Clear;

            await InitializeAsync();

            if (Class.TryGetValue(property, out var val))
            {
                Class.Remove(property);
            }

            await OnPropertyChangedAsync();
        }

        /// <summary>
        /// Removes properties from the body element.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public async Task RemovePropertiesAsync(string properties)
        {
            await SyncContext.Clear;

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

        /// <summary>
        /// Removes properties from the body element.
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        public async Task RemovePropertiesAsync(params string[] properties)
        {
            await SyncContext.Clear;

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

        private async Task OnPropertyChangedAsync()
        {
            await SyncContext.Clear;

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
