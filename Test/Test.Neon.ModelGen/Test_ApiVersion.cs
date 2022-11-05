//-----------------------------------------------------------------------------
// FILE:	    Test_ApiVersion.cs
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
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;
using Neon.CSharp;
using Neon.ModelGen;
using Neon.Xunit;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;

namespace TestModelGen
{
    public class Test_ApiVersion
    {
        [Fact]
        public void ParseVersionGroup()
        {
            // Verifies parsing of: <Version Group>[<Major>[.Minor]][-Status]

            var versionGroup = new DateTime(2022, 9, 11).ToUniversalTime();
            var apiVersion   = ApiVersion.Parse("2022-09-11");

            Assert.Equal(versionGroup, apiVersion.VersionGroup);
            Assert.Equal(-1, apiVersion.Major);
            Assert.Equal(-1, apiVersion.Minor);
            Assert.Empty(apiVersion.Status);

            apiVersion = ApiVersion.Parse("2022-09-11-alpha");

            Assert.Equal(versionGroup, apiVersion.VersionGroup);
            Assert.Equal(-1, apiVersion.Major);
            Assert.Equal(-1, apiVersion.Minor);
            Assert.Equal("alpha", apiVersion.Status);

            apiVersion = ApiVersion.Parse("2022-09-112.3");

            Assert.Equal(versionGroup, apiVersion.VersionGroup);
            Assert.Equal(2, apiVersion.Major);
            Assert.Equal(3, apiVersion.Minor);
            Assert.Empty(apiVersion.Status);

            apiVersion = ApiVersion.Parse("2022-09-112.3-alpha");

            Assert.Equal(versionGroup, apiVersion.VersionGroup);
            Assert.Equal(2, apiVersion.Major);
            Assert.Equal(3, apiVersion.Minor);
            Assert.Equal("alpha", apiVersion.Status);

            apiVersion = ApiVersion.Parse("2022-09-112");

            Assert.Equal(versionGroup, apiVersion.VersionGroup);
            Assert.Equal(2, apiVersion.Major);
            Assert.Equal(-1, apiVersion.Minor);
            Assert.Empty(apiVersion.Status);

            apiVersion = ApiVersion.Parse("2022-09-112-alpha");

            Assert.Equal(versionGroup, apiVersion.VersionGroup);
            Assert.Equal(2, apiVersion.Major);
            Assert.Equal(-1, apiVersion.Minor);
            Assert.Equal("alpha", apiVersion.Status);

            apiVersion = ApiVersion.Parse("2022-09-1123.45");

            Assert.Equal(versionGroup, apiVersion.VersionGroup);
            Assert.Equal(23, apiVersion.Major);
            Assert.Equal(45, apiVersion.Minor);
            Assert.Empty(apiVersion.Status);

            apiVersion = ApiVersion.Parse("2022-09-1123.45-alpha");

            Assert.Equal(versionGroup, apiVersion.VersionGroup);
            Assert.Equal(23, apiVersion.Major);
            Assert.Equal(45, apiVersion.Minor);
            Assert.Equal("alpha", apiVersion.Status);

            apiVersion = ApiVersion.Parse("2022-09-1123.45-alpha.0-1");

            Assert.Equal(versionGroup, apiVersion.VersionGroup);
            Assert.Equal(23, apiVersion.Major);
            Assert.Equal(45, apiVersion.Minor);
            Assert.Equal("alpha.0-1", apiVersion.Status);
        }

        [Fact]
        public void ParseOptionalVersionGroup()
        {
            // Verifies parsing of: [<Version Group>.]<Major>.<Minor>[-Status]

            var versionGroup   = new DateTime(2022, 9, 11).ToUniversalTime();
            var noVersionGroup = new DateTime(1, 1, 1).ToUniversalTime();
            var apiVersion     = ApiVersion.Parse("2022-09-11.2.3");

            Assert.Equal(versionGroup, apiVersion.VersionGroup);
            Assert.Equal(2, apiVersion.Major);
            Assert.Equal(3, apiVersion.Minor);
            Assert.Empty(apiVersion.Status);

            apiVersion = ApiVersion.Parse("2022-09-11.23.45");

            Assert.Equal(versionGroup, apiVersion.VersionGroup);
            Assert.Equal(23, apiVersion.Major);
            Assert.Equal(45, apiVersion.Minor);
            Assert.Empty(apiVersion.Status);

            apiVersion = ApiVersion.Parse("2022-09-11.23.45-ALPHA");

            Assert.Equal(versionGroup, apiVersion.VersionGroup);
            Assert.Equal(23, apiVersion.Major);
            Assert.Equal(45, apiVersion.Minor);
            Assert.Equal("ALPHA", apiVersion.Status);

            apiVersion = ApiVersion.Parse("23.45");

            Assert.Equal(noVersionGroup, apiVersion.VersionGroup);
            Assert.Equal(23, apiVersion.Major);
            Assert.Equal(45, apiVersion.Minor);
            Assert.Empty(apiVersion.Status);

            apiVersion = ApiVersion.Parse("23.45-ALPHA");

            Assert.Equal(noVersionGroup, apiVersion.VersionGroup);
            Assert.Equal(23, apiVersion.Major);
            Assert.Equal(45, apiVersion.Minor);
            Assert.Equal("ALPHA", apiVersion.Status);
        }

