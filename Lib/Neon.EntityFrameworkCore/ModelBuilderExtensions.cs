// -----------------------------------------------------------------------------
// FILE:	    ModelBuilderExtensions.cs
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
using System.Diagnostics;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Neon.Diagnostics;
namespace Neon.EntityFrameworkCore
{
    /// <summary>
    /// Extension methods for <see cref="ModelBuilder"/>, intended to be called from
    /// <see cref="DbContext.OnModelCreating(ModelBuilder)"/>.
    /// </summary>
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Applies a model wide value converter that stores every <see cref="DateTime"/> property
        /// as UTC and stamps the given <see cref="DateTimeKind"/> onto values read back.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/> being configured.</param>
        /// <param name="kind">
        /// The <see cref="DateTimeKind"/> to specify on values materialized from the database.
        /// This is nearly always <see cref="DateTimeKind.Utc"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="modelBuilder"/> is <c>null</c>.</exception>
        /// <remarks>
        /// <para>
        /// Most database providers discard the <see cref="DateTime.Kind"/> flag on the way in and
        /// hand back <see cref="DateTimeKind.Unspecified"/> on the way out, which makes it easy to
        /// accumulate <see cref="DateTime"/> values whose kind depends on whether they came from the
        /// database or from memory.  Calling this once removes that whole class of bug:
        /// </para>
        /// <list type="bullet">
        ///     <item>
        ///     <b>Writing</b> — <see cref="DateTime.ToUniversalTime()"/> is applied, so a local
        ///     time is converted and an unspecified time is <i>assumed to be local</i> and
        ///     converted.  Values already marked UTC pass through untouched.
        ///     </item>
        ///     <item>
        ///     <b>Reading</b> — <see cref="DateTime.SpecifyKind(DateTime, DateTimeKind)"/> is
        ///     applied with <paramref name="kind"/>.  This relabels the value; it does not shift it.
        ///     </item>
        /// </list>
        /// <note>
        /// Because unspecified values are treated as local on write, the round trip is only lossless
        /// when <paramref name="kind"/> is <see cref="DateTimeKind.Utc"/>.  Passing
        /// <see cref="DateTimeKind.Local"/> relabels UTC values as local without shifting them,
        /// which corrupts them by the server's UTC offset.  Passing
        /// <see cref="DateTimeKind.Unspecified"/> is a no-op relabel and leaves you where you
        /// started.
        /// </note>
        /// <note>
        /// This must be called <b>after</b> your entities have been added to the model — put it at
        /// the end of <see cref="DbContext.OnModelCreating(ModelBuilder)"/>.  Properties discovered
        /// after the call don't get a converter.
        /// </note>
        /// <note>
        /// <b>On Npgsql this must be paired with <c>timestamp with time zone</c> columns</b>, which is
        /// the default mapping for <see cref="DateTime"/>.  Npgsql accepts only
        /// <see cref="DateTimeKind.Utc"/> values for that column type and only
        /// <see cref="DateTimeKind.Unspecified"/> values for <c>timestamp without time zone</c>, so
        /// pointing this at a naive column produces a write the driver refuses.  Against
        /// <c>timestamptz</c> the read side is a no-op — Npgsql already returns UTC — and the value is
        /// all on the write side: a local or unspecified <see cref="DateTime"/> that Npgsql would
        /// otherwise reject at <c>SaveChanges()</c> time is normalized into the UTC instant it meant.
        /// </note>
        /// <example>
        /// <code language="csharp">
        /// protected override void OnModelCreating(ModelBuilder modelBuilder)
        /// {
        ///     base.OnModelCreating(modelBuilder);
        ///
        ///     // ...configure entities...
        ///
        ///     modelBuilder.SetDefaultDateTimeKind(DateTimeKind.Utc);
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        public static void SetDefaultDateTimeKind(this ModelBuilder modelBuilder, DateTimeKind kind)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));

            using var activity = TraceContext.ActivitySource?.StartActivity(nameof(SetDefaultDateTimeKind));

            activity.Tag(TraceTags.DateTimeKind, kind.ToString());

            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, kind));

            var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, kind) : v);

            var converted = 0;

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (entityType.IsKeyless)
                {
                    continue;
                }

                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                        converted++;
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                        converted++;
                    }
                }
            }

            activity.Tag(TraceTags.ModelPropertiesConverted, converted);
        }
    }
}
