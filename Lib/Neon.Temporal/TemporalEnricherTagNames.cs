// -----------------------------------------------------------------------------
// FILE:	    TemporalEnricherTagNames.cs
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

namespace Neon.Temporal
{
    /// <summary>
    /// Tag names for the Temporal Log Enrichers.
    /// </summary>
    public static class TemporalEnricherTagNames
    {
        /// <summary>
        /// The Temporal task queue name.
        /// </summary>
        public const string TaskQueue = "temporal.task_queue";

        /// <summary>
        /// The Temporal activity attempt number.
        /// </summary>
        public const string ActivityAttempt = "temporal.activity.attempt";

        /// <summary>
        /// The Temporal activity ID.
        /// </summary>
        public const string ActivityId = "temporal.activity.id";

        /// <summary>
        /// The Temporal activity is local flag.
        /// </summary>
        public const string ActivityIsLocal = "temporal.activity.is_local";

        /// <summary>
        /// The Temporal activity type.
        /// </summary>
        public const string ActivityType = "temporal.activity.type";

        /// <summary>
        /// The Temporal activity cancel reason.
        /// </summary>
        public const string ActivityCancelReason = "temporal.activity.cancel_reason";

        /// <summary>
        /// The Temporal workflow attempt number.
        /// </summary>
        public const string WorkflowAttempt = "temporal.workflow.attempt";

        /// <summary>
        /// The Temporal workflow ID.
        /// </summary>
        public const string WorkflowId = "temporal.workflow.id";

        /// <summary>
        /// The Temporal workflow namespace.
        /// </summary>
        public const string WorkflowNamespace = "temporal.workflow.namespace";

        /// <summary>
        /// The Temporal workflow run ID.
        /// </summary>
        public const string WorkflowRunId = "temporal.workflow.run_id";

        /// <summary>
        /// The Temporal workflow type.
        /// </summary>
        public const string WorkflowType = "temporal.workflow.type";

        /// <summary>
        /// The Temporal workflow continued run ID.
        /// </summary>
        public const string WorkflowContinuedRunId = "temporal.workflow.continued_run_id";
    }
}
