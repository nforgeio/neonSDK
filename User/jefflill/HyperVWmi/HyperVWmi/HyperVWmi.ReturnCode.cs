//-----------------------------------------------------------------------------
// FILE:	    HyperVWmi.ReturnCode.cs
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
    internal sealed partial class HyperVWmi
    {
        public static class ReturnCode
        {
            public const UInt32 Completed = 0;
            public const UInt32 Started = 4096;
            public const UInt32 Failed = 32768;
            public const UInt32 AccessDenied = 32769;
            public const UInt32 NotSupported = 32770;
            public const UInt32 Unknown = 32771;
            public const UInt32 Timeout = 32772;
            public const UInt32 InvalidParameter = 32773;
            public const UInt32 SystemInUse = 32774;
            public const UInt32 InvalidState = 32775;
            public const UInt32 IncorrectDataType = 32776;
            public const UInt32 SystemNotAvailable = 32777;
            public const UInt32 OutofMemory = 32778;
        }
    }
}
