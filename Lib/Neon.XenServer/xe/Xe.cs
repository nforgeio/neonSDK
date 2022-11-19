/* Copyright (c) Citrix Systems, Inc. 
 * All rights reserved. 
 * 
 * Redistribution and use in source and binary forms, 
 * with or without modification, are permitted provided 
 * that the following conditions are met: 
 * 
 * *   Redistributions of source code must retain the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer. 
 * *   Redistributions in binary form must reproduce the above 
 *     copyright notice, this list of conditions and the 
 *     following disclaimer in the documentation and/or other 
 *     materials provided with the distribution. 
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND 
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF 
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, 
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE 
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 * SUCH DAMAGE.
 */

#pragma warning disable CS1591

using System;
using System.Text;

using CommandLib;
using Neon.Common;

namespace ThinCLI
{
    internal static class MainClass
    {
        // $note(jefflill):
        //
        // The only real code change required was to have the [Main()] method
        // capture the output text and exitcode and return thouse within an
        // [ExecuteResponse].  The [thinCLIProtocol] delegates make this easy.
        //
        // The only real trick required was that the [delegateExit] handler 
        // needs to thown an exception with the exit code and the [Main()]
        // method needs to handle that.

        public static ExecuteResponse Main(string[] args)
        {
            var sbOutput = new StringBuilder();

            try
            {
                var tCliProtocol = new thinCLIProtocol(
                    dGlobalError:      s => sbOutput.AppendLine(s),
                    dGlobalUsage:      () => { },
                    dGlobalDebug:      (s, tCliProtocol) => { },
                    dConsoleWrite:     s => sbOutput.Append(s),
                    dConsoleWriteLine: s => sbOutput.AppendLine(s),
                    dConsoleReadLine:  () => "y",
                    dExit:             exitCode => throw new ProgramExitException(exitCode),
                    dProgress:         i => { },
                    conf:              new Config());

                string body = "";
                char[] eqsep = { '=' };

                for (int i = 0; i < args.Length; i++)
                {
                    string s = args[i];
                    try
                    {
                        if (s.StartsWith("server"))
                        {
                            tCliProtocol.conf.hostname = s.Split(eqsep)[1];
                        }
                        else if (s.Equals("-s"))
                        {
                            tCliProtocol.conf.hostname = args[++i];
                        }
                        else if (s.Equals("-u"))
                        {
                            tCliProtocol.conf.username = args[++i];
                        }
                        else if (s.Equals("-pw"))
                        {
                            tCliProtocol.conf.password = args[++i];
                        }
                        else if (s.Equals("-p"))
                        {
                            tCliProtocol.conf.port = Int32.Parse(args[++i]);
                        }
                        else if (s.Equals("--nossl"))
                        {
                            tCliProtocol.conf.nossl = true;
                        }
                        else if (s.Equals("-debug"))
                        {
                            tCliProtocol.conf.debug = true;
                        }
                        else if (s.Equals("-version"))
                        {
                            Console.WriteLine("ThinCLI protocol: " + tCliProtocol.major + "." + tCliProtocol.minor);
                            Environment.Exit(0);
                        }
                        else
                        {
                            body += s + "\n";
                        }
                    }
                    catch
                    {
                        Error("Failed to parse command-line arguments");
                        Usage();
                        Environment.Exit(1);
                    }
                }

                if (tCliProtocol.conf.hostname.Equals(""))
                {
                    Error("No hostname was specified.");
                    Usage();
                    Environment.Exit(1);
                }

                Messages.performCommand(body, tCliProtocol);
                return new ExecuteResponse(0, outputText: sbOutput.ToString());
            }
            catch (ProgramExitException e)
            {
                return new ExecuteResponse(e.ExitCode, outputText: sbOutput.ToString());
            }
            catch (Exception e)
            {
                sbOutput.AppendLine(NeonHelper.ExceptionError(e));
                return new ExecuteResponse(1, outputText: sbOutput.ToString());
            }
        }

        private static void Error(string x)
        {
            Console.WriteLine("Error: " + x);
        }

        private static void Debug(string msg, thinCLIProtocol tCliProtocol)
        {
            if (tCliProtocol.conf.debug)
                Console.WriteLine("Debug: " + msg);
        }

        private static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  xe -s <server> -u <username> -pw <password> [-p <port>] <command> <arguments>");
            Console.WriteLine("For help, use xe -s <server> -u <user> -pw <password> [-p <port>] help");
        }
    }
}
