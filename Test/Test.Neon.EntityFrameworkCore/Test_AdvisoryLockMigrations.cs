//-----------------------------------------------------------------------------
// FILE:        Test_AdvisoryLockMigrations.cs
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
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using global::Npgsql;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

using Neon.EntityFrameworkCore.Npgsql;
using Neon.Net;
using Neon.Tasks;
using Neon.Xunit;
using Neon.Xunit.Yugabyte;

using Test.Neon.EntityFrameworkCore.Migrations;

using Xunit;

using NpgsqlTracing = Neon.EntityFrameworkCore.Npgsql.TracerProviderBuilderExtensions;

namespace Test.Neon.EntityFrameworkCore
{
    /// <summary>
    /// Runs Entity Framework Core migrations against a live YugabyteDB instance to verify that
    /// <see cref="AdvisoryLockHistoryRepository"/> does what it claims.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the file that earns the design.  The unit tests elsewhere prove the plumbing is wired
    /// up; only these prove the two things that actually matter — that migrations run at all on a
    /// database without <c>LOCK TABLE</c>, and that concurrent migrators still exclude each other.
    /// </para>
    /// <para>
    /// <b>There are no timed waits here.</b>  Mutual exclusion is observed by holding the lock from the
    /// test itself and watching a migrator fail to get in, and the one case that genuinely needs a
    /// migrator parked mid-run parks it on a second advisory lock the test controls.  Every wait is on
    /// observable state and bounded by a <see cref="CancellationTokenSource"/>, so these tests are as
    /// fast as the server and don't get flakier on a loaded machine.  The only durations are
    /// <see cref="AdvisoryLockMigrationsOptions.Timeout"/> — the thing under test — and the deadline
    /// that turns a hang into a readable failure.
    /// </para>
    /// <note>
    /// Advisory locks are enabled by default from YugabyteDB <b>2025.0</b> onward, which the fixture's
    /// image is well past.  On older releases they're a preview feature that has to be switched on
    /// explicitly, and when it's off <c>pg_advisory_xact_lock()</c> fails rather than doing nothing —
    /// so <see cref="AdvisoryLocksAreEnabledOnTheCluster"/> checks for it first.  A failure there means
    /// the cluster can't support this repository, not that the repository is broken.
    /// </note>
    /// </remarks>
    [Trait(TestTrait.Category, TestArea.NeonEntityFrameworkCore)]
    [Collection(TestCollection.NonParallel)]
    public class Test_AdvisoryLockMigrations : IClassFixture<YugabyteFixture>
    {
        //---------------------------------------------------------------------
        // The lock key both migrators derive from the default migrations history table.  Asserted
        // against the shipped FNV-1a derivation in Test_AdvisoryLockHistoryRepository.

        private const long MigrationsLockKey = Test_AdvisoryLockHistoryRepository.PublicHistoryKey;

        /// <summary>
        /// How long a competing migrator is willing to wait.  Short, because the point is to observe it
        /// give up while the lock is provably held — not to wait out a race.
        /// </summary>
        private static readonly TimeSpan CompetitorTimeout = TimeSpan.FromSeconds(2);

        /// <summary>
        /// The deadline for waiting on observable state.  Generous: it exists to turn a hang into a
        /// readable failure, and is never reached on a working cluster.
        /// </summary>
        private static readonly TimeSpan StateDeadline = TimeSpan.FromSeconds(90);

        /// <summary>
        /// The deadline for the lock to be retaken after a commit.  Shorter than
        /// <see cref="StateDeadline"/> because reacquisition happens immediately on the way into the
        /// next migration — nothing is being waited out — so a broken reacquire fails fast instead of
        /// stretching the suite out.
        /// </summary>
        private static readonly TimeSpan ReacquireDeadline = TimeSpan.FromSeconds(15);

        private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// The Npgsql command timeout used by <see cref="TheBlockingWaitOutlastsTheCommandTimeout"/>.
        /// Long enough that the migration's own DDL still fits inside it, short enough that outlasting
        /// it doesn't stretch the suite out.
        /// </summary>
        private static readonly TimeSpan CommandTimeoutProbe = TimeSpan.FromSeconds(3);

