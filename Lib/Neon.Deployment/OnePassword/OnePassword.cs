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

        private static readonly object      syncLock = new object();
        private static string               account;
        private static string               defaultVault;
        private static string               masterPassword;
        private static string               sessionToken;

        /// <summary>
        /// Returns <c>true</c> if the class is signed-in.
        /// </summary>
        public static bool Signedin => masterPassword != null;

        /// <summary>
        /// <para>
        /// This class requires that a <b>op.exe</b> v1 client be installed and if
        /// the 1Password app is installed that it be version 8.0 or greater.
        /// </para>
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when any of the checks failed.</exception>
        public static void CheckInstallation()
        {
            // Check for the [op.exe] client presence and version.

            ExecuteResponse response;

            try
            {
                response = NeonHelper.ExecuteCapture("op.exe", new object[] { "--version" });
            }
            catch
            {
                throw new NotSupportedException("Cannot locate the 1Password [op.exe] v1 CLI.  This must be located on the PATH.");
            }

            if (response.ExitCode != 0)
            {
                throw new NotSupportedException($"1Password [op.exe] CLI returned [ExitCode={response.ExitCode}].");
            }

            try
            {
                var version = SemanticVersion.Parse(response.OutputText.Trim());

                if (version >= SemanticVersion.Parse("2"))
                {
                    throw new NotSupportedException($"Installed 1Password [op.exe] CLI [Version={version}].  Only v1+ clients are supported at this time.");
                }
            }
            catch (FormatException)
            {
                throw new NotSupportedException($"1Password [op.exe] CLI returned [Version={response.OutputText}] which cannot be parsed.");
            }

            // $hack(jefflill):
            //
            // Check for installed 1Password application version 7.x and throw an exception
            // when present.  We never installed eealier 1Passwork versions, so we won't
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
        /// Configures and signs into 1Password for the first time on a machine.  This
        /// must be called once before <see cref="Signin(string, string, string)"/> will
        /// work.
        /// </summary>
        /// <param name="signinAddress">Specifies the 1Password signin address.</param>
        /// <param name="account">Specifies the 1Password shorthand name to use for the account (e.g. "sally@neonforge.com").</param>
        /// <param name="secretKey">The 1Password secret key for the account.</param>
        /// <param name="masterPassword">Specified the master 1Password.</param>
        /// <param name="defaultVault">Specifies the default 1Password vault.</param>
        /// <remarks>
        /// <para>
        /// Typically, you'll first call <see cref="Configure(string, string, string, string, string)"/> once
        /// for a workstation to configure the signin address and 1Password secret key during manual
        /// configuration.  The account shorthand name used for that operation can then be used thereafter
        /// for calls to <see cref="Signin(string, string, string)"/> which don't require the additional 
        /// information.
        /// </para>
        /// <para>
        /// This two-stage process enhances security because both the master password and secret
        /// key are required to authenticate and the only time the secret key will need to be
        /// presented for the full login which will typically done manually once.  1Password
        /// securely stores the secret key on the workstation and it will never need to be present
        /// as plaintext again on the machine.
        /// </para>
        /// </remarks>
        public static void Configure(string signinAddress, string account, string secretKey, string masterPassword, string defaultVault)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(signinAddress), nameof(signinAddress));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(account), nameof(account));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(secretKey), nameof(secretKey));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(masterPassword), nameof(masterPassword));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(defaultVault), nameof(defaultVault));

            lock (syncLock)
            {
                // 1Password doesn't allow reconfiguring without being signed-out first.

                Signout();

                // Sign back in.

                OnePassword.account        = account;
                OnePassword.defaultVault   = defaultVault;
                OnePassword.masterPassword = masterPassword;

                var input = new StringReader(masterPassword);

                var response = NeonHelper.ExecuteCapture("op.exe",
                    new string[]
                    {
                        "--cache",
                        "signin",
                        "--raw",
                        signinAddress,
                        account,
                        secretKey,
                        "--shorthand", account
                    },
                    input: input);

                if (response.ExitCode != 0)
                {
                    Signout();
                    throw new OnePasswordException(response.AllText);
                }

                SetSessionToken(response.OutputText.Trim());
            }
        }

        /// <summary>
        /// Signs into 1Password using just the account, master password, and default vault.  You'll
        /// typically call this rather than <see cref="Configure(string, string, string, string, string)"/>
        /// which also requires the signin address as well as the secret key.
        /// </summary>
        /// <param name="account">The account's shorthand name (e.g. (e.g. "sally@neonforge.com").</param>
        /// <param name="masterPassword">The master password.</param>
        /// <param name="defaultVault">The default vault.</param>
        /// <remarks>
        /// <para>
        /// Typically, you'll first call <see cref="Configure(string, string, string, string, string)"/> once
        /// for a workstation to configure the signin address and 1Password secret key during manual
        /// configuration.  The account shorthand name used for that operation can then be used thereafter
        /// for calls to this method which don't require the additional information.
        /// </para>
        /// <para>
        /// This two-stage process enhances security because both the master password and secret
        /// key are required to authenticate and the only time the secret key will need to be
        /// presented for the full login which will typically done manually once.  1Password
        /// securely stores the secret key on the workstation and it will never need to be present
        /// as plaintext again on the machine.
        /// </para>
        /// </remarks>
        public static void Signin(string account, string masterPassword, string defaultVault)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(account), nameof(account));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(defaultVault), nameof(defaultVault));

            masterPassword = masterPassword ?? string.Empty;

            lock (syncLock)
            {
                OnePassword.account        = account;
                OnePassword.defaultVault   = defaultVault;
                OnePassword.masterPassword = masterPassword;

                var input = new StringReader(masterPassword);

                var response = NeonHelper.ExecuteCapture("op.exe",
                    new string[]
                    {
                        "--cache",
                        "signin",
                        "--raw",
                        account
                    },
                    input: input);

                if (response.ExitCode != 0)
                {
                    Signout();
                    throw new OnePasswordException(response.AllText);
                }

                SetSessionToken(response.OutputText.Trim());
            }
        }

        /// <summary>
        /// Signs out.
        /// </summary>
        public static void Signout()
        {
            lock (syncLock)
            {
                NeonHelper.ExecuteCapture("op.exe", new string[] { "signout" });

                OnePassword.account        = null;
                OnePassword.defaultVault   = null;
                OnePassword.masterPassword = null;
                OnePassword.sessionToken   = null;
            }
        }

        /// <summary>
        /// Returns a named password from the current user's standard 1Password 
        /// vault like <b>user-sally</b> by default or a custom named vault.
        /// </summary>
        /// <param name="name">The password name with optional property.</param>
        /// <param name="vault">Optionally specifies a specific vault.</param>
        /// <returns>The requested password (from the password's [password] field).</returns>
        /// <exception cref="OnePasswordException">Thrown when the requested secret or proerty doesn't exist or for other 1Password related problems.</exception>
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
        public static string GetSecretPassword(string name, string vault = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name), nameof(name));

            var parsedName = ProfileServer.ParseSecretName(name);
            var property   = parsedName.Property ?? "password";

            name = parsedName.Name;

            var retrying = false;

            lock (syncLock)
            {
                EnsureSignedIn();

retry:          var response = NeonHelper.ExecuteCapture("op.exe",
                    new string[]
                    {
                        "--cache",
                        "--session", sessionToken,
                        "get", "item", name,
                        "--vault", vault,
                        "--fields", property
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

                    case OnePasswordStatus.SessionExpired:

                        if (retrying)
                        {
                            throw new OnePasswordException(response.AllText);
                        }

                        // Obtain a fresh session token and retry the operation.

                        Signin(account, masterPassword, defaultVault);

                        retrying = true;
                        goto retry;

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
        /// <returns>The requested value (from the password's <b>value</b> field).</returns>
        /// <exception cref="OnePasswordException">Thrown when the requested secret or proerty doesn't exist or for other 1Password related problems.</exception>
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
        public static string GetSecretValue(string name, string vault = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name), nameof(name));

            var parsedName = ProfileServer.ParseSecretName(name);
            var property   = parsedName.Property ?? "value";

            name = parsedName.Name;

            var retrying = false;

            lock (syncLock)
            {
                EnsureSignedIn();

retry:          var response = NeonHelper.ExecuteCapture("op.exe",
                    new string[]
                    {
                        "--cache",
                        "--session", sessionToken,
                        "get", "item", name,
                        "--vault", vault,
                        "--fields", property
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

                    case OnePasswordStatus.SessionExpired:

                        if (retrying)
                        {
                            throw new OnePasswordException(response.AllText);
                        }

                        // Obtain a fresh session token and retry the operation.

                        Signin(account, masterPassword, defaultVault);

                        retrying = true;
                        goto retry;

                    default:

                        throw new OnePasswordException(response.AllText);
                }
            }
        }

        /// <summary>
        /// Updates the session token.
        /// </summary>
        /// <param name="sessionToken">The new session token or <c>null</c>.</param>
        private static void SetSessionToken(string sessionToken)
        {
            OnePassword.sessionToken = sessionToken;
        }

        /// <summary>
        /// Ensures that we're signed into 1Password.
        /// </summary>
        /// <exception cref="OnePasswordException">Thrown if we're not signed in.</exception>
        private static void EnsureSignedIn()
        {
            if (!Signedin)
            {
                throw new OnePasswordException("You are not signed into 1Password.");
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
