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

using OpenTelemetry;

using Quartz;

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

        /// <summary>
        /// <para>
        /// Converts an extended Quartz cron expression string into a standard Quartz
        /// expression.  Currently, this supports the <b>"R"</b> character in any of
        /// the fields except for the year.  This method will replace any <b>"R"</b>
        /// it finds with a random value for the fields it appears in.
        /// </para>
        /// <para>
        /// This is useful for situations like uploading telemetry to a global service
        /// where you don't want a potentially large number of clients being scheduled
        /// to hit the service at the same time.
        /// </para>
        /// </summary>
        /// <param name="expression">Specifies the extended schdule string.</param>
        /// <returns>The standard schedule string.</returns>
        public static string FromEnhancedCronExpression(string expression)
        {
            Covenant.Assert(!string.IsNullOrEmpty(expression), nameof(expression));

            // Temporarily replace any "R" fields with a "1" and then verify
            // that the result is a valid cron expression.

            var fields = expression.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var sb     = new StringBuilder();

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i] == "R")
                {
                    sb.AppendWithSeparator("1");
                }
                else
                {
                    sb.AppendWithSeparator(fields[i]);
                }
            }

            CronExpression.ValidateExpression(sb.ToString());

            if (!CronExpression.IsValidExpression(expression.Replace('R', '1')))
            {
                throw new FormatException($"Invalid cron expression: {expression}");
            }

            // Verify that any "R" characters appear without anything else in the hour/min/sec fields.

            foreach (var field in fields)
            {
                if (field.Contains("R") && field.Length > 1)
                {
                    throw new FormatException($"Invalid cron expression: {expression}");
                }
            }

            // Verify that the YEAR field (if present) isn't a "R".

            if (fields.Length >= 7 && fields[6] == "R")
            {
                throw new FormatException($"Invalid cron expression: {expression}");
            }

            // Convert any "R" characters into a random value for the field.

            if (fields[0] == "R")   // Seconds (zero based)
            {
                fields[0] = NeonHelper.PseudoRandomIndex(60).ToString();
            }

            if (fields[1] == "R")   // Minutes (zero based)
            {
                fields[1] = NeonHelper.PseudoRandomIndex(60).ToString();
            }

            if (fields[2] == "R")   // Hours (zero based)
            {
                fields[2] = NeonHelper.PseudoRandomIndex(24).ToString();
            }

            if (fields[3] == "R")   // Day of month (one based)
            {
                fields[3] = (NeonHelper.PseudoRandomIndex(31) + 1).ToString();
            }

            if (fields[4] == "R")   // Month (one based)
            {
                fields[4] = (NeonHelper.PseudoRandomIndex(12) + 1).ToString();
            }

            if (fields[5] == "R")   // Day of week (one based)
            {
                fields[5] = (NeonHelper.PseudoRandomIndex(7) + 1).ToString();
            }

            // Returns the converted cron schedule.

            sb.Clear();
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i] == "R")
                {
                    sb.AppendWithSeparator("1");
                }
                else
                {
                    sb.AppendWithSeparator(fields[i]);
                }
            }

            return sb.ToString();
        }
    }
}
