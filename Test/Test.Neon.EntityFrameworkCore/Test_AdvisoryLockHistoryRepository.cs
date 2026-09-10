//-----------------------------------------------------------------------------
// FILE:        Test_AdvisoryLockHistoryRepository.cs
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
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

using Neon.EntityFrameworkCore.Npgsql;
using Neon.Xunit;

using Xunit;

namespace Test.Neon.EntityFrameworkCore
{
    /// <summary>
    /// Tests <see cref="AdvisoryLockHistoryRepository"/> and the options plumbing that configures it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// None of these tests contact a database.  Everything they assert — that the service swap takes
    /// effect, that the options reach the repository, and that the derived lock key is stable — is
    /// resolvable from the Entity Framework Core service graph alone.
    /// </para>
    /// <para>
    /// The behaviour that only shows up against a real cluster — that migrations apply at all, that
    /// competing migrators are locked out, and that the lock survives the commit boundary between two
    /// migrations — is covered by <see cref="Test_AdvisoryLockMigrations"/>.
    /// </para>
    /// </remarks>
    [Trait(TestTrait.Category, TestArea.NeonEntityFrameworkCore)]
    public class Test_AdvisoryLockHistoryRepository
    {
        //---------------------------------------------------------------------
        // These are the FNV-1a values the shipped derivation produces.  They are hardcoded on
        // purpose: matching a constant is precisely what proves the key isn't process randomized the
        // way string.GetHashCode() is.  If a change to ComputeLockKey() makes these fail, understand
        // that it has invalidated every already-deployed migrator's key before updating them.

        public const long PublicHistoryKey = 1102577501055449738;
        public const long SchemaHistoryKey = 6649644946512544062;

        /// <summary>
        /// Exposes the protected surface of <see cref="AdvisoryLockHistoryRepository"/> so the tests
        /// can observe what Entity Framework Core would use at migration time.
        /// </summary>
        public class ProbeHistoryRepository : AdvisoryLockHistoryRepository
        {
            public ProbeHistoryRepository(HistoryRepositoryDependencies dependencies)
                : base(dependencies)
            {
            }

            public AdvisoryLockMigrationsOptions ProbeOptions => Options;

            public long ProbeLockKey => LockKey;

            public long ProbeComputeLockKey(string schema, string table) => ComputeLockKey(schema, table);
        }

        /// <summary>
        /// Builds a context whose <see cref="IHistoryRepository"/> is a <see cref="ProbeHistoryRepository"/>,
        /// registered through the same public entry point consumers use.
        /// </summary>
        private static TestDbContext CreateProbeContext(Action<AdvisoryLockMigrationsOptions> configure = null)
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .UseAdvisoryLockMigrationHistory(configure)
                .ReplaceService<IHistoryRepository, ProbeHistoryRepository>()
                .Options;

            return new TestDbContext(options);
        }

        [Fact]
        public void UseAdvisoryLockMigrationHistory_ReplacesTheHistoryRepository()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .UseAdvisoryLockMigrationHistory()
                .Options;

            using var context = new TestDbContext(options);

