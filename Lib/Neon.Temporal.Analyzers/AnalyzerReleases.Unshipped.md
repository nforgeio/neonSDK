; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
NEONTEMP0001 | Usage | Warning | Workflow must not perform I/O
NEONTEMP0002 | Usage | Warning | Workflow must not access external mutable state
NEONTEMP0003 | Usage | Warning | Workflow must not use threading primitives
NEONTEMP0004 | Usage | Warning | Workflow must not use system clock or .NET timers
NEONTEMP0005 | Usage | Warning | Workflow must not use random or nondeterministic identifiers
NEONTEMP0006 | Usage | Warning | Workflow must not use the default task scheduler
NEONTEMP0007 | Usage | Warning | Workflow should use Temporal task coordination APIs
NEONTEMP0008 | Usage | Warning | Workflow must not block on tasks
NEONTEMP0009 | Usage | Warning | Activity async calls should provide a CancellationToken
