//-----------------------------------------------------------------------------
// FILE:        TraceTags.cs
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
using System.Diagnostics;

namespace Neon.EntityFrameworkCore
{
    /// <summary>
    /// The activity tag (attribute) names emitted by <b>Neon.EntityFrameworkCore</b>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Where an OpenTelemetry semantic convention exists we use it verbatim so that backends
    /// which understand database spans light up without extra configuration.  Everything that
    /// has no convention is namespaced under <b>neon.efcore.*</b> so it can never collide with
    /// a future convention.
    /// </para>
    /// <note>
    /// These names are part of this library's observable contract: renaming one is a breaking
    /// change for anybody who has built a dashboard or an alert on it.
    /// </note>
    /// </remarks>
    public static class TraceTags
    {
        //---------------------------------------------------------------------
        // OpenTelemetry database semantic conventions.

        /// <summary>
        /// <b>db.collection.name</b>: the database table backing the entity being operated on.
        /// </summary>
        public const string DbCollectionName = "db.collection.name";

        /// <summary>
        /// <b>db.namespace</b>: the schema qualifying <see cref="DbCollectionName"/>, when the
        /// entity is explicitly mapped to one.
        /// </summary>
        public const string DbNamespace = "db.namespace";

        /// <summary>
        /// <b>db.operation.name</b>: the logical operation being performed, e.g. <b>EXISTS</b>,
        /// <b>DELETE</b> or <b>UPSERT</b>.  This is the operation this library is performing,
        /// not the individual SQL statements Entity Framework Core ends up sending.
        /// </summary>
        public const string DbOperationName = "db.operation.name";

        /// <summary>
        /// <b>db.system.name</b>: the database management system, e.g. <b>postgresql</b>.  Only
        /// set by provider specific packages that actually know the answer.
        /// </summary>
        public const string DbSystemName = "db.system.name";

        //---------------------------------------------------------------------
        // Neon specific tags.

        /// <summary>
        /// <b>neon.efcore.entity.type</b>: the full CLR type name of the entity.
        /// </summary>
        public const string EntityType = "neon.efcore.entity.type";

        /// <summary>
        /// <b>neon.efcore.key.count</b>: the number of primary key values supplied by the caller.
        /// </summary>
        /// <remarks>
        /// <note>
        /// Only the <i>count</i> is recorded.  Key values are deliberately never captured because
        /// they are frequently personal or otherwise sensitive data, and traces are routinely
        /// exported to third party backends.
        /// </note>
        /// </remarks>
        public const string KeyCount = "neon.efcore.key.count";

        /// <summary>
        /// <b>neon.efcore.result.exists</b>: the outcome of an existence check.
        /// </summary>
        public const string ResultExists = "neon.efcore.result.exists";

        /// <summary>
        /// <b>neon.efcore.result.rows_affected</b>: the number of rows affected by the operation.
        /// </summary>
        public const string ResultRowsAffected = "neon.efcore.result.rows_affected";

        /// <summary>
        /// <b>neon.efcore.upsert.action</b>: whether an upsert resolved to <b>insert</b> or <b>update</b>.
        /// </summary>
        public const string UpsertAction = "neon.efcore.upsert.action";

        /// <summary>
        /// <b>neon.efcore.datetime.kind</b>: the <see cref="DateTimeKind"/> applied by
        /// <see cref="ModelBuilderExtensions.SetDefaultDateTimeKind(Microsoft.EntityFrameworkCore.ModelBuilder, DateTimeKind)"/>.
        /// </summary>
        public const string DateTimeKind = "neon.efcore.datetime.kind";

        /// <summary>
        /// <b>neon.efcore.model.properties_converted</b>: the number of model properties a
        /// model building operation actually modified.
        /// </summary>
        public const string ModelPropertiesConverted = "neon.efcore.model.properties_converted";
    }
}