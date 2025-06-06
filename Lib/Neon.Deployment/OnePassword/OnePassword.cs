//-----------------------------------------------------------------------------
// FILE:        OnePassword.cs
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
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

using Neon.Common;
using Octokit;

namespace Neon.Deployment
{
    /// <summary>
    /// Wraps the 1Password CLI.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public static class OnePassword
    {
        //---------------------------------------------------------------------
        // Private types

        /// <summary>
        /// Unfortunately, the 1Password CLI doesn't appear to return specific
        /// exit codes detailing the for specific error yet.  We're going to 
        /// hack this by examining the response text.
        /// </summary>
        private enum OnePasswordStatus
        {
            /// <summary>
            /// The operation was successful.
            /// </summary>
            OK = 0,

            /// <summary>
            /// The session token has expired.
            /// </summary>
            SessionExpired,

            /// <summary>
            /// Unspecified error.
            /// </summary>
            Other
        }

        //---------------------------------------------------------------------
        // Implementation

        private static readonly object      syncLock          = new object();
        private static readonly TimeSpan    keepAliveInterval = TimeSpan.FromMinutes(5);    // Only 5 minutes seems to work reliably
        private static string               account;                // NULL when not signed-in
        private static string               defaultVault;           // NULL when not signed-in

        /// <summary>
        /// This class requires that a <b>op.exe</b> v2 client be installed and if
        /// the 1Password app is installed that it be version 8.0 or greater.
        /// </summary>
        /// <param name="opPath">
        /// Optionally specifies the fully qualified path to the 1Password CLI which
        /// will be executed instead of the CLI found on the PATH.
        /// </param>
        /// <exception cref="NotSupportedException">Thrown when any of the checks failed.</exception>
        public static void CheckInstallation(string opPath = null)
        {
            // Check for the [op.exe] CLI presence and version.

            ExecuteResponse response;

            try
            {
                response = NeonHelper.ExecuteCapture(opPath ?? "op.exe", new object[] { "--version" });
            }
            catch
            {
                throw new NotSupportedException("Cannot locate the 1Password [op.exe] v1 CLI.  This must be on the PATH.");
            }

            if (response.ExitCode != 0)
            {
                throw new NotSupportedException($"1Password [op.exe] CLI returned [ExitCode={response.ExitCode}].");
            }

            try
            {
                var version = SemanticVersion.Parse(response.OutputText.Trim());

                if (version < SemanticVersion.Parse("2"))
                {
                    throw new NotSupportedException($"1Password [op.exe] CLI [Version={version}] is installed.  v2+ is required.");
                }
            }
            catch (FormatException)
            {
                throw new NotSupportedException($"1Password [op.exe] CLI returned [Version={response.OutputText}] which cannot be parsed.");
            }

            // $hack(jefflill):
            //
            // Check for installed 1Password application version 7.x and throw an exception
            // when present.  We never installed earlier 1Password versions, so we won't
            // bother checking for those.

            var probePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "1Password", "app", "7", "1Password.exe");

            if (File.Exists(probePath))
            {
                throw new NotSupportedException("1Password v7.x is currently installed.  Only 1Password v8.x+ is supported.");
            }
        }

        /// <summary>
        /// <para>
        /// Returns <c>true</c> if the 1Password CLI needs to be configured by
        /// setting the <b>OP_DEVICE</b> environment variable.
        /// </para>
        /// <note>
        /// The CLI requires <b>OP_DEVICE</b> to be set when the 1Password application
        /// is not installed on the current machine.
        /// </note>
        /// </summary>
        public static bool CliConfigRequired
        {
            get
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OP_DEVICE")))
                {
                    return false;
                }

                // $hack(jefflill):
                //
                // I'm hardcoding detecting the 1Password installation.  This won't work
                // for 1Password v7.x or earlier but does work for v8 and hopefully future
                // releases as well.

                var probePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "Local", "1Password", "1password.sqlite");

