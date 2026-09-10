//-----------------------------------------------------------------------------
// FILE:        Test_NpgsqlProviderGuards.cs
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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using global::Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

using Neon.EntityFrameworkCore.Npgsql;
using Neon.Xunit;

using Xunit;

#pragma warning disable EF1001  // Internal EF Core API usage — that's the whole point of this file.

namespace Test.Neon.EntityFrameworkCore
{
    /// <summary>
    /// Guards the internal Entity Framework Core and Npgsql provider surface that
    /// <see cref="AdvisoryLockHistoryRepository"/> is built on.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="NpgsqlHistoryRepository"/> lives in a <b>.Internal</b> namespace, so its shape isn't
    /// covered by semantic versioning.  A provider upgrade could change it, and most of the ways it
    /// could change would be caught by the compiler — but not all of them.  A member quietly becoming
    /// non-virtual, or a lock's release semantics changing from transaction to session scope, would
    /// still compile and would break mutual exclusion at runtime <i>silently</i>.
    /// </para>
    /// <para>
    /// These tests exist so that a provider upgrade surfaces as a red test with an explanation
    /// attached, rather than as a production surprise.  When one of them fails, the fix is to read
    /// <see cref="AdvisoryLockHistoryRepository"/>'s remarks, re-derive the design against the new
    /// provider, and then update the assertion — not to delete it.
    /// </para>
    /// </remarks>
    [Trait(TestTrait.Category, TestArea.NeonEntityFrameworkCore)]
    public class Test_NpgsqlProviderGuards
    {
        [Fact]
        public void NpgsqlHistoryRepository_IsStillConstructibleFromDependencies()
        {
            var constructor = typeof(NpgsqlHistoryRepository).GetConstructor(
                bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                types:       new[] { typeof(HistoryRepositoryDependencies) });

            Assert.NotNull(constructor);
        }

        [Fact]
        public void NpgsqlHistoryRepository_AcquireDatabaseLock_IsStillOverridable()
        {
            var method = typeof(NpgsqlHistoryRepository).GetMethod(
                name:        nameof(IHistoryRepository.AcquireDatabaseLock),
                bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                types:       Type.EmptyTypes);

            Assert.NotNull(method);
            Assert.Equal(typeof(IMigrationsDatabaseLock), method.ReturnType);

            // A sealed override would silently defeat the whole approach.

            Assert.True(method.IsVirtual, "AcquireDatabaseLock() is no longer virtual.");
            Assert.False(method.IsFinal, "AcquireDatabaseLock() has been sealed.");
        }

        [Fact]
        public void NpgsqlHistoryRepository_AcquireDatabaseLockAsync_IsStillOverridable()
        {
            var method = typeof(NpgsqlHistoryRepository).GetMethod(
                name:        nameof(IHistoryRepository.AcquireDatabaseLockAsync),
                bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                types:       new[] { typeof(CancellationToken) });

            Assert.NotNull(method);
            Assert.Equal(typeof(Task<IMigrationsDatabaseLock>), method.ReturnType);

            Assert.True(method.IsVirtual, "AcquireDatabaseLockAsync() is no longer virtual.");
            Assert.False(method.IsFinal, "AcquireDatabaseLockAsync() has been sealed.");
        }

