//-----------------------------------------------------------------------------
// FILE:	    Program.DotnetVersion.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
                Program.Exit(1);
            }

            if (!File.Exists(globalJsonPath))
            {
                Console.Error.WriteLine($"*** ERROR: [global.json] file not found: {globalJsonPath}");
                Program.Exit(1);
            }

            var response = NeonHelper.ExecuteCapture("dotnet", new object[] { "--info" }, workingDirectory: Path.GetDirectoryName(globalJsonPath))
                .EnsureSuccess();

            // The information text will look something like this:

            //    .NET SDK:
            //     Version:   7.0.102
            //     Commit:    4bbdd14480
            //
            //    Runtime Environment:
            //     OS Name:     Windows
            //     OS Version:  10.0.19045
            //     OS Platform: Windows
            //     RID:         win10-x64
            //     Base Path:   C:\Program Files\dotnet\sdk\7.0.102\
            //
            //    Host:
            //      Version:      7.0.2
            //      Architecture: x64
            //      Commit:       d037e070eb
            //
            //    .NET SDKs installed:                                                                                                   
            //      3.1.426 [C:\Program Files\dotnet\sdk]
            //      5.0.408 [C:\Program Files\dotnet\sdk]
            //      6.0.101 [C:\Program Files\dotnet\sdk]
            //      6.0.112 [C:\Program Files\dotnet\sdk]
            //      6.0.307 [C:\Program Files\dotnet\sdk]
            //      6.0.404 [C:\Program Files\dotnet\sdk]
            //      7.0.100 [C:\Program Files\dotnet\sdk]                
            //      7.0.101 [C:\Program Files\dotnet\sdk]
            //      7.0.102 [C:\Program Files\dotnet\sdk]
            //
            //    .NET runtimes installed:                                                                                               
            //      Microsoft.AspNetCore.App 3.1.25 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
            //      Microsoft.AspNetCore.App 3.1.32 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
            //      Microsoft.AspNetCore.App 5.0.17 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
            //      Microsoft.AspNetCore.App 6.0.1 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
            //      Microsoft.AspNetCore.App 6.0.4 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
            //      Microsoft.AspNetCore.App 6.0.9 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
            //      Microsoft.AspNetCore.App 6.0.12 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
            //      Microsoft.AspNetCore.App 6.0.13 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
            //      Microsoft.AspNetCore.App 7.0.2 [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
            //      Microsoft.NETCore.App 3.1.25 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
            //      Microsoft.NETCore.App 3.1.32 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
            //      Microsoft.NETCore.App 5.0.17 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
            //      Microsoft.NETCore.App 6.0.1 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
            //      Microsoft.NETCore.App 6.0.4 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
            //      Microsoft.NETCore.App 6.0.9 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
            //      Microsoft.NETCore.App 6.0.12 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
            //      Microsoft.NETCore.App 6.0.13 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
            //      Microsoft.NETCore.App 7.0.1 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
            //      Microsoft.NETCore.App 7.0.2 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
            //      Microsoft.WindowsDesktop.App 3.1.25 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
            //      Microsoft.WindowsDesktop.App 3.1.32 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
            //      Microsoft.WindowsDesktop.App 5.0.17 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
            //      Microsoft.WindowsDesktop.App 6.0.1 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
            //      Microsoft.WindowsDesktop.App 6.0.4 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
            //      Microsoft.WindowsDesktop.App 6.0.9 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
            //      Microsoft.WindowsDesktop.App 6.0.12 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
            //      Microsoft.WindowsDesktop.App 6.0.13 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
            //      Microsoft.WindowsDesktop.App 7.0.2 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
            //
            //    Other architectures found:
            //      x86   [C:\Program Files (x86)\dotnet]
            //        registered at [HKLM\SOFTWARE\dotnet\Setup\InstalledVersions\x86\InstallLocation]
            //
            //    Environment variables:
            //      Not set
            //
            //    global.json file:
            //      C:\src\neonKUBE\global.json
            //
            //    Learn more:
            //      https://aka.ms/dotnet/info
            //
            //    Download .NET:
            //      https://aka.ms/dotnet/download

            // $hack(jefflill): Hopefully this format won't change in the future.
            //
            // We're going to parse the versions from the [.NET SDK] and [Host] sections.

            var infoText       = response.OutputText;
            var sdkVersion     = string.Empty;
            var runtimeVersion = string.Empty;
            var inSdkSection   = false;
            var inHostSection  = false;

            using (var reader = new StringReader(infoText))
            {
                foreach (var line in reader.Lines())
                {
                    if (line.Length == 0)
                    {
                        inSdkSection  = false;
                        inHostSection = false;

                        continue;
                    }
                    else if ((inSdkSection || inHostSection) && !char.IsWhiteSpace(line.First()))
                    {
                        inSdkSection  = false;
                        inHostSection = false;
                    }

                    var trimmed = line.Trim();

                    if (trimmed.StartsWith(".NET SDK"))
                    {
                        inSdkSection = true;

                        continue;
                    }
                    else if (trimmed.StartsWith("Host:"))
                    {
                        inHostSection = true;

                        continue;
                    }

                    string version;

                    if (trimmed.StartsWith("Version:"))
                    {
                        var fields = trimmed.Split(':', 2 );

                        version = fields[1].Trim();
                    }
                    else
                    {
                        continue;
                    }

                    if (inSdkSection)
                    {
                        sdkVersion = version;
                    }
                    else if (inHostSection)
                    {
                        runtimeVersion = version;
                    }

                    if (!string.IsNullOrEmpty(sdkVersion) && !string.IsNullOrEmpty(runtimeVersion))
                    {
                        // We have both of the versions now.

                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(sdkVersion) || string.IsNullOrEmpty(runtimeVersion))
            {
                Console.WriteLine("*** ERROR: Cannot locate SDK/runtime versions from: dotnet --info");
                Program.Exit(1);
            }

            Console.WriteLine(sdkVersion);
            Console.WriteLine(runtimeVersion);
            Program.Exit(0);
        }
    }
}
