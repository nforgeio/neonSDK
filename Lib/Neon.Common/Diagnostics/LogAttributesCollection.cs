//-----------------------------------------------------------------------------
// FILE:        LogAttributesCollection.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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

// Adapted from .NET Foundation code under their MIT license:
//
//      https://github.com/dotnet/runtime/blob/215b39abf947da7a40b0cb137eab4bceb24ad3e3/src/libraries/System.Diagnostics.DiagnosticSource/src/System/Diagnostics/ActivityTagsCollection.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Neon.Diagnostics
{
    /// <summary>
    /// LogTagsCollection is a collection class used to store tracing tags.
    /// This collection will be used with classes like <see cref="ActivityEvent"/> and <see cref="ActivityLink"/>.
    /// This collection behaves as follows:
    ///     - The collection items will be ordered according to how they are added.
    ///     - Don't allow duplication of items with the same key.
    ///     - When using the indexer to store an item in the collection:
    ///         - If the item has a key that previously existed in the collection and the value is null, the collection item matching the key will be removed from the collection.
    ///         - If the item has a key that previously existed in the collection and the value is not null, the new item value will replace the old value stored in the collection.
    ///         - Otherwise, the item will be added to the collection.
    ///     - Add method will add a new item to the collection if an item doesn't already exist with the same key. Otherwise, it will throw an exception.
    /// </summary>
    /// <remarks>
    /// Adapted from .NET Foundation code under their MIT license: 
    /// https://github.com/dotnet/runtime/blob/215b39abf947da7a40b0cb137eab4bceb24ad3e3/src/libraries/System.Diagnostics.DiagnosticSource/src/System/Diagnostics/ActivityTagsCollection.cs
    /// </remarks>
    internal class LogAttributesCollection : IDictionary<string, object>
    {
        private List<KeyValuePair<string, object>> _list = new List<KeyValuePair<string, object>>();

        /// <summary>
        /// Create a new instance of the collection.
        /// </summary>
        public LogAttributesCollection()
        {
        }

        /// <summary>
        /// Create a new instance of the collection and store the input list items in the collection.
        /// </summary>
        /// <param name="list">Initial list to store in the collection.</param>
        public LogAttributesCollection(IEnumerable<KeyValuePair<string, object>> list)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            foreach (KeyValuePair<string, object> kvp in list)
            {
                if (kvp.Key != null)
                {
                    this[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// Get or set collection item
        /// When setting a value to this indexer property, the following behavior will be observed:
        ///     - If the key previously existed in the collection and the value is null, the collection item matching the key will get removed from the collection.
        ///     - If the key previously existed in the collection and the value is not null, the value will replace the old value stored in the collection.
        ///     - Otherwise, a new item will get added to the collection.
        /// </summary>
        /// <value>Object mapped to the key</value>
        public object this[string key]
        {
            get
            {
                int index = FindIndex(key);
                return index < 0 ? null : _list[index].Value;
            }

            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                int index = FindIndex(key);
                if (value == null)
                {
                    if (index >= 0)
                    {
                        _list.RemoveAt(index);
                    }
                    return;
                }

                if (index >= 0)
                {
                    _list[index] = new KeyValuePair<string, object>(key, value);
                }
                else
                {
                    _list.Add(new KeyValuePair<string, object>(key, value));
                }
            }
        }

        /// <summary>
        /// Get the list of the keys of all stored tags.
        /// </summary>
        public ICollection<string> Keys
        {
            get
            {
                List<string> list = new List<string>(_list.Count);
                foreach (KeyValuePair<string, object> kvp in _list)
                {
                    list.Add(kvp.Key);
                }
                return list;
            }
        }

        /// <summary>
        /// Get the list of the values of all stored tags.
        /// </summary>
        public ICollection<object> Values
        {
            get
            {
                List<object> list = new List<object>(_list.Count);
                foreach (KeyValuePair<string, object> kvp in _list)
                {
                    list.Add(kvp.Value);
                }
                return list;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count => _list.Count;

        /// <summary>
        /// Clears the collection.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _list.Clear();
        }

        /// <summary>
        /// Adds an attribute with the provided key and value to the collection.
        /// This collection doesn't allow adding two attributes with the same key.
        /// </summary>
        /// <param name="key">The attribute key.</param>
        /// <param name="value">The attribute value.</param>
        public void Add(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            int index = FindIndex(key);
            if (index >= 0)
            {
                throw new InvalidOperationException($"Key [{key}] already exists.");
            }

            _list.Add(new KeyValuePair<string, object>(key, value));
        }

        /// <summary>
        /// Adds an item to the collection
        /// </summary>
        /// <param name="item">Key and value pair of the attribute to add to the collection.</param>
        public void Add(KeyValuePair<string, object> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            int index = FindIndex(item.Key);
            if (index >= 0)
            {
                throw new InvalidOperationException($"Key [{item.Key}] already exists.");
            }

            _list.Add(item);
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<string, object> item) => _list.Contains(item);

        /// <inheritdoc/>
        public bool ContainsKey(string key) => FindIndex(key) >= 0;

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => new Enumerator(_list);

        /// <inheritdoc/>
        public Enumerator GetEnumerator() => new Enumerator(_list);

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(_list);

        /// <inheritdoc/>
        public bool Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            int index = FindIndex(key);
            if (index >= 0)
            {
                _list.RemoveAt(index);
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<string, object> item) => _list.Remove(item);

        /// <inheritdoc/>
        public bool TryGetValue(string key, out object value)
        {
            int index = FindIndex(key);
            if (index >= 0)
            {
                value = _list[index].Value;
                return true;
            }

            value = null;
            return false;
        }

        /// <summary>
        /// FindIndex finds the index of item in the list having a key matching the input key.
        /// We didn't use List.FindIndex to avoid the extra allocation caused by the closure when calling the Predicate delegate.
        /// </summary>
        /// <param name="key">The key to search the item in the list</param>
        /// <returns>The index of the found item, or -1 if the item not found.</returns>
        private int FindIndex(string key)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                if (_list[i].Key == key)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <inheritdoc/>
        public struct Enumerator : IEnumerator<KeyValuePair<string, object>>, IEnumerator
        {
            private List<KeyValuePair<string, object>>.Enumerator _enumerator;
            
            internal Enumerator(List<KeyValuePair<string, object>> list) => _enumerator = list.GetEnumerator();

            /// <inheritdoc/>
            public KeyValuePair<string, object> Current => _enumerator.Current;

            object IEnumerator.Current => ((IEnumerator)_enumerator).Current;

            /// <inheritdoc/>
            public void Dispose() => _enumerator.Dispose();

            /// <inheritdoc/>
            public bool MoveNext() => _enumerator.MoveNext();

            void IEnumerator.Reset() => ((IEnumerator)_enumerator).Reset();
        }
    }
}
