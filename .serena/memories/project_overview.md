# neonSDK Overview

neonSDK is an Apache-2.0 open source repository of NEONFORGE .NET libraries, tools, services, Docker images, and tests published primarily as NuGet packages. The root solution is `neonSDK.slnx`.

Top-level structure:
- `Lib/`: production libraries and analyzer packages, including `Neon.Analyzers`, `Neon.Roslyn`, and `Neon.Roslyn.Xunit`.
- `Test/`: xUnit test projects, generally named `Test.<LibraryName>`.
- `Tools/`: CLI/tooling projects.
- `Services/`: service/sample projects.
- `Images/`: Docker/image-related projects and assets.
- `Doc/`: developer and docs files.
- `Powershell/` and `ToolBin/`: repository scripts and command wrappers.

The repo uses Central Package Management via `Directory.Packages.props`, shared MSBuild settings via `Directory.Build.props`, and a root `.editorconfig`.