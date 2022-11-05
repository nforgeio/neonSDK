//-----------------------------------------------------------------------------
// FILE:	    TelemetrySpanExtensions.cs
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
using System.Diagnostics.Contracts;

using OpenTelemetry.Trace;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Extends the <see cref="TelemetrySpan"/> class.
    /// </summary>
    public static class TelemetrySpanExtensions
    {
        /// <summary>
        /// Adds an event with tags ti a <see cref="TelemetrySpan"/>.
        /// </summary>
        /// <param name="span">The span.</param>
        /// <param name="name">The event name.</param>
        /// <param name="attributeSetter">The action that sets any tags.</param>
        /// <remarks>
        /// <note>
        /// This method does nothing when the <paramref name="span"/> is not recording.
        /// </note>
        /// </remarks>
        public static void AddEvent(this TelemetrySpan span, string name, Action<SpanAttributes> attributeSetter)
        {
            Covenant.Requires<ArgumentNullException>(span != null, nameof(span));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name), nameof(name));
            Covenant.Requires<ArgumentNullException>(attributeSetter != null, nameof(attributeSetter));

            if (!span.IsRecording)
            {
                return;
            }

            var attributes = new SpanAttributes();

            attributeSetter(attributes);

            span.AddEvent(name, attributes: attributes);
        }
    }
}
