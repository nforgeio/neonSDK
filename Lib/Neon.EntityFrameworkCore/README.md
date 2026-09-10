Neon.EntityFrameworkCore
========================

Provider agnostic Entity Framework Core extensions, instrumented with OpenTelemetry.

You can get started here: [Neon.EntityFrameworkCore](https://sdk.neonforge.com/N_Neon_EntityFrameworkCore.htm)

```bash
dotnet add package Neon.EntityFrameworkCore
```

Targets `net10.0` and Entity Framework Core 10.

## What's in here

| API | Purpose |
| --- | --- |
| `DbSet<T>.ExistsAsync(...)` | Existence check by primary key, without materializing or tracking the entity. |
| `DbSet<T>.DeleteAsync(...)` | Delete by primary key, without loading the entity first. |
| `DbSet<T>.UpsertAsync(...)` | Stage an insert or an update depending on whether the key is already present. |
| `ModelBuilder.SetDefaultDateTimeKind(...)` | Make every `DateTime` property round trip with a known `DateTimeKind`. |
| `TracerProviderBuilder.AddNeonEntityFrameworkCore()` | Collect this library's OpenTelemetry activities. |

Looking for the YugabyteDB migration locking fix? That lives in
[Neon.EntityFrameworkCore.Npgsql](https://github.com/nforgeio/neonSDK/blob/master/Lib/Neon.EntityFrameworkCore.Npgsql/README.md),
so that provider agnostic consumers of this package don't inherit a dependency on Npgsql.

## Key based `DbSet` operations

`FindAsync()` is the usual way to reach an entity by its primary key, but it materializes and tracks
the entity — wasted work when all you wanted to know was whether a row exists, or you simply want it
gone.

```csharp
using Neon.EntityFrameworkCore;

// SELECT EXISTS (SELECT 1 FROM users WHERE id = @p0) — nothing enters the change tracker.

if (await context.Users.ExistsAsync(userId))
{
    // DELETE FROM users WHERE id = @p0 — sent immediately, no round trip to load it first.

    await context.Users.DeleteAsync(userId);
}

// Stages an INSERT or an UPDATE.  You still call SaveChangesAsync().

await context.Users.UpsertAsync(user, cancellationToken);
await context.SaveChangesAsync(cancellationToken);
```

All three work with composite keys — supply the values positionally, in the order the key properties
are declared:

```csharp
await context.Memberships.ExistsAsync(new object[] { tenantId, userId }, cancellationToken);
```

Each method has a `params object[]` overload for the common single column case, and an
`(object[], CancellationToken)` overload for when you need cancellation.

Three things worth knowing before you use them:

- **`ExistsAsync()` always hits the database.** Unlike `FindAsync()`, it won't report `true` for an
  entity that has been added to the change tracker but not yet saved.
- **`DeleteAsync()` bypasses the change tracker.** It's built on `ExecuteDeleteAsync()`, so already
  loaded copies of the entity are left stale, and cascade behaviours configured in your *model* are
  not applied — the *database's* foreign key actions are what take effect.
- **`UpsertAsync()` is not atomic on its own.** The existence check and the write are separate round
  trips, so a concurrent insert of the same key in between turns the staged insert into a duplicate
  key violation at `SaveChangesAsync()` time. Where that matters, use a serializable transaction or
  your provider's native merge (`INSERT ... ON CONFLICT` on PostgreSQL).

## `DateTime` kind handling

Many providers discard `DateTime.Kind` on the way in and hand back `DateTimeKind.Unspecified` on the
way out, which makes it easy to end up with values whose kind depends on whether they came from the
database or from memory. One call at the end of `OnModelCreating()` removes the whole class of bug:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // ...configure entities...

    modelBuilder.SetDefaultDateTimeKind(DateTimeKind.Utc);
}
```

Writes apply `ToUniversalTime()`; reads apply `DateTime.SpecifyKind(value, kind)`.

- **Call it last.** Properties discovered after the call don't get a converter.
- **Pass `Utc`.** Because unspecified values are treated as local on write, the round trip is only
  lossless for `DateTimeKind.Utc`. `Local` relabels UTC values without shifting them, corrupting
  them by the server's UTC offset; `Unspecified` is a no-op.
- **On Npgsql, keep the default `timestamp with time zone` mapping.** Npgsql accepts only `Kind=Utc`
  for `timestamptz` and only `Kind=Unspecified` for `timestamp without time zone`, so pointing this
  at a naive column produces a write the driver refuses. Against `timestamptz` the read side is a
  no-op — Npgsql already returns UTC — and all the value is on the write side: a local or unspecified
  `DateTime` that would otherwise blow up at `SaveChanges()` gets normalized into the UTC instant it
  meant.

## OpenTelemetry tracing

Every public method emits an activity from a single `ActivitySource` named after the assembly.
Nothing is collected until you subscribe:

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddNeonEntityFrameworkCore()
               .AddOtlpExporter();
    });
```

`AddNeonEntityFrameworkCore()` subscribes to *this library's* activities only. For a complete picture
you'll usually want EF Core's own instrumentation and your driver's alongside it:

```csharp
tracing.AddNeonEntityFrameworkCore()
       .AddEntityFrameworkCoreInstrumentation()   // OpenTelemetry.Instrumentation.EntityFrameworkCore
       .AddNpgsql()                               // Npgsql.OpenTelemetry
       .AddOtlpExporter();
```

If you'd rather not reference the extension method, `TracerProviderBuilderExtensions.ActivitySourceName`
is the string to pass to `AddSource()`.

The cost when nothing is listening is one null check per call: no `Activity` is allocated and none of
the tag values are computed. It's safe to leave this package referenced in projects that don't use
OpenTelemetry at all.

### Activities emitted

| Activity | Kind | Display name |
| --- | --- | --- |
| `ExistsAsync` | `Client` | `EXISTS <table>` |
| `DeleteAsync` | `Client` | `DELETE <table>` |
| `UpsertAsync` | `Client` | `UPSERT <table>` |
| `SetDefaultDateTimeKind` | `Internal` | `SetDefaultDateTimeKind` |

### Tags

Names are exposed as constants on `TraceTags`. Where an OpenTelemetry semantic convention exists it's
used verbatim; everything else is namespaced under `neon.efcore.*`.

| Tag | On | Value |
| --- | --- | --- |
| `db.operation.name` | key based ops | `EXISTS`, `DELETE` or `UPSERT` |
| `db.collection.name` | key based ops | The mapped table name |
| `db.namespace` | key based ops | The mapped schema, when the entity has one |
| `neon.efcore.entity.type` | key based ops | Full CLR type name of the entity |
| `neon.efcore.key.count` | key based ops | Number of key values supplied |
| `neon.efcore.result.exists` | `ExistsAsync` | The result |
| `neon.efcore.result.rows_affected` | `DeleteAsync` | Rows the `DELETE` affected |
| `neon.efcore.upsert.action` | `UpsertAsync` | `insert` or `update` |
| `neon.efcore.datetime.kind` | `SetDefaultDateTimeKind` | The `DateTimeKind` applied |
| `neon.efcore.model.properties_converted` | `SetDefaultDateTimeKind` | How many properties got a converter |

Failures are recorded with `Activity.AddException()` and the span status is set to `Error`, so both
exception-event and error-rate views in your backend light up.

> **Primary key values are never recorded** — only their count. Keys are so often personal data, and
> traces are so routinely exported to third party backends, that capturing them would be a liability
> rather than a feature. If you need to correlate a span with a specific row, add the tag yourself at
> the call site where you can make that judgement.

## Documentation

Full API documentation: [https://sdk.neonforge.com](https://sdk.neonforge.com)

_Copyright © 2005-2024 by NEONFORGE LLC. All rights reserved._
