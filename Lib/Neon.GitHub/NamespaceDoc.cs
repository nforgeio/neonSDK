//-----------------------------------------------------------------------------
// FILE:        NamespaceDoc.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Reflection;

using Neon.Common;
using Neon.Deployment;

using Octokit;

namespace Neon.GitHub
{
    /// <summary>
    /// Combines <b>git</b> and <b>GitHub</b> functionality into the easy-to-use <see cref="GitHubRepo"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class wraps the <b>Octokit</b> and <b>LibGit2Sharp</b> packages, adding methods for common scenerios.
    /// The problem is that also these packages are nice, it's not always obvious how to perform many
    /// operations.  This package addresses some of these issues via the `GitGubRepo` class.
    /// </para>
    /// <para>
    /// For example, <b>LibGit2Sharp</b> doesn't provide methods for operations like: <b>Fetch</b>, <b>Push</b>,
    /// <b>Remove Branch</b>, or <b>Undo</b>.  <b>Octokit</b> doesn't have methods for <b>Remove Branch</b>
    /// and also seems to be missing direct support for other common operations and some of the 
    /// methods it does have, complete asynchronously which makes it harder to script a series of
    /// GitHub related operations.
    /// </para>
    /// <para>
    /// Another annoyance with **Octokit** and **LibGit2Sharp** is that the define a lot of types with
    /// the same names, like **Repository**.  This makes it more difficult to write code that uses
    /// these libraries by forcing developers to use fully qualified type names or redefine type
    /// names via **using statements**.  **GitHubRepo** helps avoid these conflicts for many scenarios.
    /// </para>
    /// </remarks>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
}
