Neon.EntityFrameworkCore.Npgsql
===============================

Entity Framework Core migration locking that works on **YugabyteDB** and other Postgres wire
compatible databases that don't implement `LOCK TABLE`.

You can get started here: [Neon.EntityFrameworkCore.Npgsql](https://sdk.neonforge.com/N_Neon_EntityFrameworkCore_Npgsql.htm)

```bash
dotnet add package Neon.EntityFrameworkCore.Npgsql
```

Targets `net10.0`, Entity Framework Core 10 and `Npgsql.EntityFrameworkCore.PostgreSQL` 10.

## The problem this solves

Any `Database.Migrate()` / `Database.MigrateAsync()` against YugabyteDB fails immediately after you
upgrade to Entity Framework Core 9 or later:

```
Npgsql.PostgresException: 0A000: ACCESS EXCLUSIVE not supported yet
```

`0A000` is `feature_not_supported`. Entity Framework Core 8 and earlier were fine.

**Why.** EF Core 9 added a database lock held across a migration run
(`IHistoryRepository.AcquireDatabaseLock`), so two processes can't apply the same migration
concurrently. The Npgsql provider implements it as a table lock:

```sql
LOCK TABLE "__EFMigrationsHistory" IN ACCESS EXCLUSIVE MODE
```

YugabyteDB doesn't implement explicit table locks and rejects the statement outright. There is no
EF Core switch to disable the lock, so replacing `IHistoryRepository` is the only intervention point.

## The fix

```csharp
using Neon.EntityFrameworkCore.Npgsql;

services.AddDbContext<MyContext>(options =>
{
    options
        .UseNpgsql(connectionString)
        .UseAdvisoryLockMigrationHistory();     // <-- after UseNpgsql()
});
```

That's it. `AdvisoryLockHistoryRepository` takes a PostgreSQL **session scoped advisory lock**
(`pg_advisory_lock`) instead of locking the table. Everything else about the migrations history
table — its schema, its name, how rows are read and written — is left to the Npgsql provider, so this
doesn't disturb your model or your existing history rows. (`dotnet ef migrations has-pending-model-changes`
reports no drift after applying it.)

### YugabyteDB cluster prerequisite

Advisory locks are **enabled by default from YugabyteDB 2025.0 onward** — nothing to configure.

They arrived in **2.25** as a preview feature that was off by default. On those older releases both
flags are required, on the master and the tserver:

```
--allowed_preview_flags_csv=ysql_yb_enable_advisory_locks
--ysql_yb_enable_advisory_locks=true
```

Without them `pg_advisory_lock()` **fails** rather than doing nothing, so the symptom changes
from _"ACCESS EXCLUSIVE not supported yet"_ to _"advisory locks not yet implemented"_. Check that
before concluding this package is broken.

> YugabyteDB also has a `yb_silence_advisory_locks_not_supported_error` setting that downgrades that
> failure to a silent no-op. **Don't use it here.** It converts a loud misconfiguration into a silent
> loss of mutual exclusion, which is the one outcome this package exists to avoid.

Requires YugabyteDB **2.25 or later**.

### Keep it opt-in

On real PostgreSQL the provider's own `LOCK TABLE` implementation works and is stricter, so leave it
in place there. Don't fold `UseAdvisoryLockMigrationHistory()` into a shared "apply our defaults"
helper — apply it only to contexts that actually target a database lacking explicit table locks.

## Configuration

The defaults mirror the `LOCK TABLE` statement being replaced: a derived lock key, and an indefinite
wait. Override them when you have a reason to.

```csharp
options
    .UseNpgsql(connectionString)
    .UseAdvisoryLockMigrationHistory(lock =>
    {
        lock.Timeout      = TimeSpan.FromMinutes(5);
        lock.PollInterval = TimeSpan.FromSeconds(2);
        lock.LockKey      = 0x4d494752_41544521;
    });
```

