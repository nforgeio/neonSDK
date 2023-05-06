//-----------------------------------------------------------------------------
// FILE:	    HttpApiException.cs
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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Neon.Common;
using Neon.Collections;
using Neon.Data;
using Neon.Diagnostics;
using Neon.Retry;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Neon.Net
{
    /// <summary>
    /// Thrown by ASP.NET web API implementations to optionally specify an error code
    /// that can ultimately be deserialized as the HTTP reason phrase by clients.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The basic idea here is that non-type REST services will typically need to return
    /// a computer readable code to specify business logic level errors so that client
    /// programs can easily identify and handle problems without the need for parsing
    /// a potentially localized error message.
    /// </para>
    /// <note>
    /// We're restricting error codes to 1-32 characters including ASCII letters, digits, 
    /// undercores, dots, or dashes.
    /// </note>
    /// <note>
    /// This error code will be transmitted back to the client as the <b>>HTTP reason phrase</b>
    /// when specified or the standard reason phrase associated with the HTTP status code otherwise.
    /// </note>
    /// </remarks>
    public class HttpApiException : Exception
    {
        //---------------------------------------------------------------------
        // Static members.

        private static Regex statusCodeRegex = new Regex(@"^[a-zA-Z0-9_\.-]{1,32}$");

        //---------------------------------------------------------------------
        // Instance members.

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="message">Optionally specifies the exception message.</param>
        /// <param name="errorCode">Optionally specifies the <see cref="HttpStatusCode"/>.  This </param>
        /// <param name="statusCode">Optionally specifies the HTTP status code.  This defaults to <see cref="HttpStatusCode.BadRequest"/>.</param>
        /// <remarks>
        /// <note>
        /// <paramref name="errorCode"/> is restricted to 1-32 characters including ASCII letters, digits, 
        /// undercores, dots, or dashes.
        /// </note>
        /// </remarks>
        public HttpApiException(string message = null, string errorCode = null,  HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(message)
        {
            if (string.IsNullOrEmpty(errorCode))
            {
                Covenant.Requires<ArgumentException>(statusCodeRegex.IsMatch(errorCode), () => nameof(errorCode), () => $"Invalid error code [{errorCode}]: Error codes must consist of 1-32 ASCII letters, digits, underscores, dots, or dashes.");
            }
            else
            {
                errorCode = null;
            }

            this.StatusCode = statusCode;
            this.ErrorCode  = errorCode;
        }

        /// <summary>
        /// Returns the HTTP status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Returns the business logic error code or <c>null</c>.
        /// </summary>
        public string ErrorCode { get; private set; }
    }
}
