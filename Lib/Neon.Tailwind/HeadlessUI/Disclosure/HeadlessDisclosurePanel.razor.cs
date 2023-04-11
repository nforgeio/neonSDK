using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using Neon.Blazor;

namespace Neon.Tailwind
{
    public partial class HeadlessDisclosurePanel : HtmlElement, IDisposable
    {
        [CascadingParameter] public HeadlessDisclosure CascadedDisclosure { get; set; } = default!;

        /// <summary>
        /// Whether the disclosure panel is enabled.
        /// </summary>
        [Parameter] public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Whether the disclosure panel is visible.
        /// </summary>
        [Parameter] public DisclosureState IsVisible { get; set; } = DisclosureState.Closed;

        protected HeadlessDisclosure Disclosure { get; set; } = default!;

        private HtmlElement rootElement;

        protected async override Task OnInitializedAsync()
        {
            await Disclosure.RegisterPanel(this);

        }
        public async Task Open()
        {
            IsVisible = DisclosureState.Open;

            await Task.CompletedTask;
        }
        public async Task Close()
        {
            IsVisible = DisclosureState.Closed;

            await Task.CompletedTask;
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            _ = Disclosure.UnregisterPanel(this);
        }

        /// <inheritdoc/>
        public override Task SetParametersAsync(ParameterView parameters)
        {
            //This is here to follow the pattern/example as implmented in Microsoft's InputBase component
            //https://github.com/dotnet/aspnetcore/blob/main/src/Components/Web/src/Forms/InputBase.cs

            parameters.SetParameterProperties(this);

            if (Disclosure == null)
            {
                if (CascadedDisclosure == null)
                    throw new InvalidOperationException($"You must use {nameof(HeadlessDisclosurePanel)} inside an {nameof(HeadlessDisclosure)}.");

                Disclosure = CascadedDisclosure;
            }
            else if (CascadedDisclosure != Disclosure)
            {
                throw new InvalidOperationException($"{nameof(HeadlessDisclosure)} does not support changing the {nameof(HeadlessDisclosurePanel)} dynamically.");
            }

            return base.SetParametersAsync(ParameterView.Empty);
        }
    }
}
