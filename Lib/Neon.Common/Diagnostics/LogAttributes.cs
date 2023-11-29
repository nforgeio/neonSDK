//-----------------------------------------------------------------------------
// FILE:        LogAttributes.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Holds attributes to be included in log events recorded by <see cref="ILogger"/> 
    /// <see cref="LoggerExtensions"/>.
    /// </summary>
    public class LogAttributes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogAttributes"/> class.
        /// </summary>
        public LogAttributes()
        {
            this.Attributes = new LogAttributesCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogAttributes"/> class by
        /// cloning another <see cref="LogAttributes"/> instance.
        /// </summary>
        /// <param name="attributes">Initial attributes to store in the collection.</param>
        public LogAttributes(LogAttributes attributes)
            : this()
        {
            Covenant.Requires<ArgumentNullException>(attributes != null, nameof(attributes));

            foreach (KeyValuePair<string, object> item in attributes.Attributes)
            {
                this.AddInternal(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogAttributes"/> class from
        /// a attributes enumerable.
        /// </summary>
        /// <param name="attributes">Initial attributes to store in the collection.</param>
        public LogAttributes(IEnumerable<KeyValuePair<string, object>> attributes)
            : this()
        {
            Covenant.Requires<ArgumentNullException>(attributes != null, nameof(attributes));

            foreach (KeyValuePair<string, object> item in attributes)
            {
                this.AddInternal(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Returns the attribute collection.
        /// </summary>
        internal LogAttributesCollection Attributes { get; }

        /// <summary>
        /// Returns the number of attributes in the collection.
        /// </summary>
        public int Count => Attributes.Count;

        /// <summary>
        /// Clears the attributes collection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Attributes.Clear();
        }

        /// <summary>
        /// Adds a <c>long</c> attribute.
        /// </summary>
        /// <param name="key">Attribute key.</param>
        /// <param name="value">Attribute value.</param>
        public void Add(string key, long value)
        {
            this.AddInternal(key, value);
        }

        /// <summary>
        /// Adds a <c>string</c> attribute.
        /// </summary>
        /// <param name="key">Attribute key.</param>
        /// <param name="value">Attribute value.</param>
        public void Add(string key, string value)
        {
            this.AddInternal(key, value);
        }

        /// <summary>
        /// Adds a <c>bool</c> attribute.
        /// </summary>
        /// <param name="key">Attribute key.</param>
        /// <param name="value">Attribute value.</param>
        public void Add(string key, bool value)
        {
            this.AddInternal(key, value);
        }

        /// <summary>
        /// Adds a <c>double</c> attribute.
        /// </summary>
        /// <param name="key">Attribute key.</param>
        /// <param name="value">Attribute value.</param>
        public void Add(string key, double value)
        {
            this.AddInternal(key, value);
        }

        /// <summary>
        /// Adds an arbitrary object attribute.
        /// </summary>
        /// <param name="key">Attribute key.</param>
        /// <param name="value">Attribute value.</param>
        public void Add(string key, object value)
        {
            AddInternal(key, value);
        }

        /// <summary>
        /// Used internally to add a attribute to the collection.
        /// </summary>
        /// <param name="item">Attribute item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddInternal(KeyValuePair<string, object> item)
        {
            this.Attributes.Add(item);
        }

        /// <summary>
        /// Used internally to add a attribute to the collection.
        /// </summary>
        /// <param name="key">Attribute key.</param>
        /// <param name="value">Attribute value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddInternal(string key, object value)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(key), nameof(key));

            this.Attributes[key] = value;
        }
    }
}
