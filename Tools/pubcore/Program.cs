//-----------------------------------------------------------------------------
// FILE:        Program.cs
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

// This tool hacks the publishing of a .NET Core console application to
// an external output folder as part of a build.  We can't use dotnet
// publish within a post-build event because this causes recursive builds
// so we're just going to hack this.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace pubcore
{
    /// <summary>
    /// Program class.
    /// </summary>
    public static class Program
    {
        private static bool verbose;

        /// <summary>
        /// Tool version number.
        /// </summary>
        public const string Version = "3.2";

        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine($"NEON PUBCORE v{Version}");

                var sbCommandLine = new StringBuilder("pubcore");

                foreach (var arg in args)
                {
                    sbCommandLine.Append(' ');

                    if (arg.Contains(' '))
                    {
                        sbCommandLine.Append($"\"{arg}\"");
                    }
                    else
                    {
                        sbCommandLine.Append(arg);
                    }
                }

                Console.WriteLine();
                Console.WriteLine(sbCommandLine.ToString());
                Console.WriteLine();

                // Parse the command line options and then remove any options
                // from the arguments.

                var noCmd   = args.Any(arg => arg == "--no-cmd");
                var keepXml = args.Any(arg => arg == "--keep-xml");

                verbose = args.Any(arg => arg == "--verbose");

                args = args.Where(arg => !arg.StartsWith("--")).ToArray();

                // Verify the number of non-option arguments.

                if (args.Length != 3 && args.Length != 4)
                {
                    Console.WriteLine(
$@"
NEON PUBCORE v{Version}

usage: pubcore [OPTIONS] PROJECT-PATH TARGET-FRAMEWORK CONFIG OUTPUT-PATH RUNTIME

ARGUMENTS:

    PROJECT-PATH        - Path to the [.csproj] file
    CONFIG              - Build configuration (like: Debug or Release)
    OUTPUT-DIR          - Path to the output directory
    [RUNTIME]           - Optionally specifies the target dotnet runtime, like: win10-x64.
                          This is obtained from the [.csproj] file when not specified.

OPTIONS:

    --no-cmd            - Do not generate a [PROJECT-NAME.cmd] file in PUBLISH-DIR
                          that executes the published program.
    --keep-xml          - Don't remove published [*.xml] files.  We remove these by
                          default, assuming that they're generatedcode comment files.

    --verbose           - Output debug info

REMARKS:

This utility is designed to be called from within a .NET Core project's
POST-BUILD event using Visual Studio post-build event macros.  Here's
an example that publishes a standalone [win10-x64] app to: %NF_BUILD%\neon

    pubcore ""$(ProjectPath)"" ""$(ConfigurationName)"" ""%NF_BUILD%\neon""

Note that you MUST ADD something like the following to a <PropertyGroup> section in
your project CSPROJ file for this to work:

    <PropertyGroup>
        <TargetFramework>net7.0</TargetFramework>
        <RuntimeIdentifier>win10-x64</RuntimeIdentifier>
    </PropertyGroup>

This command publishes the executable files to a new PUBLISH-DIR/TARGET-FRAMEWORK directory
then creates a CMD.EXE batch file named PUBLISH-DIR/TARGET-FRAMEWORK.cmd that launches the
application, forwarding any command line arguments.

The [--no-cmd] option prevents the CMD.EXE batch file from being created.

DISABLING PUBLICATION

Publishing an application can take some time and may not work in some situations
due to Visual Studio keeping files open.  We try to mitigate the latter by retrying
file delete/copy operations but sometimes these mitgations fail as well.

You can set this environment variable to disable publication:

    NEON_PUBCORE_DISABLE=true

This tool does nothing when it sees this variable.  You can set this in build
scripts or MSBUILD/CSPROJ files to quickly disable publication.
");
                    Environment.Exit(1);
                }

                // Abort when we see: NEON_PUBCORE_DISABLE=true

                if ("true".Equals(Environment.GetEnvironmentVariable("NEON_PUBCORE_DISABLE"), StringComparison.InvariantCultureIgnoreCase))
                {
                    LogVerbose($"Aborting because NEON_PUBCORE_DISABLE is defined");
                    Environment.Exit(0);
                }

                // We need to examine/set an environment variable to prevent the [dotnet publish...]
                // command below from recursively invoking the project build event that will invoke
                // the program again.

                const string recursionVar = "unix-text-68A780E5-00E7-4158-B5DE-E95C1D284911";

                if (Environment.GetEnvironmentVariable(recursionVar) == "true")
                {
                    // Looks like we've recursed, so just bail right now.

                    LogVerbose($"Aborting due to recursion.");
                    return;
                }

                Environment.SetEnvironmentVariable(recursionVar, "true");

                // Parse the arguments.

                var projectPath = args[0];

                if (!File.Exists(projectPath))
                {
                    Console.WriteLine($"Project file [{projectPath}] does not exist.");
                    Environment.Exit(1);
                }

                var config      = args[1];
                var outputDir   = Path.Combine(Path.GetDirectoryName(projectPath), args[2]);
                var programName = Path.GetFileName(outputDir);  // $hack(jefflill): assuming the program name is the same as the last segment in the output directory.
                var runtime     = args.ElementAtOrDefault(3);

                // Ensure that the output folder exists.

                Directory.CreateDirectory(outputDir);

                // Time how long publication takes.

                var stopwatch = new Stopwatch();

                stopwatch.Start();

                // It appears that [dotnet publish] is sometimes unable to write
                // output files due to locks being held by somebody else (I'm guessing
                // some part of the Visual Studio build process).  This appears to
                // be transient.  We're going to mitigate this by introducing an
                // initial 5 second delay to hopefully avoid the situation entirely
                // and then try the operation up to five times.

                var tryCount      = 5;
                var delay         = TimeSpan.FromSeconds(5);
                var runtimeOption = string.Empty;

                if (!string.IsNullOrEmpty(runtime))
                {
                    runtimeOption = $"-r {runtime}";
                }

                for (int i = 0; i < tryCount; i++)
                {
                    var process  = new Process();
                    var sbOutput = new StringBuilder();

                    process.StartInfo.FileName               = "dotnet.exe";
                    process.StartInfo.Arguments              = $"publish \"{projectPath}\" {runtimeOption} -c \"{config}\" -o \"{outputDir}\"";
                    process.StartInfo.CreateNoWindow         = true;
                    process.StartInfo.UseShellExecute        = false;
                    process.StartInfo.RedirectStandardError  = true;
                    process.StartInfo.RedirectStandardOutput = true;

                    process.OutputDataReceived += (s, e) => sbOutput.AppendLine(e.Data);
                    process.ErrorDataReceived  += (s, e) => sbOutput.AppendLine(e.Data);

                    if (i > 0)
                    {
                        Console.WriteLine($"===========================================================");
                        Console.WriteLine($"PUBCORE RETRY: dotnet {process.StartInfo.Arguments}");
                        Console.WriteLine($"===========================================================");
                    }

                    process.Start();
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    process.WaitForExit();

                    // Write the output captured from the [dotnet publish ...] operation to the program
                    // STDOUT, prefixing each line with some text so that MSBUILD/Visual Studio won't
                    // try to interpret error/warning messages.

                    using (var reader = new StringReader(sbOutput.ToString()))
                    {
                        for (var line = reader.ReadLine(); line != null; line = reader.ReadLine())
                        {
                            Console.WriteLine($"   publish: {line}");
                        }
                    }

                    // Report any errors and handle retries.

                    if (process.ExitCode != 0)
                    {
                        if (i < tryCount - 1)
                        {
                            Console.Error.WriteLine($"warning: [dotnet publish] failed with [exitcode={process.ExitCode}].  Retrying after [{delay}].");
                            Thread.Sleep(delay);
                        }
                        else
                        {
                            Console.Error.WriteLine($"error: [dotnet publish] failed with [exitcode={process.ExitCode}].");
                            Environment.Exit(process.ExitCode);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                // Create the CMD shell script when not disabled.

                var cmdPath = Path.Combine(Path.Combine(outputDir, ".."), $"{programName}.cmd");

                if (!noCmd)
                {
                    File.WriteAllText(cmdPath,
$@"@echo off
""%~dp0\{programName}\{programName}.exe"" %*
");
                }
                else
                {
                    // Delete any existing CMD.EXE script.

                    if (File.Exists(cmdPath))
                    {
                        File.Delete(cmdPath);
                    }
                }

                // For some bizarre reason, [dotnet publish] copies [dotnet.exe] to the publish
                // folder and this is causing trouble running [dotnet] commands for other apps.
                // I'm also seeing other random DLLs being published as well for single-file
                // executables, which is really strange.
                //
                // This might be a new Visual Studio 2022 (bad?) behavior.  I'm going to mitigate
                // by removing the [dotnet.exe] file from the publish folder if present.

                var dotnetPath = Path.Combine(outputDir, "dotnet.exe");

                if (File.Exists(dotnetPath))
                {
                    File.Delete(dotnetPath);
                }

                // Publish also writes the code comment XML files to the publish folder.  We're
                // going to delete all XML files there as a quick workaround by default.

                if (!keepXml)
                {
                    foreach (var xmlPath in Directory.GetFiles(outputDir, "*.xml", SearchOption.TopDirectoryOnly))
                    {
                        File.Delete(xmlPath);
                    }
                }

                // Finish up

                Console.WriteLine();
                Console.WriteLine($"Publish time:   {stopwatch.Elapsed}");
                Console.WriteLine();

                Environment.Exit(0);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"** ERROR: [{e.GetType().Name}]: {e.Message}");

                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Logs a message to STDERR when [--verbose] was specified.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void LogVerbose(string message)
        {
            if (verbose)
            {
                Console.Error.WriteLine($"DEBUG: {message}");
            }
        }
    }
}
