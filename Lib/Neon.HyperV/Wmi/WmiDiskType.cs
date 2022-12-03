//-----------------------------------------------------------------------------
// FILE:	    WmiDiskType.cs
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
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;

using Neon.Common;
using Neon.Diagnostics;

namespace Neon.HyperV
{
    /// <summary>
    /// Enumerates possible virtual disk types.
    /// </summary>
    internal static class WmiDiskType
    {
        /// <summary>
        /// Disk will be persisted at full size on underlying pyhysical storage.
        /// </summary>
        public const ushort Fixed = 2;

        /// <summary>
        /// Disk will start out small on underlying physical storage and grow
        /// as required.
        /// </summary>
        public const ushort Dynamic = 3;

        /// <summary>
        /// Disk holds differences from a parent disk.
        /// </summary>
        public const ushort Differencing = 4;
    }
}
