//-----------------------------------------------------------------------------
// FILE:	    AssemblyInternals.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
//
// The contents of this repository are for private use by NEONFORGE, LLC. and may not be
// divulged or used for any purpose by other organizations or individuals without a
// formal written and signed agreement with NEONFORGE, LLC.

using Xunit;

// Disable parallel test execution because [TestFixture] doesn't
// support this in general.

[assembly: CollectionBehavior(DisableTestParallelization = true, MaxParallelThreads = 1)]