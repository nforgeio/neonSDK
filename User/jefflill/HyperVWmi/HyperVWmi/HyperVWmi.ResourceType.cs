//-----------------------------------------------------------------------------
// FILE:	    HyperVWmi.ResourceType.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2022 by NEONFORGE LLC.  All rights reserved.
// COPYRIGHT:   NONE
//
// This code was obtained from a Microsoft code sample which did not include a
// license or copyright statement.  We're going to apply the Apache License to
// this to be compatible with the rest of the neon-sdk. 
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

#pragma warning disable CA1416  // Validate platform compatibility

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neon.HyperV
{
    internal partial class HyperVWmi
    {
        public static class ResourceType
        {
            public const UInt16 Other = 1;
            public const UInt16 ComputerSystem = 2;
            public const UInt16 Processor = 3;
            public const UInt16 Memory = 4;
            public const UInt16 IDEController = 5;
            public const UInt16 ParallelSCSIHBA = 6;
            public const UInt16 FCHBA = 7;
            public const UInt16 iSCSIHBA = 8;
            public const UInt16 IBHCA = 9;
            public const UInt16 EthernetAdapter = 10;
            public const UInt16 OtherNetworkAdapter = 11;
            public const UInt16 IOSlot = 12;
            public const UInt16 IODevice = 13;
            public const UInt16 FloppyDrive = 14;
            public const UInt16 CDDrive = 15;
            public const UInt16 DVDdrive = 16;
            public const UInt16 Serialport = 17;
            public const UInt16 Parallelport = 18;
            public const UInt16 USBController = 19;
            public const UInt16 GraphicsController = 20;
            public const UInt16 StorageExtent = 21;
            public const UInt16 Disk = 22;
            public const UInt16 Tape = 23;
            public const UInt16 OtherStorageDevice = 24;
            public const UInt16 FirewireController = 25;
            public const UInt16 PartitionableUnit = 26;
            public const UInt16 BasePartitionableUnit = 27;
            public const UInt16 PowerSupply = 28;
            public const UInt16 CoolingDevice = 29;
            public const UInt16 DisketteController = 1;
        }
    }
}
