Neon.Git
========

Combines local **git** and remote **GitHub** functionality into the easy-to-use **GitHubRepo** class.

This class wraps the **Octokit** and **LibGit2Sharp** packages, adding methods for common scenerios.
The problem is that also these packages are nice, it's not always obvious how to perform many
operations.  This package addresses some of these issues via the `GitGubRepo` class.

For example, **LibGit2Sharp** doesn't provide methods for operations like: **Fetch**, **Push**,
**Remove Branch**, or **Undo**.  **Octokit** doesn't have methods for **Remove Branch**
and also seems to be missing direct support for common operations and some of the 
methods it does have, complete asynchronously which makes it harder to script a series of
GitHub related operations.

Another annoyance with **Octokit** and **LibGit2Sharp** is that the define a lot of types with
the same names, like **Repository**.  This makes it more difficult to write code that uses
these libraries by forcing developers to use fully qualified type names or redefine type names 
via **using statements**.  **GitHubRepo** helps avoid these conflicts for many scenarios.

You can get started here: [Neon.Git](https://sdk.neonforge.com/N_Neon_Git.htm)
