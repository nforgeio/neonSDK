---
name: csharp-patterns
description: >
  Enforces team C# and .NET coding standards when writing or reviewing code.
  Use this skill whenever working with C# files (.cs), .NET projects (.csproj, .sln),
  or when the user asks you to write, generate, review, or refactor C# code.
  Also trigger when the user mentions async methods, tracing, OpenTelemetry,
  Central Package Management, data annotations, model validation, XML doc comments,
  .editorconfig, file headers, nullable reference types, implicit usings,
  or .NET services/libraries in a C# context.
  If you see a .cs file or a .csproj, consult this skill.
---

# C# Patterns

This skill defines the team's mandatory C# and .NET patterns. Follow these when
writing new code and flag violations when reviewing existing code.

There are eight core patterns. Each is explained below with the reasoning behind it,
so you can apply them thoughtfully rather than mechanically.

---

## 1. Async Methods: Neon.Tasks and SyncContext.Clear

Every `async` method must use `Neon.Tasks` (from the `Neon.Common` NuGet package)
and begin with `await SyncContext.Clear` as its very first statement.

**Why:** This clears the synchronization context at the top of every async call chain,
preventing deadlocks and ensuring continuations don't unnecessarily marshal back to a
captured context (e.g., a UI thread or ASP.NET request context). It's a lightweight
safeguard that avoids an entire class of subtle async bugs.

**The pattern:**

```csharp
using Neon.Tasks;

public async Task DoSomethingAsync(string input)
{
    await SyncContext.Clear;

    // ... rest of method
}
```

**When writing code:** Add `using Neon.Tasks;` to the file and put `await SyncContext.Clear;`
as the first line inside every async method body.

**When reviewing code:** Flag any async method that is missing `await SyncContext.Clear;`
at the top. Also flag if the `Neon.Tasks` using directive is absent.

---

## 2. TraceContext for OpenTelemetry

Every service and library should include a `TraceContext` class that provides a
lazy-initialized `ActivitySource` tied to the assembly's identity. This is the
foundation for distributed tracing via OpenTelemetry.

**Why:** Centralizing the `ActivitySource` in a `TraceContext` class means every
component in the assembly shares a single, consistently-named source. The lazy
initialization avoids overhead when tracing isn't active, and using the assembly
name/version ensures traces are automatically tagged with the right service identity.

**The pattern:**

```csharp
using System;
using System.Diagnostics;
using System.Reflection;

namespace YourNamespace
{
    internal static class TraceContext
    {
        internal static readonly AssemblyName AssemblyName = typeof(TraceContext).Assembly.GetName();

        internal static readonly string ActivitySourceName = AssemblyName.Name;

        internal static readonly Version Version = AssemblyName.Version;

        internal static ActivitySource ActivitySource => Cached.Source.Value;

        static class Cached
        {
            internal static readonly Lazy<ActivitySource> Source = new Lazy<ActivitySource>(
                () => new ActivitySource(ActivitySourceName, Version.ToString()));
        }
    }
}
```

**When writing a new service or library:** Create a `TraceContext.cs` file in the
project's root namespace. Replace `YourNamespace` with the actual project namespace.

**When reviewing code:** Check that services and libraries have a `TraceContext` class.
Flag any that create `ActivitySource` instances ad-hoc instead of going through
`TraceContext`.

---

## 3. OpenTelemetry Instrumentation

When adding tracing to methods, use the `TraceContext.ActivitySource` to create
activities (spans). For detailed guidance on instrumentation patterns, conventions,
and best practices, consult the **dotnet-skills/opentelemetry-dotnet-instrumentation**
skill.

**Basic usage:**

```csharp
public async Task ProcessOrderAsync(Order order)
{
    await SyncContext.Clear;

    using var activity = TraceContext.ActivitySource.StartActivity("ProcessOrder");
    activity?.SetTag("order.id", order.Id);

    // ... implementation
}
```

Refer to `dotnet-skills/opentelemetry-dotnet-instrumentation` for the full set of
instrumentation patterns including span naming conventions, error handling, event
logging, and propagation.

---

## 4. Central Package Management

All .NET solutions use Central Package Management (CPM). This means:

- Package versions are declared in `Directory.Packages.props` at the solution root.
- Individual `.csproj` files reference packages with `<PackageReference>` but
  **do not specify a `Version` attribute** — the version comes from
  `Directory.Packages.props`.

