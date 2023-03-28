//-----------------------------------------------------------------------------
// FILE:	    IProfileClient.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.IO;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Neon.Common;

namespace Neon.Deployment
{
    /// <summary>
    /// Defines the interface for the client used to communicate with the Neon Assistant
    /// or a custom service.  These services provides access to user and workstation specific 
    /// settings including secrets and general properties.  This is used for activities such as 
    /// CI/CD automation and integration testing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementations of this interface address the following scenarios:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <para>
    /// Gaining access to secrets.  NEONFORGE has standardized on 1Password for password management
    /// and the Neon Profile Service abstracts the details of authenticating with 1Password and
    /// accessing secrets.
    /// </para>
    /// <para>
    /// This interface supports two kinds of secrets: passwords and values.  These are somewhat 
    /// of an artifact of how we implemented this using 1Password.  Secret passwords are values
    /// retrieved from a 1Password item's <b>password field</b> and secret values correspond to
    /// a 1Password item <b>value field.</b>  We found this distinction useful because 1Password
    /// reports when passwords are insecure or duplicated but we have other secrets where these
    /// checks can be distracting.  Custom implementation can choose to follow this pattern or
    /// just treat both types of secret the same.
    /// </para>
    /// <para>
    /// You can also obtain a specific property from a secret password or value by using this syntax:
    /// </para>
    /// <example>
    /// SECRETNAME[PROPERTY]
    /// </example>
    /// <para>
    /// This is useful for obtaining both the username and password from a login, or all of the different
    /// properties from a credit card, etc.  This blurs the difference between secret passwords and secret
    /// values a bit but we're going to retain both concepts anyway.
    /// </para>
    /// </item>
    /// <item>
    /// <para>
    /// Profile values are also supported.  These are non-secret name/value pairs used for describing
    /// the local environment as required for CI/CD.  For example, we use this for describing the
    /// IP addresses available for deploying a test neonKUBE cluster.  Each developer will often
    /// need distict node IP addresses that work on the local LAN and also don't conflict with
    /// addresses assigned to other developers.
    /// </para>
    /// <para>
    /// NEONFORGE's internal implementation simply persists profile values on the local workstation
    /// as a YAML file which is referenced by our profile service.
    /// </para>
    /// </item>
    /// <item>
    /// Abstracting access to the user's master password.  NEONFORGE has implemented an internal  
    /// Windows application that implements a profile service that prompts the developer for their
    /// master 1Password, optionally caching it for a period of time so the user won't be prompted
    /// as often.  This server also handles profile and secret lookup.
    /// </item>
    /// </list>
    /// <b>Managing Sign-in:</b>
    /// <para>
    /// Some profile service implementations prompt the developer for master credentials and then
    /// cache these for a period of time, so the developer isn't innundated with password requests.
    /// </para>
    /// <para>
    /// <see cref="EnsureAuthenticated(TimeSpan)"/> can be used to have the developer sign-in the profile server, 
    /// optionally specifiying the number of seconds the server will remain signed-in afterwards.  This is useful
    /// for situations where an operation requests secrets as the operation progresses and it'll be possible 
    /// for the sign-in period to expire before the operation completes.
    /// </para>
    /// <note>
    /// We originally tried to manage this by loading any secrets at the beginning of an operation,
    /// so (hopefully) we'd obtain all of them while the user was still present to enter any credentials.
    /// This worked for operations executing as a single process, but doesn't really work well for
    /// operations that span multiple processess.  We tried to address this with client-side caching
    /// via environment variables, but that introduced other issues, so we removed caching support.
    /// </note>
    /// </remarks>
    public interface IProfileClient
    {
        /// <summary>
        /// Requests that the profile server be signed-in when it's not already signed or extend the
        /// sign-in period.  By default, the sign-in period will be extended by the default time configured
        /// for the server but this can be overridden via <paramref name="signinPeriod"/> (which comes in handy
        /// for operations that may take longer than the profile server default).
        /// </summary>
        /// <param name="signinPeriod">
        /// Optionally how long to extend the sign-in.  Passing zero (the default) or values less than zero,
        /// will extend the sign-in by the default sign-in period implemented by the profile server.
        /// </param>
        /// <exception cref="ProfileException">Thrown if the profile server returns an error, i.e. when the server is not currently signed-in..</exception>
        /// <remarks>
        /// Profile implementations that don't required that developers sign-in when 
        /// secrets are requested should treat this as a NOP and just return OK.
        /// </remarks>
        void EnsureAuthenticated(TimeSpan signinPeriod = default);

        /// <summary>
        /// Requests that the profile server sign-out from it's credential source.
        /// </summary>
        void Signout();

        /// <summary>
        /// Requests the value of a secret password from 1Password via the assistant.
        /// </summary>
        /// <param name="name">Specifies the secret name.</param>
        /// <param name="vault">Optionally specifies the 1Password vault.  This defaults to the current user (as managed by the <see cref="IProfileClient"/> implementaton).</param>
        /// <param name="masterPassword">Optionally specifies the master 1Password when it is already known.</param>
        /// <param name="nullOnNotFound">Optionally specifies that <c>null</c> should be returned rather than throwing an exception when the secret does not exist.</param>
        /// <returns>The password value.</returns>
        /// <exception cref="ProfileException">Thrown if the profile server returns an error.</exception>
        string GetSecretPassword(string name, string vault = null, string masterPassword = null, bool nullOnNotFound = false);

        /// <summary>
        ///  Requests the value of a secret value from 1Password via the assistant.
        /// </summary>
        /// <param name="name">Specifies the secret name.</param>
        /// <param name="vault">Optionally specifies the 1Password vault.  This defaults to the current user (as managed by the <see cref="IProfileClient"/> implementaton).</param>
        /// <param name="masterPassword">Optionally specifies the master 1Password when it is already known.</param>
        /// <param name="nullOnNotFound">Optionally specifies that <c>null</c> should be returned rather than throwing an exception when the secret does not exist.</param>
        /// <returns>The password value.</returns>
        /// <exception cref="ProfileException">Thrown if the profile server returns an error.</exception>
        string GetSecretValue(string name, string vault = null, string masterPassword = null, bool nullOnNotFound = false);

        /// <summary>
        /// Requests a profile value from the assistant.
        /// </summary>
        /// <param name="name">Identifies the profile value.</param>
        /// <param name="nullOnNotFound">Optionally specifies that <c>null</c> should be returned rather than throwing an exception when the profile value does not exist.</param>
        /// <returns>The password value.</returns>
        /// <exception cref="ProfileException">Thrown if the profile server returns an error.</exception>
        string GetProfileValue(string name, bool nullOnNotFound = false);
    }
}
