//-----------------------------------------------------------------------------
// FILE:        HttpResponseMessageExtensions.cs
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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Neon.BuildInfo;
using Neon.Collections;
using Neon.Common;
using Neon.Diagnostics;
using Neon.Net;
using Neon.Tasks;

namespace Neon.Common
{
    /// <summary>
    /// Implements <see cref="HttpResponseMessage"/> extension methods.
    /// </summary>
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Throws a <see cref="HttpRequestException"/> when the response status code does not
        /// indicate success.  This improves on <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/>
        /// by including the URI in the exception message.
        /// </summary>
        /// <param name="response">The HTTP response received</param>
        public static void EnsureSuccessStatusCodeEx(this HttpResponseMessage response)
        {
            Covenant.Requires<ArgumentNullException>(response != null, nameof(response));

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Response status code does not indicate success: 403 (Forbidden) for: {response.RequestMessage.RequestUri}");
            }
        }
    }
}
