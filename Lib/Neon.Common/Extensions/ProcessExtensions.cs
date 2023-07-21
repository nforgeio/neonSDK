//-----------------------------------------------------------------------------
// FILE:        ProcessExtensions.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Neon.Common
{
    /// <summary>
    /// <see cref="string"/> extension methods.
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// Signals the process to be killed and then waits for it to exit.
        /// Note that <see cref="Process.Kill"/> only initiates process
        /// termination but that the process actually dies asynchronously,
        /// whereas the method actually waits for the process to terminate.
        /// </summary>
        /// <param name="process">The target process.</param>
        public static void KillNow(this Process process)
        {
            Covenant.Requires<ArgumentNullException>(process != null, nameof(process));

            process.Kill();
            process.WaitForExit();
        }
    }
}
