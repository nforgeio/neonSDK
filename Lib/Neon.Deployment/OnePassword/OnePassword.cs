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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

using Neon.Common;

using Newtonsoft.Json.Linq;

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

        private static readonly object                                  syncLock = new object();
        private static string                                           account;        // NULL when not signed-in
        private static string                                           defaultVault;   // NULL when not signed-in
        private static ReadOnlyDictionary<string, OnePasswordVault>     vaults;         // NULL when not signed-in

        /// <summary>
        /// This class requires that a <b>op.exe</b> v2 client be installed and if
        /// the 1Password app is installed that it be version 8.0 or greater.
        /// </summary>
        /// <param name="cliPath">
        /// Optionally specifies the fully qualified path to the 1Password CLI which
        /// should be executed instead of the CLI found on the PATH.
        /// </param>
        /// <exception cref="NotSupportedException">Thrown when any of the checks failed.</exception>
        public static void CheckInstallation(string cliPath = null)
        {
            // Check for the [op.exe] CLI presence and version.

            ExecuteResponse response;

            try
            {
                response = NeonHelper.ExecuteCapture(cliPath ?? "op.exe", new object[] { "--version" });
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
            OnePassword.vaults       = null;
        }

        /// <summary>
        /// Signs into 1Password using just the account, master password, and default vault and loads
        /// loads all secrets from the default vault as well as any vaults specified by <paramref name="preloadVaults"/>.
        /// </summary>
        /// <param name="account">The account's shorthand name (e.g. (e.g. "sally@neonforge.com").</param>
        /// <param name="defaultVault">The default vault.</param>
        /// <param name="preloadVaults">Specifies additonial vaults (seperated by commas) where items will be preloaded.</param>
        /// <param name="cliPath">
        /// Optionally specifies the fully qualified path to the 1Password CLI which
        /// should be executed instead of the CLI found on the PATH.
        /// </param>
        /// <exception cref="OnePasswordException">Thrown when sign-in fails.</exception>
        public static void Signin(string account, string defaultVault, string preloadVaults, string cliPath = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(account), nameof(account));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(defaultVault), nameof(defaultVault));
            Covenant.Requires<ArgumentNullException>(preloadVaults != null, nameof(preloadVaults));

            lock (syncLock)
            {
                Clear();

                OnePassword.account      = account;
                OnePassword.defaultVault = defaultVault;

                // Start a 1Password CLI session.

                var response = NeonHelper.ExecuteCapture(cliPath ?? "op.exe",
                    new string[]
                    {
                        "signin",
                        "--account", account
                    });

                if (response.ExitCode != 0)
                {
                    Signout();
                    throw new OnePasswordException(response.AllText);
                }

                // Retrieve information for all 1Password vaults and items.  This is
                // equivalent to executing this on the command line:
                //
                //      op item list --format=json | op item get --reveal --format=json

                response = NeonHelper.ExecuteCapture(cliPath ?? "op.exe",
                    new string[]
                    {
                        "item", "list", "--format=json"
                    });

                if (response.ExitCode != 0)
                {
                    Signout();
                    throw new OnePasswordException(response.AllText);
                }

                // [op item list --format=json] returns a JSON array listing the
                // 1Password items.  We're going to parse this and filter this
                // to exclude any items not in the default vault or any of the
                // prefetch vaults.

                var vaultNames = new HashSet<string>();

                vaultNames.Add(defaultVault);

                foreach (var vault in preloadVaults.Split(','))
                {
                    var trimmedVault = vault.Trim();

                    if (trimmedVault != string.Empty && !vaultNames.Contains(trimmedVault))
                    {
                        vaultNames.Add(trimmedVault);
                    }
                }

                var allItemsArray    = JArray.Parse(response.OutputText);
                var filterItemsArray = new JArray();

                foreach (JObject item in allItemsArray)
                {
                    var vaultName = (string)item["vault"]["name"];

                    if (vaultNames.Contains(vaultName))
                    {
                        filterItemsArray.Add(item);
                    }
                }

                var test = filterItemsArray.ToString();

                // Retrieve the filtered items. 

                response = NeonHelper.ExecuteCapture(cliPath ?? "op.exe",
                    new string[]
                    {
                        "item", "get", "--reveal", "--format=json"
                    },
                    input: new StringReader(filterItemsArray.ToString()));

                if (response.ExitCode != 0)
                {
                    Signout();
                    throw new OnePasswordException(response.AllText);
                }

                // The 1Password CLI returns the item information as formatted JSON but
                // not as an array.  We'll need to extract individual item JSON by looking
                // for "{" and "}" characters at the beginning of the output lines.

                var itemReader = new StringReader(response.OutputText);
                var vaults     = new Dictionary<string, OnePasswordVault>(StringComparer.InvariantCultureIgnoreCase);
                var sb         = new StringBuilder();

                while (true)
                {
                    const string errorMsg = "Improperly formatted 1Password vault JSON.";

                    sb.Clear();

                    // Validate the first line of the vault JSON, if there is one.

                    var line = itemReader.ReadLine();

                    if (line == null)
                    {
                        break;
                    }

                    if (line.Length == 0 || line[0] != '{')
                    {
                        throw new OnePasswordException(errorMsg);
                    }

                    // Accumulate JSON lines until we get to the line with the closing "}".

                    sb.AppendLine(line);

                    while (true)
                    {
                        line = itemReader.ReadLine();

                        if (line.Length == 0)
                        {
                            throw new OnePasswordException(errorMsg);
                        }

                        sb.AppendLine(line);

                        if (line[0] == '}')
                        {
                            break;
                        }
                    }

                    // Create the vault and item if they don't already exist.

                    // [sb] now holds the item JSON so we need to extract the contents.

                    var itemObject  = JObject.Parse(sb.ToString());
                    var itemId      = (string)itemObject["id"];
                    var itemName    = (string)itemObject["title"];
                    var vaultId     = (string)itemObject["vault"]["id"];
                    var vaultName   = (string)itemObject["vault"]["name"];
                    var fieldsArray = (JArray)itemObject["fields"];

                    if (!vaults.TryGetValue(vaultName, out var vault))
                    {
                        vault = new OnePasswordVault(vaultId, vaultName);

                        vaults.Add(vaultName, vault);
                    }

                    Covenant.Assert(vault.Id == vaultId);

                    var validFieldTypes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
                    {
                        "DATE",
                        "EMAIL",
                        "MONTH_YEAR",
                        "OTP",
                        "PHONE",
                        "STRING",
                        "URL",
                    };

                    var fields = new List<OnePasswordField>();

                    foreach (var fieldToken in fieldsArray)
                    {
                        var fieldObject = (JObject)fieldToken;
                        var fieldType   = (string)fieldObject["type"];
                        var fieldName   = (string)fieldObject["label"];
                        var fieldValue  = (string)fieldObject["value"];

                        if ((!validFieldTypes.Contains(fieldType) && fieldType != "CONCEALED") || fieldValue == null)
                        {
                            continue;
                        }

                        fields.Add(new OnePasswordField(fieldName, fieldValue));
                    }

                    if (fields.Count > 0)
                    {
                        vault.Items.Add(itemName, new OnePasswordItem(itemName, fields));
                    }
                }

                OnePassword.vaults = new ReadOnlyDictionary<string, OnePasswordVault>(vaults);
            }
        }

        /// <summary>
        /// Signs out.
        /// </summary>
        public static void Signout(string cliPath = null)
        {
            lock (syncLock)
            {
                if (string.IsNullOrEmpty(OnePassword.account))
                {
                    NeonHelper.ExecuteCapture(cliPath ?? "op.exe",
                        new string[]
                        {
                            "signout",
                            "--account", OnePassword.account
                        });

                    Clear();
                }
            }
        }

        /// <summary>
        /// Checks the 1Password CLI sign-in/connection status via the <b>whoami</b> command.
        /// </summary>
        /// <param name="cliPath">
        /// Optionally specifies the fully qualified path to the 1Password CLI which
        /// should be executed instead of the CLI found on the PATH.
        /// </param>
        /// <returns><c>true</c> when the CLI is connected.</returns>
        public static bool IsSignedin(string cliPath = null)
        {
            lock (syncLock)
            {
                if (string.IsNullOrEmpty(OnePassword.account))
                {
                    return false;
                }

                var response = NeonHelper.ExecuteCapture(cliPath ?? "op.exe",
                    new string[]
                    {
                        "whoami",
                        "--account", OnePassword.account
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
        /// Returns an item password from the current user's default 1Password 
        /// vault like <b>user-sally</b> by default or a specific vault.
        /// </summary>
        /// <param name="itemRef">The item reference with optional field name (defaults to <b>"password"</b>).</param>
        /// <param name="vaultName">Optionally specifies a specific vault, otherwise the item/field will be retrieved from the default vault.</param>
        /// <returns>The requested password from the referenced item.</returns>
        /// <exception cref="OnePasswordException">Thrown when the requested 1Password is not signed-in or the vault/item/ field doesn't exist.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="itemRef"/> parameter may optionally specify the desired
        /// 1Password field to override the default <b>"password"</b> for this method.
        /// Specific field references are specified like:
        /// </para>
        /// <example>
        /// ITEMNAME[FIELDNAME]
        /// </example>
        /// </remarks>
        public static string GetSecretPassword(string itemRef, string vaultName = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(itemRef), nameof(itemRef));
            Covenant.Requires<InvalidOperationException>(!string.IsNullOrEmpty(account), "Not signed into 1Password");

            var parsedRef = ProfileServer.ParseItemRef(itemRef);
            var fieldName = parsedRef.FieldName ?? "password";

            fieldName = parsedRef.FieldName;
            vaultName = vaultName ?? defaultVault;

            lock (syncLock)
            {
                if (!vaults.TryGetValue(vaultName, out var vault))
                {
                    throw new OnePasswordException($"Vault does not exist: {vaultName}");
                }

                if (!vault.Items.TryGetValue(parsedRef.ItemName, out var item))
                {
                    throw new OnePasswordException($"Item does not exist: {vaultName}:{parsedRef.ItemName}");
                }

                if (!item.Fields.TryGetValue(fieldName, out var field))
                {
                    throw new OnePasswordException($"Field does not exist: {vaultName}:{parsedRef.ItemName}[{fieldName}]");
                }

                return field.Value;
            }
        }

        /// <summary>
        /// Returns a named field from the current user's default 1Password 
        /// vault like <b>user-sally</b> by default or a specific vault.
        /// </summary>
        /// <param name="itemRef">The item reference with optional field name (defaults to <b>"value"</b>).</param>
        /// <param name="vaultName">Optionally specifies a specific vault, otherwise the item/field will be retrieved from the default vault.</param>
        /// <returns>The requested password from the referenced item.</returns>
        /// <exception cref="OnePasswordException">Thrown when the requested 1Password is not signed-in or the vault/item/ field doesn't exist.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="itemRef"/> parameter may optionally specify the desired
        /// 1Password field to override the default <b>"password"</b> for this method.
        /// Specific field references are specified like:
        /// </para>
        /// <example>
        /// ITEMNAME[FIELDNAME]
        /// </example>
        /// </remarks>
        public static string GetSecretValue(string itemRef, string vaultName = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(itemRef), nameof(itemRef));
            Covenant.Requires<InvalidOperationException>(!string.IsNullOrEmpty(account), "Not signed into 1Password");

            var parsedRef = ProfileServer.ParseItemRef(itemRef);
            var fieldName = parsedRef.FieldName ?? "value ";

            fieldName = parsedRef.FieldName;
            vaultName = vaultName ?? defaultVault;

            lock (syncLock)
            {
                if (!vaults.TryGetValue(vaultName, out var vault))
                {
                    throw new OnePasswordException($"Vault does not exist: {vaultName}");
                }

                if (!vault.Items.TryGetValue(parsedRef.ItemName, out var item))
                {
                    throw new OnePasswordException($"Item does not exist: {vaultName}:{parsedRef.ItemName}");
                }

                if (!item.Fields.TryGetValue(fieldName, out var field))
                {
                    throw new OnePasswordException($"Field does not exist: {vaultName}:{parsedRef.ItemName}[{fieldName}]");
                }

                return field.Value;
            }
        }
    }
}
