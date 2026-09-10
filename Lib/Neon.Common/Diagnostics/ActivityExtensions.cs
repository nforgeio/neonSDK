// -----------------------------------------------------------------------------
// FILE:	    ActivityExtensions.cs
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
using System.Diagnostics;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Null tolerant <see cref="Activity"/> helpers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="ActivitySource.StartActivity(string, ActivityKind)"/> returns <c>null</c> whenever
    /// nothing is listening, which is the common case.  These extensions accept <c>null</c> so call
    /// sites can stay free of <c>if (activity != null)</c> noise while still paying nothing
    /// when tracing is disabled.
    /// </para>
    /// </remarks>
    public static class ActivityExtensions
    {

        /// <summary>
        /// Sets a tag when <paramref name="activity"/> isn't <c>null</c>.
        /// </summary>
        /// <param name="activity">The activity or <c>null</c>.</param>
        /// <param name="name">The tag name.</param>
        /// <param name="value">The tag value.</param>
        /// <returns><paramref name="activity"/> to allow fluent chaining.</returns>
        public static Activity Tag(this Activity activity, string name, object value)
        {
            return activity?.SetTag(name, value);
        }

        /// <summary>
        /// Records an exception on <paramref name="activity"/> and marks it as failed.
        /// </summary>
        /// <param name="activity">The activity or <c>null</c>.</param>
        /// <param name="e">The exception that was thrown.</param>
        /// <returns><paramref name="activity"/> to allow fluent chaining.</returns>
        /// <remarks>
        /// This records both the exception event (so the stack trace reaches the backend) and the
        /// span status, because most backends key their error rates off the status rather than the
        /// presence of an exception event.
        /// </remarks>
        public static Activity Error(this Activity activity, Exception e)
        {
            if (activity == null)
            {
                return null;
            }

            activity.AddException(e);
            activity.SetStatus(ActivityStatusCode.Error, e.Message);

            return activity;
        }
    }
}