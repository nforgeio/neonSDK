// -----------------------------------------------------------------------------
// FILE:	    DbSetExtensions.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

using Neon.Diagnostics;
using Neon.Tasks;

namespace Neon.EntityFrameworkCore
{
    /// <summary>
    /// Primary key oriented extension methods for <see cref="DbSet{TEntity}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Entity Framework Core's <see cref="DbSet{TEntity}.FindAsync(object[])"/> is the usual way
    /// to reach an entity by its primary key, but it materializes and tracks the entity, which is
    /// wasted work when all you want to know is whether a row exists or you simply want it gone.
    /// The methods here translate the same key based lookup into the narrowest query for the job.
    /// </para>
    /// <para>
    /// All of these methods work with composite primary keys.  Supply the key values positionally,
    /// in the order the key properties are declared on the entity.
    /// </para>
    /// <para><b>OBSERVABILITY</b></para>
    /// <para>
    /// Each method emits an OpenTelemetry activity when a listener is attached.  Call
    /// <see cref="TracerProviderBuilderExtensions.AddNeonEntityFrameworkCore(OpenTelemetry.Trace.TracerProviderBuilder)"/>
    /// to collect them.  Primary key <i>values</i> are never recorded as tags — only their count —
    /// because keys are so often personal data and traces are routinely exported off box.
    /// </para>
    /// </remarks>
    public static class DbSetExtensions
    {
        /// <summary>
        /// Determines whether a row with the given primary key exists, without materializing or
        /// tracking the entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dbSet">The <see cref="DbSet{TEntity}"/> to query.</param>
        /// <param name="keyValues">
        /// The primary key values, in the order the key properties are declared on the entity.
        /// </param>
        /// <param name="cancellationToken">Optionally specifies a cancellation token.</param>
        /// <returns><c>true</c> when the row exists.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbSet"/> or <paramref name="keyValues"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="TEntity"/> has no primary key.</exception>
        /// <remarks>
        /// <para>
        /// This translates to <c>SELECT EXISTS (SELECT 1 FROM ... WHERE key = ...)</c> or the
        /// provider's equivalent, so the row is never transferred to the client and nothing is
        /// added to the change tracker.
        /// </para>
        /// <note>
        /// Unlike <see cref="DbSet{TEntity}.FindAsync(object[])"/>, this always hits the database.
        /// It won't report <c>true</c> for an entity that has been added to the change tracker but
        /// not yet saved.
        /// </note>
        /// <example>
        /// <code language="csharp">
        /// if (await context.Users.ExistsAsync(new object[] { userId }, cancellationToken))
        /// {
        ///     // ...
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        public static async Task<bool> ExistsAsync<TEntity>(this DbSet<TEntity> dbSet, object[] keyValues, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            await SyncContext.Clear;

            ArgumentNullException.ThrowIfNull(dbSet, nameof(dbSet));
            ArgumentNullException.ThrowIfNull(keyValues, nameof(keyValues));

            using var activity = TraceContext.ActivitySource?.StartActivity(nameof(ExistsAsync), ActivityKind.Client);

            Describe(activity, dbSet, operation: "EXISTS", keyCount: keyValues.Length);

            try
            {
                var primaryKey = GetPrimaryKey(dbSet);
                var exists     = await dbSet.AnyAsync(
                    predicate:         BuildLambda<TEntity>(keyProperties: primaryKey.Properties, keyValues: new ValueBuffer(keyValues)),
                    cancellationToken: cancellationToken);

                activity.Tag(TraceTags.ResultExists, exists);

                return exists;
            }
            catch (Exception e)
            {
                activity.Error(e);
                throw;
            }
        }

        /// <summary>
        /// Determines whether a row with the given primary key exists, without materializing or
        /// tracking the entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dbSet">The <see cref="DbSet{TEntity}"/> to query.</param>
        /// <param name="keyValues">
        /// The primary key values, in the order the key properties are declared on the entity.
        /// </param>
        /// <returns><c>true</c> when the row exists.</returns>
        /// <remarks>
        /// <para>
        /// This overload exists for the common single column key case where passing a cancellation
        /// token would be more ceremony than the call is worth.  Use
        /// <see cref="ExistsAsync{TEntity}(DbSet{TEntity}, object[], CancellationToken)"/> when you
        /// need cancellation.
        /// </para>
        /// <example>
        /// <code language="csharp">
        /// if (await context.Users.ExistsAsync(userId))
        /// {
        ///     // ...
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        public static Task<bool> ExistsAsync<TEntity>(this DbSet<TEntity> dbSet, params object[] keyValues)
            where TEntity : class
        {
            return dbSet.ExistsAsync(keyValues: keyValues, cancellationToken: CancellationToken.None);
        }

