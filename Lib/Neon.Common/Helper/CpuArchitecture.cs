//-----------------------------------------------------------------------------
// FILE:	    CpuArchitecture.cs
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
using System.Runtime.Serialization;

namespace Neon.Common
{
    /// <summary>
    /// Enumerates the known CPU architectures.
    /// </summary>
    public enum CpuArchitecture
    {
        /// <summary>
        /// 32-bit AMD/Intel
        /// </summary>
        [EnumMember(Value = "amd32")]
        Amd32,

        /// <summary>
        /// 64-bit AMD/Intel
        /// </summary>
        [EnumMember(Value = "amd64")]
        Amd64,

        /// <summary>
        /// 32-bit ARM
        /// </summary>
        [EnumMember(Value = "arm32")]
        Arm32,

        /// <summary>
        /// 64-bit ARM
        /// </summary>
        [EnumMember(Value = "arm64")]
        Arm64
    }
}