**Why:** CPM prevents version drift across projects in a solution. When every project
pulls from a single version manifest, you avoid subtle bugs from mismatched
dependency versions and make updates a one-line change.

**Example `Directory.Packages.props`:**

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="Neon.Common" Version="4.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Logging" Version="8.0.0" />
  </ItemGroup>
</Project>
```

**Example `.csproj` reference (no version):**

```xml
<PackageReference Include="Neon.Common" />
```

**When writing code:** Never put `Version=` on a `<PackageReference>`. If you need
to add a new package, add the version to `Directory.Packages.props` and only the
package name to the `.csproj`.

**When reviewing code:** Flag any `<PackageReference>` that includes a `Version`
attribute. Also check that `Directory.Packages.props` exists and has
`ManagePackageVersionsCentrally` set to `true`.

---

## 5. Data Annotations

Use data annotations to declare validation rules on models — especially models
exposed via APIs. Place each annotation on its own line above the property, never
inline with the property declaration.

**Why:** Annotations catch invalid data at the boundary before it reaches business
logic, and they give API consumers clear, structured feedback about what's wrong.
Putting each annotation on its own line makes validation rules scannable at a
glance and keeps diffs clean when rules are added or removed.

**The pattern:**

```csharp
public class UserModel
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    [JsonPropertyName("username")]
    public string Username { get; set; }
}
```

**When writing code:** Add the relevant `System.ComponentModel.DataAnnotations`
attributes to properties on any model that crosses an API boundary or is bound
from external input. Put each attribute on its own line directly above the
property.

**When reviewing code:** Flag API-facing models whose properties lack validation
annotations where they'd be appropriate (required fields, length limits, ranges,
regex formats). Also flag any annotations stacked inline on the same line as the
property declaration — they should be split onto their own lines.

---

## 6. Comments

Comments should explain the **why** behind non-obvious logic, not the **what**.
Well-named code conveys the what on its own. In addition, every public type and
public member must have an XML documentation comment (`/// <summary>`).

**Why:** "What" comments restate the code and go stale the moment the code
changes; "why" comments capture intent, constraints, and context that aren't
recoverable from reading the code. Required XML docs on public surfaces give
consumers — including IDE tooling and generated API docs — a reliable
description of each member's contract.

**The pattern:**

```csharp
/// <summary>
/// Processes orders after validating inventory.
/// </summary>
public class OrderProcessor
{
    /// <summary>
    /// Processes the given order. Validates inventory before proceeding to avoid overselling.
    /// </summary>
    /// <param name="order">The order to process.</param>
    /// <returns>True if the order was processed successfully; otherwise, false.</returns>
    public bool Process(Order order)
    {
        // Check inventory up front: a later failure would leave the order in a
        // half-processed state that's expensive to reconcile.
        if (!InventoryService.IsInStock(order.ItemId, order.Quantity))
        {
            throw new OutOfStockException(order.ItemId);
        }

        // Proceed with order processing...
        return true;
    }
}
```

XML doc tags must be written across multiple lines — opening tag, content, and
closing tag each on their own line. Do not collapse a tag onto a single line,
even for short content.

**Correct:**

```csharp
/// <summary>
/// Example summary comment.
/// </summary>
```

**Incorrect:**

```csharp
/// <summary>Example summary comment.</summary>
```

**When writing code:** Add `/// <summary>` (plus `<param>`, `<returns>`, and
`<exception>` where relevant) to every public type and public member, with each
tag written across multiple lines as shown above. Use inline `//` comments
sparingly and only to explain reasoning — tradeoffs, invariants, references to
tickets or specs — not to narrate what the next line does.

**When reviewing code:** Flag public types or public members that are missing
XML documentation. Flag any XML doc tag collapsed onto a single line; it must
be split across opening tag, content, and closing tag. Flag inline comments
that merely restate the code (e.g., `// increment counter` above `counter++`);
suggest removing them or rewriting them to capture the reasoning. Flag stale
comments that no longer match the code they describe.

---

## 7. File Organization and .editorconfig

Each class goes in its own file, and all new code must conform to the rules in
the solution's `.editorconfig` at the project root — including any file header
comment it prescribes.

