//-----------------------------------------------------------------------------
// FILE:	    NeonController.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright (c) 2005-2022 by neonFORGE LLC.  All rights reserved.
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
using System.Threading;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;

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
        private INeonLogger logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        protected NeonController()
        {
        }

        /// <summary>
        /// Returns the controller's logger.
        /// </summary>
        public INeonLogger Logger
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
                // I should be getting either an [ILogProvider] or [ILogManager] dynamically 
                // via dependency injection rather than hardcoding a call to [LogManager.Default]
                // and then getting an [INeonLogger] from that or wrapping an [ILogger] with
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
                // See the TODO in [LogManager.cs] for more information.

                return logger = LogManager.Default.GetLogger("Web-" + base.ControllerContext.ActionDescriptor.ControllerName);
            }
        }
    }
}