        private readonly string connectionString;

        public Test_AdvisoryLockMigrations(YugabyteFixture fixture)
        {
            TestHelper.ResetDocker(this.GetType());

            var ycqlPort = NetHelper.GetUnusedTcpPort(IPAddress.Any);
            var ysqlPort = NetHelper.GetUnusedTcpPort(IPAddress.Any);
            var adminPort = NetHelper.GetUnusedTcpPort(IPAddress.Any);

            fixture.Start(ycqlPort: ycqlPort, ysqlPort: ysqlPort, adminPort: adminPort);

            this.connectionString = fixture.PostgresConnection.ConnectionString;

            MigrationGate.Position = MigrationGate.GatePosition.None;

            // The repository's lock is session scoped, which means a lock that somehow escaped its release
            // would live on in a pooled physical connection and block later tests instead of dying with a
            // transaction.  Clearing the pools ends those sessions, so each test starts from a state where
            // no leaked lock can be inherited — and so a failure here means this test, not the last one.

            NpgsqlConnection.ClearAllPools();

            ResetDatabase();
        }

        //---------------------------------------------------------------------
        // Database helpers

        /// <summary>
        /// Drops everything the migrations create, so each test starts from an unmigrated database.
        /// </summary>
        private void ResetDatabase()
        {
            Execute(
                "DROP TABLE IF EXISTS widgets",
                "DROP TABLE IF EXISTS sprockets",
                "DROP TABLE IF EXISTS \"__EFMigrationsHistory\"");
        }

        private void Execute(params string[] statements)
        {
            using var connection = OpenConnection();

            foreach (var statement in statements)
            {
                using var command = new NpgsqlCommand(statement, connection);

                command.ExecuteNonQuery();
            }
        }

        private NpgsqlConnection OpenConnection()
        {
            var connection = new NpgsqlConnection(connectionString);

            connection.Open();

            return connection;
        }

        private T Scalar<T>(string sql)
        {
            using var connection = OpenConnection();
            using var command    = new NpgsqlCommand(sql, connection);

            return (T)command.ExecuteScalar();
        }

        private bool TableExists(string name)
        {
            return Scalar<bool>($"SELECT to_regclass('public.{name}') IS NOT NULL");
        }

        private List<string> AppliedMigrations()
        {
            using var context = new MigrationTestDbContext(CreateOptions());

            return context.Database.GetAppliedMigrations().OrderBy(name => name).ToList();
        }

        //---------------------------------------------------------------------
        // Lock helpers

        /// <summary>
        /// Holds a session scoped advisory lock until disposed.
        /// </summary>
        /// <remarks>
        /// Session scoped so the test can hold it across as many operations as it likes.  This is the same
        /// kind of lock the repository now takes, on the same key space, so the two conflict exactly as two
        /// competing migrators would.
        /// </remarks>
        private sealed class HeldLock : IDisposable
        {
            private readonly NpgsqlConnection connection;
            private readonly long             key;
            private bool                      released;

            public HeldLock(NpgsqlConnection connection, long key)
            {
                this.connection = connection;
                this.key = key;

                using var command = new NpgsqlCommand($"SELECT pg_advisory_lock({key.ToString(CultureInfo.InvariantCulture)})", connection);

                command.ExecuteNonQuery();
            }

            /// <summary>
            /// Releases the lock.  Idempotent, so a test can release early to unblock something and
            /// still leave the <c>using</c> in place as a safety net.
            /// </summary>
            public void Dispose()
            {
                if (released)
                {
                    return;
                }

                released = true;

                using (var command = new NpgsqlCommand($"SELECT pg_advisory_unlock({key.ToString(CultureInfo.InvariantCulture)})", connection))
                {
                    command.ExecuteNonQuery();
                }

                connection.Dispose();
            }
        }

        /// <summary>
        /// Takes <paramref name="key"/> on a connection of its own and holds it until disposed.
        /// </summary>
        private HeldLock HoldLock(long key)
        {
            return new HeldLock(OpenConnection(), key);
        }

