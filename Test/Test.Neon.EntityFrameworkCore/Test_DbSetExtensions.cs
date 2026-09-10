//-----------------------------------------------------------------------------
// FILE:        Test_DbSetExtensions.cs
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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Neon.Common;
using Neon.EntityFrameworkCore;
using Neon.Net;
using Neon.Tasks;
using Neon.Xunit;
using Neon.Xunit.Yugabyte;

using Xunit;

namespace Test.Neon.EntityFrameworkCore
{
    /// <summary>
    /// Tests <see cref="DbSetExtensions"/> against a live YugabyteDB instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These need a real relational database because the interesting behaviour is in what the
    /// extensions <i>translate to</i>: the composite key predicates, the <c>EXISTS</c> query, and the
    /// immediate <c>DELETE</c> that bypasses the change tracker.  Running them against Yugabyte —
    /// rather than a stand-in — also means they cover the database this library's provider specific
    /// half exists to support.
    /// </para>
    /// <para>
    /// The argument validation and model level assertions that don't need a server live in
    /// <see cref="Test_Extensions"/>.
    /// </para>
    /// </remarks>
    [Trait(TestTrait.Category, TestArea.NeonEntityFrameworkCore)]
    [Collection(TestCollection.NonParallel)]
    [CollectionDefinition(TestCollection.NonParallel, DisableParallelization = true)]
    public class Test_DbSetExtensions : IClassFixture<YugabyteFixture>
    {
        private readonly DbContextOptions<TestDbContext> options;

        public Test_DbSetExtensions(YugabyteFixture fixture)
        {
            TestHelper.ResetDocker(this.GetType());

            var ycqlPort  = NetHelper.GetUnusedTcpPort(IPAddress.Any);
            var ysqlPort  = NetHelper.GetUnusedTcpPort(IPAddress.Any);
            var adminPort = NetHelper.GetUnusedTcpPort(IPAddress.Any);

            fixture.Start(ycqlPort: ycqlPort, ysqlPort: ysqlPort, adminPort: adminPort);

            this.options = TestModel.CreateOptions(fixture.PostgresConnection.ConnectionString);

            // The fixture recreates the database when it starts, but a class fixture is shared across
            // the tests in this class, so each test starts by clearing the tables itself.

            using var context = CreateContext();

            context.Database.EnsureCreated();

            context.Database.ExecuteSqlRaw($"DELETE FROM \"{TestModel.MembershipSchema}\".memberships");
            context.Database.ExecuteSqlRaw("DELETE FROM people");
        }

        /// <summary>
        /// Returns a context with <c>SetDefaultDateTimeKind(DateTimeKind.Utc)</c> applied, so that the
        /// <see cref="DateTime"/> values these tests write are normalized before they reach Npgsql
        /// regardless of their <see cref="DateTime.Kind"/>.  These tests are about the key based
        /// operations, not about timestamp handling — that's <see cref="Test_ModelBuilderExtensions"/>.
        /// </summary>
        private TestDbContext CreateContext() => new TestDbContext(options, setDefaultDateTimeKind: true);

