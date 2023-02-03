//-----------------------------------------------------------------------------
// FILE:	    DateTimeExtensions.cs
// CONTRIBUTOR: Marcus Bowyer
// COPYRIGHT:	Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Neon.Common
{
    /// <summary>
    /// <see cref="DateTime"/> extensions.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// <para>
        /// Rounds a <see cref="DateTime"/> up to the nearest specified interval, like:
        /// </para>
        /// <code language="c#">
        /// var date      = new DateTime(2010, 02, 05, 10, 35, 25, 450); // 2010/02/05 10:35:25
        /// var roundedUp = date.RoundUp(TimeSpan.FromMinutes(15));      // 2010/02/05 10:45:00
        /// </code>
        /// </summary>
        /// <param name="value">The datetime to be rounded.</param>
        /// <param name="interval">The time interval to be rounded to.</param>
        /// <returns>The rounded date.</returns>
        public static DateTime RoundUp(this DateTime value, TimeSpan interval)
        {
            Covenant.Requires<ArgumentException>(interval > TimeSpan.Zero, nameof(interval));

            var modTicks = value.Ticks % interval.Ticks;
            var delta    = modTicks != 0 ? interval.Ticks - modTicks : 0;

            return new DateTime(value.Ticks + delta, value.Kind);
        }

        /// <summary>
        /// <para>
        /// Rounds a <see cref="DateTime"/> down to the nearest specified interval, like:
        /// </para>
        /// <code language="c#">
        /// var date        = new DateTime(2010, 02, 05, 10, 35, 25, 450); // 2010/02/05 10:35:25
        /// var roundedDown = date.RoundDown(TimeSpan.FromMinutes(15));    // 2010/02/05 10:30:00
        /// </code>
        /// </summary>
        /// <param name="value">The datetime to be rounded.</param>
        /// <param name="interval">The time interval to be rounded to.</param>
        /// <returns></returns>
        public static DateTime RoundDown(this DateTime value, TimeSpan interval)
        {
            Covenant.Requires<ArgumentException>(interval > TimeSpan.Zero, nameof(interval));

            var delta = value.Ticks % interval.Ticks;

            return new DateTime(value.Ticks - delta, value.Kind);
        }

        /// <summary>
        /// <para>
        /// Rounds a <see cref="DateTime"/> to the nearest specified interval, like:
        /// </para>
        /// <code language="c#">
        /// var date             = new DateTime(2010, 02, 05, 10, 35, 25, 450);   // 2010/02/05 10:35:25
        /// var roundedToNearest = date.RoundToNearest(TimeSpan.FromMinutes(15)); // 2010/02/05 10:30:00
        /// </code>
        /// </summary>
        /// <param name="value">The datetime to be rounded.</param>
        /// <param name="interval">The time interval to be rounded to.</param>
        /// <returns></returns>
        public static DateTime RoundToNearest(this DateTime value, TimeSpan interval)
        {
            Covenant.Requires<ArgumentException>(interval > TimeSpan.Zero, nameof(interval));

            var delta   = value.Ticks % interval.Ticks;
            var roundUp = delta >= interval.Ticks / 2;
            var offset  = roundUp ? interval.Ticks : 0;

            return new DateTime(value.Ticks + offset - delta, value.Kind);
        }

        /// <summary>
        /// Converts the <see cref="DateTime"/> into the number of milliseconds since the
        /// Unix Epoc (midnight 1-1-1070 UTC).
        /// </summary>
        /// <param name="value">The time being converted.</param>
        /// <returns>The Unix time in milliseconds.</returns>
        public static long ToUnixEpochMilliseconds(this DateTime value)
        {
            // 1 tick is 100ns so we need to divide by 10,000 to convert to milliseconds.

            return (value.ToUniversalTime().Ticks - NeonHelper.UnixEpoch.Ticks) / 10000;
        }

        /// <summary>
        /// Converts the <see cref="DateTime"/> into the number of neonseconds since the
        /// Unix Epoc (midnight 1-1-1070 UTC).
        /// </summary>
        /// <param name="value">The time being converted.</param>
        /// <returns>The Unix time in naonseconds.</returns>
        public static long ToUnixEpochNanoseconds(this DateTime value)
        {
            // 1 tick is 100ns so we need to multiply by 100 to convert to nanoseconds.

            return (value.ToUniversalTime().Ticks - NeonHelper.UnixEpoch.Ticks) * 100;
        }
    }
}
