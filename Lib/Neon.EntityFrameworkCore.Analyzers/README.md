# Neon.EntityFrameworkCore.Analyzers

Roslyn analyzers for Entity Framework Core queries.

## NEONEFC0001 — EF Core query should be tagged with its call site

EF Core's [`TagWithCallSite()`](https://learn.microsoft.com/ef/core/querying/tags#tagging-with-file-name-and-line-number)
prefixes the generated SQL with the source file and line the query was written on:

```sql
-- file: C:\Work\MyService\UserRepository.cs:42

SELECT u."Id", u."Name" FROM "Users" AS u WHERE u."Active"
```

That comment is what turns a slow query sitting in a database log into a line of code you can
open.  This rule reports EF Core queries that are executed without it.

```csharp
// NEONEFC0001

var users = await context.Users
    .Where(user => user.Active)
    .ToListAsync(cancellationToken);

// OK

var users = await context.Users
    .TagWithCallSite()
    .Where(user => user.Active)
    .ToListAsync(cancellationToken);
```

The tag may appear anywhere in the chain, including on a query built up across several methods,
so factoring a query out into a helper that tags it satisfies the rule at every call site that
composes on top of it.

### Where the rule stays quiet

It only fires where the tag can actually be applied and would actually reach the database:

* The compilation must reference an EF Core that offers `TagWithCallSite()` (EF Core 6.0 or later).
* The query must trace back to a `DbSet<T>` or `DbContext.Set<T>()`.  A query the analyzer cannot
  follow to a root — one handed in as an `IQueryable<T>` parameter, say — is left alone rather
  than guessed at.
* Queries composed inside an expression tree are skipped.  They are translated as part of the
  enclosing query, which is where the tag belongs.
* LINQ over in-memory sequences is not EF Core's business and is never reported.

### Configuration

Add this to `.editorconfig` to accept a `TagWith()` call in place of `TagWithCallSite()`:

```ini
neon_efcore_tag_with_call_site_allow_tag_with = true
```

The rule's severity is set the usual way:

```ini
dotnet_diagnostic.NEONEFC0001.severity = suggestion
```
