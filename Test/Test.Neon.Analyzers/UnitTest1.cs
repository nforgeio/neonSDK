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

using System.Threading.Tasks;

using FluentAssertions;

using Neon.Roslyn.Xunit;

using OpenTelemetry.Resources;

namespace Test.Neon.Analyzers
{
    public class UnitTest1
    {

        [Fact]
        public void Test1()
        {
            var source = @"
using System.Threading.Tasks;

namespace TestNamespace
{
    public class TestClass
    {
        public async Task TestNoClearAsync()
        {
            await Task.Delay(1);
        }
    }
}";

            var testCompilation = new TestCompilationBuilder()
                .AddDiagnosticAnalyzer(new global::Neon.Analyzers.SyncContextClearAnalyzer())
                .AddSource(source)
                .Build();

            testCompilation.Diagnostics.Should().ContainSingle(d => d.Id == "NEON0001");

            source = @"
using System.Threading.Tasks;
using Neon.Tasks;

namespace TestNamespace
{
    public class TestClass
    {
        public async Task TestNoClearAsync()
        {
            await SyncContext.Clear;

            await Task.Delay(1);
        }
    }
}";

            testCompilation = new TestCompilationBuilder()
                .AddDiagnosticAnalyzer(new global::Neon.Analyzers.SyncContextClearAnalyzer())
                .AddSource(source)
                .Build();

            testCompilation.Diagnostics.Should().BeEmpty();
        }
    }
}
