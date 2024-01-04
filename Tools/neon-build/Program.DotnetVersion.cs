//-----------------------------------------------------------------------------
// FILE:        Program.DotnetVersion.cs
// CONTRIBUTOR: Jeff Lill
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
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

using Neon.Common;

namespace NeonBuild
{
    public static partial class Program
    {
        /// <summary>
        /// <para>
        /// Calls <b>dotnet --info</b> with the working directory holding the <b>global.json</b> 
        /// file specified on the command line and parses the .NET SDK version (like "7.0.102")
        /// as well as the .NET runtime version (like "7.0.2").
        /// </para>
        /// <para>
        /// The command writes the SDK version to the first output line and the corresponding
        /// runtime version to the second line.
        /// </para>
        /// </summary>
        /// <param name="commandLine">The command line.</param>
        private static void DotnetVersion(CommandLine commandLine)
        {
            var globalJsonPath = commandLine.Arguments.ElementAtOrDefault(1);

            if (string.IsNullOrEmpty(globalJsonPath))
            {
                Console.Error.WriteLine("*** ERROR: GLOBAL-JSON-PATH argument is required.");
                Program.Exit(-1);
            }

            if (!File.Exists(globalJsonPath))
            {
                Console.Error.WriteLine($"*** ERROR: [global.json] file not found: {globalJsonPath}");
                Program.Exit(1);
            }

            var globalJson        = NeonHelper.JsonDeserialize<dynamic>(File.ReadAllText(globalJsonPath));
            var sdkVersion        = globalJson["sdk"]["version"];
            var runtimeConfigPath = $"C:\\Program Files\\dotnet\\sdk\\{sdkVersion}\\dotnet.runtimeconfig.json";
            var runtimeConfig     = NeonHelper.JsonDeserialize<dynamic>(File.ReadAllText(runtimeConfigPath));
            var runtimeVersion    = runtimeConfig["runtimeOptions"]["framework"]["version"];

            Console.WriteLine(runtimeVersion);
            Program.Exit(0);
        }
    }
}
