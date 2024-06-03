// -----------------------------------------------------------------------------
// FILE:	    WorkflowLogEnricher.cs
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
                collector.Add(TemporalEnricherTagNames.WorkflowNamespace, Workflow.Info.Namespace);
                collector.Add(TemporalEnricherTagNames.TaskQueue,         Workflow.Info.TaskQueue);
                collector.Add(TemporalEnricherTagNames.WorkflowAttempt,   Workflow.Info.Attempt);
                collector.Add(TemporalEnricherTagNames.WorkflowId,        Workflow.Info.WorkflowId);
                collector.Add(TemporalEnricherTagNames.WorkflowRunId,     Workflow.Info.RunId);
                collector.Add(TemporalEnricherTagNames.WorkflowType,      Workflow.Info.WorkflowType);

                if (!string.IsNullOrEmpty(Workflow.Info.ContinuedRunId))
                {
                    collector.Add(TemporalEnricherTagNames.WorkflowContinuedRunId, Workflow.Info.ContinuedRunId);
                }
            }
        }
    }
}