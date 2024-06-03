# nanoshell:

Modifying scratch Docker images can be difficult because there's no shell and probably no
common command executables present.  We're going to handle this with a custom standalone
super simple shell that implements a handful of bare bones commands.

**NOTE:** **nanoshell** does not do anything with the **PATH** environment variable or any other
variables either.  This means that all file and directory references need to be absolute.

The **nanoshell** source code is located at `$NF_ROOT\Tools\nanoshell\source\nanoshell.c`
and is built via a build target in the `nanoshell.cs` project file and the compiled stand-alone
binary needs to be be written to: `$NF_ROOT\ToolBin\linux\amd64\nanoshell`

See `nanoshell.c` for more information about the supported commands.

## Publishing Changes;

You'll need to build and publish any changes to **nanoshell** manually by running this script:

```
wsl-util gcc --static "%NF_ROOT%\Tools\nanoshell\source" "%NF_ROOT%\ToolBin\linux\amd64\nanoshell"
```