        /// <summary>
        /// Deletes the row with the given primary key directly in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dbSet">The <see cref="DbSet{TEntity}"/> to delete from.</param>
        /// <param name="keyValues">
        /// The primary key values, in the order the key properties are declared on the entity.
        /// </param>
        /// <param name="cancellationToken">Optionally specifies a cancellation token.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbSet"/> or <paramref name="keyValues"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="TEntity"/> has no primary key.</exception>
        /// <remarks>
        /// <note>
        /// This is built on <see cref="EntityFrameworkQueryableExtensions.ExecuteDeleteAsync{TSource}(IQueryable{TSource}, CancellationToken)"/>,
        /// so the <c>DELETE</c> is sent immediately rather than being queued until
        /// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/>.  It bypasses the change
        /// tracker entirely, which means loaded copies of the entity are left stale and cascade
        /// behaviours configured in the model are <b>not</b> applied in memory — the database's own
        /// foreign key actions are what take effect.
        /// </note>
        /// <para>
        /// Deleting a key that isn't present is not an error; the statement simply affects no rows.
        /// </para>
        /// <example>
        /// <code language="csharp">
        /// await context.Users.DeleteAsync(new object[] { userId }, cancellationToken);
        /// </code>
        /// </example>
        /// </remarks>
        public static async Task DeleteAsync<TEntity>(this DbSet<TEntity> dbSet, object[] keyValues, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            await SyncContext.Clear;

            ArgumentNullException.ThrowIfNull(dbSet, nameof(dbSet));
            ArgumentNullException.ThrowIfNull(keyValues, nameof(keyValues));

            using var activity = TraceContext.ActivitySource?.StartActivity(nameof(DeleteAsync), ActivityKind.Client);

            Describe(activity, dbSet, operation: "DELETE", keyCount: keyValues.Length);

            try
            {
                var primaryKey   = GetPrimaryKey(dbSet);
                var rowsAffected = await dbSet
                    .Where(BuildLambda<TEntity>(keyProperties: primaryKey.Properties, keyValues: new ValueBuffer(keyValues)))
                    .ExecuteDeleteAsync(cancellationToken);

                activity.Tag(TraceTags.ResultRowsAffected, rowsAffected);
            }
            catch (Exception e)
            {
                activity.Error(e);
                throw;
            }
        }

        /// <summary>
        /// Deletes the row with the given primary key directly in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dbSet">The <see cref="DbSet{TEntity}"/> to delete from.</param>
        /// <param name="keyValues">
        /// The primary key values, in the order the key properties are declared on the entity.
        /// </param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <remarks>
        /// This overload exists for the common single column key case where passing a cancellation
        /// token would be more ceremony than the call is worth.  Use
        /// <see cref="DeleteAsync{TEntity}(DbSet{TEntity}, object[], CancellationToken)"/> when you
        /// need cancellation.
        /// </remarks>
        public static Task DeleteAsync<TEntity>(this DbSet<TEntity> dbSet, params object[] keyValues)
            where TEntity : class
        {
            return dbSet.DeleteAsync(keyValues: keyValues, cancellationToken: CancellationToken.None);
        }

