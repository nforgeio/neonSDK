//-----------------------------------------------------------------------------
// FILE:	    NamespaceDoc.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
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

namespace Neon.Git
{
    /// <summary>
    /// <para>
    /// Combines <b>git</b> and <b>GitHub</b> functionality into easy to use classes that behave
    /// similarily to Visual Studio's embedded git provider.
    /// </para>
    /// <note>
    /// These classes are not entirely general purpose at this point.  NEONFORGE maintainers are
    /// currently using this for CI/CD for opensource and private NEONFORGE related projects.
    /// We may make these more generic in the future.
    /// </note>
    /// <note>
    /// Note that this replicates some of the functionality provided by <b>Neon.Deployment></b>.
    /// Perhaps someday we'll have <b>Neon.Deployment</b> pick up these capabilities from this 
    /// library or potentially combine the two libraries.
    /// </note>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The basic problem here is that the <b>GitHub OctoKit</b> and <b>LibGit2Sharp</b> packages 
    /// don't really play that well together.  One problem is that quite a few types share the 
    /// same names in both packages, leading to ambiguous symbol references when trying to use
    /// both packages in the same program.
    /// </para>
    /// <para>
    /// Another problem is that you'll need to use GitHub credentials to do anything interesting
    /// (like pushing a local repo to GitHub).  The Visual Studio git provider manages these credentials
    /// for the developer, but there isn't really a standard for managing these credentials outside
    /// of Visual Studio.
    /// </para>
    /// <para>
    /// Finally, although <b>LibeGit2Sharp</b> is very powerful, it can be hard to understand and use,
    /// especially for common basic operations.  This library includes very easy-to-use methods implementing
    /// some of these operations.
    /// </para>
    /// <para>
    /// Our approach here is to using a combination of environment variables like <b>GITHUB_USERNAME</b>,
    /// <b>GITHUB_PAT</b>, and <b>GITHUB_EMAIL</b> hold the credentials or alternatively, a secret
    /// provider like <b>1Password</b> via <see cref="IProfileClient"/>.  The environment variables
    /// take precedence if they exist.
    /// </para>
    /// <note>
    /// We have an internal implementation of <see cref="IProfileClient"/> that can retrieve secrets
    /// from <b>1Password</b> for NEONFORGE maintainers, but this is not generally available at this
    /// time and may never be.
    /// </note>
    /// <note>
    /// This functionality is currently <b>tied to GitHub</b> because that's where we're hosting our
    /// projects.  We don't currently support other providers like GitLabs, etc. and may never do so.
    /// </note>
    /// <pare><b>USAGE:</b></pare>
    /// <para>
    /// Use <see cref="GitHubHelper.CreateGitHubClient(string, string, string, string)"/> to create a 
    /// <see cref="GitHubClient"/> instance.  You may explicitly pass your GitHub credentials or rely
    /// on the method to locate these as environment variables or secrets.
    /// </para>
    /// <para>
    /// Then, you can use the client to query GitHub or the <see cref="GitHubExtensions"/> methods
    /// extending <see cref="GitHubClient"/> with new operations.
    /// </para>
    /// </remarks>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
}
