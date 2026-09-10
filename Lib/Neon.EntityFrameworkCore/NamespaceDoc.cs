//-----------------------------------------------------------------------------
// FILE:        NamespaceDoc.cs
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

namespace Neon.EntityFrameworkCore
{
    /// <summary>
    /// This namespace includes provider agnostic Entity Framework Core extensions and utilities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="DbSetExtensions"/> extends <see cref="DbSet{TEntity}"/> with primary key oriented
    /// operations that avoid materializing and tracking an entity you didn't want:
    /// <list type="table">
    ///     <item>
    ///         <term><see cref="DbSetExtensions.ExistsAsync{TEntity}(DbSet{TEntity}, object[], System.Threading.CancellationToken)"/></term>
    ///         <description>
    ///         Determines whether a row with the given primary key exists, translating to an
    ///         <c>EXISTS</c> query rather than a fetch.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="DbSetExtensions.DeleteAsync{TEntity}(DbSet{TEntity}, object[], System.Threading.CancellationToken)"/></term>
    ///         <description>
    ///         Deletes the row with the given primary key immediately, without loading it first.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="DbSetExtensions.UpsertAsync{TEntity}(DbSet{TEntity}, TEntity, System.Threading.CancellationToken)"/></term>
    ///         <description>
    ///         Stages an insert or an update depending on whether the entity's primary key is already
    ///         present in the database.
    ///         </description>
    ///     </item>
    /// </list>
    /// </para>
    /// <para>
    /// <see cref="ModelBuilderExtensions"/> extends <see cref="ModelBuilder"/> with
    /// <see cref="ModelBuilderExtensions.SetDefaultDateTimeKind(ModelBuilder, DateTimeKind)"/>, which
    /// applies a model wide value converter so that every <see cref="DateTime"/> property round trips
    /// with a known <see cref="DateTimeKind"/> instead of coming back as
    /// <see cref="DateTimeKind.Unspecified"/>.
    /// </para>
    /// <para>
    /// Everything here emits OpenTelemetry activities.  <see cref="TracerProviderBuilderExtensions"/>
    /// subscribes to them and <see cref="TraceTags"/> documents the tags they carry.
    /// </para>
    /// <para>
    /// Provider specific behaviour lives in its own package.  In particular, <b>Neon.EntityFrameworkCore.Npgsql</b>
    /// adds an advisory lock based migrations history repository that makes <c>Database.Migrate()</c>
    /// work against YugabyteDB and other Postgres wire compatible databases that don't implement
    /// <c>LOCK TABLE</c>.
    /// </para>
    /// </remarks>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
}