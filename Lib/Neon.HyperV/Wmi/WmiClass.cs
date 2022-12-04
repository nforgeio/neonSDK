//-----------------------------------------------------------------------------
// FILE:	    WmiClass.cs
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
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;

using Neon.Common;
using Neon.Diagnostics;

namespace Neon.HyperV
{
    /// <summary>
    /// Defines relevant Hyper-V WMI class names.
    /// </summary>
    internal class WmiClass
    {
        //---------------------------------------------------------------------
        // Service classes

        /// <summary>
        /// Disk image management.
        /// </summary>
        public const string ImageManagementService = "Msvm_ImageManagementService";

        /// <summary>
        /// Network switch management.
        /// </summary>
        public const string VirtualEthernetSwitchManagementService = "Msvm_VirtualEthernetSwitchManagementService";

        /// <summary>
        /// Virtual machine management.
        /// </summary>
        public const string VirtualSystemManagementService = "Msvm_VirtualSystemManagementService";

        //---------------------------------------------------------------------
        // Object classes

        /// <summary>
        /// Virtual machine settings.
        /// </summary>
        public const string VirtualSystemSettingData = "Msvm_VirtualSystemSettingData";

        /// <summary>
        /// Virtual hard disk settings.
        /// </summary>
        public const string VirtualHardDiskSettingData = "Msvm_VirtualHardDiskSettingData";

        /// <summary>
        /// Virtual ethernet switch.
        /// </summary>
        public const string VirtualEthernetSwitch = "Msvm_VirtualEthernetSwitch";
    }
}
