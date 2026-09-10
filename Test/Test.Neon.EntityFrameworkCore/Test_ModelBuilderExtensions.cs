//-----------------------------------------------------------------------------
// FILE:        Test_ModelBuilderExtensions.cs
// CONTRIBUTOR: Marcus Bowyer
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
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Neon.EntityFrameworkCore;
using Neon.Net;
using Neon.Tasks;
using Neon.Xunit;
using Neon.Xunit.Yugabyte;

using Xunit;

namespace Test.Neon.EntityFrameworkCore
{
    /// <summary>
    /// Tests <see cref="ModelBuilderExtensions.SetDefaultDateTimeKind(ModelBuilder, DateTimeKind)"/>
    /// end to end against a live YugabyteDB instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The model level assertions in <see cref="Test_Extensions"/> prove the converters are attached
    /// and compute the right values.  What they can't prove is what the <i>driver</i> does with those
    /// values, which is where the real behaviour lives.
    /// </para>
    /// <para>
    /// On Npgsql a <c>timestamp with time zone</c> column accepts <see cref="DateTimeKind.Utc"/> values
    /// and <b>rejects</b> local and unspecified ones outright.  So on this provider the method earns
    /// its keep on the <i>write</i> path: it normalizes whatever kind the application happens to be
    /// holding into UTC, turning a runtime failure into a correct write.  These tests establish that
    /// failure first, then show the converters removing it.
    /// </para>
    /// </remarks>
    [Trait(TestTrait.Category, TestArea.NeonEntityFrameworkCore)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public class Test_ModelBuilderExtensions : IClassFixture<YugabyteFixture>
    {
        private readonly DbContextOptions<TestDbContext> options;

        public Test_ModelBuilderExtensions(YugabyteFixture fixture)
        {
            TestHelper.ResetDocker(this.GetType());

            var ycqlPort  = NetHelper.GetUnusedTcpPort(IPAddress.Any);
            var ysqlPort  = NetHelper.GetUnusedTcpPort(IPAddress.Any);
            var adminPort = NetHelper.GetUnusedTcpPort(IPAddress.Any);

            fixture.Start(ycqlPort: ycqlPort, ysqlPort: ysqlPort, adminPort: adminPort);

            this.options = TestModel.CreateOptions(fixture.PostgresConnection.ConnectionString);

            using var context = CreatePlainContext();

            context.Database.EnsureCreated();
            context.Database.ExecuteSqlRaw("DELETE FROM people");
        }

        private TestDbContext CreatePlainContext() => new TestDbContext(options, setDefaultDateTimeKind: false);

        private TestDbContext CreateUtcContext() => new TestDbContext(options, setDefaultDateTimeKind: true);

        [Fact]
        public async Task WithoutIt_NonUtcWritesAreRejected()
        {
            await SyncContext.Clear;

            // Establishes that the problem is real before asserting that the fix works.  Npgsql only
            // accepts Kind=Utc for a timestamptz column, so an application holding a local or
            // unspecified DateTime fails at save time — usually far from where the value was created.

            using var context = CreatePlainContext();

            context.People.Add(
                new Person()
                {
                    Id = Guid.NewGuid(),
                    Name = "Local",
                    CreatedAt = new DateTime(2024, 6, 1, 15, 30, 0, DateTimeKind.Local)
                });

            await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }

        [Fact]
        public async Task NormalizesLocalTimesOnWrite()
        {
            await SyncContext.Clear;

            var id    = Guid.NewGuid();
            var local = new DateTime(2024, 6, 3, 15, 30, 0, DateTimeKind.Local);

            using (var context = CreateUtcContext())
            {
                context.People.Add(new Person() { Id = id, Name = "Local", CreatedAt = local });

                // The same write that failed above now succeeds, because the converter has turned the
                // local time into the UTC instant Npgsql requires.

                await context.SaveChangesAsync();
            }

            using (var context = CreateUtcContext())
            {
                var reloaded = await context.People.SingleAsync(p => p.Id == id);

                // The same moment in time survives the round trip regardless of the writer's time zone.

                Assert.Equal(DateTimeKind.Utc, reloaded.CreatedAt.Kind);
                Assert.Equal(local.ToUniversalTime(), reloaded.CreatedAt);
            }
        }

        [Fact]
        public async Task NormalizesUnspecifiedTimesOnWrite()
        {
            await SyncContext.Clear;

            var id          = Guid.NewGuid();
            var unspecified = new DateTime(2024, 6, 4, 15, 30, 0, DateTimeKind.Unspecified);

            using (var context = CreateUtcContext())
            {
                context.People.Add(new Person() { Id = id, Name = "Unspecified", CreatedAt = unspecified });

                await context.SaveChangesAsync();
            }

            using (var context = CreateUtcContext())
            {
                var reloaded = await context.People.SingleAsync(p => p.Id == id);

                // Documents the assumption baked into the write path: an unspecified time is treated as
                // local and converted accordingly.  That's why UTC is the only sensible kind to pass.

                Assert.Equal(DateTimeKind.Utc, reloaded.CreatedAt.Kind);
                Assert.Equal(unspecified.ToUniversalTime(), reloaded.CreatedAt);
            }
        }

        [Fact]
        public async Task LeavesUtcValuesUnchanged()
        {
            await SyncContext.Clear;

            var id  = Guid.NewGuid();
            var utc = new DateTime(2024, 6, 2, 15, 30, 0, DateTimeKind.Utc);

            using (var context = CreateUtcContext())
            {
                context.People.Add(new Person() { Id = id, Name = "Utc", CreatedAt = utc });

                await context.SaveChangesAsync();
            }

            using (var context = CreateUtcContext())
            {
                var reloaded = await context.People.SingleAsync(p => p.Id == id);

                // Applying the converters to a model that was already correct changes nothing, which is
                // what makes it safe to add to an existing codebase.

                Assert.Equal(DateTimeKind.Utc, reloaded.CreatedAt.Kind);
                Assert.Equal(utc, reloaded.CreatedAt);
            }
        }

        [Fact]
        public async Task RoundTripsNullableColumns()
        {
            await SyncContext.Clear;

            var deletedId = Guid.NewGuid();
            var liveId    = Guid.NewGuid();
            var deletedAt = new DateTime(2024, 7, 4, 8, 0, 0, DateTimeKind.Local);

            using (var context = CreateUtcContext())
            {
                context.People.Add(new Person() { Id = deletedId, Name = "Deleted", CreatedAt = DateTime.UtcNow, DeletedAt = deletedAt });
                context.People.Add(new Person() { Id = liveId, Name = "Live", CreatedAt = DateTime.UtcNow, DeletedAt = null });

                await context.SaveChangesAsync();
            }

            using (var context = CreateUtcContext())
            {
                var deleted = await context.People.SingleAsync(p => p.Id == deletedId);
                var live    = await context.People.SingleAsync(p => p.Id == liveId);

                Assert.Equal(deletedAt.ToUniversalTime(), deleted.DeletedAt);
                Assert.Equal(DateTimeKind.Utc, deleted.DeletedAt.Value.Kind);

                // A null has to stay a null rather than becoming DateTime.MinValue.

                Assert.Null(live.DeletedAt);
            }
        }

        [Fact]
        public async Task KeepsConvertedColumnsQueryable()
        {
            await SyncContext.Clear;

            var id        = Guid.NewGuid();
            var createdAt = new DateTime(2024, 8, 10, 12, 0, 0, DateTimeKind.Utc);

            using (var context = CreateUtcContext())
            {
                context.People.Add(new Person() { Id = id, Name = "Queried", CreatedAt = createdAt });

                await context.SaveChangesAsync();
            }

            using (var context = CreateUtcContext())
            {
                // The converter has to apply in predicates too, not just on materialization — one that
                // only handled reads would make these return nothing.

                Assert.True(await context.People.AnyAsync(p => p.Id == id && p.CreatedAt == createdAt));
                Assert.True(await context.People.AnyAsync(p => p.Id == id && p.CreatedAt > createdAt.AddDays(-1)));
            }
        }

        [Fact]
        public async Task AppliesToCompositeKeyedEntitiesToo()
        {
            await SyncContext.Clear;

            var tenantId = Guid.NewGuid();
            var personId = Guid.NewGuid();
            var joinedAt = new DateTime(2024, 9, 1, 9, 0, 0, DateTimeKind.Local);

            using (var context = CreateUtcContext())
            {
                context.Memberships.Add(
                    new Membership()
                    {
                        TenantId = tenantId,
                        PersonId = personId,
                        Role = "member",
                        JoinedAt = joinedAt
                    });

                await context.SaveChangesAsync();
            }

            using (var context = CreateUtcContext())
            {
                var reloaded = await context.Memberships.SingleAsync(m => m.TenantId == tenantId && m.PersonId == personId);

                Assert.Equal(DateTimeKind.Utc, reloaded.JoinedAt.Kind);
                Assert.Equal(joinedAt.ToUniversalTime(), reloaded.JoinedAt);
            }

            using (var context = CreateUtcContext())
            {
                context.Database.ExecuteSqlRaw($"DELETE FROM \"{TestModel.MembershipSchema}\".memberships");
            }
        }
    }
}