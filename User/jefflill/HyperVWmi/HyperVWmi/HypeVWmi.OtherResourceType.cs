//-----------------------------------------------------------------------------
// FILE:	    HypeVWmi.OtherResourceType.cs
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
    internal sealed partial class HypeVWmi
    {
        public static class OtherResourceType
        {
            public const string DisketteController = "Microsoft Virtual Diskette Controller";
        }
    }
}
