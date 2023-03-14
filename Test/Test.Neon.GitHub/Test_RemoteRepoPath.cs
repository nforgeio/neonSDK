//-----------------------------------------------------------------------------
// FILE:        Test_GitHubRepo.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using LibGit2Sharp;

using Neon.Common;
using Neon.GitHub;
using Neon.IO;
using Neon.Xunit;

using Xunit;

namespace TestGitHub
{
    [Trait(TestTrait.Category, TestArea.NeonGit)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public class Test_RemoteRepoPath
    {
        [Fact]
        public void Parse()
        {
            var path = RemoteRepoPath.Parse("foo.com/owner/repo");

            Assert.Equal("foo.com", path.Server);
            Assert.Equal("owner", path.Owner);
            Assert.Equal("repo", path.Name);

            path = RemoteRepoPath.Parse("owner/repo");

            Assert.Equal("github.com", path.Server);
            Assert.Equal("owner", path.Owner);
            Assert.Equal("repo", path.Name);
        }

        [Fact]
        public void Parse_Errors()
        {
            Assert.Throws<ArgumentNullException>(() => RemoteRepoPath.Parse(null));
            Assert.Throws<ArgumentNullException>(() => RemoteRepoPath.Parse(""));
            Assert.Throws<FormatException>(() => RemoteRepoPath.Parse("foo.com"));
            Assert.Throws<FormatException>(() => RemoteRepoPath.Parse("foo.com//repo"));
            Assert.Throws<FormatException>(() => RemoteRepoPath.Parse("foo.com/owner/repo/garbage"));
            Assert.Throws<FormatException>(() => RemoteRepoPath.Parse("foo.com/owner/repo/garbage"));
        }

        [Fact]
        public void Equals_Method()
        {
            var path1a = RemoteRepoPath.Parse("github.com/owner1/repo1");
            var path1b = RemoteRepoPath.Parse("github.com/owner1/repo1");

            Assert.False(path1a.Equals(null));
            Assert.False(path1a.Equals("not a path"));
            Assert.True(path1a.Equals(path1a));
            Assert.True(path1a.Equals(path1b));

            Assert.False(path1a.Equals(RemoteRepoPath.Parse("foo.com/owner1/repo1")));
            Assert.False(path1a.Equals(RemoteRepoPath.Parse("github.com/owner2/repo1")));
            Assert.False(path1a.Equals(RemoteRepoPath.Parse("github.com/owner1/repo2")));
        }

        [Fact]
        public void Equals_Operator()
        {
            var path1a = RemoteRepoPath.Parse("github.com/owner1/repo1");
            var path1b = RemoteRepoPath.Parse("github.com/owner1/repo1");

            Assert.False(path1a == null);
            Assert.False(null == path1b);
            Assert.True(path1a == path1b);

            Assert.False(path1a == RemoteRepoPath.Parse("foo.com/owner1/repo1"));
            Assert.False(path1a == RemoteRepoPath.Parse("github.com/owner2/repo1"));
            Assert.False(path1a == RemoteRepoPath.Parse("github.com/owner1/repo2"));
        }

        [Fact]
        public void NotEquals_Operator()
        {
            var path1a = RemoteRepoPath.Parse("github.com/owner1/repo1");
            var path1b = RemoteRepoPath.Parse("github.com/owner1/repo1");

            Assert.True(path1a != null);
            Assert.True(null != path1b);
            Assert.False(path1a != path1b);

            Assert.True(path1a != RemoteRepoPath.Parse("foo.com/owner1/repo1"));
            Assert.True(path1a != RemoteRepoPath.Parse("github.com/owner2/repo1"));
            Assert.True(path1a != RemoteRepoPath.Parse("github.com/owner1/repo2"));
        }

        [Fact]
        public void GetHashCode_Method()
        {
            var path1a = RemoteRepoPath.Parse("github.com/owner1/repo1");
            var path1b = RemoteRepoPath.Parse("github.com/owner1/repo1");

            Assert.Equal(path1a.GetHashCode(), path1b.GetHashCode());
            Assert.NotEqual(path1a.GetHashCode(), RemoteRepoPath.Parse("foo.com/owner1/repo1").GetHashCode());
        }
    }
}
