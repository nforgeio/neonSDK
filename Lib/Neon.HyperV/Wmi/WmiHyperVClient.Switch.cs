//-----------------------------------------------------------------------------
// FILE:	    WmiHyperVClient.Switch.cs
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
using System.Linq;
using System.Management;
using System.Runtime;
using System.Text;
using System.Threading;

using Neon.Common;
using Neon.Diagnostics;
using static System.Net.WebRequestMethods;

namespace Neon.HyperV
{
    internal partial class WmiHyperVClient
    {
        /// <summary>
        /// Lists any virtual network switches. 
        /// </summary>
        /// <returns>The list of switches.</returns>
        /// <exception cref="HyperVException">Thrown for errors.</exception>
        public IEnumerable<VirtualSwitch> ListSwitches()
        {
            CheckDisposed();

            var switches = Wmi.Query($"select * from {WmiClass.VirtualEthernetSwitch}", scope);

            return switches.Select(
                @switch =>
                {
                    var switchType = VirtualSwitchType.Unknown;

                    return new VirtualSwitch()
                    {
                        Name = (string)@switch["ElementName"],
                        Type = switchType
                    };
                });
        }
    }
}