| Option | Default | Notes |
| --- | --- | --- |
| `Timeout` | `null` (wait forever) | With a timeout set, `pg_try_advisory_lock()` is polled and `MigrationsLockTimeoutException` is raised when it elapses. |
| `PollInterval` | 1 second | Only used when `Timeout` is set. The final wait is shortened so the total never overshoots `Timeout`. |
| `LockKey` | `null` (derived) | Overrides the key derived from the history table's schema-qualified name. |

**When to set `Timeout`.** `pg_advisory_lock()` blocks in the server indefinitely, which is the
conservative default but a poor experience in CI: a migrator stuck behind a stalled peer looks exactly
like a migrator stuck for any other reason. A timeout turns that into a diagnosable
`MigrationsLockTimeoutException`. In an orchestrated deployment the platform will restart the job
anyway, and by then the other migrator has usually finished and this one finds nothing to do.

> `MigrationsLockTimeoutException` derives from `Exception`, **not** `TimeoutException` — deliberately.
> Npgsql's transient error detector classifies every `TimeoutException` as retryable, so EF Core's
> execution strategy re-threw it as `InvalidOperationException` and left
> `catch (MigrationsLockTimeoutException)` silently dead. Deriving from `Exception` keeps it
> non-transient, so it reaches you intact.

### Large migrations need a raised command timeout

This is about your migration, not about the lock, but it bites people here first: Npgsql's command
timeout defaults to **30 seconds**, and DDL that finishes comfortably on PostgreSQL can take minutes
on YugabyteDB. A large migration will die partway through with an opaque timeout unless you raise it:

```csharp
optionsBuilder.UseNpgsql(connectionString, npgsql => npgsql.CommandTimeout(600));
```

or, per run, `context.Database.SetCommandTimeout(TimeSpan.FromMinutes(10))`.

The lock acquisition itself is **not** subject to that timeout — a blocking wait lifts it for the
`pg_advisory_lock()` statement, so "no `Timeout` configured" really does mean "wait as long as
it takes" rather than "give up after 30 seconds with an error that never mentions locks." Your
migration commands keep whatever timeout you configured.

### With `UseInternalServiceProvider()`

`UseAdvisoryLockMigrationHistory()` installs the repository with EF Core's `ReplaceService()`, and
that isn't available when you supply your own container — EF Core won't modify a service provider it
didn't build. Register the repository yourself and use `ConfigureAdvisoryLockMigrationHistory()` to
carry the options:

```csharp
var serviceProvider = new ServiceCollection()
    .AddEntityFrameworkNpgsql()
    .AddScoped<IHistoryRepository, AdvisoryLockHistoryRepository>()   // must come after the provider
    .BuildServiceProvider();

var options = new DbContextOptionsBuilder<MyContext>()
    .UseNpgsql(connectionString)
    .UseInternalServiceProvider(serviceProvider)
    .ConfigureAdvisoryLockMigrationHistory(lock => lock.Timeout = TimeSpan.FromMinutes(5))
    .Options;
```

Two things to know:

- The `AddScoped()` call has to come **after** `AddEntityFrameworkNpgsql()`, so that it wins over the
  provider's own `IHistoryRepository` registration.
- If you skip `ConfigureAdvisoryLockMigrationHistory()`, the repository still works — it just runs with
  the defaults. Options only reach it through that call, so a `Timeout` you expected to be honoured
  would be silently absent.

Calling `UseAdvisoryLockMigrationHistory()` on a builder that already has an internal service provider
throws with a message pointing here, rather than letting EF Core fail later with an error that doesn't
mention the alternative.

**When to set `LockKey`.** Advisory locks live in a single cluster wide 64-bit key space that isn't
tied to any table, so exclusion depends entirely on independent processes computing the same number.
Set it explicitly to make two contexts with *different* history tables serialize against each other,
or to move off a key that collides with an advisory lock your application already uses. A collision
only ever causes excess serialization, never lost exclusion.

