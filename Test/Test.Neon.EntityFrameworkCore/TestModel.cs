//-----------------------------------------------------------------------------
// FILE:        TestModel.cs
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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using Neon.EntityFrameworkCore;

namespace Test.Neon.EntityFrameworkCore
{
    /// <summary>
    /// A person, mapped to a table with a single column key.
    /// </summary>
    public class Person
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }

    /// <summary>
    /// A membership, mapped to a table with a composite key in a non-default schema.
    /// </summary>
    public class Membership
    {
        public Guid TenantId { get; set; }
        public Guid PersonId { get; set; }
        public string Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    /// <summary>
    /// A keyless entity, used to verify that the key based extension methods reject it with a
    /// useful message rather than a <see cref="NullReferenceException"/>.
    /// </summary>
    public class PersonCount
    {
        public int Count { get; set; }
    }

    /// <summary>
    /// Gives every <see cref="DbContext"/> instance its own model instead of sharing one per context
    /// type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Entity Framework Core caches a built model per context type for the lifetime of the process.
    /// That's the right behaviour for an application, but it makes a test context whose
    /// <see cref="DbContext.OnModelCreating(ModelBuilder)"/> varies by a constructor flag serve
    /// whichever variant some earlier test happened to build first — and it means a test that watches
    /// for the activity emitted <i>during</i> model building sees nothing if the model was already
    /// built.
    /// </para>
    /// <para>
    /// Keying on the context instance keeps the model stable for the lifetime of that context, which
    /// is what Entity Framework Core requires, while guaranteeing a fresh build per test.
    /// </para>
    /// </remarks>
    public class PerContextModelCacheKeyFactory : IModelCacheKeyFactory
    {
        /// <inheritdoc/>
        public object Create(DbContext context, bool designTime)
        {
            return (context, designTime);
        }
    }

    /// <summary>
    /// Builds the shared test model.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DateTime"/> properties keep Npgsql's default <c>timestamp with time zone</c>
    /// mapping.  That's deliberate, and it's the only mapping that works: Npgsql refuses to write a
    /// <see cref="DateTimeKind.Utc"/> value to a <c>timestamp without time zone</c> column, so pairing
    /// a naive column with
    /// <see cref="ModelBuilderExtensions.SetDefaultDateTimeKind(ModelBuilder, DateTimeKind)"/> — whose
    /// write path produces exactly that — fails at the driver.
    /// </para>
    /// <para>
    /// <see cref="Membership"/> lives in a non-default schema so that composite keys and the
    /// <c>db.namespace</c> trace tag are both covered.
    /// </para>
    /// </remarks>
    public static class TestModel
    {
        /// <summary>
        /// The schema <see cref="Membership"/> is mapped into.
        /// </summary>
        public const string MembershipSchema = "tenancy";

        /// <summary>
        /// Applies the test model to <paramref name="modelBuilder"/>.
        /// </summary>
        /// <param name="modelBuilder">The builder to configure.</param>
        /// <param name="setDefaultDateTimeKind">
        /// Pass <c>true</c> to finish by calling
        /// <see cref="ModelBuilderExtensions.SetDefaultDateTimeKind(ModelBuilder, DateTimeKind)"/>.
        /// </param>
        public static void Apply(ModelBuilder modelBuilder, bool setDefaultDateTimeKind)
        {
            modelBuilder.Entity<Person>()
                .ToTable("people");

            var membership = modelBuilder.Entity<Membership>();

            membership.ToTable("memberships", MembershipSchema);
            membership.HasKey(m => new { m.TenantId, m.PersonId });

            modelBuilder.Entity<PersonCount>()
                .HasNoKey()
                .ToView("person_counts");

            if (setDefaultDateTimeKind)
            {
                modelBuilder.SetDefaultDateTimeKind(DateTimeKind.Utc);
            }
        }

        /// <summary>
        /// Builds options for <see cref="TestDbContext"/> that give each context instance its own
        /// freshly built model.  See <see cref="PerContextModelCacheKeyFactory"/> for why that matters.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string.  Defaults to <see cref="TestDbContext.OfflineConnectionString"/> for
        /// tests that never reach a server.
        /// </param>
        public static DbContextOptions<TestDbContext> CreateOptions(string connectionString = TestDbContext.OfflineConnectionString)
        {
            return new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(connectionString)
                .ReplaceService<IModelCacheKeyFactory, PerContextModelCacheKeyFactory>()
                .Options;
        }
    }

    /// <summary>
    /// The context used by the tests.
    /// </summary>
    public class TestDbContext : DbContext
    {
        /// <summary>
        /// A connection string that is well formed but never dialed, for the tests that only need
        /// Entity Framework Core to build a model or resolve a service.
        /// </summary>
        public const string OfflineConnectionString = "Host=localhost;Port=5432;Database=test;Username=test;Password=test";

        private readonly bool setDefaultDateTimeKind;

        public TestDbContext(DbContextOptions options, bool setDefaultDateTimeKind = false)
            : base(options)
        {
            this.setDefaultDateTimeKind = setDefaultDateTimeKind;
        }

        public DbSet<Person> People { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<PersonCount> PersonCounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            TestModel.Apply(modelBuilder, setDefaultDateTimeKind);
        }
    }
}