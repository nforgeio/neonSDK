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

namespace Neon.EntityFrameworkCore.Npgsql
{
    /// <summary>
    /// This namespace includes Entity Framework Core extensions specific to the Npgsql PostgreSQL
    /// provider.
    /// </summary>
    /// <remarks>
    /// <para><b>MIGRATION LOCKING FOR YUGABYTEDB</b></para>
    /// <para>
    /// Entity Framework Core 9 began holding a database lock across a migration run, and the Npgsql
    /// provider implements that lock as <c>LOCK TABLE "__EFMigrationsHistory" IN ACCESS EXCLUSIVE
    /// MODE</c>.  YugabyteDB doesn't implement explicit table locks, so every migration fails with
    /// <b>0A000: ACCESS EXCLUSIVE not supported yet</b>.
    /// </para>
    /// <para>
    /// <see cref="AdvisoryLockHistoryRepository"/> takes a PostgreSQL session scoped advisory lock
    /// instead, which preserves the mutual exclusion Entity Framework Core intended while using only
    /// statements YugabyteDB supports.  Enable it with
    /// <see cref="DbContextOptionsBuilderExtensions.UseAdvisoryLockMigrationHistory(DbContextOptionsBuilder, Action{AdvisoryLockMigrationsOptions})"/>:
    /// </para>
    /// <code language="csharp">
    /// optionsBuilder
    ///     .UseNpgsql(connectionString)
    ///     .UseAdvisoryLockMigrationHistory();
    /// </code>
    /// <para>
    /// <see cref="AdvisoryLockMigrationsOptions"/> configures the lock key and the wait behaviour, and
    /// <see cref="MigrationsLockTimeoutException"/> is what you get when a configured wait timeout
    /// elapses.
    /// </para>
    /// <note>
    /// This is opt-in on purpose.  On real PostgreSQL the provider's own <c>LOCK TABLE</c>
    /// implementation works and is stricter, so it should stay in place there.
    /// </note>
    /// <note>
    /// Advisory locks are enabled by default from YugabyteDB 2025.0 onward.  On 2.25 through 2024.x
    /// they're a preview feature that has to be switched on explicitly — see
    /// <see cref="AdvisoryLockHistoryRepository"/> for the flags and for what it looks like when
    /// they're missing.
    /// </note>
    /// <para><b>OBSERVABILITY</b></para>
    /// <para>
    /// Lock acquisition is traced.  <see cref="TracerProviderBuilderExtensions"/> subscribes to the
    /// activities and <see cref="NpgsqlTraceTags"/> documents the tags they carry, which include the
    /// lock key — the thing you actually need in order to confirm that two migrators are excluding
    /// each other.
    /// </para>
    /// </remarks>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
}