//-----------------------------------------------------------------------------
// FILE:	    LocalHostSection.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Retry;
using NeonTask;

namespace Neon.Net
{
    /// <summary>
    /// Returned by <see cref="NetHelper.ListLocalHostsSections"/> holding information about
    /// a named section of host entries within the local <b>$/etc/hosts</b> file.
    /// </summary>
    public class LocalHostSection
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Identifies the section.</param>
        /// <param name="hostEntries">The dictionary of hostname/address entries.</param>
        public LocalHostSection(string name, Dictionary<string, IPAddress> hostEntries)
        {
            this.Name        = name;
            this.HostEntries = hostEntries;
        }

        /// <summary>
        /// Returns the section name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Returns the dictionary of hostname/address entries.
        /// </summary>
        public Dictionary<string, IPAddress> HostEntries { get; private set; }
    }
}
