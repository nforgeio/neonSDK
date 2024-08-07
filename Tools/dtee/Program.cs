//-----------------------------------------------------------------------------
// FILE:        Program.cs
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
using System.Text;
using System.Threading.Tasks;

namespace DTee
{
    /// <summary>
    /// Hosts the program entry point.
    /// </summary>
    public static class Program
    {
        public const string Version = "1.0";

        private const string usage =
$@"
dtee (double-tee) v{Version}
Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.

Used to execute a command optionally redirecting STDOUT and STDERR to files
while also continuing to write these to STDOUT and STDERR.

USAGE: dtee [--out:PATH] [--err:PATH] [--both:PATH] -- COMMAND

    --out=PATH  - specifies the file path where STDOUT will be written
    --err=PATH  - specifies the file path where STDERR will be written
    --both=PATH - specifies the file path where that STDOUT and STDERR
                  will be written
    --quiet     - specifies that STDOUT and STDERR streams should not
                  be written to the process output

    COMMAND     - specifies the batch command to be executed passed as
                  a single string

NOTE: This tool currently is compatible with Windows only.

NOTE: Lines written to the BOTH file from the standard streams
      might be somewhat mixed up.

";

        /// <summary>
        /// The program entrypoint.
        /// </summary>
        /// <param name="args">Specifies the command line arguments.</param>
        public static void Main(string[] args)
        {
            // Print usage when no argumwents were passed.

            if (args.Length == 0)
            {
                Console.WriteLine(usage);
                Environment.Exit(1);
            }

            //-----------------------------------------------------------------
            // Parse the command line arguments.

            var outPath  = (string)null;
            var errPath  = (string)null;
            var bothPath = (string)null;
            var quiet    = false;
            var command  = (string)null;
            var sepIndex = -1;

            // Look for the command separator.

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--")
                {
                    sepIndex = i;
                    break;
                }
            }

            if (sepIndex == -1)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("*** ERROR: Missing \"--\" followed by COMMAND");
                Console.Error.WriteLine();
                Environment.Exit(1);
            }

            // Make sure there's only one COMMAND argument after the separator.

            if (args.Length == sepIndex + 1)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("*** ERROR: Missing COMMAND after \"--\"");
                Console.Error.WriteLine();
                Environment.Exit(1);
            }
            else if (args.Length > sepIndex + 2)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("*** ERROR: Extra arguments after COMMAND");
                Console.Error.WriteLine();
                Environment.Exit(1);
            }

            command = args[sepIndex + 1];

            // Parse the output path options.

            for (int i = 0; i < sepIndex; i++)
            {
                var option = args[i];

                if (option.StartsWith("--out="))
                {
                    outPath = option.Substring("--out=".Length);
                }
                else if (option.StartsWith("--err="))
                {
                    errPath = option.Substring("--err=".Length);
                }
                else if (option.StartsWith("--both="))
                {
                    bothPath = option.Substring("--both=".Length);
                }
                else if (option == "--quiet")
                {
                    quiet = true;
                }
                else
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine($"*** ERROR: Invalid option: {option}");
                    Console.Error.WriteLine();
                    Environment.Exit(1);
                }
            }

            //-----------------------------------------------------------------
            // Execute the command, writing the command streams to the files
            // specified by the options as well as to this process's standard
            // output streams.

            var syncRoot   = new object();
            var encoding   = Encoding.UTF8;
            var outWriter  = outPath != null ? new StreamWriter(File.Create(outPath), encoding) : (StreamWriter)null;
            var errWriter  = errPath != null ? new StreamWriter(File.Create(errPath), encoding) : (StreamWriter)null;
            var bothWriter = bothPath != null ? new StreamWriter(File.Create(bothPath), encoding) : (StreamWriter)null;
            var exitcode   = 1;

            try
            {
                var processInfo = new ProcessStartInfo("cmd", $"/c {command}")
                {
                    UseShellExecute        = false,
                    RedirectStandardError  = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow         = true,
                    StandardOutputEncoding = encoding,
                    StandardErrorEncoding  = encoding
                };

                using var process = new Process()
                {
                    StartInfo = processInfo
                };

                process.OutputDataReceived +=
                    (s, a) =>
                    {
                        lock (syncRoot)
                        {
                            if (!quiet)
                            {
                                Console.Out.WriteLine(a.Data);
                            }

                            outWriter?.WriteLine(a.Data);
                            bothWriter?.WriteLine(a.Data);
                        }
                    };

                process.ErrorDataReceived +=
                    (s, a) =>
                    {
                        lock (syncRoot)
                        {
                            if (!quiet)
                            {
                                Console.Error.WriteLine(a.Data);
                            }

                            errWriter?.WriteLine(a.Data);
                            bothWriter?.WriteLine(a.Data);
                        }
                    };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                exitcode = process.ExitCode;
            }
            finally
            {
                outWriter?.Dispose();
                errWriter?.Dispose();
                bothWriter?.Dispose();
            }

            Environment.Exit(exitcode);
        }
    }
}
