// -----------------------------------------------------------------------------
// FILE:	    ArrayExtensions.cs
// CONTRIBUTOR: NEONFORGE Team
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Neon.ECharts
{
    public static partial class ArrayExtensions
    {
        /// <summary>
        /// Converts a list of lists to a two-dimensional array.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the lists.</typeparam>
        /// <param name="source">The list of lists to convert.</param>
        /// <returns>A two-dimensional array containing the elements from the list of lists.</returns>
        public static T[,] To2D<T>(this List<List<T>> source)
        {
            var firstDim = source.Count;
            var secondDim = source.Select(row => row.Count).FirstOrDefault();
            if (!source.All(row => row.Count == secondDim))
                throw new InvalidOperationException();
            var result = new T[firstDim, secondDim];
            for (var i = 0; i < firstDim; i++)
                for (int j = 0, count = source[i].Count; j < count; j++)
                    result[i, j] = source[i][j];
            return result;
        }
    }
}