        /// <summary>
        /// Indicates whether <paramref name="key"/> is currently held by somebody else.
        /// </summary>
        private bool LockIsHeld(long key)
        {
            using var connection  = OpenConnection();
            using var transaction = connection.BeginTransaction();
            using var command     = new NpgsqlCommand(
                $"SELECT pg_try_advisory_xact_lock({key.ToString(CultureInfo.InvariantCulture)})", connection, transaction);

            var acquired = (bool)command.ExecuteScalar();

            transaction.Rollback();

            return !acquired;
        }

        /// <summary>
        /// Kills the backend belonging to <paramref name="applicationName"/>, which is what the server
        /// sees when an application process dies: the connection drops abruptly with a transaction still
        /// open.
        /// </summary>
        /// <returns>The number of backends terminated.</returns>
        private int TerminateBackend(string applicationName)
        {
            using var connection = OpenConnection();
            using var command    = new NpgsqlCommand(
                "SELECT COUNT(*) FROM ("
                + "  SELECT pg_terminate_backend(pid) FROM pg_stat_activity"
                + "  WHERE application_name = @applicationName AND pid <> pg_backend_pid()"
                + ") AS terminated", connection);

            command.Parameters.AddWithValue("applicationName", applicationName);

            return (int)(long)command.ExecuteScalar();
        }

        /// <summary>
        /// Indicates whether the migrations lock is currently held by somebody else.
        /// </summary>
        /// <remarks>
        /// The probe is transaction scoped and immediately rolled back, so a successful acquisition
        /// doesn't leave the lock held.
        /// </remarks>
        private bool MigrationsLockIsHeld()
        {
            using var connection  = OpenConnection();
            using var transaction = connection.BeginTransaction();
            using var command     = new NpgsqlCommand(
                $"SELECT pg_try_advisory_xact_lock({MigrationsLockKey.ToString(CultureInfo.InvariantCulture)})", connection, transaction);

            var acquired = (bool)command.ExecuteScalar();

            transaction.Rollback();

            return !acquired;
        }

        /// <summary>
        /// Waits for <paramref name="condition"/> to become true, or fails with <paramref name="because"/>
        /// once the deadline passes.
        /// </summary>
        private static async Task WaitForAsync(Func<bool> condition, string because, TimeSpan? deadline = null)
        {
            await SyncContext.Clear;

            var stateDeadline = deadline ?? StateDeadline;

            using var cancellationTokenSource = new CancellationTokenSource(stateDeadline);

            while (!condition())
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    Assert.Fail($"Timed out after [{stateDeadline}] waiting for: {because}");
                }

                try
                {
                    await Task.Delay(PollInterval, cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // Fall through so the next iteration reports the failure with context.
                }
            }
        }

        //---------------------------------------------------------------------
        // Options

        /// <summary>
        /// Builds options with the advisory lock history repository installed.
        /// </summary>
        private DbContextOptions<MigrationTestDbContext> CreateOptions(Action<AdvisoryLockMigrationsOptions> configure = null)
        {
            return new DbContextOptionsBuilder<MigrationTestDbContext>()
                .UseNpgsql(connectionString)
                .UseAdvisoryLockMigrationHistory(configure)
                .Options;
        }

        /// <summary>
        /// Builds options with the Npgsql provider's stock migration lock — the one that fails on
        /// YugabyteDB.
        /// </summary>
        private DbContextOptions<MigrationTestDbContext> CreateStockOptions()
        {
            return new DbContextOptionsBuilder<MigrationTestDbContext>()
                .UseNpgsql(connectionString)
                .Options;
        }

        private async Task MigrateAsync(Action<AdvisoryLockMigrationsOptions> configure = null)
        {
            await SyncContext.Clear;

            using var context = new MigrationTestDbContext(CreateOptions(configure));

            await context.Database.MigrateAsync();
        }

        //---------------------------------------------------------------------
        // Tests

        [Fact]
        public void AdvisoryLocksAreEnabledOnTheCluster()
        {
            // If this fails, the cluster is missing the feature and every other test in this file is
            // measuring the wrong thing.

            using var connection  = OpenConnection();
            using var transaction = connection.BeginTransaction();
            using var command     = new NpgsqlCommand("SELECT pg_advisory_xact_lock(12345)", connection, transaction);

            command.ExecuteNonQuery();

            transaction.Commit();
        }

