// -----------------------------------------------------------------------------
// FILE:	    BasicFixture.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Collections.Generic;
using System.Text;

namespace Neon.Xunit
{
    /// <summary>
    /// Used by unit tests that need to know when a test run is started vs. another
    /// test execution within the same run.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Simply derive your test from this and then call <see cref="TestFixture.Start(Action)"/>
    /// (passing <c>null</c>) within your test constructor and examine the result.
    /// <see cref="TestFixtureStatus.Started"/> indicates that the fixture has just
    /// started the first test in the run and <see cref="TestFixtureStatus.AlreadyRunning"/>
    /// is returned for subsequent tests  in the run.
    /// </para>
    /// <para>
    /// Alternatively, you can pass an <see cref="Action"/> to <see cref="TestFixture.Start(Action)"/>
    /// that performs any required operations when the test fixture is first started.
    /// </para>
    /// </remarks>
    public class BasicFixture : TestFixture
    {
        /// <summary>
        /// Constructs the fixture.
        /// </summary>
        public BasicFixture()
        {
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~BasicFixture()
        {
            Dispose(false);
        }
    }
}
