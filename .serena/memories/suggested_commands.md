# Suggested Commands

Windows/PowerShell-oriented repository commands:

- List files quickly: `rg --files`
- Search source: `rg "pattern"`
- Build a project: `dotnet build <path-to-csproj>`
- Run a specific test project: `dotnet test <path-to-test-csproj>`
- Build the solution: `dotnet build neonSDK.slnx`
- Run all solution tests when appropriate: `dotnet test neonSDK.slnx`
- Check git state: `git status --short`
- Show changed files: `git diff --name-only`

For analyzer work:
- Build analyzer project: `dotnet build Lib/<AnalyzerProject>/<AnalyzerProject>.csproj`
- Run matching tests: `dotnet test Test/<TestAnalyzerProject>/<TestAnalyzerProject>.csproj`

Because this repository can be large, prefer project-specific builds/tests for routine validation, then broaden only when the change has wide impact.