        [Fact]
        public async Task WithoutTheFix_MigrationsFail()
        {
            await SyncContext.Clear;

            // Establishes that the problem this package exists for is real on this cluster, so that the
            // passing tests below can't be passing vacuously.

            using var context = new MigrationTestDbContext(CreateStockOptions());

            var e = await Assert.ThrowsAnyAsync<PostgresException>(() => context.Database.MigrateAsync());

            // 0A000 is feature_not_supported: "ACCESS EXCLUSIVE not supported yet".

            Assert.Equal("0A000", e.SqlState);
        }

        [Fact]
        public async Task MigrationsApply()
        {
            await SyncContext.Clear;

            using (var context = new MigrationTestDbContext(CreateOptions()))
            {
                var migrations = context.Database.GetPendingMigrations().ToList();
                await context.Database.MigrateAsync();
            }

            Assert.Equal(
                new[] { "20240101000001_CreateWidgets", "20240101000002_CreateSprockets" }.OrderBy(name => name),
                AppliedMigrations());

            Assert.True(TableExists("widgets"));
            Assert.True(TableExists("sprockets"));
        }

        [Fact]
        public async Task TheLockIsReleasedWhenTheRunEnds()
        {
            await SyncContext.Clear;

            // Tests the explicit pg_advisory_unlock() our Dispose() issues — and it has to work a little to
            // do that.
            //
            // The obvious version of this test (migrate, then check the lock) passes whether or not we
            // release anything, because closing a pooled connection makes Npgsql run DISCARD ALL, which
            // includes pg_advisory_unlock_all().  That backstop is welcome in production and useless in a
            // test: it hides the very thing being checked.
            //
            // So the connection is owned here rather than by Entity Framework Core.  It stays open after
            // the run, nothing is returned to any pool, and the only thing that could have released the
            // lock is our own release.

            using var connection = OpenConnection();

            var options = new DbContextOptionsBuilder<MigrationTestDbContext>()
                .UseNpgsql(connection)
                .UseAdvisoryLockMigrationHistory()
                .Options;

            using (var context = new MigrationTestDbContext(options))
            {
                await context.Database.MigrateAsync();
            }

            Assert.Equal(2, AppliedMigrations().Count);
            Assert.Equal(ConnectionState.Open, connection.State);

            Assert.False(MigrationsLockIsHeld(), "The advisory lock is still held after the run — Dispose() didn't release it.");
        }

        [Fact]
        public async Task RunningAgainIsANoop()
        {
            await SyncContext.Clear;

            await MigrateAsync();
            await MigrateAsync();

            // Recorded exactly once, which is what the history table is for — and it confirms the
            // second run took and released the lock cleanly rather than deadlocking on leftovers.

            Assert.Equal(2, AppliedMigrations().Count);
            Assert.Equal(2L, Scalar<long>("SELECT COUNT(*) FROM \"__EFMigrationsHistory\""));
        }

        [Fact]
        public async Task AMigratorWaitsForAHeldLock()
        {
            await SyncContext.Clear;

            // The simplest possible statement of mutual exclusion, with no second migrator and no
            // timing: the test holds the key a migrator needs, so the migrator must fail to get in.
            // With no lock at all — the naive "just drop it" fix — this would succeed.

            using (HoldLock(MigrationsLockKey))
            {
                var e = await Assert.ThrowsAsync<MigrationsLockTimeoutException>(
                    () => MigrateAsync(options => options.Timeout = CompetitorTimeout));

                Assert.Equal(MigrationsLockKey, e.LockKey);
                Assert.Empty(AppliedMigrations());
            }

            // Released, so the same migrator now succeeds.  This is what proves the failure above was
            // the lock and not something incidental about the configuration.

            await MigrateAsync(options => options.Timeout = CompetitorTimeout);

            Assert.Equal(2, AppliedMigrations().Count);
        }