**Why:** One-class-per-file keeps navigation predictable (the file name matches
the type), makes diffs smaller and easier to review, and avoids merge conflicts
when unrelated types change together. `.editorconfig` is the single source of
truth for formatting, naming, and analyzer rules across the solution; deferring
to it means every contributor produces consistent code without negotiating
style per file. Header comments declared there (license notices, copyright,
SPDX tags) exist for legal or provenance reasons and must appear on every file.

**The pattern:**

- One public type per `.cs` file. The file name matches the type name exactly
  (e.g., `OrderProcessor.cs` contains `class OrderProcessor`).
- Nested types are allowed inside their containing type's file, but sibling
  top-level types should be split out.
- Before writing code, check for a `.editorconfig` at the project or solution
  root and apply its rules: indentation, `using` placement and ordering,
  brace style, naming conventions, `var` preferences, and any analyzer
  severities it configures.
- If `.editorconfig` specifies a file header (often via the
  `file_header_template` key), every new file must start with that exact
  header before any `using` directives or namespace declarations.

**When writing code:** When creating a new class, create a new `.cs` file named
after the class rather than appending to an existing file. Open the solution's
`.editorconfig` first and conform to its settings; if it declares a file
header, paste it at the top of every new file you create. If there is no
`.editorconfig`, mention this to the user rather than guessing at the team's
conventions.

**When reviewing code:** Flag `.cs` files containing more than one top-level
type (unless the types are intentionally nested). Flag files whose name doesn't
match the contained type. Flag new files missing the header required by
`.editorconfig`, and flag any code that conflicts with documented
`.editorconfig` rules (indentation, naming, `using` ordering, etc.).

---

## 8. Project Settings: Nullable and ImplicitUsings Disabled

Projects must not enable `Nullable` reference types or `ImplicitUsings`. Both
are off — either omitted from the `.csproj` (the SDK default is `disable` for
older target frameworks) or explicitly set to `disable`.

**Why:** Disabling `Nullable` keeps the codebase consistent with existing
patterns — enabling it partway through produces a flood of warnings in
untouched code and pressures contributors to annotate types they don't
own. Disabling `ImplicitUsings` keeps every file's dependencies explicit: a
reader can see exactly which namespaces a file relies on without consulting an
SDK-generated global `using` list, which makes refactors and namespace moves
safer.

**The pattern:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

**When writing code:** When creating a new `.csproj`, either omit the
`Nullable` and `ImplicitUsings` properties or set them to `disable`. Do not
write nullable-annotated signatures (`string?`, `int?` on reference-like
generics, `#nullable enable` pragmas) in source. Add every required `using`
directive explicitly at the top of each `.cs` file rather than relying on
implicit imports.

**When reviewing code:** Flag any `.csproj` that sets `<Nullable>enable</Nullable>`
or `<ImplicitUsings>enable</ImplicitUsings>`. Flag `#nullable enable` pragmas
in `.cs` files. Flag source files that compile only because of implicit
usings — every namespace the file depends on should be in an explicit `using`
directive at the top.

---

## Review Checklist

When reviewing C# code, check for these violations and report them clearly:

1. Async method missing `await SyncContext.Clear;` as first statement
2. Missing `using Neon.Tasks;` in files with async methods
3. Service or library missing `TraceContext.cs`
4. Ad-hoc `ActivitySource` creation instead of using `TraceContext.ActivitySource`
5. `<PackageReference>` with a `Version` attribute in a `.csproj` file
6. Missing or misconfigured `Directory.Packages.props`
7. API-facing model properties missing appropriate data annotations, or annotations stacked inline with the property declaration instead of on their own lines
8. Public type or public member missing an XML documentation comment (`/// <summary>`)
9. XML doc tag collapsed onto a single line instead of split across opening tag, content, and closing tag
10. Inline comments that restate what the code does instead of explaining why, or comments that have gone stale relative to the code
11. `.cs` file containing more than one top-level type, or a file whose name doesn't match the contained type
12. New file missing the header required by `.editorconfig`, or code that conflicts with documented `.editorconfig` rules
13. `.csproj` with `<Nullable>enable</Nullable>` or `<ImplicitUsings>enable</ImplicitUsings>`, or source files using `#nullable enable` pragmas
14. Source file relying on implicit usings instead of explicit `using` directives at the top
