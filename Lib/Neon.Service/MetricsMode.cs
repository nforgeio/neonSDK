//-----------------------------------------------------------------------------
// FILE:	    MetricsMode.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright (c) 2005-2022 by neonFORGE LLC.  All rights reserved.
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Cryptography;
using Neon.Diagnostics;
using Neon.IO;
using Neon.Retry;
using Neon.Windows;

namespace Neon.Service
{
    /// <summary>
    /// Used control how or whether a <see cref="NeonService"/>  publishes Prometheus metrics.
    /// </summary>
    public enum MetricsMode
    {
        /// <summary>
        /// Prevents <see cref="NeonService"/> from configuring the OpenTelemetry metrics pipeline.
        /// This can be used to disable metrics or when you want to configure the pipeline yourself.
        /// </summary>
        None = 0,

        /// <summary>
        /// <see cref="NeonService"/> will configure the OpenTelemetry metrics pipeline to export
        /// metrics via the standard Prometheus scrape protocol on the port specified by 
        /// <see cref="NeonServiceOptions.MetricsPort"/>.
        /// </summary>
        Prometheus
    }
}
