//-----------------------------------------------------------------------------
// FILE:        NpgsqlTraceTags.cs
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

namespace Neon.EntityFrameworkCore.Npgsql
{
    /// <summary>
    /// The activity tag (attribute) names emitted by <b>Neon.EntityFrameworkCore.Npgsql</b>, in
    /// addition to the OpenTelemetry database convention tags declared by
    /// <see cref="Neon.EntityFrameworkCore.TraceTags"/>.
    /// </summary>
    /// <remarks>
    /// <note>
    /// These names are part of this package's observable contract: renaming one is a breaking change
    /// for anybody who has built a dashboard or an alert on it.
    /// </note>
    /// </remarks>
    public static class NpgsqlTraceTags
    {
        /// <summary>
        /// <b>neon.efcore.migrations.history_table</b>: the schema qualified migrations history table
        /// whose name the lock key was derived from.
        /// </summary>
        public const string HistoryTable = "neon.efcore.migrations.history_table";

        /// <summary>
        /// <b>neon.efcore.migrations.lock_key</b>: the 64 bit advisory lock key.  Compare this across
        /// two migrator processes to confirm they are actually excluding each other.
        /// </summary>
        public const string LockKey = "neon.efcore.migrations.lock_key";

        /// <summary>
        /// <b>neon.efcore.migrations.lock_mode</b>: <b>blocking</b> when the lock was taken with
        /// <c>pg_advisory_lock()</c>, or <b>polling</b> when it was taken by retrying
        /// <c>pg_try_advisory_lock()</c> under a timeout.
        /// </summary>
        public const string LockMode = "neon.efcore.migrations.lock_mode";

        /// <summary>
        /// <b>neon.efcore.migrations.lock_timeout_seconds</b>: the configured wait timeout.  Absent
        /// when waiting indefinitely.
        /// </summary>
        public const string LockTimeoutSeconds = "neon.efcore.migrations.lock_timeout_seconds";

        /// <summary>
        /// <b>neon.efcore.migrations.lock_attempts</b>: how many acquisition attempts were made.  A
        /// value above one means this migrator queued behind another.
        /// </summary>
        public const string LockAttempts = "neon.efcore.migrations.lock_attempts";

        /// <summary>
        /// <b>neon.efcore.migrations.lock_acquired</b>: whether the lock was ultimately acquired.
        /// </summary>
        public const string LockAcquired = "neon.efcore.migrations.lock_acquired";

        /// <summary>
        /// <b>neon.efcore.migrations.lock_reacquired</b>: <c>true</c> on the spans produced when Entity
        /// Framework Core reopened the connection part way through a run, replacing the session that held
        /// the lock, so it had to be taken again.  A healthy run has none of these — the lock is taken
        /// once and spans every migration.
        /// </summary>
        public const string LockReacquired = "neon.efcore.migrations.lock_reacquired";

        /// <summary>
        /// Set on the release activity: whether the advisory lock was given back successfully.  A
        /// <c>false</c> here isn't fatal — the session's end releases the lock anyway — but a run of them
        /// means the prompt release path is broken.
        /// </summary>
        public const string LockReleased = "neon.efcore.migrations.lock_released";
    }
}