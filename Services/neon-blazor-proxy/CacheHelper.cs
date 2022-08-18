//-----------------------------------------------------------------------------
// FILE:	    Program.cs
// CONTRIBUTOR: Marcus Bowyer
// COPYRIGHT:   Copyright (c) 2005-2022 by neonFORGE LLC.  All rights reserved.
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Diagnostics;
using Neon.Service;

using CommandLine;

using Prometheus.DotNetRuntime;
using Microsoft.Extensions.Logging;
using Neon.Tasks;
using Prometheus.DotNetRuntime.EventListening.Parsers.Util;
using Microsoft.Extensions.Caching.Distributed;
using System.Net.NetworkInformation;
using Prometheus;

namespace NeonBlazorProxy
{
    /// <summary>
    /// A cache helper.
    /// </summary>
    public class CacheHelper
    {
        private INeonLogger Logger;
        private IDistributedCache Cache;
        private DistributedCacheEntryOptions DefaultCacheOptions;

        public CacheHelper(IDistributedCache cache, INeonLogger logger)
        {
            this.Logger = logger;
            this.Cache  = cache;

            DefaultCacheOptions = new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromHours(1)
            };
        }

        /// <summary>
        /// Generate a cache key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string CreateKey(string key)
        {
            return $"{Program.Service.Name}_{key}";
        }

        /// <summary>
        /// Add an <see cref="T"/> to the Cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void Set(string key, object value, DistributedCacheEntryOptions cacheOptions = null)
        {
            key = CreateKey(key);

            Cache.Set(key, NeonHelper.JsonSerializeToBytes(value), cacheOptions ?? DefaultCacheOptions);

            Service.CacheItemsStored.Inc();
        }

        /// <summary>
        /// Add an <see cref="T"/> to the Cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task SetAsync(string key, object value, DistributedCacheEntryOptions cacheOptions = null)
        {
            await SyncContext.Clear;

            key = CreateKey(key);

            await Cache.SetAsync(key, NeonHelper.JsonSerializeToBytes(value), cacheOptions ?? DefaultCacheOptions);

            Service.CacheItemsStored.Inc();
        }

        /// <summary>
        /// Get a <see cref="T"/> from the Cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            Service.CacheLookupsRequested.Inc();

            key = CreateKey(key);

            var value = Cache.Get(key);

            if (value != null)
            {
                Service.CacheHits.Inc();

                Logger?.LogDebug($"Cache hit. [{key}]");

                return NeonHelper.JsonDeserialize<T>(value);
            }

            Service.CacheMisses.Inc();

            Logger?.LogInfo($"Cache miss. [{key}]");

            return default;
        }

        /// <summary>
        /// Get a <see cref="T"/> the Cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string key)
        {
            await SyncContext.Clear;

            Service.CacheLookupsRequested.Inc();

            key = CreateKey(key);

            var value = await Cache.GetAsync(key);

            if (value != null)
            {
                Service.CacheHits.Inc();

                Logger?.LogDebug($"Cache hit. [{key}]");

                return NeonHelper.JsonDeserialize<T>(value);
            }

            Service.CacheMisses.Inc();

            Logger?.LogInfo($"Cache miss. [{key}]");

            return default;
        }
    }
}
