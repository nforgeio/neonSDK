//-----------------------------------------------------------------------------
// FILE:	    WmiDiskCompactMode.cs
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
    /// Enumerates the possible virtual disk compaction modes.
    /// </summary>
    internal static class WmiDiskCompactMode
    {
        /// <summary>
        /// Full optimization requires the most time and resources. It will take out both empty and unused blocks. 
        /// In order to detect unused blocks, the VHDX must be mounted in read-only mode. If the VHDX isn’t mounted, 
        /// then it won’t be able to find empty blocks as easily (assume that it won’t find them at all).
        /// </summary>
        public const ushort Full = 0;

        /// <summary>
        /// The system only looks for unused blocks using the contained file system’s metadata. If you don’t mount the
        /// VHDX first, nothing will change.
        /// </summary>
        public const ushort Quick = 1;

        /// <summary>
        /// Retrim reads from the VHDX file’s metadata for blocks marked as empty and reports them as trim or unmap
        /// commands to the underlying hardware.
        /// </summary>
        public const ushort Retrim = 2;

        /// <summary>
        /// Utilizes information from the trim/unmap commands to detect unused blocks. Does not look for empty blocks and 
        /// does not query the contained file system for unused blocks.
        /// </summary>
        public const ushort Pretrimmed = 3;

        /// <summary>
        /// If the VHDX driver intercepted zero writes for existing blocks (such as from tools like sdelete, then it 
        /// has already recorded in its own tables that they’re empty. Use this mode to remove only those blocks.
        /// </summary>
        public const ushort Prezeroed = 4;
    }
}