        [Fact]
        public async Task AnExplicitLockKeyExcludesToo()
        {
            await SyncContext.Clear;

            // A configured key has to exclude just as well as the derived one, since the point of the
            // option is letting separate contexts deliberately share a key.

            const long sharedKey = 0x4d494752_41544521;

            using (HoldLock(sharedKey))
            {
                await Assert.ThrowsAsync<MigrationsLockTimeoutException>(
                    () => MigrateAsync(
                        options =>
                        {
                            options.LockKey = sharedKey;
                            options.Timeout = CompetitorTimeout;
                        }));
            }

            await MigrateAsync(options => options.LockKey = sharedKey);

            Assert.Equal(2, AppliedMigrations().Count);
        }

        [Fact]
        public async Task TheDerivedKeyIsWhatIsActuallyTaken()
        {
            await SyncContext.Clear;

            // Holding a *different* key must not block anything.  Without this, the test above would
            // still pass if the repository locked on some other value — or on nothing at all and failed
            // for an unrelated reason.

            using (HoldLock(unchecked(MigrationsLockKey + 1)))
            {
                await MigrateAsync(options => options.Timeout = CompetitorTimeout);
            }

            Assert.Equal(2, AppliedMigrations().Count);
        }

        [Fact]
        public async Task TheBlockingWaitOutlastsTheCommandTimeout()
        {
            await SyncContext.Clear;

            // Regression test for a reported issue.
            //
            // pg_advisory_xact_lock() waits in the server, but from the client's point of view it's just
            // a command executing — so Npgsql's command timeout applies, and that defaults to 30
            // seconds.  Before the fix, "no Timeout configured" quietly meant "wait 30 seconds, then die
            // with an opaque cancellation error that says nothing about locks", which is exactly what a
            // migrator queued behind a slow peer would hit.

            var options = new DbContextOptionsBuilder<MigrationTestDbContext>()
                .UseNpgsql(connectionString, npgsql => npgsql.CommandTimeout((int)CommandTimeoutProbe.TotalSeconds))
                .UseAdvisoryLockMigrationHistory()      // no Timeout, so the blocking path is used
                .Options;

            using var held = HoldLock(MigrationsLockKey);

            var migrator = Task.Run(
                async () =>
                {
                    await SyncContext.Clear;

                    using var context = new MigrationTestDbContext(options);

                    await context.Database.MigrateAsync();
                });

            // Deliberately outlast the command timeout.  This is a wait on a *configured* duration
            // rather than a guess about scheduling — exceeding it is the assertion.

            await Task.Delay(CommandTimeoutProbe * 3);

            if (migrator.IsFaulted)
            {
                await migrator;     // Rethrow, so the failure reports the real exception.
            }

            Assert.False(migrator.IsCompleted, "The migrator should still be waiting for the lock.");

            held.Dispose();

            await migrator;

            Assert.Equal(2, AppliedMigrations().Count);
        }

        [Fact]
        public async Task WorksWithAnInternalServiceProvider()
        {
            await SyncContext.Clear;

            // UseInternalServiceProvider() rules out ReplaceService(), so UseAdvisoryLockMigrationHistory()
            // can't be used.  This is the documented alternative, tested end to end because a recipe in a
            // README that nobody runs is a recipe that rots.

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkNpgsql()
                .AddScoped<IHistoryRepository, AdvisoryLockHistoryRepository>()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<MigrationTestDbContext>()
                .UseNpgsql(connectionString)
                .UseInternalServiceProvider(serviceProvider)
                .ConfigureAdvisoryLockMigrationHistory(lockOptions => lockOptions.Timeout = CompetitorTimeout)
                .Options;

            using (var context = new MigrationTestDbContext(options))
            {
                // The repository resolved out of the caller's container, and the options reached it.

                Assert.IsType<AdvisoryLockHistoryRepository>(context.GetService<IHistoryRepository>());

                await context.Database.MigrateAsync();
            }

            Assert.Equal(2, AppliedMigrations().Count);
            Assert.True(TableExists("sprockets"));

            // And the options genuinely took effect rather than being silently dropped: with the lock
            // held elsewhere, the configured timeout is what ends the wait.

            ResetDatabase();

            using (HoldLock(MigrationsLockKey))
            using (var context = new MigrationTestDbContext(options))
            {
                await Assert.ThrowsAsync<MigrationsLockTimeoutException>(() => context.Database.MigrateAsync());
            }
        }

