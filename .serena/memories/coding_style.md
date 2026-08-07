# Coding Style

C# style is governed by `.editorconfig` and the local `csharp-patterns` skill.

Key points:
- C# files use CRLF, 4 spaces, explicit usings outside namespaces, block-scoped namespaces, System usings first, no implicit usings.
- New C# files need the repository file header from `.editorconfig`.
- Public types and public members require XML documentation comments with multiline tags.
- Projects use `<Nullable>disable</Nullable>` and `<ImplicitUsings>disable</ImplicitUsings>` or omit them if inherited appropriately.
- Central Package Management is required: add package versions to `Directory.Packages.props`; project files use `<PackageReference Include="..." />` without `Version`.
- Prefer explicit types over `var` except where the existing code clearly uses another pattern.
- Async methods generally require `using Neon.Tasks;` and `await SyncContext.Clear;` as the first statement, unless the code is analyzer/test infrastructure where existing local patterns indicate otherwise.
- Keep one top-level public type per file and name files after the type.

Existing analyzer code lives under `Lib/Neon.Analyzers` using namespace `Neon.Analyzers`, Roslyn `DiagnosticAnalyzer`/`CodeFixProvider`, release tracking markdown files, and netstandard2.0 analyzer projects.