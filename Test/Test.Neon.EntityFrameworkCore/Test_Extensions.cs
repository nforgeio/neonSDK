//-----------------------------------------------------------------------------
// FILE:        Test_Extensions.cs
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
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Neon.EntityFrameworkCore;
using Neon.Tasks;
using Neon.Xunit;

using Xunit;

namespace Test.Neon.EntityFrameworkCore
{
    /// <summary>
    /// Tests the parts of <see cref="DbSetExtensions"/> and <see cref="ModelBuilderExtensions"/> that
    /// don't need a database: argument validation, the keyless entity diagnostic, and the value
    /// converters applied to the model.
    /// </summary>
    /// <remarks>
    /// The behaviour that only shows up against a real server is covered by
    /// <see cref="Test_DbSetExtensions"/> and <see cref="Test_ModelBuilderExtensions"/>, which run
    /// against a live YugabyteDB instance.
    /// </remarks>
    [Trait(TestTrait.Category, TestArea.NeonEntityFrameworkCore)]
    public class Test_Extensions
    {
        private static TestDbContext CreateContext(bool setDefaultDateTimeKind = false)
        {
            return new TestDbContext(TestModel.CreateOptions(), setDefaultDateTimeKind);
        }

        //---------------------------------------------------------------------
        // DbSetExtensions

        [Fact]
        public async Task DbSetExtensions_ValidateArguments()
        {
            await SyncContext.Clear;

            using var context = CreateContext();

            await Assert.ThrowsAsync<ArgumentNullException>(() => DbSetExtensions.ExistsAsync<Person>(null, new object[] { Guid.NewGuid() }));
            await Assert.ThrowsAsync<ArgumentNullException>(() => context.People.ExistsAsync(keyValues: null));

            await Assert.ThrowsAsync<ArgumentNullException>(() => DbSetExtensions.DeleteAsync<Person>(null, new object[] { Guid.NewGuid() }));
            await Assert.ThrowsAsync<ArgumentNullException>(() => context.People.DeleteAsync(keyValues: null));

            await Assert.ThrowsAsync<ArgumentNullException>(() => DbSetExtensions.UpsertAsync<Person>(null, new Person()));
            await Assert.ThrowsAsync<ArgumentNullException>(() => context.People.UpsertAsync(null));
        }

        [Fact]
        public async Task DbSetExtensions_RejectKeylessEntitiesWithAUsefulMessage()
        {
            await SyncContext.Clear;

            using var context = CreateContext();

            // Without the explicit check these would surface as a NullReferenceException from
            // FindPrimaryKey() returning null, which tells the caller nothing.

            foreach (var operation in new Func<Task>[]
            {
                () => context.PersonCounts.ExistsAsync(1),
                () => context.PersonCounts.DeleteAsync(1),
                () => context.PersonCounts.UpsertAsync(new PersonCount())
            })
            {
                var e = await Assert.ThrowsAsync<InvalidOperationException>(operation);

                Assert.Contains(typeof(PersonCount).FullName, e.Message);
                Assert.Contains("primary key", e.Message);
            }
        }

        //---------------------------------------------------------------------
        // ModelBuilderExtensions

        [Fact]
        public void SetDefaultDateTimeKind_ConvertsDateTimeProperties()
        {
            using var context = CreateContext(setDefaultDateTimeKind: true);

            var person = context.Model.FindEntityType(typeof(Person));

            Assert.NotNull(person.FindProperty(nameof(Person.CreatedAt)).GetValueConverter());
            Assert.NotNull(person.FindProperty(nameof(Person.DeletedAt)).GetValueConverter());

            // Non-DateTime properties are left alone.

            Assert.Null(person.FindProperty(nameof(Person.Name)).GetValueConverter());
            Assert.Null(person.FindProperty(nameof(Person.Id)).GetValueConverter());
        }

        [Fact]
        public void SetDefaultDateTimeKind_IsOptIn()
        {
            using var context = CreateContext(setDefaultDateTimeKind: false);

            var person = context.Model.FindEntityType(typeof(Person));

            Assert.Null(person.FindProperty(nameof(Person.CreatedAt)).GetValueConverter());
            Assert.Null(person.FindProperty(nameof(Person.DeletedAt)).GetValueConverter());
        }

        [Fact]
        public void SetDefaultDateTimeKind_AppliesToEveryMappedEntity()
        {
            using var context = CreateContext(setDefaultDateTimeKind: true);

            // Not just the first entity type: the composite keyed Membership in its own schema gets
            // the converter too.

            Assert.NotNull(context.Model
                .FindEntityType(typeof(Membership))
                .FindProperty(nameof(Membership.JoinedAt))
                .GetValueConverter());
        }

        [Fact]
        public void SetDefaultDateTimeKind_SkipsKeylessEntities()
        {
            using var context = CreateContext(setDefaultDateTimeKind: true);

            // Documents the current behaviour: keyless entity types are skipped entirely, so a
            // DateTime on a view or query type doesn't get a converter.

            Assert.Null(context.Model.FindEntityType(typeof(PersonCount)).FindPrimaryKey());
        }

        [Fact]
        public void SetDefaultDateTimeKind_RoundTripsThroughUtc()
        {
            using var context = CreateContext(setDefaultDateTimeKind: true);

            var converter = context.Model
                .FindEntityType(typeof(Person))
                .FindProperty(nameof(Person.CreatedAt))
                .GetValueConverter();

            var local = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Local);

            // Writing converts to UTC...

            var stored = (DateTime)converter.ConvertToProvider(local);

            Assert.Equal(local.ToUniversalTime(), stored);

            // ...and reading stamps the configured kind back on without shifting the value.

            var loaded = (DateTime)converter.ConvertFromProvider(stored);

            Assert.Equal(DateTimeKind.Utc, loaded.Kind);
            Assert.Equal(stored.Ticks, loaded.Ticks);
        }

        [Fact]
        public void SetDefaultDateTimeKind_LeavesUtcValuesUntouchedOnWrite()
        {
            using var context = CreateContext(setDefaultDateTimeKind: true);

            var converter = context.Model
                .FindEntityType(typeof(Person))
                .FindProperty(nameof(Person.CreatedAt))
                .GetValueConverter();

            var utc = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);

            // ToUniversalTime() is a no-op for values already marked UTC, so the write path is
            // idempotent — which is what makes it safe to apply to a model that's already correct.

            Assert.Equal(utc, (DateTime)converter.ConvertToProvider(utc));
        }

        [Fact]
        public void SetDefaultDateTimeKind_HandlesNullableDateTimes()
        {
            using var context = CreateContext(setDefaultDateTimeKind: true);

            var converter = context.Model
                .FindEntityType(typeof(Person))
                .FindProperty(nameof(Person.DeletedAt))
                .GetValueConverter();

            // A null has to stay a null rather than becoming DateTime.MinValue.

            Assert.Null(converter.ConvertToProvider(null));
            Assert.Null(converter.ConvertFromProvider(null));

            var utc = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);

            Assert.Equal(utc, (DateTime)converter.ConvertToProvider((DateTime?)utc));
            Assert.Equal(DateTimeKind.Utc, ((DateTime)converter.ConvertFromProvider(utc)).Kind);
        }

        [Fact]
        public void SetDefaultDateTimeKind_ValidatesArguments()
        {
            Assert.Throws<ArgumentNullException>(() => ModelBuilderExtensions.SetDefaultDateTimeKind(null, DateTimeKind.Utc));
        }
    }
}