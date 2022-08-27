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
    /// logging related tag collections to improve performance by reusing these collections
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

        /// <summary>
        /// Specifies the size of pooled tag value arrays.
        /// </summary>
        internal const int TagArgsArrayLength = 64;

        private static ObjectPool<LogTags>              tagsPool;
        private static ObjectPool<ExceptionInfo>        exceptionInfoPool;
        private static ObjectPool<List<ExceptionInfo>>  exceptionInfoListPool;

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

            tagsPool              = new DefaultObjectPool<LogTags>(new DefaultPooledObjectPolicy<LogTags>(), logPoolLimit);
            exceptionInfoPool     = new DefaultObjectPool<ExceptionInfo>(new DefaultPooledObjectPolicy<ExceptionInfo>(), logPoolLimit * 8);     // Allowing for up to 8 inner exceptions per log event
            exceptionInfoListPool = new DefaultObjectPool<List<ExceptionInfo>>(new DefaultPooledObjectPolicy<List<ExceptionInfo>>(), logPoolLimit);
        }

        /// <summary>
        /// <para>
        /// Returns a <see cref="LogTags"/> instance from the underlying object pool.
        /// </para>
        /// <note>
        /// Be sure to return the instance by passing it to <see cref="ReturnLogTags(LogTags)"/>
        /// when you are finished with it.
        /// </note>
        /// </summary>
        /// <returns>A <see cref="LogTags"/> instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static LogTags GetLogTags()
        {
            return tagsPool.Get();
        }

        /// <summary>
        /// Returns a <see cref="LogTags"/> instance obtained via <see cref="GetLogTags()"/>
        /// to the underlying pool so it can be reused.
        /// </summary>
        /// <param name="tags">The tags being returned to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ReturnLogTags(LogTags tags)
        {
            tags.Clear();

            tagsPool.Return(tags);
        }

        /// <summary>
        /// <para>
        /// Returns an <see cref="ExceptionInfo"/> to be used for rendering exception information.
        /// </para>
        /// <note>
        /// Be sure to return the instance by passing it to <see cref="ReturnExceptionInfo(ExceptionInfo)"/>
        /// when you are finished with it.
        /// </note>
        /// </summary>
        /// <returns>The <see cref="ExceptionInfo"/> instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ExceptionInfo GetExceptionInfo()
        {
            return exceptionInfoPool.Get();
        }

        /// <summary>
        /// Returns a <see cref="LogTags"/> instance obtained via <see cref="GetExceptionInfo()"/>
        /// to the underlying pool so it can be reused.
        /// </summary>
        /// <param name="info">The exception information being returned to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ReturnExceptionInfo(ExceptionInfo info)
        {
            info.Clear();

            exceptionInfoPool.Return(info);
        }

        /// <summary>
        /// <para>
        /// Returns a list to be used for rendering exception information.
        /// </para>
        /// <note>
        /// Be sure to return the instance by passing it to <see cref="ReturnExceptionInfoList(List{ExceptionInfo})"/>
        /// when you are finished with it.
        /// </note>
        /// </summary>
        /// <returns>The <see cref="ExceptionInfo"/> list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static List<ExceptionInfo> GetExceptionInfoList()
        {
            return exceptionInfoListPool.Get();
        }

        /// <summary>
        /// Returns a <see cref="LogTags"/> instance obtained via <see cref="GetExceptionInfoList()"/>
        /// to the underlying pool so it can be reused.
        /// </summary>
        /// <param name="list">The list being returned to the pool.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ReturnExceptionInfoList(List<ExceptionInfo> list)
        {
            list.Clear();

            exceptionInfoListPool.Return(list);
        }
    }
}
