//-----------------------------------------------------------------------------
// FILE:        Test_Tracing.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Microsoft.EntityFrameworkCore;

using Neon.EntityFrameworkCore;
using Neon.Xunit;

using Xunit;

using NpgsqlTracing = Neon.EntityFrameworkCore.Npgsql.TracerProviderBuilderExtensions;

namespace Test.Neon.EntityFrameworkCore
{
    /// <summary>
    /// Tests the OpenTelemetry instrumentation.
    /// </summary>
    /// <remarks>
    /// These use a raw <see cref="ActivityListener"/> rather than the OpenTelemetry SDK, because what
    /// matters is the <see cref="ActivitySource"/> contract the SDK subscribes to: the source name,
    /// the activity names, and the tags.
    /// </remarks>
    [Trait(TestTrait.Category, TestArea.NeonEntityFrameworkCore)]
    public class Test_Tracing
    {
        private const string TestActivitySourceName = "Test.Neon.EntityFrameworkCore";

        /// <summary>
        /// Collects the activities a source produces while <paramref name="action"/> runs.
        /// </summary>
        /// <remarks>
        /// <note>
        /// <see cref="ActivityListener"/> subscriptions are process wide, so a naive implementation
        /// would also collect activities produced by test classes xUnit is running in parallel with
        /// this one.  The action is wrapped in a root activity and the results are filtered by trace
        /// ID, which keeps each call to this method looking only at its own work.
        /// </note>
        /// </remarks>
        private static List<Activity> Collect(string activitySourceName, Action action)
        {
            var collected = new List<Activity>();
            var syncRoot  = new object();

            using var testSource = new ActivitySource(TestActivitySourceName);

            using var listener = new ActivityListener()
            {
                ShouldListenTo  = source => source.Name == activitySourceName || source.Name == TestActivitySourceName,
                Sample          = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStopped = activity =>
                {
                    lock (syncRoot)
                    {
                        collected.Add(activity);
                    }
                }
            };

            ActivitySource.AddActivityListener(listener);

            using var root = testSource.StartActivity("collect");

            Assert.NotNull(root);

            action();

            lock (syncRoot)
            {
                return collected
                    .Where(activity => activity.TraceId == root.TraceId && activity.Source.Name == activitySourceName)
                    .ToList();
            }
        }

        [Fact]
        public void ActivitySourceNames_MatchTheAssemblyNames()
        {
            // These strings are what consumers put in AddSource() and what shows up as
            // otel.library.name in the backend, so they're part of the contract.

            Assert.Equal("Neon.EntityFrameworkCore", TracerProviderBuilderExtensions.ActivitySourceName);
            Assert.Equal("Neon.EntityFrameworkCore.Npgsql", NpgsqlTracing.ActivitySourceName);
        }

        [Fact]
        public void AddSourceExtensions_ValidateArguments()
        {
            Assert.Throws<ArgumentNullException>(() => TracerProviderBuilderExtensions.AddNeonEntityFrameworkCore(null));
            Assert.Throws<ArgumentNullException>(() => NpgsqlTracing.AddNeonEntityFrameworkCoreNpgsql(null));
        }

        [Fact]
        public void SetDefaultDateTimeKind_IsTraced()
        {
            // The activity is emitted during model building, and Entity Framework Core normally builds
            // a model once per context type per process.  TestModel.CreateOptions() replaces the model
            // cache key factory so this test gets a real build to watch.

            var activities = Collect(
                TracerProviderBuilderExtensions.ActivitySourceName,
                () =>
                {
                    using var context = new TestDbContext(TestModel.CreateOptions(), setDefaultDateTimeKind: true);

                    _ = context.Model;
                });

            var activity = Assert.Single(activities.Where(activity => activity.OperationName == "SetDefaultDateTimeKind"));

            Assert.Equal(ActivityKind.Internal, activity.Kind);
            Assert.Equal(nameof(DateTimeKind.Utc), activity.GetTagItem(TraceTags.DateTimeKind));

            // Person contributes CreatedAt and DeletedAt, Membership contributes JoinedAt.  The
            // keyless PersonCount is skipped.

            Assert.Equal(3, activity.GetTagItem(TraceTags.ModelPropertiesConverted));
        }

        [Fact]
        public void SetDefaultDateTimeKind_CostsNothingWhenNobodyIsListening()
        {
            // No listener attached, so StartActivity() returns null and every tag value goes
            // uncomputed.  This asserts that the null path doesn't throw, which is the failure mode a
            // missing null check would produce.

            using var context = new TestDbContext(TestModel.CreateOptions(), setDefaultDateTimeKind: true);

            Assert.NotNull(context.Model);
        }

        [Fact]
        public void KeyBasedOperations_RecordFailuresOnTheActivity()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .Options;

            var activities = Collect(
                TracerProviderBuilderExtensions.ActivitySourceName,
                () =>
                {
                    using var context = new TestDbContext(options);

                    // The keyless entity fails before any database access, which gives us a
                    // deterministic failure to observe.

                    Assert.ThrowsAsync<InvalidOperationException>(() => context.PersonCounts.ExistsAsync(1)).Wait();
                });

            var activity = Assert.Single(activities.Where(activity => activity.OperationName == "ExistsAsync"));

            Assert.Equal(ActivityKind.Client, activity.Kind);
            Assert.Equal(ActivityStatusCode.Error, activity.Status);
            Assert.Contains(activity.Events, e => e.Name == "exception");
        }

        [Fact]
        public void KeyBasedOperations_DescribeTheTargetTable()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .Options;

            var activities = Collect(
                TracerProviderBuilderExtensions.ActivitySourceName,
                () =>
                {
                    using var context = new TestDbContext(options);

                    // Forces the model to build before the activity starts, so the tags reflect the
                    // mapped table rather than a half-built model.

                    _ = context.Model;

                    // This reaches the database and fails there, which is fine — the tags we care
                    // about are set before the query runs.

                    try
                    {
                        context.Memberships.ExistsAsync(new object[] { Guid.NewGuid(), Guid.NewGuid() }).Wait();
                    }
                    catch (Exception)
                    {
                        // Expected: there's no server behind the connection string.
                    }
                });

            var activity = Assert.Single(activities.Where(activity => activity.OperationName == "ExistsAsync"));

            Assert.Equal("EXISTS", activity.GetTagItem(TraceTags.DbOperationName));
            Assert.Equal("memberships", activity.GetTagItem(TraceTags.DbCollectionName));
            Assert.Equal("tenancy", activity.GetTagItem(TraceTags.DbNamespace));
            Assert.Equal(typeof(Membership).FullName, activity.GetTagItem(TraceTags.EntityType));
            Assert.Equal(2, activity.GetTagItem(TraceTags.KeyCount));
            Assert.Equal("EXISTS memberships", activity.DisplayName);
        }

        [Fact]
        public void DelegatingOverloads_DontDoubleCount()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseNpgsql(TestDbContext.OfflineConnectionString)
                .Options;

            var activities = Collect(
                TracerProviderBuilderExtensions.ActivitySourceName,
                () =>
                {
                    using var context = new TestDbContext(options);

                    Assert.ThrowsAsync<InvalidOperationException>(() => context.PersonCounts.ExistsAsync(1)).Wait();
                });

            // The params overload delegates to the real one, so exactly one span should come out.

            Assert.Single(activities.Where(activity => activity.OperationName == "ExistsAsync"));
        }
    }
}