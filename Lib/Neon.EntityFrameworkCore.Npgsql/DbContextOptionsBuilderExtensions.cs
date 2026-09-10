//-----------------------------------------------------------------------------
// FILE:        DbContextOptionsBuilderExtensions.cs
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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Neon.EntityFrameworkCore.Npgsql
{
    /// <summary>
    /// Extension methods for <see cref="DbContextOptionsBuilder"/>.
    /// </summary>
    public static class DbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// Replaces the Npgsql provider's migration lock — a <c>LOCK TABLE ... IN ACCESS EXCLUSIVE
        /// MODE</c> statement — with a PostgreSQL session scoped advisory lock, so that
        /// migrations can run against Postgres wire compatible databases that don't implement
        /// explicit table locks.  YugabyteDB is the case this exists for.
        /// </summary>
        /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder"/> being configured.</param>
        /// <param name="configure">
        /// Optionally configures the lock key and wait behaviour.  The defaults derive the key from
        /// the migrations history table and wait indefinitely, matching the statement being replaced.
        /// </param>
        /// <returns>The <paramref name="optionsBuilder"/> to allow fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="optionsBuilder"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the configured options can't work.</exception>
        /// <remarks>
        /// <para>
        /// Call this <b>after</b> <c>UseNpgsql()</c>, since it replaces a service the Npgsql provider
        /// registers:
        /// </para>
        /// <code language="csharp">
        /// services.AddDbContext&lt;MyContext&gt;(options =>
        /// {
        ///     options
        ///         .UseNpgsql(connectionString)
        ///         .UseAdvisoryLockMigrationHistory();
        /// });
        /// </code>
        /// <para>
        /// To fail fast rather than wait forever when another migrator is already running — usually
        /// what you want in CI and in orchestrated deployments, where the platform will restart the
        /// job anyway:
        /// </para>
        /// <code language="csharp">
        /// options
        ///     .UseNpgsql(connectionString)
        ///     .UseAdvisoryLockMigrationHistory(lock =>
        ///     {
        ///         lock.Timeout      = TimeSpan.FromMinutes(5);
        ///         lock.PollInterval = TimeSpan.FromSeconds(2);
        ///     });
        /// </code>
        /// <note>
        /// <b>Keep this opt-in.</b>  On real PostgreSQL the provider's own <c>LOCK TABLE</c>
        /// implementation works and is stricter, so this should be applied only to contexts that
        /// actually target a database lacking explicit table locks — not folded into a shared
        /// "apply our defaults" helper.
        /// </note>
        /// <note>
        /// Requires YugabyteDB 2.25 or later.  Advisory locks are enabled by default from 2025.0
        /// onward; on 2.25 through 2024.x they're a preview feature that has to be switched on
        /// explicitly, and when it's off the lock statement fails instead of doing nothing.  See
        /// <see cref="AdvisoryLockHistoryRepository"/> for the full story.
        /// </note>
        /// <para>
        /// This only changes how the migration lock is taken.  Everything else about the migrations
        /// history table — its schema, its name, how rows are read and written — is left to the
        /// Npgsql provider, so applying it doesn't disturb your model or your existing history rows.
        /// </para>
        /// </remarks>
        public static DbContextOptionsBuilder UseAdvisoryLockMigrationHistory(
            this DbContextOptionsBuilder optionsBuilder,
            Action<AdvisoryLockMigrationsOptions> configure = null)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));

            // ReplaceService() can't work against a service provider Entity Framework Core didn't
            // build, and the error it raises much later doesn't say what to do instead.  Catch it here
            // while the call site is still on the stack.

            if (optionsBuilder.Options.FindExtension<CoreOptionsExtension>()?.InternalServiceProvider != null)
            {
                throw new InvalidOperationException(
                    $"[{nameof(UseAdvisoryLockMigrationHistory)}()] cannot be used together with [UseInternalServiceProvider()],"
                    + $" because it relies on [ReplaceService()] to swap [{nameof(IHistoryRepository)}] and Entity Framework Core"
                    + $" can't modify a service provider it didn't build."
                    + $"  Register the repository in your own container instead and use"
                    + $" [{nameof(ConfigureAdvisoryLockMigrationHistory)}()] to carry the options:"
                    + $"  services.AddScoped<{nameof(IHistoryRepository)}, {nameof(AdvisoryLockHistoryRepository)}>()"
                    + $" — see the Neon.EntityFrameworkCore.Npgsql README.");
            }

            ConfigureAdvisoryLockMigrationHistory(optionsBuilder, configure);

            optionsBuilder.ReplaceService<IHistoryRepository, AdvisoryLockHistoryRepository>();

            return optionsBuilder;
        }

        /// <summary>
        /// Carries <see cref="AdvisoryLockMigrationsOptions"/> to an
        /// <see cref="AdvisoryLockHistoryRepository"/> that you register yourself, <b>without</b>
        /// replacing any services.  Use this only when
        /// <see cref="UseAdvisoryLockMigrationHistory(DbContextOptionsBuilder, Action{AdvisoryLockMigrationsOptions})"/>
        /// can't be used — in practice, when the context is configured with
        /// <c>UseInternalServiceProvider()</c>.
        /// </summary>
        /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder"/> being configured.</param>
        /// <param name="configure">Optionally configures the lock key and wait behaviour.</param>
        /// <returns>The <paramref name="optionsBuilder"/> to allow fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="optionsBuilder"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the configured options can't work.</exception>
        /// <remarks>
        /// <para>
        /// <c>UseInternalServiceProvider()</c> hands Entity Framework Core a container you built, and
        /// Entity Framework Core refuses to modify it — so <c>ReplaceService()</c>, which is how
        /// <see cref="UseAdvisoryLockMigrationHistory(DbContextOptionsBuilder, Action{AdvisoryLockMigrationsOptions})"/>
        /// installs the repository, isn't available.  Register it in your own container and configure it
        /// here instead:
        /// </para>
        /// <code language="csharp">
        /// var serviceProvider = new ServiceCollection()
        ///     .AddEntityFrameworkNpgsql()
        ///     .AddScoped&lt;IHistoryRepository, AdvisoryLockHistoryRepository&gt;()   // after the provider
        ///     .BuildServiceProvider();
        ///
        /// var options = new DbContextOptionsBuilder&lt;MyContext&gt;()
        ///     .UseNpgsql(connectionString)
        ///     .UseInternalServiceProvider(serviceProvider)
        ///     .ConfigureAdvisoryLockMigrationHistory(lock => lock.Timeout = TimeSpan.FromMinutes(5))
        ///     .Options;
        /// </code>
        /// <note>
        /// The <c>AddScoped()</c> call has to come <b>after</b> <c>AddEntityFrameworkNpgsql()</c>, so
        /// that it wins over the provider's own registration.
        /// </note>
        /// <note>
        /// This method is only about the options.  If you don't call it, a self-registered
        /// <see cref="AdvisoryLockHistoryRepository"/> still works correctly — it just runs with the
        /// defaults, waiting indefinitely on a key derived from the migrations history table.
        /// </note>
        /// </remarks>
        public static DbContextOptionsBuilder ConfigureAdvisoryLockMigrationHistory(
            this DbContextOptionsBuilder optionsBuilder,
            Action<AdvisoryLockMigrationsOptions> configure = null)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));

            var options = new AdvisoryLockMigrationsOptions();

            configure?.Invoke(options);

            options.Validate();

            // Snapshot the options so a caller holding onto the instance can't mutate the
            // configuration after the fact.

            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
                .AddOrUpdateExtension(new AdvisoryLockMigrationsOptionsExtension(options.Clone()));

            return optionsBuilder;
        }

        /// <summary>
        /// Carries <see cref="AdvisoryLockMigrationsOptions"/> to a self-registered
        /// <see cref="AdvisoryLockHistoryRepository"/>.  This is the strongly typed counterpart of
        /// <see cref="ConfigureAdvisoryLockMigrationHistory(DbContextOptionsBuilder, Action{AdvisoryLockMigrationsOptions})"/>,
        /// for use with <see cref="DbContextOptionsBuilder{TContext}"/>.
        /// </summary>
        /// <typeparam name="TContext">The <see cref="DbContext"/> type.</typeparam>
        /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder{TContext}"/> being configured.</param>
        /// <param name="configure">Optionally configures the lock key and wait behaviour.</param>
        /// <returns>The <paramref name="optionsBuilder"/> to allow fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="optionsBuilder"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the configured options can't work.</exception>
        /// <remarks>
        /// See <see cref="ConfigureAdvisoryLockMigrationHistory(DbContextOptionsBuilder, Action{AdvisoryLockMigrationsOptions})"/>
        /// for the full documentation and an example.
        /// </remarks>
        public static DbContextOptionsBuilder<TContext> ConfigureAdvisoryLockMigrationHistory<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            Action<AdvisoryLockMigrationsOptions> configure = null)
            where TContext : DbContext
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));

            ConfigureAdvisoryLockMigrationHistory((DbContextOptionsBuilder)optionsBuilder, configure);

            return optionsBuilder;
        }

        /// <summary>
        /// Replaces the Npgsql provider's migration lock with a PostgreSQL session scoped
        /// advisory lock.  This is the strongly typed counterpart of
        /// <see cref="UseAdvisoryLockMigrationHistory(DbContextOptionsBuilder, Action{AdvisoryLockMigrationsOptions})"/>,
        /// for use with <see cref="DbContextOptionsBuilder{TContext}"/>.
        /// </summary>
        /// <typeparam name="TContext">The <see cref="DbContext"/> type.</typeparam>
        /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder{TContext}"/> being configured.</param>
        /// <param name="configure">Optionally configures the lock key and wait behaviour.</param>
        /// <returns>The <paramref name="optionsBuilder"/> to allow fluent chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="optionsBuilder"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the configured options can't work.</exception>
        /// <remarks>
        /// See <see cref="UseAdvisoryLockMigrationHistory(DbContextOptionsBuilder, Action{AdvisoryLockMigrationsOptions})"/>
        /// for the full documentation, including the YugabyteDB cluster flags this depends on.
        /// </remarks>
        public static DbContextOptionsBuilder<TContext> UseAdvisoryLockMigrationHistory<TContext>(
            this DbContextOptionsBuilder<TContext> optionsBuilder,
            Action<AdvisoryLockMigrationsOptions> configure = null)
            where TContext : DbContext
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));

            UseAdvisoryLockMigrationHistory((DbContextOptionsBuilder)optionsBuilder, configure);

            return optionsBuilder;
        }
    }
}