        /// <summary>
        /// Inserts a person and returns their key.  A separate context is used so nothing the tests
        /// observe afterwards can be served from this one's change tracker.
        /// </summary>
        private async Task<Guid> AddPersonAsync(string name = "Alice")
        {
            await SyncContext.Clear;

            using var context = CreateContext();

            var person = new Person()
            {
                Id        = Guid.NewGuid(),
                Name      = name,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            context.People.Add(person);

            await context.SaveChangesAsync();

            return person.Id;
        }

        /// <summary>
        /// Inserts a membership and returns its composite key.
        /// </summary>
        private async Task<object[]> AddMembershipAsync(string role = "member")
        {
            await SyncContext.Clear;

            using var context = CreateContext();

            var membership = new Membership()
            {
                TenantId = Guid.NewGuid(),
                PersonId = Guid.NewGuid(),
                Role     = role,
                JoinedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            context.Memberships.Add(membership);

            await context.SaveChangesAsync();

            return new object[] { membership.TenantId, membership.PersonId };
        }

        //---------------------------------------------------------------------
        // ExistsAsync()

        [Fact]
        public async Task ExistsAsync_ReportsAbsentRows()
        {
            await SyncContext.Clear;

            using var context = CreateContext();

            Assert.False(await context.People.ExistsAsync(Guid.NewGuid()));
        }

        [Fact]
        public async Task ExistsAsync_ReportsPresentRows()
        {
            await SyncContext.Clear;

            var id = await AddPersonAsync();

            using var context = CreateContext();

            Assert.True(await context.People.ExistsAsync(id));
        }

        [Fact]
        public async Task ExistsAsync_HandlesCompositeKeys()
        {
            await SyncContext.Clear;

            var key = await AddMembershipAsync();

            using var context = CreateContext();

            Assert.True(await context.Memberships.ExistsAsync(key));

            // Both halves of the key have to match: a right tenant with the wrong person must not
            // report a hit, which is what a predicate built from only the first key property would do.

            Assert.False(await context.Memberships.ExistsAsync(new object[] { key[0], Guid.NewGuid() }));
            Assert.False(await context.Memberships.ExistsAsync(new object[] { Guid.NewGuid(), key[1] }));
        }

        [Fact]
        public async Task ExistsAsync_IgnoresTheChangeTracker()
        {
            await SyncContext.Clear;

            using var context = CreateContext();

            var person = new Person()
            {
                Id        = Guid.NewGuid(),
                Name      = "Unsaved",
                CreatedAt = DateTime.UtcNow
            };

            context.People.Add(person);

            // Documents the difference from FindAsync(), which would return the tracked entity:
            // ExistsAsync() always asks the database.

            Assert.False(await context.People.ExistsAsync(person.Id));

            await context.SaveChangesAsync();

            Assert.True(await context.People.ExistsAsync(person.Id));
        }

        [Fact]
        public async Task ExistsAsync_LeavesTheChangeTrackerEmpty()
        {
            await SyncContext.Clear;

            var id = await AddPersonAsync();

            using var context = CreateContext();

            Assert.True(await context.People.ExistsAsync(id));

            // The whole point of ExistsAsync() over FindAsync(): the entity is never materialized.

            Assert.Empty(context.ChangeTracker.Entries());
        }

        [Fact]
        public async Task ExistsAsync_ForwardsTheCancellationToken()
        {
            await SyncContext.Clear;

            var id = await AddPersonAsync();

            using var context = CreateContext();

            using var cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.Cancel();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => context.People.ExistsAsync(new object[] { id }, cancellationTokenSource.Token));
        }

        //---------------------------------------------------------------------
        // DeleteAsync()

        [Fact]
        public async Task DeleteAsync_RemovesTheRowImmediately()
        {
            await SyncContext.Clear;

            var id = await AddPersonAsync();

            using var context = CreateContext();

            await context.People.DeleteAsync(id);

            // No SaveChangesAsync() — ExecuteDelete() has already sent the statement.  A fresh context
            // proves the row is gone from the database rather than just from this one's view.

            using var verifyContext = CreateContext();

            Assert.False(await verifyContext.People.ExistsAsync(id));
        }

        [Fact]
        public async Task DeleteAsync_IsAnoopForAbsentKeys()
        {
            await SyncContext.Clear;

            var survivor = await AddPersonAsync();

            using var context = CreateContext();

            // Deleting a key that isn't there affects no rows and isn't an error.

            await context.People.DeleteAsync(Guid.NewGuid());

            Assert.True(await context.People.ExistsAsync(survivor));
        }

        [Fact]
        public async Task DeleteAsync_OnlyRemovesTheKeyedRow()
        {
            await SyncContext.Clear;

            var doomed   = await AddPersonAsync("Doomed");
            var survivor = await AddPersonAsync("Survivor");

            using var context = CreateContext();

            await context.People.DeleteAsync(doomed);

            Assert.False(await context.People.ExistsAsync(doomed));
            Assert.True(await context.People.ExistsAsync(survivor));
        }

        [Fact]
        public async Task DeleteAsync_HandlesCompositeKeys()
        {
            await SyncContext.Clear;

            var doomed   = await AddMembershipAsync("doomed");
            var survivor = await AddMembershipAsync("survivor");

            using var context = CreateContext();

            await context.Memberships.DeleteAsync(doomed);

            Assert.False(await context.Memberships.ExistsAsync(doomed));
            Assert.True(await context.Memberships.ExistsAsync(survivor));
        }

        //---------------------------------------------------------------------
        // UpsertAsync()

        [Fact]
        public async Task UpsertAsync_InsertsWhenAbsent()
        {
            await SyncContext.Clear;

            var person = new Person()
            {
                Id        = Guid.NewGuid(),
                Name      = "Inserted",
                CreatedAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            using var context = CreateContext();

            await context.People.UpsertAsync(person);

            // Staged, not saved.

            Assert.Equal(EntityState.Added, context.Entry(person).State);

            await context.SaveChangesAsync();

            using var verifyContext = CreateContext();

            Assert.Equal("Inserted", (await verifyContext.People.SingleAsync(p => p.Id == person.Id)).Name);
        }

        [Fact]
        public async Task UpsertAsync_UpdatesWhenPresent()
        {
            await SyncContext.Clear;

            var id = await AddPersonAsync("Original");

            using var context = CreateContext();

            var person = new Person()
            {
                Id        = id,
                Name      = "Replaced",
                CreatedAt = new DateTime(2024, 4, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            await context.People.UpsertAsync(person);

            Assert.Equal(EntityState.Modified, context.Entry(person).State);

            await context.SaveChangesAsync();

            using var verifyContext = CreateContext();

            Assert.Equal("Replaced", (await verifyContext.People.SingleAsync(p => p.Id == id)).Name);
            Assert.Equal(1, await verifyContext.People.CountAsync());
        }

        [Fact]
        public async Task UpsertAsync_RequiresSaveChanges()
        {
            await SyncContext.Clear;

            var person = new Person()
            {
                Id        = Guid.NewGuid(),
                Name      = "Never saved",
                CreatedAt = DateTime.UtcNow
            };

            using (var context = CreateContext())
            {
                await context.People.UpsertAsync(person);
            }

            // Documents that UpsertAsync() only stages the change, unlike DeleteAsync().

            using var verifyContext = CreateContext();

            Assert.False(await verifyContext.People.ExistsAsync(person.Id));
        }

        [Fact]
        public async Task UpsertAsync_HandlesCompositeKeys()
        {
            await SyncContext.Clear;

            var key = await AddMembershipAsync("member");

            using var context = CreateContext();

            var membership = new Membership()
            {
                TenantId = (Guid)key[0],
                PersonId = (Guid)key[1],
                Role     = "owner",
                JoinedAt = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            await context.People.UpsertAsync(new Person() { Id = Guid.NewGuid(), Name = "Filler", CreatedAt = DateTime.UtcNow });
            await context.Memberships.UpsertAsync(membership);

            Assert.Equal(EntityState.Modified, context.Entry(membership).State);

            await context.SaveChangesAsync();

            using var verifyContext = CreateContext();

            var reloaded = await verifyContext.Memberships.SingleAsync(m => m.TenantId == membership.TenantId && m.PersonId == membership.PersonId);

            Assert.Equal("owner", reloaded.Role);
            Assert.Equal(1, await verifyContext.Memberships.CountAsync());
        }
    }
}