// -----------------------------------------------------------------------------
// FILE:	    HeadlessComboboxInput.razor.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using Neon.Tasks;

namespace Neon.Tailwind
{
    public partial class HeadlessComboboxInput<TValue> : ComponentBase
    {
        [CascadingParameter]
        public HeadlessCombobox<TValue> CascadedCombobox { get; set; } = default!;


        [Parameter]
        public Expression<Func<TValue, string>> DisplayValue { get; set; } = default!;

        [Parameter]
        public EventCallback<string> ValueChanged { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

        private string inputValue;
        private Func<TValue, string> displayValue;

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            displayValue = DisplayValue.Compile();
        }
        private async Task ValueChangedAsync()
        {
            if (ValueChanged.HasDelegate)
            {
                await ValueChanged.InvokeAsync(inputValue);
            }
        }

        public async Task SetValueAsync(TValue value)
        {
            if (value == null)
            {
                inputValue = string.Empty;
            }

            inputValue = displayValue(value);

            await Task.CompletedTask;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            {
                CascadedCombobox.InputElement = this;
            }
        }

        private async Task OnClickedAsync()
        {
            await SyncContext.Clear;

            await CascadedCombobox.Open();
        }
    }
}
