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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

            var typedAttrs = bar.GetCustomAttributes<MyTestAttr>();
            typedAttrs.Should().HaveCount(0);

            typedAttrs = bar.GetCustomAttributes<MyTestAttr>(true);
            typedAttrs.Should().HaveCount(1);


        }

        [Fact]
        public void Test_GetDefault()
        {

            var testCompilation = new TestCompilationBuilder()
                .AddAssembly(typeof(DefaultValueAttribute).Assembly)
                .AddSource($@"
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace TestNamespace;

public class Foo
{{

    public Foo() {{}}

    [DefaultValue(""foo"")]
    public string Name {{ get; set; }}

    public Bar Bar {{ get; set; }} = new Bar();

    [DefaultValue(null)]
    public Baz Baz {{ get; set; }} = new Baz();

    public DateTime Date {{ get; set; }}
}}

public class Bar
{{
    public string BarName {{ get; set; }}

    [DefaultValue(MyEnum.Foo)]
    public MyEnum MyEnumZero {{ get; set; }}

    [DefaultValue(MyEnum.Bar)]
    public MyEnum MyEnumOne {{ get; set; }}

    [DefaultValue(MyEnum.Baz)]
    public MyEnum MyEnumTwo {{ get; set; }}
}}

public class Baz
{{
    [DefaultValue(""baz"")]
    public string BazName {{ get; set; }}
}}

public enum MyEnum
    {{
        [EnumMember(Value = ""foo123"")]
        Foo = 0,
        Bar = 1,
        [EnumMember(Value = ""baz456"")]
        Baz = 2
    }}
")
                .Build();

            var context = new MetadataLoadContext(testCompilation.Compilation);

            var foo = context.ResolveType("TestNamespace.Foo");

            var _default = foo.GetDefault();

            string s = JsonSerializer.Serialize(_default);

            s.Should().NotBeNull();
        }
    }
}
