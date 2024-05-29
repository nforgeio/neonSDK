// -----------------------------------------------------------------------------
// FILE:	    EnumerationExtensions.cs
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Neon.Common
{
    /// <summary>
    /// Implements <see cref="IEnumerable{T}"/> extension methods.
    /// </summary>
    public static class EnumerationExtensions
    {
        /// <summary>
        /// Returns up to the specified number of items from the front of an enumerable.
        /// If the enumerable includes less than <paramref name="maxCount"/> then this
        /// will return all of the items.
        /// </summary>
        /// <typeparam name="T">Specifies the enumerab;e's item type.</typeparam>
        /// <param name="items">Specifies the enumerable to take items from.</param>
        /// <param name="maxCount">Specifies the maximumnumber of items to to take.</param>
        /// <returns>The taken items.</returns>
        public static IEnumerable<T> TakeUpTo<T>(this IEnumerable<T> items, int maxCount)
        {
            Covenant.Requires<ArgumentNullException>(items != null, nameof(items));
            Covenant.Requires<ArgumentException>(maxCount >= 0, nameof(maxCount));

            return items.Take(Math.Min(maxCount, items.Count()));
        }
    }
}
