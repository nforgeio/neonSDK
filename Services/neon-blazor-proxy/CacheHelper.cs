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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;

using Neon.Common;
using Neon.Diagnostics;
using Neon.Service;
using Neon.Tasks;

namespace NeonBlazorProxy
{
    /// <summary>
    /// A cache helper.
    /// </summary>
    public class CacheHelper
    {
        private ILogger                         logger;
        private IDistributedCache               cache;
        private DistributedCacheEntryOptions    defaultCacheOptions;

        public CacheHelper(IDistributedCache cache, ILogger logger)
        {
            this.logger = logger;
            this.cache  = cache;

            defaultCacheOptions = new DistributedCacheEntryOptions()
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

            cache.Set(key, NeonHelper.JsonSerializeToBytes(value), cacheOptions ?? defaultCacheOptions);

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

            await cache.SetAsync(key, NeonHelper.JsonSerializeToBytes(value), cacheOptions ?? defaultCacheOptions);

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

            var value = cache.Get(key);

            if (value != null)
            {
                Service.CacheHits.Inc();

                logger?.LogDebug($"Cache hit: [key={key}]");

                return NeonHelper.JsonDeserialize<T>(value);
            }

            Service.CacheMisses.Inc();

            logger?.LogInformationEx(() => $"Cache miss: [key={key}]");

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

            var value = await cache.GetAsync(key);

            if (value != null)
            {
                Service.CacheHits.Inc();

                logger?.LogDebug($"Cache hit: [key={key}]");

                return NeonHelper.JsonDeserialize<T>(value);
            }

            Service.CacheMisses.Inc();

            logger?.LogInformationEx(() => $"Cache miss: [key={key}]");

            return default;
        }
    }
}
