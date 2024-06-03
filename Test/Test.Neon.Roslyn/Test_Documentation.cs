// -----------------------------------------------------------------------------
// FILE:	    UnitTest1.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
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
using System.Xml;

using FluentAssertions;

using Neon.Roslyn;

namespace Test.Neon.Roslyn
{
    public class Test_Documentation
    {
        [Fact]
        public void TestSummaryWithSeeRef()
        {
            var xml = $@"<member name=""T:Test_Analyzers.V1ExampleEntity"">
    <summary>
    This is an example description. A <see cref=""T:Test_Analyzers.V1ExampleEntity""/> is a <see cref=""T:k8s.IKubernetesObject`1""/>
    with a <see cref=""T:Test_Analyzers.V1ExampleEntity.V1ExampleSpec""/> and a <see cref=""T:Test_Analyzers.V1ExampleEntity.V1ExampleStatus""/>.
    </summary>
</member>
";
            var summary = $@"This is an example description. A Test_Analyzers.V1ExampleEntity is a k8s.IKubernetesObject`1 with a Test_Analyzers.V1ExampleEntity.V1ExampleSpec and a Test_Analyzers.V1ExampleEntity.V1ExampleStatus.";

            DocumentationComment.From(xml, Environment.NewLine).SummaryText.Trim().Should().BeEquivalentTo(summary);
        }
    }
}