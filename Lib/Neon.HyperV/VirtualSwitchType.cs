//-----------------------------------------------------------------------------
// FILE:        VirtualSwitchType.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.HyperV.PowerShell;
using Neon.Common;

namespace Neon.HyperV
{
    /// <summary>
    /// Enumerates the known Hyper-V virtual switch types.
    /// </summary>
    public enum VirtualSwitchType
    {
        // WARNING: The ordinal values here must match the related WMI values.

        /// <summary>
        /// The switch can communicate only with virtual machines using the
        /// same switch.
        /// </summary>
        Private = VMSwitchType.Private,

        /// <summary>
        /// The switch can communicate with the host operating system as well as
        /// any hosted virtual machines connected to an <see cref="External"/>
        /// or <see cref="Internal"/> switch.  The switch cannot communicate
        /// with anything outside of the host until it's assigned an IP address
        /// and NAT is enabled.
        /// </summary>
        Internal = VMSwitchType.Internal,

        /// <summary>
        /// The switch can communicate with the host operating system as well as
        /// any networks the host can reach.
        /// </summary>
        External = VMSwitchType.External
    }
}
