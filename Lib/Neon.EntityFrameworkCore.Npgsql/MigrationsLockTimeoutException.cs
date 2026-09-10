//-----------------------------------------------------------------------------
// FILE:        MigrationsLockTimeoutException.cs
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
    /// Thrown by <see cref="AdvisoryLockHistoryRepository"/> when the migrations advisory lock
    /// couldn't be acquired within <see cref="AdvisoryLockMigrationsOptions.Timeout"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This nearly always means another migrator is holding the lock and hasn't finished — which is
    /// the mechanism working as intended, not a fault.  The usual response is to let the platform
    /// restart the job and try again, by which point the other migrator will normally have applied
    /// the migrations and this one will find nothing to do.
    /// </para>
    /// <note>
    /// <b>This deliberately does not derive from <see cref="TimeoutException"/>.</b>  It's the obvious
    /// base class, and it was the original one, but Npgsql's transient error detector classifies every
    /// <see cref="TimeoutException"/> as retryable — so Entity Framework Core's execution strategy
    /// swallowed this and re-threw it as <see cref="InvalidOperationException"/> ("An exception has
    /// been raised that is likely due to a transient failure"), leaving
    /// <c>catch (MigrationsLockTimeoutException)</c> silently dead and the real cause one
    /// <see cref="Exception.InnerException"/> down.  Deriving straight from <see cref="Exception"/>
    /// keeps it non-transient in Npgsql's eyes, so it reaches the caller intact.  Please don't
    /// "tidy" the base class back — the integration tests in <b>Test.Neon.EntityFrameworkCore</b>
    /// will catch it, but only if they're run against a live cluster.
    /// </note>
    /// </remarks>
    public class MigrationsLockTimeoutException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lockKey">The advisory lock key that couldn't be acquired.</param>
        /// <param name="timeout">The timeout that elapsed.</param>
        /// <param name="attempts">The number of acquisition attempts made.</param>
        public MigrationsLockTimeoutException(long lockKey, TimeSpan timeout, int attempts)
            : base($"Timed out after [{timeout}] and [{attempts}] attempt(s) waiting for the migrations advisory lock [{lockKey}].  Another migration is probably still running against this database.")
        {
            LockKey = lockKey;
            Timeout = timeout;
            Attempts = attempts;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">Optionally specifies the inner exception.</param>
        public MigrationsLockTimeoutException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Returns the advisory lock key that couldn't be acquired.
        /// </summary>
        public long LockKey { get; private set; }

        /// <summary>
        /// Returns the timeout that elapsed.
        /// </summary>
        public TimeSpan Timeout { get; private set; }

        /// <summary>
        /// Returns the number of acquisition attempts that were made.
        /// </summary>
        public int Attempts { get; private set; }
    }
}