                return !File.Exists(probePath);
            }
        }

        /// <summary>
        /// Clears fields to indicate the underlying secret manager is signed-out.
        /// </summary>
        private static void Clear()
        {
            OnePassword.account      = null;
            OnePassword.defaultVault = null;
        }

        /// <summary>
        /// Signs into 1Password using just the account, master password, and default vault.
        /// </summary>
        /// <param name="account">The account's shorthand name (e.g. (e.g. "sally@neonforge.com").</param>
        /// <param name="defaultVault">The default vault.</param>
        /// <param name="opPath">
        /// Optionally specifies the fully qualified path to the 1Password CLI which
        /// will be executed instead of the CLI found on the PATH.
        /// </param>
        /// <exception cref="OnePasswordException">Thrown when signin fails.</exception>
        /// <returns>
        /// The time (UTC) when the session will be expire if not extended.
        /// </returns>
        public static DateTime? Signin(string account, string defaultVault, string opPath = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(account), nameof(account));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(defaultVault), nameof(defaultVault));

            lock (syncLock)
            {
                Clear();

                OnePassword.account      = account;
                OnePassword.defaultVault = defaultVault;

                var response = NeonHelper.ExecuteCapture(opPath ?? "op.exe",
                    new string[]
                    {
                        "--account", account,
                        "--cache",
                        "signin",
                    });

                if (response.ExitCode != 0)
                {
                    Signout();
                    throw new OnePasswordException(response.AllText);
                }

                return DateTime.UtcNow + keepAliveInterval;
            }
        }

        /// <summary>
        /// Signs out.
        /// </summary>
        /// <param name="opPath">
        /// Optionally specifies the fully qualified path to the 1Password CLI which
        /// will be executed instead of the CLI found on the PATH.
        /// </param>
        public static void Signout(string opPath = null)
        {
            lock (syncLock)
            {
                if (string.IsNullOrEmpty(OnePassword.account))
                {
                    NeonHelper.ExecuteCapture(opPath ?? "op.exe",
                        new string[]
                        {
                            "--account", OnePassword.account,
                            "signout"
                        });

                    Clear();
                }
            }
        }

        /// <summary>
        /// Checks the 1Password CLI sign-in/connection status via the <b>whoami</b> command.
        /// </summary>
        /// <returns><c>true</c> when the CLI is connected.</returns>
        public static bool IsSignedin(string opPath = null)
        {
            lock (syncLock)
            {
                if (string.IsNullOrEmpty(OnePassword.account))
                {
                    return false;
                }

                var response = NeonHelper.ExecuteCapture(opPath ?? "op.exe",
                    new string[]
                    {
                        "--account", OnePassword.account,
                        "whoami"
                    });

                if (response.Success)
                {
                    return true;
                }

                Clear();

                return false;
            }
        }

        /// <summary>
        /// Returns a named password from the current user's standard 1Password 
        /// vault like <b>user-sally</b> by default or a custom named vault.
        /// </summary>
        /// <param name="name">The password name with optional property.</param>
        /// <param name="vault">Optionally specifies a specific vault.</param>
        /// <param name="opPath">
        /// Optionally specifies the fully qualified path to the 1Password CLI which
        /// will be executed instead of the CLI found on the PATH.
        /// </param>
        /// <returns>The requested password (from the password's [password] field).</returns>
        /// <exception cref="OnePasswordException">Thrown when the requested secret or property doesn't exist or for other 1Password related problems.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="name"/> parameter may optionally specify the desired
        /// 1Password property to override the default <b>"password"</b> for this
        /// method.  Properties are specified like:
        /// </para>
        /// <example>
        /// SECRETNAME[PROPERTY]
        /// </example>
        /// </remarks>
        public static string GetSecretPassword(string name, string vault = null, string opPath = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name), nameof(name));

            var parsedName = ProfileServer.ParseSecretName(name);
            var property   = parsedName.Property ?? "password";

            name = parsedName.Name;

            lock (syncLock)
            {
                var response = NeonHelper.ExecuteCapture(opPath ?? "op.exe",
                    new string[]
                    {
                        "--cache",
                        "--account", OnePassword.account,
                        "item", "get", name,
                        "--vault", vault,
                        "--fields", property,
                        "--reveal"
                    });

                switch (GetStatus(response))
                {
                    case OnePasswordStatus.OK:

                        var value = response.OutputText.Trim();

                        if (value == string.Empty)
                        {
                            throw new OnePasswordException($"Property [{property}] returned an empty string.  Does it exist?.");
                        }

                        return value;

                    default:

                        throw new OnePasswordException(response.AllText);
                }
            }
        }

        /// <summary>
        /// Returns a named value from the current user's standard 1Password 
        /// vault like <b>user-sally</b> by default or a custom named vault.
        /// </summary>
        /// <param name="name">The password name with optional property.</param>
        /// <param name="vault">Optionally specifies a specific vault.</param>
        /// <param name="opPath">
        /// Optionally specifies the fully qualified path to the 1Password CLI which
        /// will be executed instead of the CLI found on the PATH.
        /// </param>
        /// <returns>The requested value (from the password's <b>value</b> field).</returns>
        /// <exception cref="OnePasswordException">Thrown when the requested secret or property doesn't exist or for other 1Password related problems.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="name"/> parameter may optionally specify the desired
        /// 1Password property to override the default <b>"value"</b> for this
        /// method.  Properties are specified like:
        /// </para>
        /// <example>
        /// SECRETNAME[PROPERTY]
        /// </example>
        /// </remarks>
        public static string GetSecretValue(string name, string vault = null, string opPath = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name), nameof(name));

            var parsedName = ProfileServer.ParseSecretName(name);
            var property   = parsedName.Property ?? "value";

            name = parsedName.Name;

            lock (syncLock)
            {
                var response = NeonHelper.ExecuteCapture(opPath ?? "op.exe",
                    new string[]
                    {
                        "--cache",
                        "item", "get", name,
                        "--vault", vault,
                        "--fields", property,
                        "--reveal"
                    });

                switch (GetStatus(response))
                {
                    case OnePasswordStatus.OK:

                        var value = response.OutputText.Trim();

                        if (value == string.Empty)
                        {
                            throw new OnePasswordException($"Property [{property}] returned an empty string.  Does it exist?.");
                        }

                        return value;

                    default:

                        throw new OnePasswordException(response.AllText);
                }
            }
        }

        /// <summary>
        /// Attempts to extend the 1Password session by 30 minutes.
        /// </summary>
        /// <param name="opPath">
        /// Optionally specifies the fully qualified path to the 1Password CLI which
        /// will be executed instead of the CLI found on the PATH.
        /// </param>
        /// <returns>
        /// The new session expiration time (UTC) or <c>null</c> when we're not signed
        /// into the underlying secret manager.
        /// </returns>
        public static DateTime? ExtendSession(string opPath = null)
        {
            Covenant.Assert(!string.IsNullOrEmpty(opPath), nameof(opPath));

            lock (syncLock)
            {
                var response = NeonHelper.ExecuteCapture(opPath ?? "op.exe",
                    new string[]
                    {
                        "item", "list"
                    });

                if (response.Success)
                {
                    return DateTime.UtcNow + TimeSpan.FromMinutes(30);
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="OnePasswordStatus"/> corresponding to a 1Password CLI response.
        /// </summary>
        /// <param name="response">The 1Password CLI response.</param>
        /// <returns>The status code.</returns>
        private static OnePasswordStatus GetStatus(ExecuteResponse response)
        {
            Covenant.Requires<ArgumentNullException>(response != null, nameof(response));
            
            // $hack(jefflill):
            //
            // The 1Password CLI doesn't return useful exit codes at this time,
            // so we're going to try to figure out what we need from the response
            // text returned by the CLI.

            if (response.ExitCode == 0)
            {
                return OnePasswordStatus.OK;
            }
            else
            {
                if (response.AllText.Contains("session expired") || response.AllText.Contains("You are not currently signed in"))
                {
                    return OnePasswordStatus.SessionExpired;
                }
                else
                {
                    return OnePasswordStatus.Other;
                }
            }
        }
    }
}