> Whatever key you choose must be stable across processes, machines and releases. **Never derive one
> from `string.GetHashCode()`** — .NET randomizes string hashing per process, so two migrators would
> compute different keys, fail to exclude each other, and never tell you. That's why the built-in
> derivation is a hand rolled FNV-1a hash.

## Design notes

These are the decisions that are easy to get wrong, recorded because getting them wrong fails
*silently*.

**Advisory lock, not a no-op.** A no-op `IMigrationsDatabaseLock` is a two line fix and restores the
EF Core 8 behaviour. But it discards the protection the lock exists for: nothing stops two migrator
pods applying the same migration concurrently. Yugabyte supports advisory locks, so there's no reason
to give that up.

**`pg_advisory_lock`, not `pg_advisory_xact_lock`.** This is the decision worth understanding before
changing anything here.

EF Core runs **one transaction per migration**, and asks the lock to reacquire after each new
transaction begins (`MigrationCommandExecutor` calls
`ReacquireIfNeeded(connectionOpened, transactionRestarted: true)`). A *transaction* scoped lock
therefore dies at every commit and is retaken on the way into the next migration — which works, but
leaves a window after each commit in which nothing holds the lock and a competing migrator could slip
in. A *session* scoped lock ignores commits entirely: one acquisition covers the whole run, which is
what EF Core's "one lock for the migration" model is actually asking for.

Note this is a deliberate **departure from the statement being replaced**. `LOCK TABLE` is transaction
scoped, and the stock `NpgsqlHistoryRepository` reports `LockReleaseBehavior.Transaction` — so that
per-commit window exists on real PostgreSQL too. This package doesn't have it.

Two things follow, both improvements: a migration that suppresses its transaction is still protected
(a transaction scoped lock would have had nothing to live in), and reacquisition is only needed when
the **connection** is replaced, since that's what replaces the session.

**The cost is an explicit release, and it's contained.** `Dispose()` issues `pg_advisory_unlock()`.
That release:

- **never throws.** EF Core disposes the lock in a `finally` while the transaction is still open, and
  if the migration failed that transaction is aborted, where every statement errors with `25P02`.
  Throwing there would replace the migration's real exception with a meaningless one.
- **isn't the only path.** The lock belongs to the session, so it dies with it: a killed process
  releases it when the connection drops (tested), and Npgsql's pool reset runs `DISCARD ALL`, which
  includes `pg_advisory_unlock_all()`.

**The reacquire condition is tied to the scope.** Ours consults `connectionReopened` only, because a
session lock survives commits. **If this is ever changed back to `pg_advisory_xact_lock`, that
condition must regain `|| transactionRestarted == true` in the same commit** — otherwise every
migration after the first runs unprotected, and it would still pass every test that applies a single
migration. `TheLockSurvivesTheCommitBoundary` is the test that catches the mismatch.

### Enable transactional DDL — crash safety depends on it

**Do this before running migrations against YugabyteDB.** Set on every **yb-tserver**:

```
--ysql_yb_ddl_transaction_block_enabled=true
```

YugabyteDB ships this **disabled**, and with it off `CREATE TABLE` survives both an explicit `ROLLBACK`
and a lost connection. A migrator that dies partway through a migration then leaves the schema change
in place while its `__EFMigrationsHistory` row rolls back — so the migration still counts as pending,
the retry re-runs it, and it fails with `42P07 duplicate_table`, needing manual cleanup. Row changes
are transactional either way; it's only schema changes that escape.

With the flag on, an interrupted migration rolls back completely:

| | Outcome after a crash mid-migration |
| --- | --- |
| The interrupted migration's schema changes | Rolled back |
| Its `__EFMigrationsHistory` row | Rolled back — still pending, so it will be retried |
| Migrations that already committed | Kept. EF Core commits each migration separately, so a **run** is never one atomic unit — a crash leaves the database consistent, but at a migration boundary rather than where it started |
| The advisory lock | Released, though not instantly: backend cleanup is asynchronous |

