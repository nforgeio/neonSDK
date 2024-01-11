// -----------------------------------------------------------------------------
// FILE:	    WorkflowLogEnricher.cs
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

using Temporalio.Workflows;

namespace Neon.Temporal
{
    /// <summary>
    /// Augments log messages with information about the current Temporal workflow execution context.
    /// </summary>
    public class WorkflowLogEnricher : ILogEnricher
    {
        /// <inheritdoc/>
        public void Enrich(IEnrichmentTagCollector collector)
        {
            if (Workflow.InWorkflow)
            {
                collector.Add("namespace", Workflow.Info.Namespace);
                collector.Add("run_id", Workflow.Info.RunId);
                collector.Add("workflow_id", Workflow.Info.WorkflowId);
                collector.Add("workflow_type", Workflow.Info.WorkflowType);
                collector.Add("task_queue", Workflow.Info.TaskQueue);
                collector.Add("attempt", Workflow.Info.Attempt);

                if (!string.IsNullOrEmpty(Workflow.Info.ContinuedRunId))
                {
                    collector.Add("continued_run_id", Workflow.Info.ContinuedRunId);
                }
            }
        }
    }
}