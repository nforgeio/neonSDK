//-----------------------------------------------------------------------------
// FILE:	    TagArgs.cs
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
using System.Text;
using Microsoft.Extensions.ObjectPool;

using Neon.Common;

namespace Neon.Diagnostics
{
    /// <summary>
    /// Holds an object array that will be used for passing tag values to the underlying
    /// log extenssions methods.  These will be pooled to avoid memory allocations when
    /// possible.
    /// </summary>
    internal class TagArgs
    {
        /// <summary>
        /// Constructs an instance with an <see cref="DiagnosticPools.TagArgsArrayLength"/> argument array.
        /// </summary>
        public TagArgs()
        {
            Values = new object[DiagnosticPools.TagArgsArrayLength];
        }

        /// <summary>
        /// Constructs an instance with the specified number of elements in the argument array.
        /// </summary>
        /// <param name="length">The length of the argument array.</param>
        public TagArgs(int length)
        {
            Values = new object[length];
        }

        /// <summary>
        /// Returns the object array to be used to hold tag values.
        /// </summary>
        public object[] Values { get; private set; }
    }
}
