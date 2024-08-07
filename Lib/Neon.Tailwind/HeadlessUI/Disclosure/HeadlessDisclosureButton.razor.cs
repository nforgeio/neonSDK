using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using Neon.Blazor;

namespace Neon.Tailwind
{
    public partial class HeadlessDisclosureButton : HtmlElement, IDisposable
    {
        [CascadingParameter] public HeadlessDisclosure CascadedDisclosure { get; set; } = default!;

        [Parameter] public bool IsEnabled { get; set; } = true;

        protected HeadlessDisclosure Disclosure { get; set; } = default!;



        /// <inheritdoc/>

        protected override async Task OnInitializedAsync()
        {
            await Disclosure.RegisterButton(this);
            TagName = "button";

        }
        public async void HandleClick()
        {
            if (IsEnabled)
               await Disclosure.Toggle();
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            _ = Disclosure.UnregisterButton(this);
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
                    throw new InvalidOperationException($"You must use {nameof(HeadlessDisclosureButton)} inside an {nameof(HeadlessDisclosure)}.");

                Disclosure = CascadedDisclosure;
            }
            else if (CascadedDisclosure != Disclosure)
            {
                throw new InvalidOperationException($"{nameof(HeadlessDisclosure)} does not support changing the {nameof(HeadlessDisclosureButton)} dynamically.");
            }

            return base.SetParametersAsync(ParameterView.Empty);
        }

    }
}
