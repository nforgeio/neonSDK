//-----------------------------------------------------------------------------
// FILE:        Test_WmiHyperV_Net.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

using Neon.Common;
using Neon.Cryptography;
using Neon.Deployment;
using Neon.IO;
using Neon.HyperV;
using Neon.Xunit;

namespace TestHyperV
{
    /// <summary>
    /// Low-level WMI virtual disk tests networking.
    /// </summary>
    [Trait(TestTrait.Category, TestArea.NeonHyperV)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public partial class Test_WmiHyperV_Net
    {
        [Fact]
        public void Switch_List()
        {
            using (var wmiClient = new WmiHyperVClient())
            {
                Assert.NotEmpty(wmiClient.ListSwitches());
            }
        }
    }
}
