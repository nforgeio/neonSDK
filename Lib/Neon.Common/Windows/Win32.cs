//-----------------------------------------------------------------------------
// FILE:        Win32.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2024 by NEONFORGE LLC.  All rights reserved.
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Csv;
using Neon.IO;

namespace Neon.Windows
{
    /// <summary>
    /// Low-level Windows system calls.
    /// </summary>
    public static class Win32
    {
        /// <summary>
        /// Returns the total installed physical RAM as kilobytes.
        /// </summary>
        /// <param name="TotalMemoryInKilobytes">Returns as the physical RAM as KiB.</param>
        /// <returns><c>true</c> on success.</returns>
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPhysicallyInstalledSystemMemory(out ulong TotalMemoryInKilobytes);

        /// <summary>
        /// Has Windows encrypt a file or folder at rest.
        /// </summary>
        /// <param name="path">The file or folder path.</param>
        /// <returns><c>true</c> on success.</returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EncryptFile(string path);

        /// <summary>
        /// Has Windows decrypt a file or folder at rest.
        /// </summary>
        /// <param name="path">The file or folder path.</param>
        /// <returns><c>true</c> on success.</returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DecryptFile(string path);

        /// <summary>
        /// Obtains information about memory utilization on the current Windows machine.
        /// </summary>
        /// <param name="lpBuffer">Returns as a <see cref="MEMORYSTATUSEX"/> with the infirmation.</param>
        /// <returns><c>true</c> on success.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "GlobalMemoryStatusEx", SetLastError = true)]
        public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        //---------------------------------------------------------------------
        // Z-Order positioning

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        /// <summary>
        /// Relocates a window to be topmost in the z-order such that it will be
        /// above all other windows on the desktop.
        /// </summary>
        /// <param name="hWnd">Specifies the low-level handle for the target window.</param>
        public static void MakeWindowTopmost(IntPtr hWnd)
        {
            Covenant.Requires<ArgumentNullException>(hWnd != IntPtr.Zero, nameof(hWnd));

            SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }
    }
}