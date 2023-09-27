// -----------------------------------------------------------------------------
// FILE:	    NeonExtendedHelper.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
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
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

using Neon.Common;

namespace Neon.Common
{
    /// <summary>
    /// Implements extended helper methods.
    /// </summary>
    public static class NeonExtendedHelper
    {
        /// <summary>
        /// Modifies the <b>everyone</b> permissions for a unix domain socket.  We use this
        /// for setting everyone permissions for unix domain sockets because it appears that
        /// these sockets only allow writes for the current or admin users by default when
        /// created by a gRPC server and perhaps in other situations.
        /// </summary>
        /// <param name="socketPath">Specifies the path the the unix domain socket.</param>
        /// <param name="read">Optionally grants read access.</param>
        /// <param name="write">Optionally grants write access.</param>
        public static void SetUnixDomainSocketEveryonePermissions(string socketPath, bool read = false, bool write = false)
        {
            // $todo(jefflill):
            //
            // We should relocate this method to a common shared library.  I thought about
            // putting this in [Neon.Common] (NetHelper) but the [System.IO.FileSystem.AccessControl]
            // nuget package is over 300K and I didn't want to incur the that much overhead
            // for something that will be rarely used.
            //
            // Perhaps we should add a [Neon.Common.Extensions] library to NEONSDK.

            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(socketPath), nameof(socketPath));

            if (NeonHelper.IsWindows)
            {
#pragma warning disable CA1416 // Validate platform compatibility

                // Apparently, the [System.IO.FileSystem.AccessControl] nuget package
                // doesn't support long (>255) file names without a special format.

                if (socketPath.Length > 255)
                {
                    socketPath = @"\\?\" + socketPath;
                }

                var fileInfo          = new FileInfo(socketPath);
                var permissions       = fileInfo.GetAccessControl();
                var everyoneSid       = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                var everyoneReadRule  = new FileSystemAccessRule(everyoneSid, FileSystemRights.Read, AccessControlType.Allow);
                var everyoneWriteRule = new FileSystemAccessRule(everyoneSid, FileSystemRights.Write, AccessControlType.Allow);

                permissions.RemoveAccessRule(everyoneReadRule);
                permissions.RemoveAccessRule(everyoneWriteRule);

                if (read)
                {
                    permissions.AddAccessRule(everyoneReadRule);
                }

                if (write)
                {
                    permissions.AddAccessRule(everyoneWriteRule);
                }

                fileInfo.SetAccessControl(permissions);

#pragma warning restore CA1416 // Validate platform compatibility
            }
            else
            {
                // $hack(jefflill):
                //
                // We're going exec some standard unix tools here to fetch the
                // socket's current permissions and then update them.  It would
                // be cleaner to pinvoke these as libc calls.

                // $todo(jefflill):
                //
                // This hasn't been tested!

                const int READ  = 4;
                const int WRITE = 2;

                var response = NeonHelper.ExecuteCapture("stat", new object[] { "'%a'", socketPath })
                    .EnsureSuccess();

                var curPermissions = Convert.ToInt32(response.OutputText.Trim(), fromBase: 8);
                var newPermissions = curPermissions & ~0xFFFFFFF0;      // Zero the everyone permissions

                if (read)
                {
                    newPermissions |= READ;
                }

                if (write)
                {
                    newPermissions |= WRITE;
                }

                if (newPermissions != curPermissions)
                {
                    NeonHelper.ExecuteCapture("chmod", new object[] { Convert.ToString(newPermissions, toBase: 8), socketPath })
                        .EnsureSuccess();
                }
            }
        }
    }
}
