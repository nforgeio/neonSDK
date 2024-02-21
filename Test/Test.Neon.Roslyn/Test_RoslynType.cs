// -----------------------------------------------------------------------------
// FILE:	    Test_RoslynType.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using Neon.Roslyn;
using Neon.Roslyn.Xunit;

namespace Test.Neon.Roslyn
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public class MyTestAttr : Attribute { }
    public class Test_RoslynType
    {
        [Fact]
        public void Test_CustomAttributes()
        {

            var testCompilation = new TestCompilationBuilder()
                .AddAssembly(typeof(AttributeUsageAttribute).Assembly)
                .AddAssembly(typeof(MyTestAttr).Assembly)
                .AddSource($@"
using System;
using Test.Neon.Roslyn;

namespace Foo;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class MyAttribute : System.Attribute
{{
}}

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class MyInheritedAttribute : System.Attribute
{{
}}

[MyInheritedAttribute]
[MyAttribute]
[MyTestAttr]
public class Bar
{{
    public void Method1() {{}}
}}


public class Baz : Bar
{{
    public void Method1() {{}}
}}
")
                .Build();

            var context = new MetadataLoadContext(testCompilation.Compilation);

            var bar = context.ResolveType("Foo.Baz");

            var attr = bar.GetCustomAttributes(true);

            attr.Should().HaveCount(2);

            attr = bar.GetCustomAttributes(typeof(MyTestAttr), true);
            attr.Should().HaveCount(1);

        }
    }
}
