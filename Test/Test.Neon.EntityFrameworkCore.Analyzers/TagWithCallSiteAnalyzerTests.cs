// -----------------------------------------------------------------------------
// FILE:	    TagWithCallSiteAnalyzerTests.cs
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
using System.Linq;

using FluentAssertions;

using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

using Neon.EntityFrameworkCore.Analyzers;
using Neon.Roslyn.Xunit;

namespace Test.Neon.EntityFrameworkCore.Analyzers
{
    public class TagWithCallSiteAnalyzerTests
    {
        private const string DiagnosticId = TagWithCallSiteAnalyzer.DiagnosticId;

        [Fact]
        public void ReportsUntaggedQueryExecutedDirectlyOnDbSet()
        {
            var testCompilation = BuildCompilation(@"
        public async Task RunAsync()
        {
            var users = await context.Users.ToListAsync();
        }");

            AssertCompiles(testCompilation);
            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == DiagnosticId);
        }

        [Fact]
        public void ReportsUntaggedQueryBuiltFromComposableOperators()
        {
            var testCompilation = BuildCompilation(@"
        public async Task RunAsync()
        {
            var users = await context.Users
                .AsNoTracking()
                .Where(user => user.Active)
                .OrderBy(user => user.Name)
                .ToListAsync();
        }");

            AssertCompiles(testCompilation);
            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == DiagnosticId);
        }