            Assert.IsType<AdvisoryLockHistoryRepository>(context.GetService<IHistoryRepository>());
        }

        [Fact]
        public void UseAdvisoryLockMigrationHistory_RejectsAnInternalServiceProvider()
        {
            // ReplaceService() can't touch a container Entity Framework Core didn't build.  Entity
            // Framework Core's own error for this says what went wrong but not what to do instead, so
            // we raise our own at the call site and point at the alternative.

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .UseInternalServiceProvider(serviceProvider);

            var e = Assert.Throws<InvalidOperationException>(() => optionsBuilder.UseAdvisoryLockMigrationHistory());

            Assert.Contains(nameof(DbContextOptionsBuilderExtensions.ConfigureAdvisoryLockMigrationHistory), e.Message);
            Assert.Contains("UseInternalServiceProvider", e.Message);
        }

        [Fact]
        public void ConfigureAdvisoryLockMigrationHistory_CarriesOptionsWithoutReplacingServices()
        {
            // The escape hatch for the UseInternalServiceProvider() case: options only, no service
            // swap.  Registering the repository is the caller's job there, so this must leave whatever
            // IHistoryRepository the provider registered alone.

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .ConfigureAdvisoryLockMigrationHistory(lockOptions => lockOptions.Timeout = TimeSpan.FromMinutes(2))
                .Options;

            using var context = new TestDbContext(options);

            Assert.IsNotType<AdvisoryLockHistoryRepository>(context.GetService<IHistoryRepository>());
        }

        [Fact]
        public void ConfigureAdvisoryLockMigrationHistory_ReachesASelfRegisteredRepository()
        {
            // Proves the options actually arrive, which is the whole point of the method: the extension
            // is the only channel between the options builder and a repository the caller newed up in
            // their own container.

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .ConfigureAdvisoryLockMigrationHistory(
                    lockOptions =>
                    {
                        lockOptions.Timeout = TimeSpan.FromMinutes(2);
                        lockOptions.LockKey = 99;
                    })
                .ReplaceService<IHistoryRepository, ProbeHistoryRepository>()
                .Options;

            using var context = new TestDbContext(options);

            var historyRepository = (ProbeHistoryRepository)context.GetService<IHistoryRepository>();

            Assert.Equal(TimeSpan.FromMinutes(2), historyRepository.ProbeOptions.Timeout);
            Assert.Equal(99, historyRepository.ProbeLockKey);
        }

        [Fact]
        public void ASelfRegisteredRepositoryFallsBackToTheDefaults()
        {
            // Somebody who registers the repository without carrying any options — the workaround people
            // reach for first — must still get a working lock rather than a null reference.

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .ReplaceService<IHistoryRepository, ProbeHistoryRepository>()
                .Options;

            using var context = new TestDbContext(options);

            var historyRepository = (ProbeHistoryRepository)context.GetService<IHistoryRepository>();

            Assert.NotNull(historyRepository.ProbeOptions);
            Assert.Null(historyRepository.ProbeOptions.Timeout);
            Assert.Equal(PublicHistoryKey, historyRepository.ProbeLockKey);
        }

        [Fact]
        public void UseAdvisoryLockMigrationHistory_KeepsTheLockSessionScoped()
        {
            // The session owns the lock, not the transaction, so this has to stay Explicit.  Reporting
            // Transaction here would describe a lock that dies at each commit — which ours doesn't — and
            // would misdescribe the contract to anyone reading the service graph.

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .UseAdvisoryLockMigrationHistory()
                .Options;

            using var context = new TestDbContext(options);

            Assert.Equal(LockReleaseBehavior.Explicit, context.GetService<IHistoryRepository>().LockReleaseBehavior);
        }

        [Fact]
        public void UseAdvisoryLockMigrationHistory_DefaultsToWaitingIndefinitely()
        {
            using var context = CreateProbeContext();

            var historyRepository = (ProbeHistoryRepository)context.GetService<IHistoryRepository>();

            Assert.Null(historyRepository.ProbeOptions.LockKey);
            Assert.Null(historyRepository.ProbeOptions.Timeout);
            Assert.Equal(TimeSpan.FromSeconds(1), historyRepository.ProbeOptions.PollInterval);
        }

        [Fact]
        public void UseAdvisoryLockMigrationHistory_PassesOptionsThrough()
        {
            using var context = CreateProbeContext(
                lockOptions =>
                {
                    lockOptions.Timeout      = TimeSpan.FromMinutes(3);
                    lockOptions.PollInterval = TimeSpan.FromMilliseconds(250);
                    lockOptions.LockKey      = 42;
                });

            var historyRepository = (ProbeHistoryRepository)context.GetService<IHistoryRepository>();

            Assert.Equal(TimeSpan.FromMinutes(3), historyRepository.ProbeOptions.Timeout);
            Assert.Equal(TimeSpan.FromMilliseconds(250), historyRepository.ProbeOptions.PollInterval);
            Assert.Equal(42, historyRepository.ProbeOptions.LockKey);
        }

        [Fact]
        public void LockKey_DerivesFromTheHistoryTableByDefault()
        {
            using var context = CreateProbeContext();

            var historyRepository = (ProbeHistoryRepository)context.GetService<IHistoryRepository>();

            Assert.Equal(PublicHistoryKey, historyRepository.ProbeLockKey);
        }

        [Fact]
        public void LockKey_HonorsAnExplicitOverride()
        {
            using var context = CreateProbeContext(lockOptions => lockOptions.LockKey = -12345);

            var historyRepository = (ProbeHistoryRepository)context.GetService<IHistoryRepository>();

            Assert.Equal(-12345, historyRepository.ProbeLockKey);
        }

        [Fact]
        public void LockKey_FollowsTheConfiguredHistoryTable()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "myschema"))
                .UseAdvisoryLockMigrationHistory()
                .ReplaceService<IHistoryRepository, ProbeHistoryRepository>()
                .Options;

            using var context = new TestDbContext(options);

            var historyRepository = (ProbeHistoryRepository)context.GetService<IHistoryRepository>();

            Assert.Equal(SchemaHistoryKey, historyRepository.ProbeLockKey);
        }

        [Fact]
        public void ComputeLockKey_IsStableAndNotProcessRandomized()
        {
            using var context = CreateProbeContext();

            var historyRepository = (ProbeHistoryRepository)context.GetService<IHistoryRepository>();

            // Matching hardcoded constants is the assertion that matters: a per-process randomized
            // hash — which is what string.GetHashCode() would give us — could not.

            Assert.Equal(PublicHistoryKey, historyRepository.ProbeComputeLockKey("public", "__EFMigrationsHistory"));
            Assert.Equal(SchemaHistoryKey, historyRepository.ProbeComputeLockKey("myschema", "__EFMigrationsHistory"));

            Assert.NotEqual(
                historyRepository.ProbeComputeLockKey("public", "__EFMigrationsHistory"),
                historyRepository.ProbeComputeLockKey("public", "MigrationHistory"));

            Assert.NotEqual(
                historyRepository.ProbeComputeLockKey("public", "__EFMigrationsHistory"),
                historyRepository.ProbeComputeLockKey("other", "__EFMigrationsHistory"));
        }

        [Fact]
        public void UseAdvisoryLockMigrationHistory_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(
                () => ((DbContextOptionsBuilder)null).UseAdvisoryLockMigrationHistory());

            Assert.Throws<ArgumentNullException>(
                () => ((DbContextOptionsBuilder<TestDbContext>)null).UseAdvisoryLockMigrationHistory());
        }

        [Fact]
        public void UseAdvisoryLockMigrationHistory_RejectsUnusableOptions()
        {
            var builder = new DbContextOptionsBuilder<TestDbContext>().UseNpgsql(TestDbContext.OfflineConnectionString);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => builder.UseAdvisoryLockMigrationHistory(lockOptions => lockOptions.Timeout = TimeSpan.Zero));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => builder.UseAdvisoryLockMigrationHistory(lockOptions => lockOptions.Timeout = TimeSpan.FromSeconds(-1)));

            Assert.Throws<ArgumentOutOfRangeException>(
                () => builder.UseAdvisoryLockMigrationHistory(lockOptions => lockOptions.PollInterval = TimeSpan.Zero));
        }

        [Fact]
        public void Options_AreSnapshotAtConfigurationTime()
        {
            AdvisoryLockMigrationsOptions escaped = null;

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .UseAdvisoryLockMigrationHistory(
                    lockOptions =>
                    {
                        lockOptions.Timeout = TimeSpan.FromMinutes(1);

                        escaped = lockOptions;
                    })
                .ReplaceService<IHistoryRepository, ProbeHistoryRepository>()
                .Options;

            // Mutating the instance the callback saw must not reach the frozen configuration.

            escaped.Timeout = TimeSpan.FromHours(99);

            using var context = new TestDbContext(options);

            var historyRepository = (ProbeHistoryRepository)context.GetService<IHistoryRepository>();

            Assert.Equal(TimeSpan.FromMinutes(1), historyRepository.ProbeOptions.Timeout);
        }

        [Fact]
        public void DifferentLockOptions_DontShareSpecializedServices()
        {
            // The options participate in Entity Framework Core's internal service provider
            // fingerprint, so two contexts configured differently must not silently share whichever
            // provider happened to be built first.

            var first = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .UseAdvisoryLockMigrationHistory(lockOptions => lockOptions.Timeout = TimeSpan.FromMinutes(1))
                .ReplaceService<IHistoryRepository, ProbeHistoryRepository>()
                .Options;

            var second = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .UseAdvisoryLockMigrationHistory(lockOptions => lockOptions.Timeout = TimeSpan.FromMinutes(2))
                .ReplaceService<IHistoryRepository, ProbeHistoryRepository>()
                .Options;

            using var firstContext  = new TestDbContext(first);
            using var secondContext = new TestDbContext(second);

            var firstRepository  = (ProbeHistoryRepository)firstContext.GetService<IHistoryRepository>();
            var secondRepository = (ProbeHistoryRepository)secondContext.GetService<IHistoryRepository>();

            Assert.Equal(TimeSpan.FromMinutes(1), firstRepository.ProbeOptions.Timeout);
            Assert.Equal(TimeSpan.FromMinutes(2), secondRepository.ProbeOptions.Timeout);
        }

        [Fact]
        public void MigrationsLockTimeoutException_CarriesTheDiagnostics()
        {
            var e = new MigrationsLockTimeoutException(lockKey: 1234, timeout: TimeSpan.FromSeconds(30), attempts: 7);

            Assert.Equal(1234, e.LockKey);
            Assert.Equal(TimeSpan.FromSeconds(30), e.Timeout);
            Assert.Equal(7, e.Attempts);
            Assert.Contains("1234", e.Message);

            // Deriving from TimeoutException would be the obvious choice, but Npgsql's transient error
            // detector treats every TimeoutException as retryable, and Entity Framework Core's
            // execution strategy then re-throws it as InvalidOperationException — which would make
            // catch (MigrationsLockTimeoutException) silently useless.  See the type's remarks.

            Assert.IsNotAssignableFrom<TimeoutException>(e);
        }

        [Fact]
        public void UseAdvisoryLockMigrationHistory_LeavesTheModelAlone()
        {
            // Swapping the history repository must not disturb the model, otherwise it would show up
            // as phantom migration drift.

            var plainOptions = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .Options;

            var lockedOptions = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .UseAdvisoryLockMigrationHistory()
                .Options;

            using var plainContext  = new TestDbContext(plainOptions);
            using var lockedContext = new TestDbContext(lockedOptions);

            Assert.Equal(
                plainContext.Model.GetEntityTypes().Select(entityType => entityType.Name).OrderBy(name => name),
                lockedContext.Model.GetEntityTypes().Select(entityType => entityType.Name).OrderBy(name => name));
        }
    }
}