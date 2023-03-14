//-----------------------------------------------------------------------------
// FILE:	    SeverityNumber.cs
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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Neon.Diagnostics
{
    /// <summary>
    /// The standard OpenTelemetry log severity numbers.
    /// </summary>
    internal enum SeverityNumber
    {
        SEVERITY_NUMBER_UNSPECIFIED = 0,
        SEVERITY_NUMBER_TRACE       = 1,
        SEVERITY_NUMBER_TRACE2      = 2,
        SEVERITY_NUMBER_TRACE3      = 3,
        SEVERITY_NUMBER_TRACE4      = 4,
        SEVERITY_NUMBER_DEBUG       = 5,
        SEVERITY_NUMBER_DEBUG2      = 6,
        SEVERITY_NUMBER_DEBUG3      = 7,
        SEVERITY_NUMBER_DEBUG4      = 8,
        SEVERITY_NUMBER_INFO        = 9,
        SEVERITY_NUMBER_INFO2       = 10,
        SEVERITY_NUMBER_INFO3       = 11,
        SEVERITY_NUMBER_INFO4       = 12,
        SEVERITY_NUMBER_WARN        = 13,
        SEVERITY_NUMBER_WARN2       = 14,
        SEVERITY_NUMBER_WARN3       = 15,
        SEVERITY_NUMBER_WARN4       = 16,
        SEVERITY_NUMBER_ERROR       = 17,
        SEVERITY_NUMBER_ERROR2      = 18,
        SEVERITY_NUMBER_ERROR3      = 19,
        SEVERITY_NUMBER_ERROR4      = 20,
        SEVERITY_NUMBER_FATAL       = 21,
        SEVERITY_NUMBER_FATAL2      = 22,
        SEVERITY_NUMBER_FATAL3      = 23,
        SEVERITY_NUMBER_FATAL4      = 24
    }
}
