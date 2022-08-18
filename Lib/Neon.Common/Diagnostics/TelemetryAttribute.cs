//-----------------------------------------------------------------------------
// FILE:	    TelemetryAttribute.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright (c) 2005-2022 by neonFORGE LLC.  All rights reserved.
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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using Neon.Common;

using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;

namespace Neon.Diagnostics
{
    /// <summary>
    /// <para>
    /// Used to specifies attributes to be added log and trace events.  Each attribute has
    /// a string key and an optional <c>object</c> value.
    /// </para>
    /// <note>
    /// <para>
    /// Keys may only include ASCII letters, digits, underscores, dashes or periods at this time
    /// for compatibility with the largest possible number of telemetry related systems and this
    /// type limits keys to 255 characters.  There appears to still be some debate in the OpenTelemetry 
    /// world around attribute and label names:
    /// </para>
    /// <para>
    /// https://github.com/open-telemetry/opentelemetry-specification/issues/504
    /// </para>
    /// <para>
    /// We're going to take the more restrictive approach for now and potentially reconsider if or
    /// when OpenTelemetry makes a decision on this.
    /// </para>
    /// </note>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Telemetry attributes can be used to add additional information to log events recorded via the
    /// <see cref="INeonLogger"/> logger and <see cref="TelemetrySpan.AddEvent(string, DateTimeOffset, SpanAttributes)"/> methods
    /// </para>
    /// </remarks>
    public struct TelemetryAttribute
    {
        //---------------------------------------------------------------------
        // Static members

        /// <summary>
        /// Matches 1-255 ASCII letters, digits, periods, dashes, or underscores.
        /// </summary>
        private static Regex keyRegex = new Regex(@"^([a-zA-Z\d\.\-_]){1,255}$", RegexOptions.Compiled);

        /// <summary>
        /// Validates a key string.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <exception cref="ArgumentException">Thrown if the key is invalid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateKey(string key)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(key), nameof(key));

            if (!keyRegex.IsMatch(key))
            {
                throw new ArgumentException($"Attribute key [{key}] is is invalid.  Keys are limited to 1-255 ASCII letters, digits, periods, dashes, or underscores.", nameof(key));
            }
        }

        //---------------------------------------------------------------------
        // Instance members

        /// <summary>
        /// Constructs a <see cref="TelemetryAttribute"/> with just a key.
        /// </summary>
        /// <param name="key">The attribute key (must not be <c>null</c> or empty).</param>
        public TelemetryAttribute(string key)
        {
            ValidateKey(key);

            this.Key   = key;
            this.Value = null;
        }

        /// <summary>
        /// Constructs a <see cref="TelemetryAttribute"/> with a key and value.
        /// </summary>
        /// <param name="key">The attribute key (must not be <c>null</c> or empty).</param>
        /// <param name="value">The attribute value (may be <c>nulll</c>.</param>
        public TelemetryAttribute(string key, object value)
        {
            ValidateKey(key);

            this.Key   = key;
            this.Value = value;
        }

        /// <summary>
        /// Returns the attribute key.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Returns the attribute value.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Validates the attribute.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the key is invalid.</exception>
        public void Validate()
        {
            ValidateKey(Key);
        }
    }
}
