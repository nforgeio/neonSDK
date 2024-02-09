//-----------------------------------------------------------------------------
// FILE:        TailwindMiddleware.cs
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;

namespace Neon.Tailwind
{
    internal class NodeRunner : IDisposable
    {
        private Process process;

        [UnsupportedOSPlatform("browser")]
        public NodeRunner(string executable, string[] args, CancellationToken cancellationToken = default)
        {
            var pidFile = $"{AppContext.BaseDirectory}tailwind.pid";

            if (File.Exists(pidFile))
            {
                try 
                { 
                    var pid = int.Parse(File.ReadAllText(pidFile));
                    Process.GetProcesses().Where(p => p.Id == pid).FirstOrDefault()?.Kill(true);
                }
                catch
                {
                    // not running
                }
            }
            
            var processStartInfo = new ProcessStartInfo(executable)
            {
                Arguments              = string.Join(' ', args),
                UseShellExecute        = false,
                RedirectStandardInput  = false,
                RedirectStandardOutput = false,
                RedirectStandardError  = false,
            };

            process = Process.Start(processStartInfo);
            process.EnableRaisingEvents = true;

            var currentProcess = Process.GetCurrentProcess();
            var parentPropertyInfo = typeof(Process).GetProperty("ParentProcessId", BindingFlags.Instance | BindingFlags.NonPublic);
            var parentProcess = Process.GetProcessById((int)parentPropertyInfo.GetValue(currentProcess));

            File.WriteAllText(pidFile, parentProcess.Id.ToString());

            cancellationToken.Register(((IDisposable)this).Dispose);
        }

        [UnsupportedOSPlatform("browser")]
        void IDisposable.Dispose()
        {
            if (process != null && !process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process = null;
            }
        }
    }
}
