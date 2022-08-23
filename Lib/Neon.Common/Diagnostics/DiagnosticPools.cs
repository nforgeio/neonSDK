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
        /// Specifies the maximum number of items to be retained in all diagnostic pools.
        /// </summary>
        internal const int DefaultPoolLimit = 64;

        /// <summary>
        /// Specifies the size of pooled tag value arrays.
        /// </summary>
        internal const int TagArgsArrayLength = 64;

        private static ObjectPool<LogTags>          tagsPool;
        private static ObjectPool<StringBuilder>    stringBuilderPool;
        private static ObjectPool<TagArgs>          tagArgsPool;

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

            tagsPool          = new DefaultObjectPool<LogTags>(new DefaultPooledObjectPolicy<LogTags>(), logPoolLimit);
            stringBuilderPool = new DefaultObjectPool<StringBuilder>(new DefaultPooledObjectPolicy<StringBuilder>(), logPoolLimit);
            tagArgsPool       = new DefaultObjectPool<TagArgs>(new DefaultPooledObjectPolicy<TagArgs>(), logPoolLimit);
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
        internal static LogTags GetLogTags()
        {
            var logTags = tagsPool.Get();

            logTags.Tags.Clear();

            return logTags;
        }

        /// <summary>
        /// Returns a <see cref="LogTags"/> instance obtained via <see cref="GetLogTags()"/>
        /// to the underlying pool so it can be reused.
        /// </summary>
        /// <param name="tags">The tags being returned to the pool.</param>
        internal static void ReturnLogTags(LogTags tags)
        {
            tagsPool.Return(tags);
        }

        /// <summary>
        /// <para>
        /// Returns a <see cref="StringBuilder"/> instance from the underlying object pool.
        /// </para>
        /// <note>
        /// Be sure to return the instance by passing it to <see cref="ReturnStringBuilder(StringBuilder)"/>
        /// when you are finished with it.
        /// </note>
        /// </summary>
        /// <returns>A <see cref="StringBuilder"/> instance.</returns>
        internal static StringBuilder GetStringBuilder()
        {
            var sb = stringBuilderPool.Get();

            sb.Clear();

            return sb;
        }

        /// <summary>
        /// Returns a <see cref="LogTags"/> instance obtained via <see cref="GetStringBuilder()"/>
        /// to the underlying pool so it can be reused.
        /// </summary>
        /// <param name="builder">The string builder being returned to the pool.</param>
        internal static void ReturnStringBuilder(StringBuilder builder)
        {
            stringBuilderPool.Return(builder);
        }

        /// <summary>
        /// <para>
        /// Returns a <see cref="TagArgs"/> instance from the pool including a <see cref="TagArgs.Values"/>
        /// array with at least <paramref name="length"/> elements.
        /// </para>
        /// <note>
        /// You must eventually pass the result to <see cref="ReturnTagArgs(TagArgs)"/> so that
        /// it will be returned to the pool.
        /// </note>
        /// </summary>
        /// <param name="length">The minimum length of the <see cref="TagArgs.Values"/> array to be returned.</param>
        /// <returns>A <see cref="TagArgs"/> instance.</returns>
        /// <remarks>
        /// Note that the pool for this allocates argument arrays with <see cref="TagArgsArrayLength"/> elements.
        /// This means that when <paramref name="length"/> is &lt;= <see cref="TagArgsArrayLength"/> then the
        /// result will be retrieved from the pool.  For lengths large than this, this method allocate a new
        /// <see cref="TagArgs"/> with the <see cref="TagArgs.Values"/> array length matching the length passed.
        /// </remarks>
        internal static TagArgs GetTagArgs(int length)
        {
            if (length > TagArgsArrayLength)
            {
                return new TagArgs(length);
            }
            else
            {
                // $note(jefflill):
                //
                // We could clear the array by setting everything to NULL here,
                // but that's really not necessary, so we're not going to do this
                // to improve performance.

                return tagArgsPool.Get();
            }
        }

        /// <summary>
        /// Returns <paramref name="tagArgs"/> back to the pool as long as its <see cref="TagArgs.Values"/>
        /// array has <see cref="TagArgsArrayLength"/> elements.  This method does nothing when the 
        /// array length is not the same, leaving those instances to eventually be garbage collected.
        /// </summary>
        /// <param name="tagArgs">The arguments being returned.</param>
        internal static void ReturnTagArgs(TagArgs tagArgs)
        {
            if (tagArgs.Values.Length == TagArgsArrayLength)
            {
                tagArgsPool.Return(tagArgs);
            }
        }
    }
}