Restarting a crashed migrator is then a complete recovery strategy — no manual intervention.

Two caveats from YugabyteDB's own docs: transactional DDL is an **early access** feature, and
*concurrent* DDL against one database is unsupported and produces conflict errors. That second point is
an argument for this package rather than against it — serializing migrators is exactly how you avoid
concurrent DDL.

`PlatformProbe_DdlIsTransactional` in `Test.Neon.EntityFrameworkCore` asserts this is configured and
fails with the cluster's DDL settings listed if it isn't. It's a separate test from the migration ones
on purpose: they pass either way, and the difference would otherwise only surface as a stranded
migration in production.

### Known limitations

- **Transaction-suppressing migrations get no protection.** With no transaction to own the lock, it
  releases as soon as the acquiring statement autocommits. This isn't a regression: the `LOCK TABLE`
  statement being replaced would have failed outright with `25P01` in the same situation.
- **Crash safety needs `--ysql_yb_ddl_transaction_block_enabled=true`.** Without it YugabyteDB doesn't
  roll DDL back, and an interrupted migration strands a schema change that nothing has recorded. See
  above — this is a platform property, not a property of the lock.
- **A multi-migration run isn't atomic.** A crash leaves the migrations that already committed applied.
  That's EF Core's design on every provider, PostgreSQL included.
- **The key derives from the *configured* schema,** defaulting to `public`. A deployment that relies
  on `search_path` to resolve the history table into another schema will have two sessions serialize
  on the same key while using different tables. That over-serializes (safe) rather than
  under-serializing (not safe). Set `LockKey` if you'd rather they didn't.

### Maintenance

`AdvisoryLockHistoryRepository` derives from `NpgsqlHistoryRepository`, an **internal** provider type
in a `.Internal` namespace (hence the `EF1001` suppression). Its shape isn't covered by semantic
versioning, which couples this package to the Npgsql provider's major version. Two things contain
that cost:

- The provider package is pinned to a single major version in `Directory.Packages.props`.
- `Test.Neon.EntityFrameworkCore` carries guard tests that fail loudly if any of the overridden
  members change shape, so a provider upgrade surfaces as a red test rather than a runtime surprise.

## OpenTelemetry tracing

Lock acquisition is traced, which is how you confirm the mechanism is actually doing its job —
particularly that two migrators computed the *same* key and that one of them queued.

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddNeonEntityFrameworkCore()          // Neon.EntityFrameworkCore
               .AddNeonEntityFrameworkCoreNpgsql()    // this package
               .AddNpgsql()                           // Npgsql client spans (optional)
               .AddOtlpExporter();
    });
