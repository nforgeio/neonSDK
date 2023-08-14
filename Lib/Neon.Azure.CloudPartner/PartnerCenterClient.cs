// -----------------------------------------------------------------------------
// FILE:	    PartnerCenterClient.cs
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
using System.Text;

using Neon.Common;
using Neon.Net;

namespace Neon.Azure.CloudPartner
{
    /// <summary>
    /// Implements some Microsoft Azure Partner Center APIs.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Azure Partner Center has a REST API but does not appear to have any
    /// .NET packages implementing nice wrappers around this.  Rather than hardcoding
    /// REST calls in different places, we're going to do that in our own SDK.
    /// </para>
    /// <para>
    /// The Partner Center has a small number of REST API for managing offers
    /// but the schemas for the data submitted to and retrieved from the REST
    /// API differ based on the offer type.
    /// </para>
    /// </remarks>
    public class PartnerCenterClient
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public PartnerCenterClient()
        {
            JsonClient = new JsonClient();
        }

        /// <summary>
        /// Returns the <see cref="JsonClient"/> used to communicate with the Partner Center.
        /// </summary>
        internal JsonClient JsonClient { get; private set; }

        /// <summary>
        /// Returns the base URI for the Partner Center.
        /// </summary>
        internal Uri BaseAddress { get; private set; } = new Uri("https://graph.microsoft.com/rp");
    }
}
