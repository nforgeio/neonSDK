//-----------------------------------------------------------------------------
// FILE:        AdvisoryLockHistoryRepository.cs
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
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

using Neon.Diagnostics;
using Neon.EntityFrameworkCore;
using Neon.Tasks;

using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

#pragma warning disable EF1001  // Internal EF Core API usage — see the class remarks.

namespace Neon.EntityFrameworkCore.Npgsql
{
    /// <summary>
    /// An <see cref="IHistoryRepository"/> for the Npgsql provider that guards a migration run with
    /// a PostgreSQL <b>session scoped advisory lock</b> instead of <c>LOCK TABLE ... IN ACCESS
    /// EXCLUSIVE MODE</c>.  This makes <c>Database.Migrate()</c> work against Postgres wire
    /// compatible databases that don't implement explicit table locks, YugabyteDB in particular.
    /// </summary>
    /// <remarks>
    /// <para><b>THE PROBLEM</b></para>
    /// <para>
    /// Entity Framework Core 9 introduced a database lock held for the duration of a migration run,
    /// so that two processes can't apply the same migration concurrently.  The Npgsql provider
    /// implements that lock by issuing:
    /// </para>
    /// <code language="sql">
    /// LOCK TABLE "__EFMigrationsHistory" IN ACCESS EXCLUSIVE MODE
    /// </code>
    /// <para>
    /// YugabyteDB doesn't implement explicit table locks and rejects the statement outright, so every
    /// migration fails immediately with:
    /// </para>
    /// <code>
    /// Npgsql.PostgresException: 0A000: ACCESS EXCLUSIVE not supported yet
    /// </code>
    /// <para>
    /// There is no Entity Framework Core switch that turns the lock off, so replacing
    /// <see cref="IHistoryRepository"/> is the only intervention point.  The same code path is fine
    /// on Entity Framework Core 8 and earlier, which is why this only shows up on upgrade.
    /// </para>
    /// <para><b>THE FIX, AND WHY IT'S THIS ONE</b></para>
    /// <para>
    /// Returning a no-op lock would also make migrations run, and would restore exactly the
    /// Entity Framework Core 8 behaviour, but it silently discards the protection the lock exists to
    /// provide: nothing would stop two migrator pods applying the same migration at the same time.
    /// YugabyteDB 2.25 and later support advisory locks, which give the same mutual exclusion, so
    /// there's no reason to give it up.
    /// </para>
    /// <para>
    /// The lock taken is <c>pg_advisory_lock()</c> — <b>session</b> scoped — rather than the transaction
    /// scoped <c>pg_advisory_xact_lock()</c>, and <see cref="LockReleaseBehavior"/> reports
    /// <see cref="LockReleaseBehavior.Explicit"/> to match.  The reason is that Entity Framework Core
    /// runs each migration in its own transaction: a transaction scoped lock would die at every commit
    /// and have to be retaken on the way into the next migration, leaving a brief window after each
    /// commit in which nothing holds the lock and a competing migrator could get in.  A session scoped
    /// lock spans the whole run, which is what Entity Framework Core's "one lock for the migration"
    /// model is actually asking for.
    /// </para>
    /// <para>
    /// Two consequences follow, both good.  Reacquisition is only needed when the <i>connection</i> is
    /// replaced rather than after every commit — see <c>NeedsReacquire</c> — and a migration that
    /// suppresses its transaction is still protected, where a transaction scoped lock would have had
    /// nothing to live in.
    /// </para>
    /// <para>
    /// The cost is that the lock has to be given back, which the returned
    /// <see cref="IMigrationsDatabaseLock"/> does on dispose.  That release is best effort and never
    /// throws — Entity Framework Core disposes the lock while a possibly-aborted transaction is still
    /// open — and it isn't the only safety net: a session scoped lock dies with its session, so a killed
    /// process releases it when the connection drops, and Npgsql's pool reset (<c>DISCARD ALL</c>,
    /// which includes <c>pg_advisory_unlock_all()</c>) releases it when the connection is returned.
    /// </para>
    /// <note>
    /// This is a deliberate departure from the <c>LOCK TABLE</c> statement being replaced, which was
    /// transaction scoped — as is the stock Npgsql provider's <see cref="LockReleaseBehavior"/>.  The
    /// per-commit window described above therefore exists on real PostgreSQL too; this class doesn't
    /// have it.
    /// </note>
    /// <para><b>YUGABYTEDB PREREQUISITE</b></para>
    /// <para>
    /// Advisory locks are <b>enabled by default from YugabyteDB 2025.0 onward</b>, so there's nothing
    /// to configure on a current cluster.  They arrived in <b>2.25</b> as a preview feature that was
    /// off by default; on those older releases both flags are required, on the master and the tserver:
    /// </para>
    /// <code>
    /// --allowed_preview_flags_csv=ysql_yb_enable_advisory_locks
    /// --ysql_yb_enable_advisory_locks=true
    /// </code>
    /// <para>
    /// Without them <c>pg_advisory_lock()</c> <b>fails</b> rather than doing nothing, so the
    /// symptom becomes "advisory locks not yet implemented" instead of "ACCESS EXCLUSIVE not supported
    /// yet".  Check that before concluding this class is broken.
    /// </para>
    /// <note>
    /// YugabyteDB also has a <c>yb_silence_advisory_locks_not_supported_error</c> setting that
    /// downgrades that failure to a silent no-op.  Don't use it with this class: it converts a loud
    /// misconfiguration into a silent loss of mutual exclusion, which is the one outcome this class
    /// exists to avoid.
    /// </note>
    /// <para><b>LIMITATIONS</b></para>
    /// <list type="bullet">
    ///     <item>
    ///     The lock is scoped to the connection Entity Framework Core uses for the migration run, so it
    ///     excludes other <i>migrators</i> but not arbitrary schema changes made by hand on another
    ///     session.  That's the same guarantee the <c>LOCK TABLE</c> statement being replaced offered.
    ///     </item>
    ///     <item>
    ///     <para>
    ///     <b>Crash safety requires transactional DDL, which YugabyteDB ships disabled.</b>  Set
    ///     <c>--ysql_yb_ddl_transaction_block_enabled=true</c> on every tserver.  Without it a
    ///     <c>CREATE TABLE</c> survives both an explicit rollback and a lost connection, so a migrator that
    ///     dies partway through a migration leaves the schema change in place while its history row rolls
    ///     back — the migration still counts as pending, the retry fails with <b>42P07</b>, and recovery is
    ///     manual.  With the flag on, an interrupted migration rolls back completely and restarting the
    ///     migrator is enough.
    ///     </para>
    ///     <para>
    ///     Either way the advisory lock is released, so the replacement migrator isn't locked out, and
    ///     either way migrations that already committed stay applied: Entity Framework Core commits each
    ///     migration separately, so a multi-migration <i>run</i> is never one atomic unit on any provider.
    ///     </para>
    ///     </item>
    ///     <item>
    ///     The key is derived from the <i>configured</i> history table schema, defaulting to
    ///     <b>public</b> when none is set.  A deployment that relies on <c>search_path</c> to resolve
    ///     the history table into some other schema will have two sessions serialize on the same key
    ///     while using different tables.  That over-serializes, which is safe, rather than
    ///     under-serializing, which wouldn't be.  Set
    ///     <see cref="AdvisoryLockMigrationsOptions.LockKey"/> if you'd rather they didn't.
    ///     </item>
    /// </list>
    /// <para><b>MAINTENANCE</b></para>
    /// <note>
    /// <see cref="NpgsqlHistoryRepository"/> is an internal provider type (hence the <b>EF1001</b>
    /// suppression), so its shape is not covered by semantic versioning and this class is coupled to
    /// the Npgsql provider's major version.  That's why the provider package is pinned to a single
    /// major in <c>Directory.Packages.props</c>, and why <b>Test.Neon.EntityFrameworkCore</b> carries
    /// guard tests that fail loudly if the members overridden here change shape.
    /// </note>
    /// <para><b>USAGE</b></para>
    /// <para>
    /// Register it with
    /// <see cref="DbContextOptionsBuilderExtensions.UseAdvisoryLockMigrationHistory(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder, Action{AdvisoryLockMigrationsOptions})"/>
    /// rather than naming the type yourself:
    /// </para>
    /// <code language="csharp">
    /// optionsBuilder
    ///     .UseNpgsql(connectionString)
    ///     .UseAdvisoryLockMigrationHistory();
    /// </code>
    /// <note>
    /// This is <b>opt-in on purpose</b>.  On real PostgreSQL the provider's own <c>LOCK TABLE</c>
    /// implementation works and is stricter, so don't fold this into a general purpose "apply our
    /// defaults" call.
    /// </note>
    /// </remarks>
    public class AdvisoryLockHistoryRepository : NpgsqlHistoryRepository
    {
        //---------------------------------------------------------------------
        // Static members

