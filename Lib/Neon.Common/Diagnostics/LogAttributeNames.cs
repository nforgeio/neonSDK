//-----------------------------------------------------------------------------
// FILE:	    LogAttributeNames.cs
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

namespace Neon.Diagnostics
{
    /// <summary>
    /// Defines attributes names used when emitting log records.
    /// </summary>
    public class LogAttributeNames
    {
        //---------------------------------------------------------------------
        // These are the names we use for exporting the built-in LogRecord properties.

        /// <summary>
        /// Identifies our internal message body. 
        /// </summary>
        public const string InternalBody = "{Body}";

        /// <summary>
        /// Identifies the MSFT logger implementation's attribute that holds the message format string.
        /// </summary>
        public const string InternalOriginalFormat = "{OriginalFormat}";

        /// <summary>
        /// Identifies the MSFT logger implementation's attribute that holds the category name.
        /// </summary>
        public const string CategoryName = "dotnet.ilogger.category";

        /// <summary>
        /// Identifies a related exception.
        /// </summary>
        public const string Exception = "exception";

        //---------------------------------------------------------------------
        // These are attribute names reserved by NeonSDK and other Neon related projects.

        /// <summary>
        /// <b>bool:</b> Indicates that the log event is related to a transient error.
        /// </summary>
        public const string NeonTransient = "neon.transient";
    }
}
