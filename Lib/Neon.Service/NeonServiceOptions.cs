//-----------------------------------------------------------------------------
// FILE:        NeonServiceOptions.cs
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

using Microsoft.Extensions.Diagnostics.Enrichment;
using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

namespace Neon.Service
{
    /// <summary>
    /// Optionally passed to the <see cref="NeonService"/> constructor to specify additional
    /// service related options.
    /// </summary>
    public class NeonServiceOptions
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public NeonServiceOptions()
        {
        }

        /// <summary>
        /// <para>
        /// Indicates that the current git branch name and commit should be appended 
        /// to the service version passed to the <see cref="NeonService"/> constructor.
        /// This defaults to <c>true</c>.
        /// </para>
        /// <note>
        /// This is ignored when your project is not using git for source control.
        /// </note>
        /// </summary>
        public bool AppendGitInfo { get; set; } = true;

        /// <summary>
        /// <para>
        /// Optionally specifies the <see cref="ILoggerFactory"/> the service will use for logging purposes.
        /// When set to <c>null</c> (the default), the <see cref="NeonService"/> class will configure
        /// the OpenTelemetry logging pipeline and configure a logger factory for the service and NEONSDK libraries.
        /// </para>
        /// <para>
        /// You can set this when you need a completely custom logging configuration.
        /// </para>
        /// </summary>
        public ILoggerFactory LoggerFactory { get; set; } = null;

        /// <summary>
        /// <para>
        /// Optionally specifies the <see cref="TracerProvider"/> the service will use for tracing purposes.
        /// When set to <c>null</c> (the default), the <see cref="NeonService"/> class will configure the 
        /// tracing pipeline for the service and NEONSDK libraries.
        /// </para>
        /// <para>
        /// You can set this when you need a completely custom tracing configuration.
        /// </para>
        /// </summary>
        public TracerProvider TracerProvider { get; set; }

        /// <summary>
        /// <para>
        /// Optionally specifies the folder path where the service will maintain the <b>health-status</b>
        /// file and deploy the <b>health-check</b> and <b>ready-check</b> binaries.  See the class 
        /// documentation for more information: <see cref="Neon.Service"/>.
        /// </para>
        /// <para>
        /// This defaults to: <b>/</b> to make it easy to configure the Kubernetes probes.
        /// You can disable this feature by passing <b>"DISABLED"</b> instead.
        /// </para>
        /// <note>
        /// Health status generation only works on Linux.  This feature is dsabled on Windows and OS/X.
        /// </note>
        /// </summary>
        public string HealthFolder { get; set; } = null;

        /// <summary>
        /// <para>
        /// Optionally specifies a service map describing this service and potentially other services.
        /// Service maps can be used to run services locally on developer workstations via <b>Neon.Xunit.NeonServiceFixture</b>
        /// or other means to avoid port conflicts or to emulate a cluster of services without Kubernetes
        /// or containers.  This is a somewhat advanced topic that needs documentation.
        /// </para>
        /// <note>
        /// <see cref="ServiceMap"/> is a somewhat dated concept that doesn't make a lot of sense in
        /// the Kubernetes world.  We recommend that new code avoid this.
        /// </note>
        /// </summary>
        public ServiceMap ServiceMap { get; set; } = null;

        /// <summary>
        /// <para>
        /// Optionally specifies the path where Kubernetes may write a termination message
        /// before terminating the pod hosting the message.  The <see cref="NeonService"/>
        /// class will check for this file when it receives a termination signal when 
        /// running on Linux and write the file contents to the log before terminating.
        /// </para>
        /// <para>
        /// This defaults to: <b>/dev/termination-log</b>
        /// </para>
        /// <note>
        /// This is ignored for all platforms besides Linux.
        /// </note>
        /// </summary>
        public string TerminationMessagePath { get; set; } = null;

        /// <summary>
        /// Optionally specifies the termination timeout (defaults to <see cref="ProcessTerminator.DefaultGracefulTimeout"/>).  
        /// See <see cref="ProcessTerminator"/> for more information.
        /// </summary>
        public TimeSpan GracefulShutdownTimeout { get; set; } = default;

        /// <summary>
        /// Optionally specifies the minimum time to wait before allowing termination to proceed.
        /// This defaults to <see cref="ProcessTerminator.DefaultMinShutdownTime"/>.  See 
        /// <see cref="ProcessTerminator"/> for more information.
        /// </summary>
        public TimeSpan MinShutdownTime { get; set; } = default;

        /// <summary>
        /// Optionally specifies log enrichers to add to the Logging pipeline.
        /// </summary>
        public IEnumerable<ILogEnricher> LogEnrichers { get; set; } = null;

        /// <summary>
        /// Optionally specifies the prefix to use for environment variables that will be loaded into configuration.
        /// </summary>
        public string EnvironmentVariablePrefix { get; set; } = null;

        /// <summary>
        /// Optionally specifies the character to use to replace dots (<b>.</b>) in environment variable names.
        /// </summary>
        public string EnvironmentVariableDotReplacement { get; set; } = null;
    }
}
