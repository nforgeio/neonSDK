//-----------------------------------------------------------------------------
// FILE:	    Program.cs
// CONTRIBUTOR: Marcus Bowyer
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using NeonTask;

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

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        public CacheHelper(IDistributedCache cache, ILogger logger)
        {
            this.cache  = cache;
            this.logger = logger;

            defaultCacheOptions = new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromHours(1)
            };
        }

        /// <summary>
        /// Generate a cache key by prepending a key6 value with the service name.
        /// </summary>
        /// <param name="key">The key value.</param>
        /// <returns></returns>
        private string CreateKey(string key)
        {
            return $"{Program.Service.Name}_{key}";
        }

        /// <summary>
        /// Add a value to the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cacheOptions">Optionally specifies cache options.</param>
        public void Set(string key, object value, DistributedCacheEntryOptions cacheOptions = null)
        {
            using (var activity = TelemetryHub.ActivitySource.StartActivity())
            {
                key = CreateKey(key);

                cache.Set(key, NeonHelper.JsonSerializeToBytes(value), cacheOptions ?? defaultCacheOptions);

                Service.CacheItemsStored.Inc();
            }
        }

        /// <summary>
        /// Asynchronously adds a value to the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cacheOptions">Optionally specifies cache options.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        public async Task SetAsync(string key, object value, DistributedCacheEntryOptions cacheOptions = null)
        {
            await SyncContext.Clear;

            using (var activity = TelemetryHub.ActivitySource.StartActivity())
            {
                key = CreateKey(key);

                await cache.SetAsync(key, NeonHelper.JsonSerializeToBytes(value), cacheOptions ?? defaultCacheOptions);

                Service.CacheItemsStored.Inc();
            }
        }

        /// <summary>
        /// Fetchs a value from the cache.
        /// </summary>
        /// <typeparam name="T">Specifies the object type.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>The cached item.</returns>
        public T Get<T>(string key)
        {
            using (var activity = TelemetryHub.ActivitySource.StartActivity())
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
        }

        /// <summary>
        /// Fetches an item from the cache asynchronously.
        /// </summary>
        /// <typeparam name="T">Specifies the object type.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>The cached item.</returns>
        public async Task<T> GetAsync<T>(string key)
        {
            await SyncContext.Clear;

            using (var activity = TelemetryHub.ActivitySource.StartActivity())
            {
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
}
