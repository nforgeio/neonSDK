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
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Neon.EntityFrameworkCore
{
    /// <summary>
    /// Extension methods for DbSet.
    /// </summary>
    public static class DbSetExtensions
    {
        /// <summary>
        /// Checks if the entity exists in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dbSet">The DbSet instance.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="keyValues">The key values of the entity.</param>
        /// <returns>True if the entity exists in the database, otherwise false.</returns>
        public static async Task<bool> ExistsAsync<TEntity>(this DbSet<TEntity> dbSet, object[] keyValues, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var primaryKey = dbSet.EntityType.FindPrimaryKey();

            return await dbSet.AnyAsync(BuildLambda<TEntity>(keyProperties: primaryKey.Properties, keyValues: new ValueBuffer(keyValues)));
        }

        /// <summary>
        /// Checks if the entity exists in the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dbSet">The DbSet instance.</param>
        /// <param name="keyValues">The key values of the entity.</param>
        /// <returns>True if the entity exists in the database, otherwise false.</returns>
        public static Task<bool> ExistsAsync<TEntity>(this DbSet<TEntity> dbSet, params object[] keyValues)
            where TEntity : class
        {
            return dbSet.ExistsAsync(keyValues: keyValues, cancellationToken: CancellationToken.None);
        }

        /// <summary>
        /// Deletes the entity from the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dbSet">The DbSet instance.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="keyValues">The key values of the entity.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task DeleteAsync<TEntity>(this DbSet<TEntity> dbSet, object[] keyValues, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var primaryKey = dbSet.EntityType.FindPrimaryKey();

            return dbSet.Where(BuildLambda<TEntity>(keyProperties: primaryKey.Properties, keyValues: new ValueBuffer(keyValues))).ExecuteDeleteAsync(cancellationToken);
        }

        /// <summary>
        /// Deletes the entity from the database.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dbSet">The DbSet instance.</param>
        /// <param name="keyValues">The key values of the entity.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static Task DeleteAsync<TEntity>(this DbSet<TEntity> dbSet, params object[] keyValues)
            where TEntity : class
        {
            return dbSet.DeleteAsync(keyValues: keyValues, cancellationToken: CancellationToken.None);
        }

        /// <summary>
        /// Upserts a value by checking if it exists in the database and either updates or inserts it.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="dbSet">The DbSet instance.</param>
        /// <param name="value">The value to upsert.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task UpsertAsync<TEntity>(this DbSet<TEntity> dbSet, TEntity value, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var primaryKey = dbSet.EntityType.FindPrimaryKey();

            var pkValues = new object[primaryKey.Properties.Count];
            for (int i = 0; i < primaryKey.Properties.Count; i++)
            {
                pkValues[i] = typeof(TEntity).GetProperty(primaryKey.Properties[i].Name).GetValue(value);
            }

            var predicate = BuildLambda<TEntity>(keyProperties: primaryKey.Properties, keyValues: new ValueBuffer(pkValues.ToArray()));

            if (await dbSet.AnyAsync(predicate: predicate, cancellationToken: cancellationToken))
            {
                dbSet.Update(value);
            }
            else
            {
                await dbSet.AddAsync(value, cancellationToken: cancellationToken);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<Pending>")]
        private static Expression<Func<TEntity, bool>> BuildLambda<TEntity>(IReadOnlyList<IProperty> keyProperties, ValueBuffer keyValues)
        {
            var entityParameter = Expression.Parameter(typeof(TEntity), "e");

            return Expression.Lambda<Func<TEntity, bool>>(
                ExpressionExtensions.BuildPredicate(keyProperties, keyValues, entityParameter), entityParameter);
        }
    }
}
