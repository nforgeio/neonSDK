//-----------------------------------------------------------------------------
// FILE:	    HtmlElement.cs
// CONTRIBUTOR: Marcus Bowyer
// COPYRIGHT:  	Copyright (c) 2005-2022 by neonFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neon.Blazor
{
    public class HtmlElement : ComponentBase
    {
        /// <summary>
        /// The tag ID.
        /// </summary>
        [Parameter] 
        public string Id { get; set; } = GenerateId();

        /// <summary>
        /// The tag name to use for this element.
        /// </summary>
        [Parameter] 
        public string TagName { get; set; } = "div";

        /// <summary>
        /// The element type.
        /// </summary>
        [Parameter] 
        public string Type { get; set; } = null;

        /// <summary>
        /// Additional attributes to be applied to the element.
        /// </summary>
        [Parameter(CaptureUnmatchedValues = true)] 
        public IReadOnlyDictionary<string, object> Attributes { get; set; }

        /// <summary>
        /// The child content.
        /// </summary>
        [Parameter] 
        public RenderFragment ChildContent { get; set; }
        
        /// <summary>
        /// Events to prevent default behaviour.
        /// </summary>
        [Parameter] 
        public List<string> PreventDefaultOn { get; set; } = new();

        /// <summary>
        /// Propagation events to prevent default behaviour.
        /// </summary>
        [Parameter] 
        public List<string> StopPropagationOn { get; set; } = new();

        private ElementReference elementReference;

        /// <summary>
        /// Generates an ID.
        /// </summary>
        /// <returns></returns>
        public static string GenerateId() => Guid.NewGuid().ToString("N");

        /// <inheritdoc/>
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (string.IsNullOrEmpty(TagName))
            {
                builder.AddContent(0, ChildContent);
                return;
            }

            builder.OpenElement(0, TagName);
            builder.AddAttribute(1, "id", Id);
            builder.AddAttribute(2, "type", Type);
            builder.AddMultipleAttributes(3, Attributes);
            foreach (var eventName in PreventDefaultOn.Where(s => !string.IsNullOrEmpty(s)))
                builder.AddEventPreventDefaultAttribute(4, eventName, true);
            foreach (var eventName in StopPropagationOn.Where(s => !string.IsNullOrEmpty(s)))
                builder.AddEventStopPropagationAttribute(5, eventName, true);
            builder.AddElementReferenceCapture(6, r => OnSetElementReference(r));
            builder.AddContent(6, ChildContent);
            builder.CloseElement();
        }
        
        /// <summary>
        /// Set the element reference.
        /// </summary>
        /// <param name="reference"></param>
        public void OnSetElementReference(ElementReference reference) => elementReference = reference;

        /// <summary>
        /// Focuses the current element.
        /// </summary>
        /// <returns></returns>
        public ValueTask FocusAsync() => elementReference.FocusAsync();

        /// <summary>
        /// Get the Element reference.
        /// </summary>
        /// <returns></returns>
        public ElementReference AsElementReference() => elementReference;

        /// <summary>
        /// Element reference operator.
        /// </summary>
        /// <param name="element"></param>
        public static implicit operator ElementReference(HtmlElement element) => element == null ? default : element.elementReference;
    }
}
