//-----------------------------------------------------------------------------
// FILE:        AdvisoryLockMigrationsOptionsExtension.cs
// CONTRIBUTOR: Marcus Bowyer
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
using System.Globalization;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Neon.EntityFrameworkCore.Npgsql
{
    /// <summary>
    /// Carries <see cref="AdvisoryLockMigrationsOptions"/> on the
    /// <see cref="Microsoft.EntityFrameworkCore.DbContextOptions"/> so that
    /// <see cref="AdvisoryLockHistoryRepository"/> — which Entity Framework Core constructs itself
    /// via dependency injection — can read them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Being a <see cref="IDbContextOptionsExtension"/> also makes the settings participate in the
    /// options fingerprint Entity Framework Core uses to cache its internal service provider.  Two
    /// contexts configured with different lock settings therefore get different providers rather than
    /// silently sharing whichever one was built first.
    /// </para>
    /// </remarks>
    internal sealed class AdvisoryLockMigrationsOptionsExtension : IDbContextOptionsExtension
    {
        private DbContextOptionsExtensionInfo info;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The advisory lock options.</param>
        public AdvisoryLockMigrationsOptionsExtension(AdvisoryLockMigrationsOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Returns the configured options.
        /// </summary>
        public AdvisoryLockMigrationsOptions Options { get; private set; }

        /// <inheritdoc/>
        public DbContextOptionsExtensionInfo Info => info ??= new ExtensionInfo(this);

        /// <inheritdoc/>
        public void ApplyServices(IServiceCollection services)
        {
            // Nothing to register: the IHistoryRepository swap is performed by
            // DbContextOptionsBuilder.ReplaceService(), which is what lets us keep working with
            // whatever the Npgsql provider registered rather than having to reproduce it.
        }

        /// <inheritdoc/>
        public void Validate(IDbContextOptions options)
        {
            Options.Validate();
        }

        //---------------------------------------------------------------------
        // Private types

        private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            private string logFragment;

            public ExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }

            private new AdvisoryLockMigrationsOptionsExtension Extension => (AdvisoryLockMigrationsOptionsExtension)base.Extension;

            public override bool IsDatabaseProvider => false;

            public override string LogFragment
            {
                get
                {
                    if (logFragment != null)
                    {
                        return logFragment;
                    }

                    var options = Extension.Options;
                    var key     = options.LockKey.HasValue ? options.LockKey.Value.ToString(CultureInfo.InvariantCulture) : "derived";
                    var wait    = options.Timeout.HasValue ? $"{options.Timeout.Value.TotalSeconds}s" : "indefinite";

                    return logFragment = $"AdvisoryLockMigrationHistory(key={key}, timeout={wait}) ";
                }
            }

            public override int GetServiceProviderHashCode()
            {
                var options = Extension.Options;

                return HashCode.Combine(options.LockKey, options.Timeout, options.PollInterval);
            }

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
            {
                if (other is not ExtensionInfo otherInfo)
                {
                    return false;
                }

                var options      = Extension.Options;
                var otherOptions = otherInfo.Extension.Options;

                return options.LockKey == otherOptions.LockKey
                    && options.Timeout == otherOptions.Timeout
                    && options.PollInterval == otherOptions.PollInterval;
            }

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
                var options = Extension.Options;

                debugInfo["Neon:" + nameof(AdvisoryLockMigrationsOptions.LockKey)] = options.LockKey?.ToString(CultureInfo.InvariantCulture) ?? "derived";
                debugInfo["Neon:" + nameof(AdvisoryLockMigrationsOptions.Timeout)] = options.Timeout?.ToString() ?? "indefinite";
                debugInfo["Neon:" + nameof(AdvisoryLockMigrationsOptions.PollInterval)] = options.PollInterval.ToString();
            }
        }
    }
}