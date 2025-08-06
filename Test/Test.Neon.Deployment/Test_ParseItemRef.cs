//-----------------------------------------------------------------------------
// FILE:        Test_ParseSecretNames.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

using Neon.Common;
using Neon.Deployment;
using Neon.IO;
using Neon.Xunit;

namespace TestDeployment
{
    [Trait(TestTrait.Category, TestArea.NeonDeployment)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public partial class Test_ParseItemRef
    {
        [Fact]
        public void Parse_Null()
        {
            Assert.Throws<ArgumentNullException>(() => ProfileServer.ParseItemRef(null));
        }

        [Fact]
        public void Parse_Empty()
        {
            Assert.Equal(string.Empty, ProfileServer.ParseItemRef(string.Empty).ItemName);
            Assert.Null(ProfileServer.ParseItemRef(string.Empty).FieldName);
        }

        [Fact]
        public void Parse_NoProperty()
        {
            Assert.Equal("test", ProfileServer.ParseItemRef("test").ItemName);
            Assert.Null(ProfileServer.ParseItemRef("test").FieldName);
        }

        [Fact]
        public void Parse_WithProperty()
        {
            Assert.Equal("test", ProfileServer.ParseItemRef("test[property]").ItemName);
            Assert.Equal("property", ProfileServer.ParseItemRef("test[property]").FieldName);
        }

        [Fact]
        public void Parse_BadProperty()
        {
            Assert.Equal("test[property", ProfileServer.ParseItemRef("test[property").ItemName);
            Assert.Null(ProfileServer.ParseItemRef("test[property").FieldName);

            Assert.Equal("testproperty]", ProfileServer.ParseItemRef("testproperty]").ItemName);
            Assert.Null(ProfileServer.ParseItemRef("testproperty]").FieldName);

            Assert.Equal("test", ProfileServer.ParseItemRef("test[]").ItemName);
            Assert.Null(ProfileServer.ParseItemRef("test[]").FieldName);
        }
    }
}