        [Fact]
        public void Compare()
        {
            Assert.True(ApiVersion.Parse("2022-9-11").CompareTo(ApiVersion.Parse("2022-9-11")) == 0);
            Assert.True(ApiVersion.Parse("2022-9-11.2.3").CompareTo(ApiVersion.Parse("2022-9-11.2.3")) == 0);
            Assert.True(ApiVersion.Parse("2022-9-11.2.3-alpha").CompareTo(ApiVersion.Parse("2022-9-11.2.3-alpha")) == 0);
            Assert.True(ApiVersion.Parse("2022-9-11.2.3-ALPHA").CompareTo(ApiVersion.Parse("2022-9-11.2.3-alpha")) == 0);
            Assert.True(ApiVersion.Parse("2.3").CompareTo(ApiVersion.Parse("2.3")) == 0);
            Assert.True(ApiVersion.Parse("2.3-ALPHA").CompareTo(ApiVersion.Parse("2.3-alpha")) == 0);

            Assert.True(ApiVersion.Parse("2022-9-11").CompareTo(ApiVersion.Parse("2022-9-12")) < 0);
            Assert.True(ApiVersion.Parse("2.3").CompareTo(ApiVersion.Parse("2022-9-12.2.3")) < 0);
            Assert.True(ApiVersion.Parse("2.3").CompareTo(ApiVersion.Parse("3.4")) < 0);
            Assert.True(ApiVersion.Parse("2.3-alpha").CompareTo(ApiVersion.Parse("2.3-beta")) < 0);
            Assert.True(ApiVersion.Parse("2.3-alpha").CompareTo(ApiVersion.Parse("2.3")) < 0);
            Assert.True(ApiVersion.Parse("2.3").CompareTo(ApiVersion.Parse("2.3-alpha")) > 0);
        }

        [Fact]
        public void Render_ToString()
        {
            Assert.Equal("2022-09-11", ApiVersion.Parse("2022-09-11").ToString());
            Assert.Equal("2022-09-11-alpha", ApiVersion.Parse("2022-09-11-alpha").ToString());
            Assert.Equal("2022-09-112", ApiVersion.Parse("2022-09-112").ToString());
            Assert.Equal("2022-09-112-alpha", ApiVersion.Parse("2022-09-112-alpha").ToString());
            Assert.Equal("2022-09-11.2.3", ApiVersion.Parse("2022-09-112.3").ToString());
            Assert.Equal("2022-09-11.2.3-alpha", ApiVersion.Parse("2022-09-112.3-alpha").ToString());

            Assert.Equal("2022-09-11.2.3", ApiVersion.Parse("2022-09-11.2.3").ToString());
            Assert.Equal("2022-09-11.2.3-alpha", ApiVersion.Parse("2022-09-11.2.3-alpha").ToString());

            Assert.Equal("2.3", ApiVersion.Parse("2.3").ToString());
            Assert.Equal("2.3-alpha", ApiVersion.Parse("2.3-alpha").ToString());
        }

        [Fact]
        public void ParseError()
        {
            Assert.Throws<FormatException>(() => ApiVersion.Parse("2022-09-11."));
            Assert.Throws<FormatException>(() => ApiVersion.Parse("2022-09-11.2"));
            Assert.Throws<FormatException>(() => ApiVersion.Parse("2022-09-11-"));
            Assert.Throws<FormatException>(() => ApiVersion.Parse("2022-09-11.2.3-"));
            Assert.Throws<FormatException>(() => ApiVersion.Parse("2022-09-11.2.3-$"));
            Assert.Throws<FormatException>(() => ApiVersion.Parse("2022-09-11.a.2"));
            Assert.Throws<FormatException>(() => ApiVersion.Parse("2022-09-11.1.a"));

            Assert.Throws<FormatException>(() => ApiVersion.Parse("a"));
            Assert.Throws<FormatException>(() => ApiVersion.Parse("1.a"));
        }
    }
}
