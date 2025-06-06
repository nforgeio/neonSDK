// -----------------------------------------------------------------------------
// FILE:	    BrowserTimeProvider.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neon.Blazor
{
    public class BrowserTimeProvider : TimeProvider
    {
        private TimeZoneInfo browserTimeZone;

        public event EventHandler LocalTimeZoneChanged;

        public override TimeZoneInfo LocalTimeZone
            => browserTimeZone ?? base.LocalTimeZone;

        public bool IsLocalTimeZoneSet => browserTimeZone != null;

        // Set the local time zone
        public void SetBrowserTimeZone(string timeZone)
        {
            if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZone, out var timeZoneInfo))
            {
                timeZoneInfo = null;
            }

            if (timeZoneInfo != LocalTimeZone)
            {
                browserTimeZone = timeZoneInfo;
                LocalTimeZoneChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
