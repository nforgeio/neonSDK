//-----------------------------------------------------------------------------
// FILE:	    DiagnosticPools.cs
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
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.Extensions.ObjectPool;

using Neon.Common;

namespace Neon.Diagnostics
{
    /// <summary>
    /// This class is used to pool internal diagnostic related objects so they can
    /// be reused to avoid unnecessary memory allocations and related GC pressure.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Currently this class maintains an internal pool of collections to cache internal
    /// logging related attribute collections to improve performance by reusing these collections
    /// to avoid unnecessary allocations and GC pressure.  The defauilt settings should
    /// work for most applications, but it's possible to configure some settings via
    /// environment variables.
    /// </para>
    /// <list type="table">
    /// <item>
    ///     <term><b>NEON_DIAGNOSTICS_POOLLIMIT</b></term>
    ///     <description>
    ///     <para>
    ///     <b>(positive integer):</b> Specifies the maximum number of items that will be
    ///     retained by each diagnostic related pool.  This defaults to <b>64</b> when the
    ///     environment variable is not present or does not represent a positive integer value.
    ///     </para>
    ///     <note>
    ///     This setting doesn't limit the number of objects that can be created and returned
    ///     by the object pools, it just limits the number of objects that are retained by
    ///     the pool after instances are returned.  Objects returned when the pool is already
    ///     full will be garbage collected normally.
    ///     </note>
    ///     <para>
    ///     You may wish to increase this for applications that emit a very large number of 
    ///     log events or reduce this for applications that need to conserve memory.
    ///     </para>
    ///     </description>
    /// </item>
    /// </list>
    /// </remarks>
    public static class DiagnosticPools
    {
        /// <summary>
        /// Specifies the default maximum number of items to be retained in all diagnostic pools.
        /// </summary>
        internal const int DefaultPoolLimit = 64;

        private static ObjectPool<LogAttributes>            logAttributesPool;
        private static ObjectPool<ActivityTagsCollection>   activityAttributesPool;

        /// <summary>
        /// Default constructor.
        /// </summary>
        static DiagnosticPools()
        {
            var logPoolLimitVar = Environment.GetEnvironmentVariable("NEON_DIAGNOSTICS_POOLLIMIT");
            var logPoolLimit    = 0;

            if (!string.IsNullOrEmpty(logPoolLimitVar))
            {
                if (int.TryParse(logPoolLimitVar, out var limit) && limit > 0)
                {
                    logPoolLimit = limit;
                }
            }

            if (logPoolLimit == 0)
            {
                logPoolLimit = DefaultPoolLimit;
            }

            logAttributesPool      = new DefaultObjectPool<LogAttributes>(new DefaultPooledObjectPolicy<LogAttributes>(), logPoolLimit);
            activityAttributesPool = new DefaultObjectPool<ActivityTagsCollection>(new DefaultPooledObjectPolicy<ActivityTagsCollection>(), logPoolLimit);
        }

        /// <summary>
        /// <para>
        /// Returns a <see cref="LogAttributes"/> instance from the underlying object pool.
        /// </para>
        /// <note>
        /// Be sure to return the instance by passing it to <see cref="ReturnLogAttributes(LogAttributes)"/>
        /// when you are finished with it.
        /// </note>
        /// </summary>
        /// <returns>A <see cref="LogAttributes"/> instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static LogAttributes GetLogAttributes()
        {
            return logAttributesPool.Get();
        }

        /// <summary>
        /// Returns a <see cref="LogAttributes"/> instance obtained via <see cref="GetLogAttributes()"/>
        /// to the underlying pool so it can be reused.
        /// </summary>
        /// <param name="tags">The tags being returned to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ReturnLogAttributes(LogAttributes tags)
        {
            tags.Clear();

            logAttributesPool.Return(tags);
        }

        /// <summary>
        /// <para>
        /// Returns a <see cref="ActivityTagsCollection"/> instance from the underlying object pool.
        /// </para>
        /// <note>
        /// Be sure to return the instance by passing it to <see cref="ReturnActivityAttributes(ActivityTagsCollection)"/>
        /// when you are finished with it.
        /// </note>
        /// </summary>
        /// <returns>A <see cref="ActivityTagsCollection"/> instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ActivityTagsCollection GetActivityAttributes()
        {
            return activityAttributesPool.Get();
        }

        /// <summary>
        /// Returns a <see cref="ActivityTagsCollection"/> instance obtained via <see cref="GetActivityAttributes()"/>
        /// to the underlying pool so it can be reused.
        /// </summary>
        /// <param name="tags">The tags being returned to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ReturnActivityAttributes(ActivityTagsCollection tags)
        {
            tags.Clear();

            activityAttributesPool.Return(tags);
        }
    }
}
