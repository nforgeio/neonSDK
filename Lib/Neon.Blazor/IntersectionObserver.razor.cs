﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neon.Blazor
{
    public partial class IntersectionObserver : HtmlElement, IAsyncDisposable, IDisposable
    {
        [Parameter]
        public EventCallback<IntersectionChangedEventArgs> OnIntersectionChanged { get; set; }

        [Parameter]
        public string RootMargin { get; set; } = "0px";
        
        [Parameter]
        public double[] Threshold { get; set; } = null;
        
        [Inject]
        public IJSRuntime JS { get; set; }

        [Inject]
        public IHttpContextAccessor HttpContextAccessor { get; set; }

        private IJSObjectReference jsModule;

        private IJSObjectReference intersectionObserver;

        protected HtmlElement rootElement;
        public IntersectionObserver()
        {
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!HttpContextAccessor.HttpContext.WebSockets.IsWebSocketRequest)
            {
                return;
            }

            if (jsModule == null)
            {
                jsModule = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/Neon.Blazor/interop.js");
            }

            if (firstRender)
            {
                intersectionObserver = await jsModule.InvokeAsync<IJSObjectReference>("construct", new
                {
                    RootMargin = RootMargin,
                    Threshold = Threshold
                });

                if (rootElement != null)
                {
                    var elementRef = rootElement.AsElementReference();
                    await intersectionObserver!.InvokeVoidAsync("observe", rootElement.AsElementReference());
                }
            }
            

            await base.OnAfterRenderAsync(firstRender);
        }

        public void Dispose()
        {
        }

        public async ValueTask DisposeAsync()
        {
            if (intersectionObserver is not null)
            {
                await intersectionObserver.InvokeVoidAsync("disconnect");

                await intersectionObserver.DisposeAsync();
            }

            if (jsModule is not null)
            {
                await jsModule.DisposeAsync();
            }
        }

        private Task OnIntersectionChangedInternal(IntersectionChangedEventArgs args)
        {
            return OnIntersectionChanged.InvokeAsync(new IntersectionChangedEventArgs()
            {
                Ratio = args.Ratio,
                IsIntersecting = args.IsIntersecting,
                IsVisible = args.IsVisible
            });
        }
    }

    public class IntersectionChangedEventArgs : EventArgs
    {
        public double Ratio { get; set; }
        public bool IsVisible { get; set; }
        public bool IsIntersecting { get; set; }
    }

    [EventHandler("onintersectionchanged", typeof(IntersectionChangedEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
    public static class EventHandlers
    {
    }
}