        [Fact]
        public void NpgsqlHistoryRepository_LockReleaseBehavior_IsStillOverridable()
        {
            var property = typeof(NpgsqlHistoryRepository).GetProperty(
                name:        nameof(IHistoryRepository.LockReleaseBehavior),
                bindingAttr: BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(property);
            Assert.Equal(typeof(LockReleaseBehavior), property.PropertyType);

            Assert.True(property.GetMethod.IsVirtual, "LockReleaseBehavior is no longer virtual.");
            Assert.False(property.GetMethod.IsFinal, "LockReleaseBehavior has been sealed.");
        }

        [Fact]
        public void HistoryRepository_StillExposesTheTableIdentity()
        {
            // The default lock key is derived from these two, so losing them means losing the
            // derivation.

            foreach (var name in new[] { "TableName", "TableSchema" })
            {
                var property = typeof(HistoryRepository).GetProperty(
                    name:        name,
                    bindingAttr: BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                Assert.NotNull(property);
                Assert.Equal(typeof(string), property.PropertyType);
            }
        }

        [Fact]
        public void HistoryRepositoryDependencies_StillExposesWhatWeUse()
        {
            var expected = new (string Name, Type Type)[]
            {
                ( nameof(HistoryRepositoryDependencies.RawSqlCommandBuilder), typeof(IRawSqlCommandBuilder) ),
                ( nameof(HistoryRepositoryDependencies.Connection),           typeof(IRelationalConnection) ),
                ( nameof(HistoryRepositoryDependencies.CurrentContext),       typeof(ICurrentDbContext) ),
                ( nameof(HistoryRepositoryDependencies.CommandLogger),        typeof(IRelationalCommandDiagnosticsLogger) )
            };

            foreach (var (name, type) in expected)
            {
                var property = typeof(HistoryRepositoryDependencies).GetProperty(name);

                Assert.NotNull(property);
                Assert.Equal(type, property.PropertyType);
            }
        }

        [Fact]
        public void MigrationsDatabaseLock_StillHasTheMembersWeImplement()
        {
            var type = typeof(IMigrationsDatabaseLock);

            // HistoryRepository is a *protected* interface member, which is why it can't be named
            // with nameof() from out here.  It's asserted by name anyway so that a rename shows up as
            // a failure rather than as our implementation quietly satisfying nothing.

            Assert.NotNull(type.GetProperty(
                name: "HistoryRepository",
                bindingAttr: BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));

            Assert.NotNull(type.GetMethod(
                name: nameof(IMigrationsDatabaseLock.ReacquireIfNeeded),
                types: new[] { typeof(bool), typeof(bool?) }));

            Assert.NotNull(type.GetMethod(
                name: nameof(IMigrationsDatabaseLock.ReacquireIfNeededAsync),
                types: new[] { typeof(bool), typeof(bool?), typeof(CancellationToken) }));

            // The lock is released by the ambient transaction, so both dispose paths have to remain
            // no-ops we're allowed to implement.

            Assert.True(typeof(IDisposable).IsAssignableFrom(type));
            Assert.True(typeof(IAsyncDisposable).IsAssignableFrom(type));
        }

        [Fact]
        public void OurOverridesStillOverride()
        {
            // Guards against the base class renaming a member: the code would still compile as a
            // brand new method that Entity Framework Core never calls, leaving migrations to take the
            // provider's LOCK TABLE and fail on Yugabyte again.

            var type = typeof(AdvisoryLockHistoryRepository);

            var acquire = type.GetMethod(
                name:        nameof(IHistoryRepository.AcquireDatabaseLock),
                bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                types:       Type.EmptyTypes);

            var acquireAsync = type.GetMethod(
                name:        nameof(IHistoryRepository.AcquireDatabaseLockAsync),
                bindingAttr: BindingFlags.Public | BindingFlags.Instance,
                types:       new[] { typeof(CancellationToken) });

            var behavior = type.GetProperty(nameof(IHistoryRepository.LockReleaseBehavior));

            Assert.Equal(type, acquire.DeclaringType);
            Assert.Equal(type, acquireAsync.DeclaringType);
            Assert.Equal(type, behavior.DeclaringType);

            Assert.NotEqual(acquire.GetBaseDefinition(), acquire);
            Assert.NotEqual(acquireAsync.GetBaseDefinition(), acquireAsync);
        }

        [Fact]
        public void ProviderLockIsStillTransactionScoped()
        {
            // Recorded because our design is a deliberate departure from it: the provider's lock is
            // transaction scoped, ours is session scoped, and the difference is the whole reason our lock
            // survives the commit between two migrations while the provider's has to be retaken.  If the
            // provider ever moves to session or explicit scope, that argument needs revisiting — and so
            // does the comparison drawn in the README.

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .Options;

            using var context = new TestDbContext(options);

            var historyRepository = context.GetService<IHistoryRepository>();

            Assert.IsAssignableFrom<NpgsqlHistoryRepository>(historyRepository);
            Assert.Equal(LockReleaseBehavior.Transaction, historyRepository.LockReleaseBehavior);
        }
    }
}