```

Each package has its own `ActivitySource`, per the OpenTelemetry convention of one source per
instrumentation library, so you subscribe to the ones you use and each reports its own version.

> Migrations usually run in a short lived job rather than in your service, so remember to configure a
> tracer provider **in the migrator too** — otherwise the spans this exists to give you are never
> collected. Short lived processes should also dispose or flush the provider before exit so the final
> batch is exported.

### Activity emitted

`AcquireDatabaseLock` (`Client`), displayed as `LOCK <history table>`.

| Tag | Value |
| --- | --- |
| `db.system.name` | `postgresql` |
| `db.namespace` | History table schema |
| `db.collection.name` | History table name |
| `db.operation.name` | `LOCK` |
| `neon.efcore.migrations.history_table` | Schema qualified history table |
| `neon.efcore.migrations.lock_key` | The 64-bit advisory lock key |
| `neon.efcore.migrations.lock_mode` | `blocking` or `polling` |
| `neon.efcore.migrations.lock_timeout_seconds` | Configured timeout; absent when waiting indefinitely |
| `neon.efcore.migrations.lock_attempts` | Attempts made; above one means this migrator queued |
| `neon.efcore.migrations.lock_acquired` | Whether the lock was ultimately granted |
| `neon.efcore.migrations.lock_reacquired` | `true` on spans produced by a reacquisition after a commit or reconnect |

Names are exposed as constants on `NpgsqlTraceTags` and, for the convention tags,
`Neon.EntityFrameworkCore.TraceTags`. Failures are recorded with `Activity.AddException()` and the
span status is set to `Error`.

## Verification

`Test.Neon.EntityFrameworkCore` runs migrations against a live YugabyteDB instance (via
`YugabyteFixture`), so the claims above are tested rather than asserted:

- **The problem is real.** Without this package, `MigrateAsync()` fails with `0A000` on current
  Yugabyte — so the passing tests below can't be passing vacuously.
- **Migrations apply**, and re-running is a no-op that records each migration exactly once.
- **A migrator waits for a held lock.** The test takes the lock itself, and a migrator with a
  `Timeout` set fails with `MigrationsLockTimeoutException` rather than proceeding — then succeeds
  once the lock is released, which is what proves the lock caused the failure.
- **It's the derived key that's actually taken.** Holding a *different* key blocks nothing.
- **The lock is released when the run ends** — no `pg_advisory_unlock()` needed, because the
  transaction owns it.
- **The lock survives the commit boundary.** With two pending migrations, a competitor is still
  locked out while the *second* one runs. This is the case that catches a broken reacquire condition;
  every other test here applies migrations from a standing start and would pass without it.
- **An explicit `LockKey` excludes** just as the derived one does.
- **A blocking wait outlasts the command timeout** — with a 3s command timeout configured, a migrator
  waiting on a held lock is still waiting well past it, instead of dying with an opaque
  "Timeout during reading attempt".
- **`UseInternalServiceProvider()` works** via the recipe above, options included — the snippet in this
  README is executed, not just written down.
- **A crash mid-migration rolls back cleanly.** Simulated by killing the migrator's backend with the
  migration parked after its schema change: neither the table nor the history row survives, migrations
  that already committed do, the lock is released when the session dies, and simply re-running finishes
  the job. The transactional-DDL configuration this depends on is pinned by its own test.
- **One acquisition covers the whole run,** and the lock is given back once. Counted from the
  ActivitySource across a two-migration run: exactly one acquire, no reacquisition, one release.
- **The explicit release works,** verified against a connection the test owns rather than a pooled one —
  otherwise `DISCARD ALL` releases the lock on close and the assertion passes whether our release runs
  or not.

Both load-bearing decisions have been checked in the failing direction too: removing the release, and
switching the acquire back to `pg_advisory_xact_lock` while leaving the reacquire condition alone. Each
breaks a test that names the cause.

The concurrency tests contain no timed waits — the migrator is parked on a second advisory lock the
test controls, and every wait is on observable state bounded by a `CancellationTokenSource`. The
commit-boundary test has been checked in both directions: breaking the reacquire condition to
`connectionReopened` alone makes it, and only it, fail.

## Documentation

Full API documentation: [https://sdk.neonforge.com](https://sdk.neonforge.com)

### Further reading

- [YugabyteDB explicit locking](https://docs.yugabyte.com/stable/explore/transactions/explicit-locking/)
- [Enabling advisory locks in YugabyteDB (forum)](https://forum.yugabyte.com/t/enable-advisory-locks/4584)
- [PostgreSQL advisory lock functions](https://www.postgresql.org/docs/current/functions-admin.html#FUNCTIONS-ADVISORY-LOCKS)
- EF Core `Migrator` — lock/transaction ordering: `dotnet/efcore`, `src/EFCore.Relational/Migrations/Internal/Migrator.cs`
- Npgsql history repository — the `LOCK TABLE`: `npgsql/efcore.pg`, `src/EFCore.PG/Migrations/Internal/NpgsqlHistoryRepository.cs`

_Copyright © 2005-2024 by NEONFORGE LLC. All rights reserved._
