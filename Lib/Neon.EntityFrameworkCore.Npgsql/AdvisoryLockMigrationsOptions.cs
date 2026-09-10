//-----------------------------------------------------------------------------
// FILE:        AdvisoryLockMigrationsOptions.cs
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
    /// Configures <see cref="AdvisoryLockHistoryRepository"/>.  Obtain an instance via the
    /// <c>configure</c> callback on
    /// <see cref="DbContextOptionsBuilderExtensions.UseAdvisoryLockMigrationHistory(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder, Action{AdvisoryLockMigrationsOptions})"/>.
    /// </summary>
    /// <remarks>
    /// The defaults deliberately mirror the behaviour of the <c>LOCK TABLE</c> statement they
    /// replace: a derived lock key and an indefinite wait.  Override them only when you have a
    /// reason to.
    /// </remarks>
    public class AdvisoryLockMigrationsOptions
    {
        private TimeSpan pollInterval = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Overrides the advisory lock key.  Defaults to <c>null</c>, meaning the key is derived
        /// from the schema and name of the migrations history table.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Advisory locks live in a single cluster wide 64 bit key space that is not tied to any
        /// table, so mutual exclusion depends entirely on independent processes computing the same
        /// number.  Set this explicitly when the derived key isn't what you want:
        /// </para>
        /// <list type="bullet">
        ///     <item>
        ///     To make two <see cref="Microsoft.EntityFrameworkCore.DbContext"/>s with
        ///     <b>different</b> history tables serialize against each other — for example when
        ///     several bounded contexts migrate the same physical database and must not interleave.
        ///     </item>
        ///     <item>
        ///     To move off a key that collides with an advisory lock your application already uses
        ///     for something else.  A collision only ever causes excess serialization, never lost
        ///     exclusion, so this is a performance concern rather than a correctness one.
        ///     </item>
        /// </list>
        /// <note>
        /// Whatever you choose must be stable across processes and across restarts.  Never derive it
        /// from <see cref="object.GetHashCode()"/> on a <see cref="string"/> — .NET randomizes
        /// string hashing per process, so two migrators would compute different keys, fail to
        /// exclude each other, and never tell you about it.
        /// </note>
        /// </remarks>
        public long? LockKey { get; set; } = null;

        /// <summary>
        /// How long to wait for the lock before giving up.  Defaults to <c>null</c>, meaning wait
        /// indefinitely.
        /// </summary>
        /// <remarks>
        /// <para>
        /// With the default of <c>null</c>, <c>pg_advisory_lock()</c> is used and the call
        /// blocks in the server until the lock is granted.  This matches the <c>LOCK TABLE</c>
        /// statement it replaces and is the safe default, but a migrator that is stuck behind a
        /// stalled peer looks exactly like a migrator that is stuck for any other reason.
        /// </para>
        /// <para>
        /// Setting a timeout switches to polling <c>pg_try_advisory_lock()</c> every
        /// <see cref="PollInterval"/>, and raises <see cref="MigrationsLockTimeoutException"/> once
        /// the timeout elapses.  A diagnosable failure is usually a much better experience in CI and
        /// in orchestrated deployments where the platform will restart the job anyway.
        /// </para>
        /// <note>
        /// The timeout covers only the wait for the lock.  It does not bound how long the migrations
        /// themselves take once the lock is held.
        /// </note>
        /// </remarks>
        public TimeSpan? Timeout { get; set; } = null;

        /// <summary>
        /// How long to wait between attempts when <see cref="Timeout"/> is set.  Defaults to one
        /// second.  Ignored when <see cref="Timeout"/> is <c>null</c>.
        /// </summary>
        /// <remarks>
        /// The final wait is shortened as needed so that the total wait never overshoots
        /// <see cref="Timeout"/>.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when set to a value that isn't positive.</exception>
        public TimeSpan PollInterval
        {
            get => pollInterval;

            set
            {
                if (value <= TimeSpan.Zero)
                {
                    throw new ArgumentOutOfRangeException(nameof(PollInterval), value, $"[{nameof(PollInterval)}] must be positive.");
                }

                pollInterval = value;
            }
        }

        /// <summary>
        /// Validates the option combination, throwing when it can't work.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="Timeout"/> isn't positive.</exception>
        internal void Validate()
        {
            if (Timeout.HasValue && Timeout.Value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(Timeout), Timeout.Value, $"[{nameof(Timeout)}] must be positive when specified.  Use [null] to wait indefinitely.");
            }
        }

        /// <summary>
        /// Returns an independent copy, so that the instance handed to the caller's configuration
        /// callback can't be mutated after the options have been frozen into the
        /// <see cref="Microsoft.EntityFrameworkCore.DbContextOptions"/>.
        /// </summary>
        internal AdvisoryLockMigrationsOptions Clone()
        {
            return new AdvisoryLockMigrationsOptions()
            {
                LockKey = LockKey,
                Timeout = Timeout,
                PollInterval = PollInterval
            };
        }
    }
}