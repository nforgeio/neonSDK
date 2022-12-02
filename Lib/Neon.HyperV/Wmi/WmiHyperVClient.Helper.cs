//-----------------------------------------------------------------------------
// FILE:	    HyperVWmi.Helper.cs
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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using Neon.BuildInfo;
using Neon.Common;
using Neon.Diagnostics;
using Newtonsoft.Json;
using YamlDotNet.Core.Tokens;

namespace Neon.HyperV
{
    internal partial class WmiHyperVClient
    {
        /// <summary>
        /// Returns the named service object from the current scope.
        /// </summary>
        /// <param name="serviceName">The service object name.</param>
        /// <returns>The service <see cref="ManagementObject"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the requested service doesn't exist.</exception>
        public ManagementObject GetService(string serviceName)
        {
            var wmiPath      = new ManagementPath(serviceName);
            var serviceClass = new ManagementClass(scope, wmiPath, null);

            return serviceClass.GetInstances().Single();
        }

        /// <summary>
        /// Invokes a named service method, passing the arguments passed (if any), 
        /// and returning the result object.
        /// </summary>
        /// <param name="service">The target service name.</param>
        /// <param name="method">The target service method name.</param>
        /// <param name="args">Optionally specifies arguments to be passed to the method.</param>
        /// <exception cref="HyperVException">Thrown for operation failures.</exception>
        private Dictionary<string, object> Invoke(string service, string method, Dictionary<string, object> args = null)
        {
            Covenant.Requires<ArgumentNullException>(service != null, nameof(service));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(method), nameof(method));

            using (var targetService = GetService(service))
            {
                using (var inParams = targetService.GetMethodParameters(method))
                {
                    inParams.SetProperties(args);

                    using (var outParams = targetService.InvokeMethod(method, inParams, null))
                    {
                        if (outParams == null)
                        {
                            throw new HyperVException($"WMI [{targetService["Name"]}.{method}] returned NULL.");
                        }

                        return outParams.ToDictionary();
                    }
                }
            }
        }

        /// <summary>
        /// Invokes a named service method as a job, passing the arguments passed (if any).
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
                    inParams.SetProperties(args);

                    using (var outParams = targetService.InvokeMethod(method, inParams, null))
                    {
                        if (outParams == null)
                        {
                            throw new HyperVException($"WMI [{targetService["Name"]}.{method}] returned NULL.");
                        }

                        if ((UInt32)outParams["ReturnValue"] != WmiReturnCode.Started)
                        {
                            throw new HyperVException($"WMI [{targetService["Name"]}.{method}] job wasn't started.");
                        }

                        // Wait for the job to complete (or fail).

                        var jobPath = (string)outParams["Job"];
                        var job     = new ManagementObject(scope, new ManagementPath(jobPath), null);

                        job.Get();

                        while ((UInt16)job["JobState"] == WmiJobState.Starting || (UInt16)job["JobState"] == WmiJobState.Running)
                        {
                            Thread.Sleep(1000);
                            job.Get();
                        }

                        // Determine whether the job failed.

                        var jobState = (UInt16)job["JobState"];

                        if (jobState != WmiJobState.Completed)
                        {
                            throw new HyperVException($"WMI [{service}.{method}] error: [code={job["ErrorCode"]}]: {job["ErrorDescription"]}");
                        }
                    }
                }
            }
        }
    }
}
