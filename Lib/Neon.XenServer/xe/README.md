The code in this folder is lightly modified from:

	https://github.com/xcp-ng/xenadmin/tree/development/xe

The main changes are:

1. Copy the source files from the **Xe** project
2. Copy the source files from the **CommandLib** project
3. Modify **Xe.cs** by having the `Main()` method return an `ExecuteResponse` with the exitcode and output text
4. Change all of the XU related types to **internal**
5. Changed `throw e` to just `throw` in a couple places to ix build warnings
6. Added `#pragma warning disable CS1591` to fix build warnings
