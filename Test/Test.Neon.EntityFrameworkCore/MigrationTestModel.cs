//-----------------------------------------------------------------------------
// FILE:        MigrationTestModel.cs
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
using System.Globalization;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Test.Neon.EntityFrameworkCore.Migrations
{
    /// <summary>
    /// A widget, created by the first migration.
    /// </summary>
    public class Widget
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// A sprocket, created by the second migration.
    /// </summary>
    public class Sprocket
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    /// <summary>
    /// A context with hand written migrations, used to exercise the migration lock end to end.
    /// </summary>
    /// <remarks>
    /// <note>
    /// The migrations below are written by hand rather than scaffolded.  Entity Framework Core
    /// discovers migrations by reflection — any <see cref="Migration"/> subclass carrying a
    /// <see cref="MigrationAttribute"/> and a matching <see cref="DbContextAttribute"/> — so no design
    /// time tooling or model snapshot is needed to <i>apply</i> them.
    /// </note>
    /// </remarks>
    public class MigrationTestDbContext : DbContext
    {
        public MigrationTestDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Widget> Widgets { get; set; }
        public DbSet<Sprocket> Sprockets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Widget>().ToTable("widgets");
            modelBuilder.Entity<Sprocket>().ToTable("sprockets");
        }
    }

    /// <summary>
    /// Lets a test park the second migration mid-run, so that a competing migrator can be caught
    /// trying to acquire the migrations lock while the first migrator still holds it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is how the "lock survives the commit boundary" case is tested.  Entity Framework Core
    /// commits after each migration and begins a new transaction for the next one, so a transaction
    /// scoped lock dies at every commit and has to be retaken.  Holding the <i>second</i> migration
    /// open and proving a competitor still can't get in is what distinguishes a correct reacquire from
    /// one that leaves the rest of the run unprotected.
    /// </para>
    /// <para>
    /// The gate is a second advisory lock on <see cref="Key"/>, which the test takes on its own
    /// session before starting the migrator.  When gating is on, the second migration's first act is to
    /// ask for that same key, so it blocks <i>in the server</i> — holding its transaction, and
    /// therefore the migrations lock, open for exactly as long as the test wants and no longer.  A
    /// timed sleep would be doing the same job by guesswork.
    /// </para>
    /// <note>
    /// A static is safe here only because the migration tests share a non-parallel xUnit collection.
    /// </note>
    /// </remarks>
    public static class MigrationGate
    {
        /// <summary>
        /// Where in the second migration the gate is placed.
        /// </summary>
        public enum GatePosition
        {
            /// <summary>
            /// No gate: the migration runs straight through.
            /// </summary>
            None = 0,

            /// <summary>
            /// Park before doing any work, so that "the second migration has not been applied" is an
            /// unambiguous statement while a test observes it.
            /// </summary>
            BeforeWork,

            /// <summary>
            /// Park after the schema change but before the commit — the state an application would be in
            /// if it died partway through a migration.
            /// </summary>
            AfterWork
        }

        /// <summary>
        /// The advisory lock key used as the gate.  Any value distinct from the migrations lock key
        /// works; this one is arbitrary.
        /// </summary>
        public const long Key = 0x0A7E_0A7E;

        /// <summary>
        /// A second key the migration takes immediately before parking at
        /// <see cref="GatePosition.AfterWork"/>.
        /// </summary>
        /// <remarks>
        /// This is how a test knows the schema change has actually executed.  It can't look for the table
        /// itself — that's uncommitted and invisible from another session — but an advisory lock held by
        /// the migration's transaction <i>is</i> visible, so probing for this key turns "the migration has
        /// done its work and is now parked" into something observable rather than something to sleep on.
        /// </remarks>
        public const long WorkDoneKey = 0x0A7E_D01E;

        /// <summary>
        /// Where the second migration should park.  Defaults to <see cref="GatePosition.None"/>.
        /// </summary>
        public static GatePosition Position { get; set; } = GatePosition.None;

        /// <summary>
        /// Emits the gate acquisition when <paramref name="position"/> is the configured one.
        /// </summary>
        internal static void Apply(MigrationBuilder migrationBuilder, GatePosition position)
        {
            if (Position != position)
            {
                return;
            }

            if (position == GatePosition.AfterWork)
            {
                Lock(migrationBuilder, WorkDoneKey);
            }

            Lock(migrationBuilder, Key);
        }

        private static void Lock(MigrationBuilder migrationBuilder, long key)
        {
            migrationBuilder.Sql($"SELECT pg_advisory_xact_lock({key.ToString(CultureInfo.InvariantCulture)})");
        }
    }

    /// <summary>
    /// The first migration: creates the <b>widgets</b> table.
    /// </summary>
    [DbContext(typeof(MigrationTestDbContext))]
    [Migration("20240101000001_CreateWidgets")]
    public class CreateWidgets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "widgets",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_widgets", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("widgets");
        }
    }

    /// <summary>
    /// The second migration: creates the <b>sprockets</b> table, optionally parking partway through.
    /// </summary>
    [DbContext(typeof(MigrationTestDbContext))]
    [Migration("20240101000002_CreateSprockets")]
    public class CreateSprockets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            MigrationGate.Apply(migrationBuilder, MigrationGate.GatePosition.BeforeWork);

            migrationBuilder.CreateTable(
                name: "sprockets",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sprockets", x => x.Id);
                });

            MigrationGate.Apply(migrationBuilder, MigrationGate.GatePosition.AfterWork);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("sprockets");
        }
    }
}