        /// <summary>
        /// The schema assumed when the migrations history table isn't explicitly mapped to one.
        /// </summary>
        private const string DefaultSchema = "public";

        //---------------------------------------------------------------------
        // Instance members

        private AdvisoryLockMigrationsOptions   options;
        private long?                           lockKey;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dependencies">The history repository dependencies injected by Entity Framework Core.</param>
        public AdvisoryLockHistoryRepository(HistoryRepositoryDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        /// Returns <see cref="LockReleaseBehavior.Explicit"/>, telling Entity Framework Core that the
        /// lock lives until it's released rather than until the current transaction ends.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This matches what <c>pg_advisory_lock()</c> actually does: the lock belongs to the
        /// <i>session</i>, so it survives every commit in the run and is released by
        /// <c>pg_advisory_unlock()</c> or when the session ends.
        /// </para>
        /// <note>
        /// Stated explicitly rather than inherited, so that a change to the base class can't quietly
        /// change the contract this class depends on.  <b>Test.Neon.EntityFrameworkCore</b> asserts the
        /// value.
        /// </note>
        /// </remarks>
        public override LockReleaseBehavior LockReleaseBehavior => LockReleaseBehavior.Explicit;

        /// <summary>
        /// Returns the options this repository was configured with, falling back to the defaults when
        /// the type was registered without going through
        /// <see cref="DbContextOptionsBuilderExtensions.UseAdvisoryLockMigrationHistory(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder, Action{AdvisoryLockMigrationsOptions})"/>.
        /// </summary>
        protected virtual AdvisoryLockMigrationsOptions Options
        {
            get
            {
                if (options != null)
                {
                    return options;
                }

                var contextOptions = Dependencies.CurrentContext.Context.GetService<IDbContextOptions>();
                var extension      = contextOptions?.FindExtension<AdvisoryLockMigrationsOptionsExtension>();

                return options = extension?.Options ?? new AdvisoryLockMigrationsOptions();
            }
        }

        /// <summary>
        /// Returns the 64 bit advisory lock key used to serialize migration runs.
        /// </summary>
        /// <remarks>
        /// This is <see cref="AdvisoryLockMigrationsOptions.LockKey"/> when one was configured, and
        /// otherwise <see cref="ComputeLockKey(string, string)"/> applied to the schema and name of
        /// the migrations history table.
        /// </remarks>
        protected virtual long LockKey
        {
            get
            {
                return lockKey ??= Options.LockKey ?? ComputeLockKey(TableSchema ?? DefaultSchema, TableName);
            }
        }

        /// <summary>
        /// Derives a stable advisory lock key from the schema qualified name of the migrations
        /// history table.
        /// </summary>
        /// <param name="schema">The history table's schema.</param>
        /// <param name="table">The history table's name.</param>
        /// <returns>The advisory lock key.</returns>
        /// <remarks>
        /// <note>
        /// This is a hand rolled FNV-1a hash rather than <see cref="string.GetHashCode()"/> for a
        /// load bearing reason: .NET randomizes string hashing per process, so two migrator processes
        /// would derive <i>different</i> keys, fail to exclude each other, and give no indication that
        /// anything was wrong.  Any override must be equally stable across processes, machines and
        /// releases.
        /// </note>
        /// <para>
        /// The full 64 bits of the hash are used, reinterpreted as a signed <see cref="long"/>, since
        /// that's the width PostgreSQL advisory lock keys occupy.
        /// </para>
        /// </remarks>
        protected virtual long ComputeLockKey(string schema, string table)
        {
            const ulong offsetBasis = 14695981039346656037;
            const ulong prime       = 1099511628211;

            var hash = offsetBasis;

            unchecked
            {
                foreach (var ch in $"{schema}.{table}")
                {
                    hash ^= ch;
                    hash *= prime;
                }

                return (long)hash;
            }
        }

        /// <inheritdoc/>
        public override IMigrationsDatabaseLock AcquireDatabaseLock()
        {
            Acquire(reacquired: false);

            return new AdvisoryLock(this);
        }

        /// <inheritdoc/>
        public override async Task<IMigrationsDatabaseLock> AcquireDatabaseLockAsync(
            CancellationToken cancellationToken = default)
        {
            await SyncContext.Clear;

            await AcquireAsync(reacquired: false, cancellationToken: cancellationToken);

            return new AdvisoryLock(this);
        }

        /// <summary>
        /// Acquires the advisory lock, blocking or polling according to
        /// <see cref="AdvisoryLockMigrationsOptions.Timeout"/>.
        /// </summary>
        /// <param name="reacquired">
        /// <c>true</c> when this is a reacquisition after a commit or a reconnect rather than the
        /// initial acquisition.  Recorded on the activity only.
        /// </param>
        /// <exception cref="MigrationsLockTimeoutException">
        /// Thrown when a timeout was configured and elapsed before the lock was granted.
        /// </exception>
        private void Acquire(bool reacquired)
        {
            var key     = LockKey;
            var timeout = Options.Timeout;

            using var activity = StartAcquireActivity(key, timeout, reacquired);

            try
            {
                if (timeout == null)
                {
                    // Block in the server until the lock is granted, matching the LOCK TABLE
                    // statement this replaces.  See SuspendCommandTimeout() for why the timeout has
                    // to be lifted for this one statement.

                    using (SuspendCommandTimeout())
                    {
                        BuildAcquireCommand(key).ExecuteNonQuery(BuildParameters());
                    }

                    Succeeded(activity, attempts: 1);

                    return;
                }

                var stopwatch = Stopwatch.StartNew();
                var attempts  = 0;

                while (true)
                {
                    attempts++;

                    if ((bool)BuildTryAcquireCommand(key).ExecuteScalar(BuildParameters()))
                    {
                        Succeeded(activity, attempts);

                        return;
                    }

                    var remaining = timeout.Value - stopwatch.Elapsed;

                    if (remaining <= TimeSpan.Zero)
                    {
                        throw new MigrationsLockTimeoutException(key, timeout.Value, attempts);
                    }

                    Thread.Sleep(remaining < Options.PollInterval ? remaining : Options.PollInterval);
                }
            }
            catch (Exception e)
            {
                activity.Error(e);
                throw;
            }
        }

        /// <summary>
        /// Asynchronously acquires the advisory lock, blocking or polling according to
        /// <see cref="AdvisoryLockMigrationsOptions.Timeout"/>.
        /// </summary>
        /// <param name="reacquired">
        /// <c>true</c> when this is a reacquisition after a commit or a reconnect rather than the
        /// initial acquisition.  Recorded on the activity only.
        /// </param>
        /// <param name="cancellationToken">Optionally specifies a cancellation token.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="MigrationsLockTimeoutException">
        /// Thrown when a timeout was configured and elapsed before the lock was granted.
        /// </exception>
        private async Task AcquireAsync(bool reacquired, CancellationToken cancellationToken)
        {
            await SyncContext.Clear;

            var key     = LockKey;
            var timeout = Options.Timeout;

            using var activity = StartAcquireActivity(key, timeout, reacquired);

            try
            {
                if (timeout == null)
                {
                    // Block in the server until the lock is granted, matching the LOCK TABLE
                    // statement this replaces.  See SuspendCommandTimeout() for why the timeout has
                    // to be lifted for this one statement.

                    using (SuspendCommandTimeout())
                    {
                        await BuildAcquireCommand(key).ExecuteNonQueryAsync(BuildParameters(), cancellationToken);
                    }

                    Succeeded(activity, attempts: 1);

                    return;
                }

                var stopwatch = Stopwatch.StartNew();
                var attempts  = 0;

                while (true)
                {
                    attempts++;

                    if ((bool)await BuildTryAcquireCommand(key).ExecuteScalarAsync(BuildParameters(), cancellationToken))
                    {
                        Succeeded(activity, attempts);

                        return;
                    }

                    var remaining = timeout.Value - stopwatch.Elapsed;

                    if (remaining <= TimeSpan.Zero)
                    {
                        throw new MigrationsLockTimeoutException(key, timeout.Value, attempts);
                    }

                    await Task.Delay(remaining < Options.PollInterval ? remaining : Options.PollInterval, cancellationToken);
                }
            }
            catch (Exception e)
            {
                activity.Error(e);
                throw;
            }
        }

        /// <summary>
        /// Releases the advisory lock.  Best effort: this never throws.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A session scoped lock has to be given back, which is the one real cost of choosing it over the
        /// transaction scoped form.  Two things keep that from being fragile.
        /// </para>
        /// <para>
        /// First, this can't throw.  Entity Framework Core disposes the lock inside a <c>finally</c>, while
        /// the transaction is still open — and if the migration failed, that transaction is in an aborted
        /// state where every statement errors with <b>25P02</b>.  Throwing from there would replace the
        /// migration's real exception with a meaningless one.
        /// </para>
        /// <para>
        /// Second, failing to release isn't fatal.  The lock belongs to the session, so it goes away when
        /// the session does: if the process dies the connection drops and PostgreSQL releases it, and when
        /// Entity Framework Core closes the connection Npgsql's pool reset runs <c>DISCARD ALL</c>, which
        /// includes <c>pg_advisory_unlock_all()</c>.  The explicit release is the prompt path, not the only
        /// one.
        /// </para>
        /// </remarks>
        private void Release()
        {
            using var activity = TraceContext.ActivitySource?.StartActivity("ReleaseDatabaseLock", ActivityKind.Client);

            activity.Tag(NpgsqlTraceTags.LockKey, LockKey);

            try
            {
                BuildReleaseCommand(LockKey).ExecuteNonQuery(BuildParameters());

                activity.Tag(NpgsqlTraceTags.LockReleased, true);
            }
            catch (Exception e)
            {
                // Swallowed on purpose — see the remarks.  The activity records it so that a release that
                // is quietly failing every run is still visible in a trace backend.

                activity.Tag(NpgsqlTraceTags.LockReleased, false);
                activity.Error(e);
            }
        }

        /// <summary>
        /// Builds the blocking acquisition command.
        /// </summary>
        private IRelationalCommand BuildAcquireCommand(long key)
        {
            return Dependencies.RawSqlCommandBuilder.Build($"SELECT pg_advisory_lock({key.ToString(CultureInfo.InvariantCulture)})");
        }

        /// <summary>
        /// Lifts the ADO.NET command timeout for the duration of the blocking lock acquisition,
        /// restoring the previous value on dispose.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>pg_advisory_lock()</c> waits in the <i>server</i>, but the wait is still a command
        /// execution as far as the client is concerned — so Npgsql's command timeout applies to it, and
        /// that timeout defaults to <b>30 seconds</b>.  Left alone, an unbounded wait quietly becomes a
        /// 30 second one that fails with an opaque cancellation error rather than with anything that
        /// mentions locks: a migrator queued behind a peer taking longer than half a minute would die,
        /// and the promise that no configured
        /// <see cref="AdvisoryLockMigrationsOptions.Timeout"/> means "wait as long as it takes" would be
        /// false.
        /// </para>
        /// <para>
        /// Only the lock statement is affected, and only while it's waiting.  The migration's own
        /// commands keep whatever timeout the application configured — which matters, because on
        /// YugabyteDB a large migration can genuinely need a raised timeout of its own.
        /// </para>
        /// <note>
        /// When a <see cref="AdvisoryLockMigrationsOptions.Timeout"/> <i>is</i> configured the
        /// acquisition polls with <c>pg_try_advisory_lock()</c> instead, and every one of those
        /// statements returns immediately — so the command timeout is never in play on that path and
        /// isn't touched.
        /// </note>
        /// </remarks>
        private IDisposable SuspendCommandTimeout()
        {
            return new SuspendedCommandTimeout(Dependencies.Connection);
        }

        /// <summary>
        /// Builds the non-blocking acquisition command, which returns <c>true</c> when the lock was
        /// granted and <c>false</c> when it is held elsewhere.
        /// </summary>
        private IRelationalCommand BuildTryAcquireCommand(long key)
        {
            return Dependencies.RawSqlCommandBuilder.Build($"SELECT pg_try_advisory_lock({key.ToString(CultureInfo.InvariantCulture)})");
        }

        /// <summary>
        /// Builds the release command.
        /// </summary>
        private IRelationalCommand BuildReleaseCommand(long key)
        {
            return Dependencies.RawSqlCommandBuilder.Build($"SELECT pg_advisory_unlock({key.ToString(CultureInfo.InvariantCulture)})");
        }

        /// <summary>
        /// Builds the parameter object required to execute a command on the migration connection.
        /// </summary>
        private RelationalCommandParameterObject BuildParameters()
        {
            return new RelationalCommandParameterObject(
                connection: Dependencies.Connection,
                parameterValues: null,
                readerColumns: null,
                context: Dependencies.CurrentContext.Context,
                logger: Dependencies.CommandLogger);
        }

        /// <summary>
        /// Starts the lock acquisition activity, tagged so that two migrators can be compared side by
        /// side in a trace backend.  Returns <c>null</c> when nothing is listening.
        /// </summary>
        private Activity StartAcquireActivity(long key, TimeSpan? timeout, bool reacquired)
        {
            var activity = TraceContext.ActivitySource?.StartActivity("AcquireDatabaseLock", ActivityKind.Client);

            if (activity == null)
            {
                return null;
            }

            var schema = TableSchema ?? DefaultSchema;

            activity.Tag(TraceTags.DbSystemName, "postgresql")
                    .Tag(TraceTags.DbNamespace, schema)
                    .Tag(TraceTags.DbCollectionName, TableName)
                    .Tag(TraceTags.DbOperationName, "LOCK")
                    .Tag(NpgsqlTraceTags.HistoryTable, $"{schema}.{TableName}")
                    .Tag(NpgsqlTraceTags.LockKey, key)
                    .Tag(NpgsqlTraceTags.LockMode, timeout == null ? "blocking" : "polling")
                    .Tag(NpgsqlTraceTags.LockAcquired, false);

            if (timeout != null)
            {
                activity.Tag(NpgsqlTraceTags.LockTimeoutSeconds, timeout.Value.TotalSeconds);
            }

            if (reacquired)
            {
                activity.Tag(NpgsqlTraceTags.LockReacquired, true);
            }

            activity.DisplayName = $"LOCK {TableName}";

            return activity;
        }

        /// <summary>
        /// Records a successful acquisition on the activity.
        /// </summary>
        private static void Succeeded(Activity activity, int attempts)
        {
            activity.Tag(NpgsqlTraceTags.LockAcquired, true)
                    .Tag(NpgsqlTraceTags.LockAttempts, attempts);
        }

        //---------------------------------------------------------------------
        // Private types

        /// <summary>
        /// Sets the connection's command timeout to infinite and restores the previous value when
        /// disposed.  See <see cref="SuspendCommandTimeout"/>.
        /// </summary>
        private sealed class SuspendedCommandTimeout : IDisposable
        {
            private const int Infinite = 0;   // ADO.NET's "no limit"

            private readonly IRelationalConnection connection;
            private readonly int?                  previous;

            public SuspendedCommandTimeout(IRelationalConnection connection)
            {
                this.connection = connection;
                this.previous = connection.CommandTimeout;

                connection.CommandTimeout = Infinite;
            }

            public void Dispose()
            {
                connection.CommandTimeout = previous;
            }
        }

        /// <summary>
        /// The <see cref="IMigrationsDatabaseLock"/> handed back to Entity Framework Core.  Disposing it
        /// releases the session scoped advisory lock it represents.
        /// </summary>
        private sealed class AdvisoryLock : IMigrationsDatabaseLock
        {
            private readonly AdvisoryLockHistoryRepository historyRepository;
            private bool                                   released;

            public AdvisoryLock(AdvisoryLockHistoryRepository historyRepository)
            {
                this.historyRepository = historyRepository;
            }

            /// <inheritdoc/>
            public IHistoryRepository HistoryRepository => historyRepository;

            /// <inheritdoc/>
            public IMigrationsDatabaseLock ReacquireIfNeeded(bool connectionReopened, bool? transactionRestarted)
            {
                if (NeedsReacquire(connectionReopened, transactionRestarted))
                {
                    historyRepository.Acquire(reacquired: true);
                }

                return this;
            }

            /// <inheritdoc/>
            public async Task<IMigrationsDatabaseLock> ReacquireIfNeededAsync(
                bool connectionReopened,
                bool? transactionRestarted,
                CancellationToken cancellationToken = default)
            {
                await SyncContext.Clear;

                if (NeedsReacquire(connectionReopened, transactionRestarted))
                {
                    await historyRepository.AcquireAsync(reacquired: true, cancellationToken: cancellationToken);
                }

                return this;
            }

            /// <summary>
            /// Decides whether the lock has to be taken again.
            /// </summary>
            /// <param name="connectionReopened">Set when Entity Framework Core reopened the connection.</param>
            /// <param name="transactionRestarted">Set when Entity Framework Core started a new transaction.</param>
            /// <remarks>
            /// <note>
            /// <b>This condition is load bearing, and it is tied to the lock's scope.</b>  Our lock belongs
            /// to the <i>session</i>, so it survives every commit and only has to be retaken when the
            /// session itself is replaced — which is exactly what a reopened connection means.  A new
            /// transaction changes nothing, so <paramref name="transactionRestarted"/> is deliberately not
            /// consulted.
            /// </note>
            /// <note>
            /// If this class is ever changed back to <c>pg_advisory_xact_lock()</c>, this condition
            /// <b>must</b> gain <c>|| transactionRestarted == true</c> at the same time.  A transaction
            /// scoped lock dies at every commit, so without that a multi-migration run would continue
            /// <b>unprotected</b> after the first one — and it would still pass every test that applies a
            /// single migration.  <c>TheLockSurvivesTheCommitBoundary</c> is the test that catches the
            /// mismatch.
            /// </note>
            /// </remarks>
            private static bool NeedsReacquire(bool connectionReopened, bool? transactionRestarted)
            {
                return connectionReopened;
            }

            /// <inheritdoc/>
            /// <remarks>
            /// <note>
            /// This is where the lock is actually given back, and it does the work <b>synchronously</b> even
            /// though the type offers <see cref="DisposeAsync"/>.  That's not an oversight: Entity Framework
            /// Core's migrator calls the synchronous <c>Dispose()</c> from its <c>finally</c> block on both
            /// the sync and async paths, so an async-only release would simply never run.
            /// </note>
            /// </remarks>
            public void Dispose()
            {
                if (released)
                {
                    return;
                }

                released = true;

                historyRepository.Release();
            }

            /// <inheritdoc/>
            public ValueTask DisposeAsync()
            {
                // Releasing is a single round trip and Dispose() is idempotent, so there's nothing to gain
                // from a separate async path — and Entity Framework Core doesn't use this one anyway.

                Dispose();

                return default;
            }
        }
    }
}