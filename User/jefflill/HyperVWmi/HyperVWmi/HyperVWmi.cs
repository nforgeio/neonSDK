//-----------------------------------------------------------------------------
// FILE:	    HyperVWmi.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
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

#pragma warning disable CA1416  // Validate platform compatibility

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;

using Neon.Common;
using Neon.Diagnostics;
using YamlDotNet.Serialization;

namespace Neon.HyperV
{
    /// <summary>
    /// Abstracts access to the low-level Hyper-V WMI functions that
    /// we'll user for implementing our own Hyper-V wrappers.
    /// </summary>
    internal sealed partial class HyperVWmi : IDisposable
    {
        private ManagementScope     scope = new ManagementScope(@"root\virtualization\v2");

        /// <summary>
        /// Constructor.
        /// </summary>
        public HyperVWmi()
        {
            scope.Connect();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            scope = null;
        }

        /// <summary>
        /// Ensures that the instance is no0t disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when the instance has been disposed.</exception>
        private void CheckDisposed()
        {
            if (scope == null)
            {
                throw new ObjectDisposedException(nameof(HyperVWmi));
            }
        }

        //---------------------------------------------------------------------
        // Helpers

        private static class JobState
        {
            public const UInt16 New = 2;
            public const UInt16 Starting = 3;
            public const UInt16 Running = 4;
            public const UInt16 Suspended = 5;
            public const UInt16 ShuttingDown = 6;
            public const UInt16 Completed = 7;
            public const UInt16 Terminated = 8;
            public const UInt16 Killed = 9;
            public const UInt16 Exception = 10;
            public const UInt16 Service = 11;
        }

        private static class WmiService
        {
            public const string ImageManagement = "Msvm_ImageManagementService";
        }

        /// <summary>
        /// Creates a dictionary of parameter names and their values for use by <see cref="Invoke"/>
        /// and <see cref="InvokeJob"/> methods.  We're going to use this passing in parameters and 
        /// returning out parameters to WMI service methods to simplfy things for callers.
        /// </summary>
        /// <param name="copyFrom">Optionally specifies a management object whose properties will be used to populate the dictionary.</param>
        /// <returns>The new parameter dictionary.</returns>
        private Dictionary<string, object> CreateParams(ManagementBaseObject copyFrom = null)
        {
            var @params = new Dictionary<string, object>();

            if (copyFrom != null)
            {
                foreach (var param in copyFrom.Properties)
                {
                    @params[param.Name] = param.Value;
                }
            }

            return @params;
        }

        /// <summary>
        /// Returns the named service object from the current scope.
        /// </summary>
        /// <param name="serviceName">The service object name.</param>
        /// <returns>The service <see cref="ManagementObject"/>.</returns>
        public ManagementObject GetService(string serviceName)
        {
            var wmiPath      = new ManagementPath(serviceName);
            var serviceClass = new ManagementClass(scope, wmiPath, null);

            foreach (ManagementObject service in serviceClass.GetInstances())
            {
                return service;
            }

            return null;
        }

        /// <summary>
        /// Invokes a named service method as a job, passing the arguments passed (if any), 
        /// returning the method result.
        /// </summary>
        /// <param name="service">The target service name.</param>
        /// <param name="method">The target service method name.</param>
        /// <param name="args">Optionally specifies arguments to be passed to the method.</param>
        /// <exception cref="HyperVException">Thrown for operation failures.</exception>
        private void InvokeJob(string service, string method, Dictionary<string, object> args = null)
        {
            Covenant.Requires<ArgumentNullException>(service != null, nameof(service));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(method), nameof(method));

            using (var targetService = GetService(service))
            {
                using (var inParams = targetService.GetMethodParameters(method))
                {
                    if (args != null)
                    {
                        foreach (var arg in args)
                        {
                            inParams[arg.Key] = arg.Value;
                        }
                    }

                    using (var outParams = targetService.InvokeMethod(method, inParams, null))
                    {
                        if (outParams == null)
                        {
                            throw new HyperVException($"WMI [{targetService["Name"]}.{method}] returned NULL.");
                        }

                        if ((UInt32)outParams["ReturnValue"] != ReturnCode.Started)
                        {
                            throw new HyperVException($"WMI [{targetService["Name"]}.{method}] job wasn't started.");
                        }

                        // Wait for the job to complete (or fail).

                        var jobPath = (string)outParams["Job"];
                        var job     = new ManagementObject(scope, new ManagementPath(jobPath), null);

                        job.Get();

                        while ((UInt16)job["JobState"] == JobState.Starting || (UInt16)job["JobState"] == JobState.Running)
                        {
                            Thread.Sleep(1000);
                            job.Get();
                        }

                        // Determine whether the job failed.

                        var jobState = (UInt16)job["JobState"];

                        if (jobState != JobState.Completed)
                        {
                            throw new HyperVException($"WMI [{service}.{method}] error: [code={job["ErrorCode"]}]: {job["ErrorDescription"]}");
                        }
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        // Wrapped operation implementations.

        /// <summary>
        /// Validates a VHD or VHDX file.
        /// </summary>
        /// <param name="path">Path to the disk file.</param>
        /// <exception cref="HyperVException">Thrown when the disk is not valid.</exception>
        public void ValidateDisk(string path)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(path));
            CheckDisposed();

            InvokeJob(WmiService.ImageManagement, "ValidateVirtualHardDisk", new Dictionary<string, object>() { { "Path", path } });
        }
    }
}
