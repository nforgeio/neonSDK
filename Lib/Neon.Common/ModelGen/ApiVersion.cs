//-----------------------------------------------------------------------------
// FILE:        ApiVersion.cs
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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Neon.ModelGen
{
    /// <summary>
    /// Used to specify ASP.NET API versions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// API versions are formatted like:
    /// </para>
    /// <code>
    /// [VERSIONGROUP.]MAJOR.MINOR[-STATUS]
    ///
    /// or:
    ///
    /// VERSIONGROUP[MAJOR[.MINOR]][-STATUS]
    /// </code>
    /// <para>
    /// where <b>VERSIONGROUP</b> is a date formatted like <b>YYYY-MM-DD</b>,
    /// <b>MAJOR</b> and <b>MINOR</b> are non-negative integers, and <b>STATUS</b>
    /// is an alphanumberic string starting with a letter.  Here are some valid
    /// version strings:
    /// </para>
    /// <list type="table">
    /// <item>
    ///     <term><b>1.0</b></term>
    ///     <description>
    ///     major and minor version numbers
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b>1.0-alpha</b></term>
    ///     <description>
    ///     major and minor version numbers with status
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b>2023-04-09</b></term>
    ///     <description>
    ///     Version group date
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b>2023-04-09-alpha</b></term>
    ///     <description>
    ///     Version group date with status
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b>2023-04-09.1.0</b></term>
    ///     <description>
    ///     Version group with major and minor versions
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b>2023-04-09.1.0-alpha</b></term>
    ///     <description>
    ///     Version group with major and minor versions plus status
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b>2023-04.091</b></term>
    ///     <description>
    ///     <para>
    ///     Version group with only major version <b>(1)</b>
    ///     </para>
    ///     <note>
    ///     This format is a bit odd and may be confusing.  We recommend that 
    ///     you avoid using this.
    ///     </note>
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b>2023-04.091-alpha</b></term>
    ///     <description>
    ///     <para>
    ///     Version group with only major version <b>(1)</b> and status
    ///     </para>
    ///     <note>
    ///     This format is a bit odd and may be confusing.  We recommend that 
    ///     you avoid using this.
    ///     </note>
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b>2023-04.091.0</b></term>
    ///     <description>
    ///     <para>
    ///     Version group with major and minor versions
    ///     </para>
    ///     <note>
    ///     This format is a bit odd and may be confusing.  We recommend that 
    ///     you avoid using this.
    ///     </note>
    ///     </description>
    /// </item>
    /// <item>
    ///     <term><b>2023-04.091.0-alpha</b></term>
    ///     <description>
    ///     <para>
    ///     Version group with major and minor versions with status
    ///     </para>
    ///     <note>
    ///     This format is a bit odd and may be confusing.  We recommend that 
    ///     you avoid using this.
    ///     </note>
    ///     </description>
    /// </item>
    /// </list>
    /// </remarks>
    public class ApiVersion : IComparable<ApiVersion>
    {
        //---------------------------------------------------------------------
        // Static members

        private static DateTime     minVersionGroup         = new DateTime(1, 1, 1).ToUniversalTime();
        private static Regex        majorMinorRegex         = new Regex(@"(?<major>\d+)\.(?<minor>\d+)");
        private static Regex        majorOptionalMinorRegex = new Regex(@"((?<major>\d+)(\.(?<minor>\d+))?)?");

        /// <summary>
        /// <para>
        /// Parses a <see cref="ApiVersion"/>, formatted like:
        /// </para>
        /// <para>
        /// API versions are formatted like:
        /// </para>
        /// <code>
        /// [VERSIONGROUP.]MAJOR.MINOR[-STATUS]
        ///
        /// or:
        ///
        /// VERSIONGROUP[MAJOR[.MINOR]][-STATUS]
        /// </code>
        /// <para>
        /// where <b>VERSIONGROUP</b> is a date formatted like <b>YYYY-MM-DD</b>,
        /// <b>MAJOR</b> and <b>MINOR</b> are non-negative integers, and <b>STATUS</b>
        /// is an alphanumberic string starting with a letter.
        /// </para>
        /// </summary>
        /// <param name="version">The version string,</param>
        /// <returns>The parsed <see cref="ApiVersion"/>.</returns>
        /// <exception cref="FormatException">Thrown for an invalid version string.</exception>
        public static ApiVersion Parse(string version)
        {
            var apiVersion = new ApiVersion();
            var pos        = 0;

            // Look for a version group at the beginning of the string.

            if (version.Length >= "YYYY-MM-DD".Length &&
                char.IsDigit(version[0]) && char.IsDigit(version[1]) && char.IsDigit(version[2]) && char.IsDigit(version[3]) && version[4] == '-' &&
                char.IsDigit(version[5]) && char.IsDigit(version[6]) && version[7] == '-' &&
                char.IsDigit(version[8]) && char.IsDigit(version[9]))
            {
                // Parse the version group date.

                var year  = int.Parse(version.Substring(0, 4));
                var month = int.Parse(version.Substring(5, 2));
                var day   = int.Parse(version.Substring(8, 2));

                apiVersion.VersionGroup = new DateTime(year, month, day).ToUniversalTime();

                pos = "YYYY-MM-DD".Length;
            }
            else
            {
                apiVersion.VersionGroup = new DateTime(0001, 1, 1).ToUniversalTime();
            }

            if (pos >= version.Length)
            {
                return apiVersion;
            }

            // If the next character is a "." then we're parsing:
            //
            //      [<Version Group>.]<Major>.<Minor>[-Status]
            //
            // otherwise we're parsing:
            //
            //      <Version Group>[<Major>[.Minor]][-Status]

            if (version[pos] == '.')
            {
                pos++;

                var match = majorMinorRegex.Match(version, pos);

                if (!match.Success)
                {
                    throw new FormatException($"Invalid major/minor versions: [version={version}]");
                }

                apiVersion.Major = int.Parse(match.Groups["major"].Value);
                apiVersion.Minor = int.Parse(match.Groups["minor"].Value);

                pos += match.Length;
            }
            else
            {
                var match = majorOptionalMinorRegex.Match(version, pos);

                if (!match.Success)
                {
                    throw new FormatException($"Invalid major/(optional)minor versions: [version={version}]");
                }

                var majorString = match.Groups["major"].Value;

                if (majorString.Length > 0)
                {
                    apiVersion.Major = int.Parse(match.Groups["major"].Value);

                    var minorString = match.Groups["minor"].Value;

                    if (minorString.Length > 0)
                    {
                        apiVersion.Minor = int.Parse(minorString);
                    }

                    pos += match.Length;
                }
            }

            if (pos >= version.Length)
            {
                return apiVersion;
            }

            if (version[pos] != '-')
            {
                throw new FormatException($"Unrecognized API version.");
            }

            // Parse the optional status

            if (version[pos] != '-')
            {
                throw new FormatException($"Invalid status part: [version={version}]");
            }

            apiVersion.Status = version.Substring(pos + 1);

            if (apiVersion.Status.Length == 0)
            {
                throw new FormatException($"Invalid status part: [version={version}]");
            }

            if (!char.IsLetter(apiVersion.Status[0]))
            {
                throw new FormatException($"Invalid status part: [version={version}].  Status part must start with a letter.");
            }

            foreach (var ch in apiVersion.Status)
            {
                if ('a' <= ch && ch <= 'z' ||
                    'A' <= ch && ch <= 'Z' ||
                    char.IsDigit(ch) || ch == '.' || ch == '-')
                {
                    continue;
                }

                throw new FormatException($"Invalid character '{ch}' in status part: [version={version}]");
            }

            return apiVersion;
        }

        //---------------------------------------------------------------------
        // Instance members

        /// <summary>
        /// Private constructor.
        /// </summary>
        private ApiVersion()
        {
        }

        /// <summary>
        /// <para>
        /// Returns the version group date.
        /// </para>
        /// <note>
        /// This returns as <b>0001-01-01</b> when the parsed version string
        /// didn't include a version group.
        /// </note>
        /// </summary>
        public DateTime VersionGroup { get; private set; } = minVersionGroup;

        /// <summary>
        /// <para>
        /// Returns the major version number.
        /// </para>
        /// <note>
        /// This returns as <b>-1</b> when the parsed version string didn't include a major version.
        /// </note>
        /// </summary>
        public int Major { get; private set; } = -1;

        /// <summary>
        /// <para>
        /// Returns the minor version number.
        /// </para>
        /// <note>
        /// This returns as <b>-1</b> when the parsed version string didn't include a minor version.
        /// </note>
        /// </summary>
        public int Minor { get; private set; } = -1;

        /// <summary>
        /// <para>
        /// Returns the status part.
        /// </para>
        /// <note>
        /// This return an empty string when the parse version string didn't include status.
        /// Note also that versions with a <see cref="Status"/> are considered to be <b>greater</b>
        /// than a version without a <see cref="Status"/> when all other properties are the same.
        /// </note>
        /// </summary>
        public string Status { get; private set; } = String.Empty;

        //---------------------------------------------------------------------
        // IComparible<T> implemention

        /// <inheritdoc/>
        public int CompareTo(ApiVersion other)
        {
            if (this.VersionGroup < other.VersionGroup)
            {
                return -1;
            }
            else if (this.VersionGroup > other.VersionGroup)
            {
                return 1;
            }

            if (this.Major < other.Major)
            {
                return -1;
            }
            else if (this.Major > other.Major)
            {
                return 1;
            }

            if (this.Minor < other.Minor)
            {
                return -1;
            }
            else if (this.Minor > other.Minor)
            {
                return 1;
            }

            // We need versions with a status to be considered as less than versions without
            // a status.  This way a version like "1.2" will be greater than "1.2-alpha".

            if (this.Status.Length == 0 && other.Status.Length > 0)
            {
                return 1;
            }
            else if (this.Status.Length > 0 && other.Status.Length == 0)
            {
                return -1;
            }

            return string.Compare(this.Status, other.Status, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var sb = new StringBuilder();

            if (VersionGroup != minVersionGroup)
            {
                sb.Append(VersionGroup.ToString("yyyy-MM-dd"));

                if (Major >= 0 && Minor >= 0)
                {
                    sb.Append('.');
                    sb.Append(Major);
                    sb.Append('.');
                    sb.Append(Minor);
                }
                else if (Major >= 0)
                {
                    sb.Append(Major);

                    if (Minor >= 0)
                    {
                        sb.Append('.');
                        sb.Append(Minor);
                    }
                }
            }
            else
            {
                if (Major >= 0 && Minor >= 0)
                {
                    sb.Append(Major);
                    sb.Append('.');
                    sb.Append(Minor);
                }
                else if (Major >= 0)
                {
                    sb.Append(Major);

                    if (Minor >= 0)
                    {
                        sb.Append('.');
                        sb.Append(Minor);
                    }
                }
            }

            if (!string.IsNullOrEmpty(Status))
            {
                sb.Append('-');
                sb.Append(Status);
            }

            return sb.ToString();
        }
    }
}
