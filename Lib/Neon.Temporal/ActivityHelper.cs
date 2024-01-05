// -----------------------------------------------------------------------------
// FILE:	    ActivityHelper.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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

using Neon.Tasks;

using Temporalio.Activities;

namespace Neon.Temporal
{
    /// <summary>
    /// Activity Helper Methods.
    /// </summary>
    public static class ActivityHelper
    {
        /// <summary>
        /// Automatically heartbeats the current activity context.
        /// </summary>
        /// <param name="interval">The interval between heartbeats. This is optional and will default to 1/3 the Heartbeat Timeout</param>
        /// <returns></returns>
        public static AsyncTimer AutoHeartbeat(TimeSpan? interval = null)
        {
            if (!interval.HasValue)
            {
                if (ActivityExecutionContext.HasCurrent
                    && ActivityExecutionContext.Current.Info.HeartbeatTimeout.HasValue)
                {
                    interval = TimeSpan.FromTicks(ActivityExecutionContext.Current.Info.HeartbeatTimeout.Value.Ticks / 3);
                }
                else
                {
                    interval = TimeSpan.MaxValue;
                }
            }

            var timer = new AsyncTimer(async () =>
            {
                await SyncContext.Clear;

                if (ActivityExecutionContext.HasCurrent)
                {
                    ActivityExecutionContext.Current.Heartbeat();
                }
            });

            timer.Start(interval.Value);

            return timer;
        }
    }
}
