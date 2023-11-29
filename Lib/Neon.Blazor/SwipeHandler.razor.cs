using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Neon.Blazor
{
    public partial class SwipeHandler : ComponentBase, IDisposable
    {
        /// <summary>
        /// The element ID.
        /// </summary>
        [Parameter]
        public string Id { get; set; } = HtmlElement.GenerateId();

        /// <summary>
        /// The callback to be called on swipe events.
        /// </summary>
        [Parameter]
        public EventCallback<SwipeDirection> OnSwipe { get; set; }

        /// <summary>
        /// The styled Dialog panel.
        /// </summary>
        [Parameter]
        public RenderFragment<SwipeHandler> ChildContent { get; set; }

        /// <summary>
        /// Additional HTML attributes to be applied to the <see cref="RenderFragment"/>.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        private HtmlElement rootElement;
        private TouchPoint _startPoint { get; set; }
        private TouchPoint _endPoint { get; set; }
        private DateTime _startTime { get; set; }
        private SwipeDirection swipeDirection { get; set; }

        /// <inheritdoc/>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
        }

        /// <inheritdoc/>
        public void Dispose() { }

        /// <summary>
        /// The current element reference.
        /// </summary>
        /// <param name="element"></param>
        public static implicit operator ElementReference(SwipeHandler element)
        {
            return element?.rootElement ?? default!;
        }

        /// <summary>
        /// Focuses the touch handler.
        /// </summary>
        /// <returns></returns>
        public ValueTask FocusAsync()
        {
            return rootElement?.FocusAsync() ?? ValueTask.CompletedTask;
        }

        /// <summary>
        /// Handles the touch start event.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task HandleTouchStart(TouchEventArgs args)
        {
            _startPoint = args.TargetTouches[0];
            _startTime = DateTime.UtcNow;

            await Task.CompletedTask;
        }

        /// <summary>
        /// Handles touch end event.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task HandleTouchEnd(TouchEventArgs args)
        {
            _endPoint = args.ChangedTouches[0];

            var diffX = _startPoint.ClientX - _endPoint.ClientX;
            var diffY = _startPoint.ClientY - _endPoint.ClientY;
            var diffT = DateTime.Now - _startTime;

            var velocityX = Math.Abs(diffX / diffT.Milliseconds);
            var velocityY = Math.Abs(diffY / diffT.Milliseconds);

            if (Math.Abs(diffX) < 100 && Math.Abs(diffY) < 100)
            {
                return;
            }

            if (velocityX >= 0.10)
            {
                swipeDirection = (diffX < 0) ? SwipeDirection.Left : SwipeDirection.Right;
            }
            else if (velocityY >= 0.10)
            {
                swipeDirection = (diffY < 0) ? SwipeDirection.Down : SwipeDirection.Up;
            }
            else
            {
                return;
            }

            await OnSwipe.InvokeAsync(swipeDirection);
        }

        /// <summary>
        /// Handles touch move event.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task HandleTouchMove(TouchEventArgs args)
        {
            await Task.CompletedTask;
        }
    }
}
