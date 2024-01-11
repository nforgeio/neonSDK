// -----------------------------------------------------------------------------
// FILE:	    ActivityLogEnricher.cs
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
                collector.Add("activity_id", ActivityExecutionContext.Current.Info.ActivityId);
                collector.Add("activity_type", ActivityExecutionContext.Current.Info.ActivityType);
                collector.Add("workflow_id", ActivityExecutionContext.Current.Info.WorkflowId);
                collector.Add("workflow_run_id", ActivityExecutionContext.Current.Info.WorkflowRunId);
                collector.Add("workflow_namespace", ActivityExecutionContext.Current.Info.WorkflowNamespace);
                collector.Add("workflow_type", ActivityExecutionContext.Current.Info.WorkflowType);
                collector.Add("attempt", ActivityExecutionContext.Current.Info.Attempt);
                collector.Add("is_local", ActivityExecutionContext.Current.Info.IsLocal);
                collector.Add("task_queue", ActivityExecutionContext.Current.Info.TaskQueue);
            }
        }
    }
}