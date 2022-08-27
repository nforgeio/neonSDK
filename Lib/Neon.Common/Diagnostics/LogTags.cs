//-----------------------------------------------------------------------------
// FILE:	    LogTags.cs
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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Holds tags to be included in log events recorded by <see cref="ILogger"/> logging 
    /// methods.
    /// </summary>
    public class LogTags
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogTags"/> class.
        /// </summary>
        public LogTags()
        {
            this.Tags = new LogTagsCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogTags"/> class by
        /// cloning another <see cref="LogTags"/> instance.
        /// </summary>
        /// <param name="tags">Initial tags to store in the collection.</param>
        public LogTags(LogTags tags)
            : this()
        {
            Covenant.Requires<ArgumentNullException>(tags != null, nameof(tags));

            foreach (KeyValuePair<string, object> item in tags.Tags)
            {
                this.AddInternal(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogTags"/> class from
        /// a tag enumerable.
        /// </summary>
        /// <param name="tags">Initial tags to store in the collection.</param>
        public LogTags(IEnumerable<KeyValuePair<string, object>> tags)
            : this()
        {
            Covenant.Requires<ArgumentNullException>(tags != null, nameof(tags));

            foreach (KeyValuePair<string, object> item in tags)
            {
                this.AddInternal(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Returns the tag collection.
        /// </summary>
        internal LogTagsCollection Tags { get; }

        /// <summary>
        /// Returns the number of tags in the collection.
        /// </summary>
        public int Count => Tags.Count;

        /// <summary>
        /// Clears the tags collection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            Tags.Clear();
        }

        /// <summary>
        /// Adds a <c>long</c> tags.
        /// </summary>
        /// <param name="key">Tag key.</param>
        /// <param name="value">Tag value.</param>
        public void Add(string key, long value)
        {
            this.AddInternal(key, value);
        }

        /// <summary>
        /// Adds a <c>string</c> tags.
        /// </summary>
        /// <param name="key">Tag key.</param>
        /// <param name="value">Tag value.</param>
        public void Add(string key, string value)
        {
            this.AddInternal(key, value);
        }

        /// <summary>
        /// Adds a <c>bool</c> tags.
        /// </summary>
        /// <param name="key">Tag key.</param>
        /// <param name="value">Tag value.</param>
        public void Add(string key, bool value)
        {
            this.AddInternal(key, value);
        }

        /// <summary>
        /// Adds a <c>double</c> tags.
        /// </summary>
        /// <param name="key">Tag key.</param>
        /// <param name="value">Tag value.</param>
        public void Add(string key, double value)
        {
            this.AddInternal(key, value);
        }

        /// <summary>
        /// Adds an arbitrary object tags.
        /// </summary>
        /// <param name="key">Tag key.</param>
        /// <param name="value">Tag value.</param>
        public void Add(string key, object value)
        {
            AddInternal(key, value);
        }

        /// <summary>
        /// Used internally to add a tag item to the collection.
        /// </summary>
        /// <param name="item">Tag item.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddInternal(KeyValuePair<string, object> item)
        {
            this.Tags.Add(item);
        }

        /// <summary>
        /// Used internally to add a tag to the collection.
        /// </summary>
        /// <param name="key">Tag key.</param>
        /// <param name="value">Tag value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddInternal(string key, object value)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(key), nameof(key));

            this.Tags[key] = value;
        }
    }
}
