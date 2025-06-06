//-----------------------------------------------------------------------------
// FILE:        NetworkException.cs
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neon.Net
{
    /// <summary>
    /// Indicates network related problems.
    /// </summary>
    public class NetworkException : Exception
    {
        /// <summary>
        /// Constuctor.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The optional inner exception.</param>
        public NetworkException(string message, Exception inner = null)
            : base(message, inner)
        {
        }
    }
}
