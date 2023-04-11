using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neon.Diagnostics;

namespace Neon.Blazor
{
    public partial class IntersectionObserver : ComponentBase, IAsyncDisposable, IDisposable
    {
        [Parameter]
        public EventCallback<IntersectionChangedEventArgs> OnIntersectionChanged { get; set; }

        [Parameter]
        public RenderFragment<IntersectionObserverContext> ChildContent { get; set; }

        [Parameter]
        public string Id { get; set; } = HtmlElement.GenerateId();

        [Parameter]
        public string RootMargin { get; set; } = "0px";

        [Parameter]
        public double[] Threshold { get; set; } = null;

        [Parameter]
        public string DocumentQuery { get; set; } = null;

        [Inject]
        public IJSRuntime JS { get; set; }

        [Inject]
        public IHttpContextAccessor HttpContextAccessor { get; set; }

        [Inject]
        public IServiceProvider ServiceProvider { get; set; }

        public event Action IntersectionChanged;

        public bool IsIntersecting => IntersectionObserverContext.IsIntersecting;
        public bool IsVisible => IntersectionObserverContext.IsVisible;
        public double Ratio => IntersectionObserverContext.Ratio;

        private IJSObjectReference jsModule;

        private IJSObjectReference intersectionObserver;

        private ILogger<IntersectionObserver> logger;

        protected HtmlElement rootElement { get; set; }
        private IntersectionObserverContext IntersectionObserverContext { get; set; } = new IntersectionObserverContext();
        private ElementReference elementReference;

        protected override void OnInitialized()
        {
            this.logger = this.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger<IntersectionObserver>();

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
                try
                {
                    intersectionObserver = await jsModule.InvokeAsync<IJSObjectReference>("construct", new
                    {
                        RootMargin = RootMargin,
                        Threshold = Threshold
                    });

                    if (rootElement != null)
                    {
                        var elementRef = rootElement.AsElementReference();
                        await intersectionObserver!.InvokeVoidAsync("observe", elementRef);

                        logger?.LogDebugEx(() => $"observing element [{elementRef.Id}]");
                    }
                }
                catch (Exception e)
                {
                    logger?.LogErrorEx(e);
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

        /// <summary>
        /// Set the element reference.
        /// </summary>
        /// <param name="reference"></param>
        public void OnSetElementReference(ElementReference reference)
        {
            elementReference = reference;
        }

        private async Task OnIntersectionChangedInternal(IntersectionChangedEventArgs args)
        {
            if (this.IntersectionObserverContext == null)
            {
                this.IntersectionObserverContext = new IntersectionObserverContext();
            }

            this.IntersectionObserverContext.IsIntersecting = args.IsIntersecting;
            this.IntersectionObserverContext.IsVisible = args.IsVisible;

            await InvokeAsync(IntersectionChanged);
            
            await OnIntersectionChanged.InvokeAsync(new IntersectionChangedEventArgs()
            {
                Ratio          = args.Ratio,
                IsIntersecting = args.IsIntersecting,
                IsVisible      = args.IsVisible
            });
        }
    }

    public class IntersectionChangedEventArgs : EventArgs
    {
        public double Ratio { get; set; }
        public bool IsVisible { get; set; }
        public bool IsIntersecting { get; set; }
    }

    public class IntersectionObserverContext
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
