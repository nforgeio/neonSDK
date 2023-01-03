//-----------------------------------------------------------------------------
// FILE:	    NeonHelper.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Neon.BuildInfo;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using Neon.Diagnostics;

namespace Neon.Common
{
    /// <summary>
    /// Provides global common utilities and state.
    /// </summary>
    public static partial class NeonHelper
    {
        /// <summary>
        /// Identifies the production neonSDK container image registry.
        /// </summary>
        public const string NeonSdkProdRegistry = "ghcr.io/neonrelease";

        /// <summary>
        /// Identifies the development neonSDK container image registry.
        /// </summary>
        public const string NeonSdkDevRegistry = "ghcr.io/neonrelease-dev";

        /// <summary>
        /// Returns the appropriate public container neonSDK registry to be used for the git 
        /// branch the assembly was built from.  This returns <see cref="NeonSdkProdRegistry"/> for
        /// release branches and <see cref="NeonSdkDevRegistry"/> for all other branches.
        /// </summary>
        public static string NeonSdkBranchRegistry => ThisAssembly.Git.Branch.StartsWith("release-", StringComparison.InvariantCultureIgnoreCase) ? NeonSdkProdRegistry : NeonSdkDevRegistry;

        /// <summary>
        /// Used for thread synchronization.
        /// </summary>
        private static object syncRoot = new object();

        /// <summary>
        /// Ordinal value of an ASCII carriage return.
        /// </summary>
        public const int CR = 0x0D;

        /// <summary>
        /// Ordinal value of an ASCII linefeed.
        /// </summary>
        public const int LF = 0x0A;

        /// <summary>
        /// Ordinal value of an ASCII horizontal TAB.
        /// </summary>
        public const int HT = 0x09;

        /// <summary>
        /// Ordinal value of an ASCII escape character.
        /// </summary>
        public const int ESC = 0x1B;

        /// <summary>
        /// Ordinal value of an ASCII TAB character.
        /// </summary>
        public const int TAB = 0x09;

        /// <summary>
        /// A string consisting of a CRLF sequence.
        /// </summary>
        public const string CRLF = "\r\n";

        /// <summary>
        /// Returns the native text line ending for the current environment.
        /// </summary>
        public static readonly string LineEnding = IsWindows ? "\r\n" : "\n";

        /// <summary>
        /// Returns the characters used as wildcards for the current file system.
        /// </summary>
        public static char[] FileWildcards { get; private set; } = new char[] { '*', '?' };

        /// <summary>
        /// Returns the date format string used for serialize dates with millisecond
        /// precision to strings like: <b>2018-06-05T14:30:13.000Z</b>
        /// </summary>
        public const string DateFormatTZ = "yyyy-MM-ddTHH:mm:ss.fffZ";

        /// <summary>
        /// Returns the date format string used for serialize dates with millisecond
        /// precision to strings like: <b>2018-06-05T14:30:13.000+00:00</b>
        /// </summary>
        public const string DateFormatTZOffset = "yyyy-MM-ddTHH:mm:ss.fff+00:00";

        /// <summary>
        /// Returns the date format string used for serialize dates with microsecond
        /// precision to strings like: <b>2018-06-05T14:30:13.000000Z</b>
        /// </summary>
        public const string DateFormatMicroTZ = "yyyy-MM-ddTHH:mm:ss.ffffffZ";

        /// <summary>
        /// Returns the date format string used for serialize dates with microsecond
        /// precision to strings like: <b>2018-06-05T14:30:13.000000+00:00</b>
        /// </summary>
        public const string DateFormatMicroTZOffset = "yyyy-MM-ddTHH:mm:ss.ffffff+00:00";

        /// <summary>
        /// Returns the date format string used for serialize dates with 100 nanosecond
        /// precision to strings like: <b>2018-06-05T14:30:13.000000Z</b>
        /// </summary>
        public const string DateFormat100NsTZ = "yyyy-MM-ddTHH:mm:ss.fffffffZ";

        /// <summary>
        /// Returns the date format string used for serialize dates with 100 nanosecond
        /// precision to strings like: <b>2018-06-05T14:30:13.000000+00:00</b>
        /// </summary>
        public const string DateFormat100NsTZOffset = "yyyy-MM-ddTHH:mm:ss.fffffff+00:00";

        /// <summary>
        /// Returns the Unix epoch time: 01-01-1970 (UTC).
        /// </summary>
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0).ToUniversalTime();

        /// <summary>
        /// Returns the prefix to be used for Neon related Prometheus names.
        /// </summary>
        public const string NeonMetricsPrefix = "neon.";

        /// <summary>
        /// The URI for the public AWS S3 bucket where we persist cluster VM images 
        /// and other things.
        /// </summary>
        public const string NeonPublicBucketUri = "https://neon-public.s3.us-west-2.amazonaws.com";

        /// <summary>
        /// The URI for Kubernetes <b>Services</b> deployed to namespaces for forwarding OTEL
        /// Collector log and trace information to the local Tempo installation or elsewhere.
        /// </summary>
        public const string NeonKubeOtelCollectorUri = "http://grafana-agent-node.neon-monitor.svc.cluster.local:4317";

