//-----------------------------------------------------------------------------
// FILE:        RunCommand.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Neon.Common;
using Neon.IO;
using Neon.WSL;

namespace WslUtil
{
    /// <summary>
    /// Implements the <b>run</b> command.
    /// </summary>
    [Command]
    public class RunCommand : CommandBase
    {
        private const string usage = @"
Uploads a Bash script to the target WSL distro and executes it.

USAGE:

    wsl-util run SCRIPT-PATH [DISTRO-NAME]

ARGUMENTS:

    SCRIPT-PATH     - Path to the script file on Windows
    DISTRO-NAME     - Optionally identifies the target WSL distro
                      defaults to: neon-ubuntu-20.04

REMARKS:

This command returns the exitcode and output from the script.

";
        /// <inheritdoc/>
        public override string[] Words => new string[] { "run" };

        /// <inheritdoc/>
        public override void Help()
        {
            Console.WriteLine(usage);
        }

        /// <inheritdoc/>
        public override async Task RunAsync(CommandLine commandLine)
        {
            if (commandLine.HasHelpOption || commandLine.Arguments.Length == 0)
            {
                Console.WriteLine(usage);
                Program.Exit(commandLine.HasHelpOption ? 0 : -1);
            }

            Console.WriteLine();

            var scriptPath = commandLine.Arguments.ElementAtOrDefault(0);
            var distroName = commandLine.Arguments.ElementAtOrDefault(1) ?? "neon-ubuntu-20.04";

            if (string.IsNullOrEmpty(scriptPath))
            {
                Console.Error.WriteLine("*** ERROR: SCRIPT-PATH argument expected.");
                Program.Exit(-1);
            }

            Console.WriteLine();

            // Ensure that the WSL distro exists.

            if (!Wsl2Proxy.List().Any(name => name.Equals(distroName, StringComparison.CurrentCultureIgnoreCase)))
            {
                Console.Error.WriteLine($"*** ERROR: WSL distro [{distroName}] does not exist.");
                Program.Exit(1);
            }

            var distro = new Wsl2Proxy(distroName);

            if (!distro.IsDebian)
            {
                Console.Error.WriteLine($"*** ERROR: The [{distro.Name}] distro is running: {distro.OSRelease["ID"]}/{distro.OSRelease["ID_LIKE"]}");
                Console.Error.WriteLine($"           A Debian/Ubuntu based distribution is required.");
                Program.Exit(1);
            }

            var response = distro.ExecuteScript(File.ReadAllText(scriptPath));

            Console.Write(response.AllText);
            Program.Exit(response.ExitCode);

            await Task.CompletedTask;
        }
    }
}