        [Fact]
        public void PlatformProbe_DdlIsTransactional()
        {
            // Guards the cluster configuration the migration behaviour depends on.
            //
            // YugabyteDB ships transactional DDL *disabled*, and with it off a CREATE TABLE survives both
            // an explicit ROLLBACK and a lost connection — so an interrupted migration strands a partially
            // applied schema change that nothing has recorded.  The fixture turns it on with
            // --ysql_yb_ddl_transaction_block_enabled=true, which is postmaster scoped and therefore can't
            // be set per session.
            //
            // If this test fails, check that flag before looking anywhere else: the migration tests below
            // would still pass without it, and the difference would only show up as data loss in
            // production.

            var ddlSettings = new List<string>();

            Execute("DROP TABLE IF EXISTS probe_ddl", "DROP TABLE IF EXISTS probe_dml");
            Execute("CREATE TABLE probe_dml (id int)");

            // 1. An explicit ROLLBACK.

            using (var connection = OpenConnection())
            {
                using var transaction = connection.BeginTransaction();

                using (var command = new NpgsqlCommand("CREATE TABLE probe_ddl (id int)", connection, transaction))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new NpgsqlCommand("INSERT INTO probe_dml VALUES (1)", connection, transaction))
                {
                    command.ExecuteNonQuery();
                }

                transaction.Rollback();
            }

            var ddlSurvivedRollback = TableExists("probe_ddl");
            var dmlSurvivedRollback = Scalar<long>("SELECT COUNT(*) FROM probe_dml") > 0;

            // 2. A killed backend, which is what a crashing process looks like.

            Execute("DROP TABLE IF EXISTS probe_ddl", "DELETE FROM probe_dml");

            const string applicationName = "ddl-probe";

            using (var connection = new NpgsqlConnection($"{connectionString};Application Name={applicationName}"))
            {
                connection.Open();

                using var transaction = connection.BeginTransaction();

                using (var command = new NpgsqlCommand("CREATE TABLE probe_ddl (id int)", connection, transaction))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new NpgsqlCommand("INSERT INTO probe_dml VALUES (1)", connection, transaction))
                {
                    command.ExecuteNonQuery();
                }