        /// <summary>
        /// The <see cref="Neon.Common.ServiceContainer"/> instance returned by 
        /// <see cref="ServiceContainer"/>.
        /// </summary>
        private static ServiceContainer serviceContainer;

        /// <summary>
        /// Set to <c>true</c> when the special UTF-8 encoding provider with the misspelled
        /// name <b>utf8</b> (without the dash) has been initialized.  See 
        /// <see cref="RegisterMisspelledUtf8Provider()"/> for more information.
        /// </summary>
        private static bool specialUtf8EncodingProvider = false;

        /// <summary>
        /// The root dependency injection service container used by Neon class libraries. 
        /// and applications.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This instance implements both the <see cref="IServiceCollection"/> and <see cref="IServiceProvider"/>
        /// interfaces and supports adding, removing, and locating services over the lifetime
        /// of the application.  This is more flexible than the default Microsoft injection
        /// pattern, where services are added to an <see cref="IServiceCollection"/> at startup
        /// and then a read-only snapshot is taken via a <b>BuildServiceProvider()</b> call
        /// that is used throughout the remaining application lifespan.
        /// </para>
        /// <para>
        /// This is implemented by a <see cref="ServiceCollection"/> by default.  It is possible
        /// to replace this very early during application initialization but the default 
        /// implementation should suffice for most purposes.
        /// </para>
        /// </remarks>
        public static ServiceContainer ServiceContainer
        {
            get
            {
                lock (syncRoot)
                {
                    if (serviceContainer == null)
                    {
                        serviceContainer = new ServiceContainer();
                    }

                    return serviceContainer;
                }
            }

            set { serviceContainer = value; }
        }

        /// <summary>
        /// Ensures that a special UTF-8 text encoding provider misnamed as <b>utf8</b>
        /// (without the dash) is registered.  This is required sometimes because
        /// certain REST APIs may return incorrect <b>charset</b> values.
        /// </summary>
        public static void RegisterMisspelledUtf8Provider()
        {
            lock (syncRoot)
            {
                if (specialUtf8EncodingProvider)
                {
                    return;
                }

                Encoding.RegisterProvider(new SpecialUtf8EncodingProvider());
                specialUtf8EncodingProvider = true;
            }
        }

        /// <summary>
        /// Converts the number of nanoseconds from the Unix Epoch (1/1/1970 12:00:00am)
        /// into a UTC cref="DateTime"/> (UTC).
        /// </summary>
        /// <returns>The converted <see cref="DateTime"/>.</returns>
        public static DateTime UnixEpochNanosecondsToDateTimeUtc(long nanoseconds)
        {
            return UnixEpoch + TimeSpan.FromTicks(nanoseconds / 100);
        }

        /// <summary>
        /// Converts the number of milliseconds from the Unix Epoch (1/1/1970 12:00:00am)
        /// into a UTC cref="DateTime"/> (UTC).
        /// </summary>
        /// <returns>The converted <see cref="DateTime"/>.</returns>
        public static DateTime UnixEpochMillisecondsToDateTimeUtc(long milliseconds)
        {
            return UnixEpoch + TimeSpan.FromMilliseconds(milliseconds);
        }

        /// <summary>
        /// Returns the path to the current user's HOME folder.
        /// </summary>
        public static string UserHomeFolder
        {
            get
            {
                if (NeonHelper.IsWindows)
                {
                    return Environment.GetEnvironmentVariable("USERPROFILE");
                }
                else if (NeonHelper.IsLinux || NeonHelper.IsOSX)
                {
                    return System.Environment.GetEnvironmentVariable("HOME");
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        /// <summary>
        /// <para>
        /// Returns the path to the development folder for NEONFORGE developers.  This folder
        /// is used to hold build and test related files and is named <b>.neondev</b> under
        /// the current user's home folder.
        /// </para>
        /// <para>Related: <see cref="UserNeonDevBuildFolder"/>, <see cref="UserNeonDevTestFolder"/></para>
        /// <note>
        /// This property ensures that the folder exists.
        /// </note>
        /// </summary>
        public static string UserNeonDevFolder
        {
            get
            {
                var path = Path.Combine(UserHomeFolder, ".neondev");

                Directory.CreateDirectory(path);

                return path;
            }
        }

        /// <summary>
        /// <para>
        /// Returns the path to the development/build folder for NEONFORGE developers.  This folder
        /// is used to hold build related files and is named <b>.neondev/build</b> under
        /// the current user's home folder.
        /// </para>
        /// <note>
        /// This property ensures that the folder exists.
        /// </note>
        /// </summary>
        public static string UserNeonDevBuildFolder => Directory.CreateDirectory(Path.Combine(UserNeonDevFolder, "build")).FullName;

        /// <summary>
        /// <para>
        /// Returns the path to the development/build folder for NEONFORGE developers.  This folder
        /// is used to hold unit test related files and is named <b>.neondev/test</b> under
        /// the current user's home folder.
        /// </para>
        /// <note>
        /// This property ensures that the folder exists.
        /// </note>
        /// </summary>
        public static string UserNeonDevTestFolder => Directory.CreateDirectory(Path.Combine(UserNeonDevFolder, "test")).FullName;
    }
}