        /// <summary>
        /// Stages an insert or an update for an entity depending on whether its primary key is
        /// already present in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dbSet">The <see cref="DbSet{TEntity}"/> to upsert into.</param>
        /// <param name="value">The entity to insert or update.</param>
        /// <param name="cancellationToken">Optionally specifies a cancellation token.</param>
        /// <returns>The tracking <see cref="Task"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="dbSet"/> or <paramref name="value"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <typeparamref name="TEntity"/> has no primary key.</exception>
        /// <remarks>
        /// <note>
        /// This only <b>stages</b> the change: you still need to call
        /// <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> for it to reach the database.
        /// </note>
        /// <note>
        /// The existence check and the eventual write are separate round trips, so this is not
        /// atomic on its own.  A concurrent insert of the same key in between can turn the staged
        /// insert into a duplicate key violation at <c>SaveChangesAsync()</c> time.  Where that
        /// matters, either run the upsert inside a serializable transaction or use your provider's
        /// native merge support (<c>INSERT ... ON CONFLICT</c> on PostgreSQL).
        /// </note>
        /// <example>
        /// <code language="csharp">
        /// await context.Users.UpsertAsync(user, cancellationToken);
        /// await context.SaveChangesAsync(cancellationToken);
        /// </code>
        /// </example>
        /// </remarks>
        public static async Task UpsertAsync<TEntity>(this DbSet<TEntity> dbSet, TEntity value, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            await SyncContext.Clear;

            ArgumentNullException.ThrowIfNull(dbSet, nameof(dbSet));
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            using var activity = TraceContext.ActivitySource?.StartActivity(nameof(UpsertAsync), ActivityKind.Client);

            try
            {
                var primaryKey = GetPrimaryKey(dbSet);

                Describe(activity, dbSet, operation: "UPSERT", keyCount: primaryKey.Properties.Count);

            var pkValues = new object[primaryKey.Properties.Count];

            for (int i = 0; i < primaryKey.Properties.Count; i++)
            {
                pkValues[i] = typeof(TEntity).GetProperty(primaryKey.Properties[i].Name).GetValue(value);
            }

                var predicate = BuildLambda<TEntity>(keyProperties: primaryKey.Properties, keyValues: new ValueBuffer(pkValues));

            if (await dbSet.AnyAsync(predicate: predicate, cancellationToken: cancellationToken))
            {
                    activity.Tag(TraceTags.UpsertAction, "update");

                dbSet.Update(value);
            }
            else
            {
                    activity.Tag(TraceTags.UpsertAction, "insert");

                await dbSet.AddAsync(value, cancellationToken: cancellationToken);
            }
        }
            catch (Exception e)
            {
                activity.Error(e);
                throw;
            }
        }

        /// <summary>
        /// Returns the primary key of <typeparamref name="TEntity"/>, failing with a message that
        /// names the type rather than with a bare <see cref="NullReferenceException"/>.
        /// </summary>
        private static IKey GetPrimaryKey<TEntity>(DbSet<TEntity> dbSet)
            where TEntity : class
        {
            var primaryKey = dbSet.EntityType.FindPrimaryKey();

            if (primaryKey == null)
            {
                throw new InvalidOperationException(
                    $"Entity type [{typeof(TEntity).FullName}] has no primary key, so it cannot be located by key.  " +
                    $"Keyless entity types are not supported by the [DbSetExtensions] key based methods.");
            }

            return primaryKey;
        }

        /// <summary>
        /// Tags <paramref name="activity"/> with the entity/table being operated on.  Does nothing
        /// when nothing is listening, so the model metadata lookups are never paid for by
        /// deployments that don't export traces.
        /// </summary>
        private static void Describe<TEntity>(Activity activity, DbSet<TEntity> dbSet, string operation, int keyCount)
            where TEntity : class
        {
            if (activity == null)
            {
                return;
            }

            var entityType = dbSet.EntityType;
            var table      = entityType.GetTableName();
            var schema     = entityType.GetSchema();

            activity.Tag(TraceTags.DbOperationName, operation)
                    .Tag(TraceTags.EntityType, typeof(TEntity).FullName)
                    .Tag(TraceTags.KeyCount, keyCount);

            if (table != null)
            {
                activity.Tag(TraceTags.DbCollectionName, table);
            }

            if (schema != null)
            {
                activity.Tag(TraceTags.DbNamespace, schema);
            }

            // Follow the OpenTelemetry database span naming convention of "{operation} {target}".

            activity.DisplayName = table == null ? operation : $"{operation} {table}";
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "ExpressionExtensions.BuildPredicate() is the only practical way to build a composite key predicate from a ValueBuffer.")]
        private static Expression<Func<TEntity, bool>> BuildLambda<TEntity>(IReadOnlyList<IProperty> keyProperties, ValueBuffer keyValues)
        {
            var entityParameter = Expression.Parameter(typeof(TEntity), "e");

            return Expression.Lambda<Func<TEntity, bool>>(
                ExpressionExtensions.BuildPredicate(keyProperties, keyValues, entityParameter), entityParameter);
        }
    }
}
