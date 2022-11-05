//-----------------------------------------------------------------------------
// FILE:	    CadenceLogTags.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
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
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Neon.Cadence;
using Neon.Cadence.Internal;
using Neon.Common;
using Neon.Diagnostics;
using Neon.Tasks;

namespace Neon.Cadence
{
    /// <summary>
    /// Identifies Cadence related logging tag names.
    /// </summary>
    public static class CadenceLogTags
    {
        /// <summary>
        /// The prefix in commonfor all Cadence log tag names.
        /// </summary>
        public const string Prefix = "neon.cadence.";

        /// <summary>
        /// <b>string value:</b> Used to identify the current activity via its workflow ID.
        /// </summary>
        public const string ActivityId = $"{Prefix}activity-id";

        /// <summary>
        /// <b>string value:</b> Used the identify the activity type name.
        /// </summary>
        public const string ActivityTypeName = $"{Prefix}activity-type-name";

        /// <summary>
        /// <b>bool value:</b> Used to indicate that the activvity is local.
        /// </summary>
        public const string ActivityIsLocal = $"{Prefix}activity-is-local";

        /// <summary>
        /// <b>string value:</b> Used to identify the current workflow via its workflow ID.
        /// </summary>
        public const string WorkflowId = $"{Prefix}workflow-id";

        /// <summary>
        /// <b>string value:</b> Used the identify the workflow type name.
        /// </summary>
        public const string WorkflowTypeName = $"{Prefix}workflow-type-name";
    }
}