                Assert.True(TerminateBackend(applicationName) > 0);

                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {
                    // Expected: the connection is gone.
                }
            }

            var ddlSurvivedCrash = TableExists("probe_ddl");
            var dmlSurvivedCrash = Scalar<long>("SELECT COUNT(*) FROM probe_dml") > 0;

            Execute("DROP TABLE IF EXISTS probe_ddl", "DROP TABLE IF EXISTS probe_dml");

            // Report any DDL related settings the cluster exposes, so that a future reader can see whether
            // a mitigation has appeared without having to go digging.

            using (var connection = OpenConnection())
            using (var command = new NpgsqlCommand(
                "SELECT name, setting, context FROM pg_settings WHERE name LIKE '%ddl%' ORDER BY name", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ddlSettings.Add($"{reader.GetString(0)}={reader.GetString(1)} (context={reader.GetString(2)})");
                }
            }

            var settings = ddlSettings.Count == 0 ? "none" : string.Join(", ", ddlSettings);
            var hint     = $"  Is --ysql_yb_ddl_transaction_block_enabled=true set on the tserver?  DDL settings: {settings}";

            // Row changes are transactional regardless of the flag.

            Assert.False(dmlSurvivedRollback, "An INSERT survived an explicit ROLLBACK.");
            Assert.False(dmlSurvivedCrash, "An INSERT survived a killed backend.");

            // Schema changes are too, once transactional DDL is on.  Both halves matter: an explicit
            // rollback and a lost connection are different code paths in the server, and it's the second
            // one that decides whether a crashing migrator can strand a half-applied migration.

            Assert.False(ddlSurvivedRollback, $"CREATE TABLE survived an explicit ROLLBACK.{hint}");
            Assert.False(ddlSurvivedCrash, $"CREATE TABLE survived a killed backend.{hint}");
        }

        [Fact]
        public async Task ACrashMidMigrationRollsBackThatMigration()
        {
            await SyncContext.Clear;

            // Answers a reported concern: "there's no rollback if the app crashes while the migration is
            // being applied."
            //
            // The second migration is parked *after* creating its table but before committing, which is
            // exactly the state a dying process leaves behind, and then its backend is killed — what the
            // server sees when a process dies: connection gone, transaction still open.
            //
            // What this pins down:
            //
            //   * the interrupted migration rolls back completely, table and history row alike — but only
            //     because transactional DDL is enabled (see PlatformProbe_DdlIsTransactional);
            //   * earlier migrations in the run stay applied, since each commits separately, so a run of
            //     several migrations is not one atomic unit;
            //   * the advisory lock is released, so the replacement migrator isn't locked out;
            //   * therefore a plain retry finishes the job with no manual intervention.

            const string applicationName = "crash-probe";

            MigrationGate.Position = MigrationGate.GatePosition.AfterWork;

            using var gate = HoldLock(MigrationGate.Key);

            var options = new DbContextOptionsBuilder<MigrationTestDbContext>()
                .UseNpgsql($"{connectionString};Application Name={applicationName}")
                .UseAdvisoryLockMigrationHistory()
                .Options;

            var migrator = Task.Run(
                async () =>
                {
                    await SyncContext.Clear;

                    using var context = new MigrationTestDbContext(options);

                    await context.Database.MigrateAsync();
                });

            // Wait until the schema change has run and the migration is parked.  The table itself can't be
            // observed from here — it's uncommitted — but the lock the migration takes on its way to the
            // gate can be.

            await WaitForAsync(
                () => LockIsHeld(MigrationGate.WorkDoneKey),
                "the second migration to create its table and park before committing");

            Assert.Contains("20240101000001_CreateWidgets", AppliedMigrations());
            Assert.DoesNotContain("20240101000002_CreateSprockets", AppliedMigrations());

            // Pull the plug.

            Assert.True(TerminateBackend(applicationName) > 0, "No migrator backend was found to terminate.");

            await Assert.ThrowsAnyAsync<Exception>(() => migrator);

            // The interrupted migration left nothing behind — no history row, and no table either.  The
            // table is the interesting half: it only rolls back because transactional DDL is enabled, and
            // this is the assertion that would catch that configuration being lost.

            Assert.DoesNotContain("20240101000002_CreateSprockets", AppliedMigrations());
            Assert.False(TableExists("sprockets"), "The interrupted migration's table survived the crash — check --ysql_yb_ddl_transaction_block_enabled on the tserver.");

            // The migration that had already committed is untouched.  A run of several migrations is not
            // one atomic unit on any provider: Entity Framework Core commits each one separately.

            Assert.Contains("20240101000001_CreateWidgets", AppliedMigrations());
            Assert.True(TableExists("widgets"));

            // Nothing is *stuck*: the lock belongs to the session, and the session died with the backend, so
            // PostgreSQL released it without anybody calling pg_advisory_unlock().  This is the assertion
            // that matters most for the session scoped design — its one theoretical weakness is a lock that
            // outlives the process holding it, and this is the case where that would show up.
            //
            // The release isn't instant.  Terminating a backend is asynchronous, so the lock stays held for
            // a moment afterwards; this waits for it rather than asserting at the instant the migrator's
            // task faulted, which is earlier than the server has finished cleaning up.

            await WaitForAsync(
                () => !MigrationsLockIsHeld(),
                "the advisory lock to be released after the crash — if this times out, a crashed migrator "
                + "locks out its replacement until something clears the lock by hand, which would be a "
                + "serious strike against session scope");

            // And recovery needs no help: the interrupted migration is still pending and nothing it did
            // survives, so simply running again finishes the job.  Restarting a crashed migrator is a
            // complete recovery strategy.

            MigrationGate.Position = MigrationGate.GatePosition.None;

            await MigrateAsync();

            Assert.Equal(2, AppliedMigrations().Count);
            Assert.True(TableExists("sprockets"));
        }

        [Fact]
        public async Task TheLockIsTakenExactlyOnceForTheWholeRun()
        {
            await SyncContext.Clear;

            // The point of the session scoped lock: one acquisition covers the entire run.
            //
            // Entity Framework Core commits after each migration and begins a new transaction for the
            // next, calling ReacquireIfNeeded(connectionOpened, transactionRestarted: true) each time.  A
            // transaction scoped lock would have died at every one of those commits and had to be retaken,
            // leaving a window after each commit where nothing held it.  A session scoped lock ignores
            // commits entirely, so there is nothing to retake and no window.
            //
            // Counting the acquisitions is how that difference is made visible: exactly one, with no
            // reacquisition, across a two-migration run.

            var acquisitions = new List<Activity>();
            var releases     = new List<Activity>();
            var syncRoot     = new object();

            using var listener = new ActivityListener()
            {
                ShouldListenTo  = source => source.Name == NpgsqlTracing.ActivitySourceName,
                Sample          = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStopped = activity =>
                {
                    lock (syncRoot)
                    {
                        switch (activity.OperationName)
                        {
                            case "AcquireDatabaseLock":

                                acquisitions.Add(activity);
                                break;

                            case "ReleaseDatabaseLock":

                                releases.Add(activity);
                                break;
                        }
                    }
                }
            };

            ActivitySource.AddActivityListener(listener);

            await MigrateAsync();

            Assert.Equal(2, AppliedMigrations().Count);

            lock (syncRoot)
            {
                var acquisition = Assert.Single(acquisitions);

                Assert.Equal(true, acquisition.GetTagItem(NpgsqlTraceTags.LockAcquired));
                Assert.Equal(MigrationsLockKey, acquisition.GetTagItem(NpgsqlTraceTags.LockKey));

                // Not a reacquisition, because there was nothing to reacquire.

                Assert.NotEqual(true, acquisition.GetTagItem(NpgsqlTraceTags.LockReacquired));

                // And it was given back once, explicitly — the cost of session scope.

                var release = Assert.Single(releases);

                Assert.Equal(true, release.GetTagItem(NpgsqlTraceTags.LockReleased));
                Assert.Equal(MigrationsLockKey, release.GetTagItem(NpgsqlTraceTags.LockKey));
            }
        }

        [Fact]
        public async Task TheLockSurvivesTheCommitBoundary()
        {
            await SyncContext.Clear;

            // This is the test that proves the lock spans the whole run rather than just the first
            // migration.
            //
            // Entity Framework Core commits after each migration and opens a new transaction for the next
            // one.  A transaction scoped lock would have died at that commit, and everything after it would
            // depend on the reacquire condition being right; a session scoped lock is simply unaffected.
            // Either way this is the case that a test applying migrations from a standing start can't
            // reach — so it is the one that catches a lock whose scope and reacquire condition disagree.
            //
            // So: park the migrator inside its *second* migration and prove a competitor is still locked
            // out.

            MigrationGate.Position = MigrationGate.GatePosition.BeforeWork;

            using var gate = HoldLock(MigrationGate.Key);

            var migrator = Task.Run(() => MigrateAsync());

            // Wait for the first migration to commit — the commit boundary a transaction scoped lock
            // wouldn't have survived.

            await WaitForAsync(
                () => AppliedMigrations().Contains("20240101000001_CreateWidgets"),
                "the first migration to be committed");

            // The migrator is now parked on the gate inside the second migration, and the migrations lock
            // must still be held.  Checked as its own step so that a failure here is unambiguous.

            await WaitForAsync(
                () => MigrationsLockIsHeld(),
                "the migrations lock to still be held during the second migration — if this is the only "
                + "failure, the lock's scope and its reacquire condition disagree, and every migration "
                + "after the first is running unprotected (see AdvisoryLockHistoryRepository.NeedsReacquire)",
                deadline: ReacquireDeadline);

            Assert.DoesNotContain("20240101000002_CreateSprockets", AppliedMigrations());

            await Assert.ThrowsAsync<MigrationsLockTimeoutException>(
                () => MigrateAsync(options => options.Timeout = CompetitorTimeout));

            // Open the gate and let the parked migrator finish.

            gate.Dispose();

            await migrator;

            Assert.Equal(2, AppliedMigrations().Count);
            Assert.True(TableExists("sprockets"));
        }
    }
}