//-----------------------------------------------------------------------------
// FILE:	    ExceptionFilter.cs
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
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

using Neon.Common;
using Neon.Data;
using Neon.Diagnostics;
using Neon.Net;

namespace Neon.Web
{
    /// <summary>
    /// <para>
    /// Implements the custom ASP.NET middleware exception filter that handles 
    /// <see cref="HttpApiException"/> and other exceptions by formatting the
    /// error details as JSON in the response content as well as logging exception.
    /// </para>
    /// <para>
    /// This filter also logs any intercepted exceptions.
    /// </para>
    /// </summary>
    internal class ExceptionFilter : IActionFilter, IOrderedFilter
    {
        //---------------------------------------------------------------------
        // Private types

        private class ErrorDetails
        {
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="e">The exception.</param>
            /// <param name="errorCode">Optionally specifies the API error code.</param>
            /// <param name="statusCode">Optionally specifies the HTTP status code.  This defaults to <see cref="StatusCodes.Status400BadRequest"/>.</param>
            public ErrorDetails(Exception e, string errorCode = null, int statusCode = StatusCodes.Status400BadRequest)
            {
                this.Type       = e.GetType().FullName;
                this.Message    = e.Message;
                this.ErrorCode  = errorCode;
                this.StatusCode = statusCode;
            }

            /// <summary>
            /// The fully qualified exception type name.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// The exception message.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// The API error code.
            /// </summary>
            public string ErrorCode { get; set; }

            /// <summary>
            /// The HTTP status code.
            /// </summary>
            public int StatusCode { get; set; }
        }

        //---------------------------------------------------------------------
        // Static members

        // $todo(jefflill):
        //
        // I should be getting either an [ILogProvider] or [IelemetryHub] dynamically 
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

        private static ILogger logger = TelemetryHub.CreateLogger("NeonSdkExceptionFilter");

        //---------------------------------------------------------------------
        // Implementation

        /// <inheritdoc/>
        public int Order => 1000;   // $hack(jefflill): hardcoding this for now.

        /// <inheritdoc/>
        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        /// <inheritdoc/>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Log the exception.

            logger.LogErrorEx(context.Exception);

            // Format the details as JSON.

            if (context.Exception is HttpApiException apiException)
            {
                context.Result = new JsonResult(new ErrorDetails(apiException, apiException.ErrorCode, (int)apiException.StatusCode));
            }
            else
            {
                context.Result = new JsonResult(new ErrorDetails(context.Exception));

            }
            
            context.ExceptionHandled = true;
        }
    }
}
