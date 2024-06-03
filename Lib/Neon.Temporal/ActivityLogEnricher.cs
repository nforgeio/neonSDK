// -----------------------------------------------------------------------------
// FILE:	    ActivityLogEnricher.cs
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

using Microsoft.Extensions.Diagnostics.Enrichment;

using Temporalio.Activities;

namespace Neon.Temporal
{
    /// <summary>
    /// Augments log messages with information about the current Temporal activity execution context.
    /// </summary>
    public class ActivityLogEnricher : ILogEnricher
    {
        /// <inheritdoc/>
        public void Enrich(IEnrichmentTagCollector collector)
        {
            if (ActivityExecutionContext.HasCurrent)
            {
                collector.Add(TemporalEnricherTagNames.WorkflowNamespace, ActivityExecutionContext.Current.Info.WorkflowNamespace);
                collector.Add(TemporalEnricherTagNames.TaskQueue,         ActivityExecutionContext.Current.Info.TaskQueue);
                collector.Add(TemporalEnricherTagNames.ActivityAttempt,   ActivityExecutionContext.Current.Info.Attempt);
                collector.Add(TemporalEnricherTagNames.ActivityId,        ActivityExecutionContext.Current.Info.ActivityId);
                collector.Add(TemporalEnricherTagNames.ActivityIsLocal,   ActivityExecutionContext.Current.Info.IsLocal);
                collector.Add(TemporalEnricherTagNames.ActivityType,      ActivityExecutionContext.Current.Info.ActivityType);
                collector.Add(TemporalEnricherTagNames.WorkflowId,        ActivityExecutionContext.Current.Info.WorkflowId);
                collector.Add(TemporalEnricherTagNames.WorkflowRunId,     ActivityExecutionContext.Current.Info.WorkflowRunId);
                collector.Add(TemporalEnricherTagNames.WorkflowType,      ActivityExecutionContext.Current.Info.WorkflowType);

                if (ActivityExecutionContext.Current.CancellationToken.IsCancellationRequested)
                {
                    collector.Add(TemporalEnricherTagNames.ActivityCancelReason, ActivityExecutionContext.Current.CancelReason);
                }
            }
        }
    }
}