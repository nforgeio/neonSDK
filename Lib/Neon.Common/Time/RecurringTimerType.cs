//-----------------------------------------------------------------------------
// FILE:        RecurringTimerType.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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

using System;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace Neon.Time
{
    /// <summary>
    /// Enumerates the possible <see cref="RecurringTimer" /> types.
    /// </summary>
    public enum RecurringTimerType
    {
        /// <summary>
        /// The timer never fires.
        /// </summary>
        Disabled,

        /// <summary>
        /// The timer will be fired once per minute. 
        /// </summary>
        Minute,

        /// <summary>
        /// The timer will be fired once every 15 minutes.
        /// </summary>
        QuarterHour,

        /// <summary>
        /// The timer will be fired once per hour.
        /// </summary>
        Hourly,

        /// <summary>
        /// The timer will be fired once per day.
        /// </summary>
        Daily,

        /// <summary>
        /// The timer is fired on a specified interval rather than a 
        /// specific period offset.  This is similar to how <see cref="PolledTimer" />
        /// works.
        /// </summary>
        Interval
    }
}
