//-----------------------------------------------------------------------------
// FILE:	    LogAttributes.cs
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
using System.Diagnostics;
using System.Diagnostics.Contracts;

using Neon.Common;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Holds attributes to be included in log events recorded by <see cref="INeonLogger"/> logging 
    /// methods.
    /// </summary>
    /// <remarks>LogAttributes is a wrapper around <see cref="ActivityTagsCollection"/> class.</remarks>
    public class LogAttributes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogAttributes"/> class.
        /// </summary>
        public LogAttributes()
        {
            this.Attributes = new ActivityTagsCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogAttributes"/> class.
        /// </summary>
        /// <param name="attributes">Initial attributes to store in the collection.</param>
        public LogAttributes(LogAttributes attributes)
            : this()
        {
            Covenant.Requires<ArgumentNullException>(attributes != null, nameof(attributes));

            foreach (KeyValuePair<string, object> kvp in attributes.Attributes)
            {
                this.AddInternal(kvp.Key, kvp.Value);
            }
        }

        internal ActivityTagsCollection Attributes { get; }

        /// <summary>
        /// Adds a <c>long</c> attribute.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <param name="value">Entry value.</param>
        public void Add(string key, long value)
        {
            this.AddInternal(key, value);
        }

        /// <summary>
        /// Adds a <c>string</c> attribute.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <param name="value">Entry value.</param>
        public void Add(string key, string value)
        {
            this.AddInternal(key, value);
        }

        /// <summary>
        /// Adds a <c>bool</c> attribute.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <param name="value">Entry value.</param>
        public void Add(string key, bool value)
        {
            this.AddInternal(key, value);
        }

        /// <summary>
        /// Adds a <c>double</c> attribute.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <param name="value">Entry value.</param>
        public void Add(string key, double value)
        {
            this.AddInternal(key, value);
        }

        /// <summary>
        /// Adds an arbitrary object attribute.
        /// </summary>
        /// <param name="key">Entry key.</param>
        /// <param name="value">Entry value.</param>
        public void Add(string key, object value)
        {
            AddInternal(key, value);
        }

        private void AddInternal(string key, object value)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(key), nameof(key));

            this.Attributes[key] = value;
        }
    }
}