        [Fact]
        public void ReportsEachKindOfQueryExecution()
        {
            var testCompilation = BuildCompilation(@"
        public async Task RunAsync()
        {
            var any     = await context.Users.AnyAsync();
            var count   = context.Users.Count();
            var first   = context.Users.FirstOrDefault();
            var list    = context.Users.ToList();
            var deleted = await context.Users.Where(user => !user.Active).ExecuteDeleteAsync();
        }");

            AssertCompiles(testCompilation);
            testCompilation.Diagnostics.Count(diagnostic => diagnostic.Id == DiagnosticId).Should().Be(5);
        }

        [Fact]
        public void ReportsQueryExecutedByForEach()
        {
            var testCompilation = BuildCompilation(@"
        public void Run()
        {
            foreach (var user in context.Users.Where(user => user.Active))
            {
                System.Console.WriteLine(user.Name);
            }
        }");

            AssertCompiles(testCompilation);
            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == DiagnosticId);
        }

        [Fact]
        public void ReportsQueryRootedAtSetMethod()
        {
            var testCompilation = BuildCompilation(@"
        public async Task RunAsync()
        {
            var users = await context.Set<User>().Where(user => user.Active).ToListAsync();
        }");

            AssertCompiles(testCompilation);
            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == DiagnosticId);
        }

        [Fact]
        public void ReportsQueryComposedThroughLocal()
        {
            var testCompilation = BuildCompilation(@"
        public async Task RunAsync()
        {
            var query = context.Users.Where(user => user.Active);
            var users = await query.ToListAsync();
        }");

            AssertCompiles(testCompilation);
            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == DiagnosticId);
        }

        [Fact]
        public void AcceptsQueryTaggedAnywhereInTheChain()
        {
            var testCompilation = BuildCompilation(@"
        public async Task RunAsync()
        {
            var atRoot  = await context.Users.TagWithCallSite().Where(user => user.Active).ToListAsync();
            var atEnd   = await context.Users.Where(user => user.Active).TagWithCallSite().ToListAsync();
            var query   = context.Users.TagWithCallSite();
            var byLocal = await query.Where(user => user.Active).ToListAsync();
        }");

            AssertCompiles(testCompilation);
            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id == DiagnosticId);
        }

        [Fact]
        public void IgnoresInMemoryLinq()
        {
            var testCompilation = BuildCompilation(@"
        public void Run()
        {
            var names = new System.Collections.Generic.List<string>();
            var upper = names.Where(name => name.Length > 0).Select(name => name.ToUpper()).ToList();

            foreach (var name in names.Where(n => n.Length > 0))
            {
                System.Console.WriteLine(name);
            }
        }");

            AssertCompiles(testCompilation);
            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id == DiagnosticId);
        }

        [Fact]
        public void IgnoresQueriesWhoseRootCannotBeDetermined()
        {
            var testCompilation = BuildCompilation(@"
        public async Task RunAsync(IQueryable<User> supplied)
        {
            var users = await supplied.Where(user => user.Active).ToListAsync();
        }");

            AssertCompiles(testCompilation);
            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id == DiagnosticId);
        }

        [Fact]
        public void IgnoresChainsPassingThroughUnrecognizedHelpers()
        {
            // The helper tags the query itself, which the analyzer cannot see from the call site,
            // so it must not report here.

            var testCompilation = BuildCompilation(@"
        public async Task RunAsync()
        {
            var users = await context.Users.Limit(10).ToListAsync();
        }
    }

    public static class QueryHelpers
    {
        public static IQueryable<T> Limit<T>(this IQueryable<T> source, int limit)
        {
            return source.TagWithCallSite().Take(limit);
        }");

            AssertCompiles(testCompilation);
            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id == DiagnosticId);
        }

        [Fact]
        public void IgnoresDbSetMembersThatCannotBeTagged()
        {
            var testCompilation = BuildCompilation(@"
        public async Task RunAsync(User user)
        {
            context.Users.Add(user);

            var found = await context.Users.FindAsync(1);

            await context.SaveChangesAsync();
        }");

            AssertCompiles(testCompilation);
            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id == DiagnosticId);
        }

        [Fact]
        public void ReportsOnlyTheEnclosingQueryWhenASubqueryIsProjected()
        {
            var testCompilation = BuildCompilation(@"
        public async Task RunAsync()
        {
            var projected = await context.Users
                .Select(user => context.Users.Where(other => other.Name == user.Name).ToList())
                .ToListAsync();
        }");

            AssertCompiles(testCompilation);
            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == DiagnosticId);
        }

        [Fact]
        public void RejectsTagWithByDefault()
        {
            var testCompilation = BuildCompilation(@"
        public async Task RunAsync()
        {
            var users = await context.Users.TagWith(""by hand"").ToListAsync();
        }");

            AssertCompiles(testCompilation);
            testCompilation.Diagnostics.Should().ContainSingle(diagnostic => diagnostic.Id == DiagnosticId);
        }

        [Fact]
        public void AcceptsTagWithWhenConfiguredTo()
        {
            var testCompilation = BuildCompilation(@"
        public async Task RunAsync()
        {
            var users = await context.Users.TagWith(""by hand"").ToListAsync();
        }",
                builder => builder.AddOption(TagWithCallSiteAnalyzer.AllowTagWithOption, "true"));

            AssertCompiles(testCompilation);
            testCompilation.Diagnostics.Should().NotContain(diagnostic => diagnostic.Id == DiagnosticId);
        }

        [Fact]
        public void ReportsTheQueryExecutionOperator()
        {
            var testCompilation = BuildCompilation(@"
        public async Task RunAsync()
        {
            var users = await context.Users.ToListAsync();
        }");

            var diagnostic = testCompilation.Diagnostics.Single(d => d.Id == DiagnosticId);

            diagnostic.GetMessage().Should().Contain("ToListAsync");
            diagnostic.Severity.Should().Be(DiagnosticSeverity.Warning);
        }

        /// <summary>
        /// Wraps <paramref name="members"/> in a repository class with a <c>DbContext</c> to hand
        /// the analyzer a realistic query root, and compiles it against the real EF Core
        /// assemblies rather than a stub, so the operators resolve exactly as they do in
        /// consuming code.
        /// </summary>
        private static TestCompilation BuildCompilation(string members, Action<TestCompilationBuilder> configure = null)
        {
            var source = @"
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace TestNamespace
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
    }

    public class TestDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
    }

    public class UserRepository
    {
        private TestDbContext context;
" + members + @"
    }
}";

            var builder = new TestCompilationBuilder()
                .AddDiagnosticAnalyzer(new TagWithCallSiteAnalyzer())
                .AddAssembly(typeof(DbContext).Assembly)
                .AddAssembly(typeof(ModelBuilder).Assembly)
                .AddAssembly(typeof(System.Linq.Queryable).Assembly)
                .AddAssembly(typeof(System.Linq.Expressions.Expression).Assembly)
                .AddAssembly(typeof(System.Threading.Tasks.Task).Assembly)
                .AddSource(source);

            configure?.Invoke(builder);

            return builder.Build();
        }

        /// <summary>
        /// Guards the tests against a broken fixture: were the EF Core references to go missing,
        /// the analyzer would go quiet and every negative test would pass for the wrong reason.
        /// </summary>
        private static void AssertCompiles(TestCompilation testCompilation)
        {
            var errors = testCompilation.Diagnostics
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                .ToList();

            errors.Should().BeEmpty(
                because: "the test source must compile: " +
                         string.Join("; ", errors.Select(error => error.ToString())));
        }
    }
}
