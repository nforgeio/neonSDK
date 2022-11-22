//-----------------------------------------------------------------------------
// FILE:	    NeonController.cs
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
using System.IO;
using System.Net;
using System.Threading;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

using Neon.Common;
using Neon.Diagnostics;
using Neon.Net;

namespace Neon.Web
{
    /// <summary>
    /// Enhances the <see cref="Controller"/> class to simplify and enhance web application logging.  Use this
    /// as the base class for ASP.NET UX applications.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class includes the <see cref="Logger"/> property which returns a logger that can be
    /// used for logging controller events.  This logger will have it's module set to <b>WEB-CONTROLLER-NAME</b>
    /// and can be used as a simplier alternative to doing dependency injection to your controller's constructor.
    /// </para>
    /// </remarks>
    [TypeFilter(typeof(LoggingExceptionFilter))]
    public abstract class NeonController : Controller
    {
        private ILogger logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        protected NeonController()
        {
        }

        /// <summary>
        /// Returns the controller's logger.
        /// </summary>
        public ILogger Logger
        {
            get
            {
                // Lazy load the logger for better performance in the common case
                // where nothing is logged for a request.

                if (logger != null)
                {
                    return logger;
                }

                // $todo(jefflill):
                //
                // I should be getting either an [ILogProvider] or [ITelemetryHub] dynamically 
                // via dependency injection rather than hardcoding a call to [TelemetryHub.Default]
                // and then getting an [ILogger] from that or wrapping an [ILogger] with
                // a [NeonLoggerShim].
                //
                // I'm not entirely sure how to accomplish this.  I believe the only way is
                // to add a [ILogProvider] parameter to this class' constructor (as well as
                // that of any derived classes) and then inspect the actual instance type
                // passed and then decide whether we need a [NeonLoggerShim] or not.
                //
                // It would be unforunate though to require derived classes to have to handle
                // this.  An alternative might be use property injection, but I don't think
                // the ASP.NET pipeline supports that.
                //
                // See the TODO in [TelemetryHub.cs] for more information.

                return logger = TelemetryHub.CreateLogger("Web-" + base.ControllerContext.ActionDescriptor.ControllerName);
            }
        }

        /// <summary>
        /// Throws a <see cref="HttpApiException"/> when <paramref name="condition"/> is <c>false</c>.
        /// </summary>
        /// <param name="condition">The condition being checked.</param>
        /// <param name="message">Optionally specifies a human readable message.</param>
        /// <param name="errorCode">Optionally specifies a computer readable error code string.</param>
        /// <param name="statusCode">Optionally specifies the HTTP status code.  This defaults to <see cref="HttpStatusCode.BadRequest"/>.</param>
        /// <remarks>
        /// <note>
        /// <paramref name="errorCode"/> is restricted to 1-32 characters including ASCII letters, digits, 
        /// undercores, dots, or dashes.
        /// </note>
        /// </remarks>
        /// <exception cref="HttpApiException">Thrown when <paramref name="condition"/> is <c>false</c>.</exception>
        public void Requires(bool condition, string message = null, string errorCode = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            if (!condition)
            {
                throw new HttpApiException(message, errorCode, statusCode);
            }
        }

        /// <summary>
        /// Throws a <see cref="HttpApiException"/> when the <paramref name="predicate"/> function returns <c>false</c>.
        /// </summary>
        /// <param name="predicate">Called to retrieve the condition value..</param>
        /// <param name="message">Optionally specifies a human readable message.</param>
        /// <param name="errorCode">Optionally specifies a computer readable error code string.</param>
        /// <param name="statusCode">Optionally specifies the HTTP status code.  This defaults to <see cref="HttpStatusCode.BadRequest"/>.</param>
        /// <remarks>
        /// <note>
        /// <paramref name="errorCode"/> is restricted to 1-32 characters including ASCII letters, digits, 
        /// undercores, dots, or dashes.
        /// </note>
        /// </remarks>
        /// <exception cref="HttpApiException">Thrown when <paramref name="predicate"/> returns <c>false</c>.</exception>
        public void Requires(Func<bool> predicate, string message = null, string errorCode = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            Covenant.Requires<ArgumentNullException>(predicate != null, nameof(predicate));

            if (!predicate())
            {
                throw new HttpApiException(message, errorCode, statusCode);
            }
        }

        /// <summary>
        /// Throws a <see cref="HttpApiException"/> when <paramref name="value"/> is <c>null</c>.
        /// </summary>
        /// <param name="value">The object value being checked.</param>
        /// <param name="name">Identifies the value being checked.</param>
        /// <param name="errorCode">Optionally specifies a computer readable error code string.</param>
        /// <param name="statusCode">Optionally specifies the HTTP status code.  This defaults to <see cref="HttpStatusCode.BadRequest"/>.</param>
        /// <remarks>
        /// <note>
        /// <paramref name="errorCode"/> is restricted to 1-32 characters including ASCII letters, digits, 
        /// undercores, dots, or dashes.
        /// </note>
        /// </remarks>
        /// <exception cref="HttpApiException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public void RequiresNotNull(object value, string name, string errorCode = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            if (value == null)
            {
                throw new HttpApiException($"[{name}] cannot be null.", errorCode, statusCode);
            }
        }

        /// <summary>
        /// Throws a <see cref="HttpApiException"/> when <paramref name="value"/> is <c>null</c> or empty.
        /// </summary>
        /// <param name="value">The string value being checked.</param>
        /// <param name="name">Identifies the value being checked.</param>
        /// <param name="errorCode">Optionally specifies a computer readable error code string.</param>
        /// <param name="statusCode">Optionally specifies the HTTP status code.  This defaults to <see cref="HttpStatusCode.BadRequest"/>.</param>
        /// <remarks>
        /// <note>
        /// <paramref name="errorCode"/> is restricted to 1-32 characters including ASCII letters, digits, 
        /// undercores, dots, or dashes.
        /// </note>
        /// </remarks>
        /// <exception cref="HttpApiException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
        public void RequiresNotNull(string value, string name, string errorCode = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new HttpApiException($"[{name}] cannot be null or empty.", errorCode, statusCode);
            }
        }
    }
}
