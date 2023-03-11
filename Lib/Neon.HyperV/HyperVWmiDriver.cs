//-----------------------------------------------------------------------------
// FILE:	    HyperVWmiDriver.cs
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
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Neon.Common;
using Neon.Net;

using Microsoft.Vhd.PowerShell.Cmdlets;

namespace Neon.HyperV
{
    //-------------------------------------------------------------------------
    // IMPLEMENTATION NOTE:
    //
    // Microsoft doesn't really have a nice Hyper-V API other than the PowerShell
    // cmdlets.  Hyper-V can be controlled via WMI but I couldn't find much in the
    // way of documentation for how that works.  I was able to hack around and get
    // some things working a few months late December 2022 but I abandoned that 
    // effort in favor of a new approach described here:
    //
    //      https://github.com/jscarle/HyperV.NET
    //
    // The gist of this is to use ILSpy to decompile the Hyper-V cmdlets and adapt
    // that code here.
    //
    // Here's an explaination of how to obtain the source code for a cmdlet:
    //
    //      https://learn.microsoft.com/en-us/archive/blogs/luisdem/get-the-source-code-of-the-powershell-cmdlets
    //
    // The basic approach is to use this PowerShell command to locate the cmdlet DLL
    // that includes the code implementing a specific Hyper-V related PowerShell COMMAND:
    //
    //      (Get-Command COMMAND).dll
    //
    // and then fire up ILSpy to have a look.

    /// <summary>
    /// Uses WMI to implement <see cref="IHyperVDriver"/>.
    /// </summary>
    internal sealed class HyperVWmiDriver : IHyperVDriver
    {
        //---------------------------------------------------------------------
        // Private types

        /// <summary>
        /// Holds arguments being passed to a cmdlet.
        /// </summary>
        private class CmdletArgs
        {
            private Dictionary<string, object> args = new Dictionary<string, object>();

            /// <summary>
            /// Adds a switch argument.
            /// </summary>
            /// <param name="name">Specifies the switch name.</param>
            public void AddSwitch(string name)
            {
                Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name), nameof(name));

                args.Add(name, null);
            }

            /// <summary>
            /// Adds a named argument.
            /// </summary>
            /// <param name="name">Specifes the argument name.</param>
            /// <param name="value">Specifies the argument value.</param>
            public void AddArg(string name, object value)
            {
                Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(name), nameof(name));
                Covenant.Requires<ArgumentNullException>(value != null, nameof(value));

                args.Add(name, value);
            }

            /// <summary>
            /// Copies the arguments to a <see cref="PowerShell"/> instance to prepare for
            /// a command execution.
            /// </summary>
            /// <param name="powershell"></param>
            public void CopyArgsTo(PowerShell powershell)
            {
                Covenant.Requires<ArgumentNullException>(powershell != null, nameof(powershell));

                foreach (var arg in args)
                {
                    if (arg.Value == null)
                    {
                        powershell.AddParameter(arg.Key);
                    }
                    else
                    {
                        powershell.AddParameter(arg.Key, arg.Value);
                    }
                }
            }
        }

        //---------------------------------------------------------------------
        // Implementation

        private HyperVClient client;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HyperVWmiDriver(HyperVClient client)
        {
            Covenant.Requires<ArgumentNullException>(client != null, nameof(client));

            this.client = client;

            if (!NeonHelper.IsWindows)
            {
                throw new NotSupportedException($"{nameof(HyperVWmiDriver)} is only supported on Windows.");
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~HyperVWmiDriver()
        {
            Dispose();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            client = null;
        }

        /// <summary>
        /// Releases all associated resources.
        /// </summary>
        /// <param name="disposing">Pass <c>true</c> if we're disposing, <c>false</c> if we're finalizing.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Ensures that the instance has not been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the instance has been disposed.</exception>
        private void CheckDisposed()
        {
            if (client == null)
            {
                throw new ObjectDisposedException(nameof(HyperVClient));
            }
        }

        /// <summary>
        /// Invokes the <typeparamref name="TCmdlet"/> cmdlet with the parameters passed.
        /// </summary>
        /// <typeparam name="TCmdlet">Specifies the target cmdlet implementation.</typeparam>
        /// <param name="args">Specifies any arguments to be passed.</param>
        /// <returns>The comand results.</returns>
        private IList<PSObject> Invoke<TCmdlet>(CmdletArgs args)
            where TCmdlet : PSCmdlet, new()
        {
            Covenant.Requires<ArgumentNullException>(args != null, nameof(args)); 

            var cmdletAttr          = typeof(TCmdlet).GetCustomAttribute<CmdletAttribute>();
            var cmdletName          = $"{cmdletAttr.VerbName}-{cmdletAttr.NounName}";
            var initialSessionState = InitialSessionState.Create();

            initialSessionState.Commands.Add(new SessionStateCmdletEntry(cmdletName, typeof(TCmdlet), null));

            using (var runspace = RunspaceFactory.CreateRunspace(initialSessionState))
            {
                runspace.Open();

                using (var powershell = PowerShell.Create(runspace))
                {
                    powershell.Runspace = runspace;

                    powershell.AddCommand(cmdletName);
                    args?.CopyArgsTo(powershell);

                    return powershell.Invoke();
                }
            }
        }

        /// <inheritdoc/>
        public void NewVM(
            string      machineName,
            int         processorCount,
            long        startupMemoryBytes,
            int         generation       = 1,
            string      drivePath        = null,
            string      switchName       = null,
            bool        checkPointDrives = false)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void AddVmDrive(string machineName, string drivePath)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void DismountVhd(string drivePath)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));

            var args = new CmdletArgs();

            args.AddArg("Path", drivePath);

            Invoke<DismountVHD>(args);
        }

        /// <inheritdoc/>
        public void EnableVmNestedVirtualization(string machineName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void InsertVmDvdDrive(string machineName, string isoPath)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(isoPath), nameof(isoPath));

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void EjectDvdDrive(string machineName)
        {
            CheckDisposed();
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(machineName), nameof(machineName));

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualNat> ListNats()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualSwitch> ListSwitches()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<string> ListVmDrives(string machineName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualNetworkAdapter> ListVmNetAdapters(string machineName, bool waitForAddresses = false)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualMachine> ListVms()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void MountVhd(string drivePath, bool readOnly = false)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));

            var args = new CmdletArgs();

            args.AddArg("Path", drivePath);

            Invoke<MountVhd>(args);
        }

        /// <inheritdoc/>
        public void NewNetIPAddress(string switchName, IPAddress address, NetworkCidr subnet)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void NewNat(string switchName, NetworkCidr subnet)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void NewVhd(string drivePath, bool isDynamic, long sizeBytes, int blockSizeBytes)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));
            Covenant.Requires<ArgumentException>(sizeBytes > 0, nameof(sizeBytes));
            Covenant.Requires<ArgumentException>(blockSizeBytes > 0, nameof(blockSizeBytes));

            var args = new CmdletArgs();

            if (isDynamic)
            {
                args.AddSwitch("Dynamic");
            }
            else
            {
                args.AddSwitch("Fixed");
            }

            args.AddArg("Path", drivePath);
            args.AddArg("SizeBytes", sizeBytes);
            args.AddArg("BlockSizeBytes", blockSizeBytes);

            Invoke<NewVhd>(args);
        }

        /// <inheritdoc/>
        public void NewSwitch(string switchName, string targetAdapter = null, bool @internal = false)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void OptimizeVhd(string drivePath)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));

            var args = new CmdletArgs();

            args.AddArg("Path", drivePath);

            Invoke<OptimizeVhd>(args);
        }

        /// <inheritdoc/>
        public void RemoveNat(string natName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveVm(string switchName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void RemoveSwitch(string switchName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void ResizeVhd(string drivePath, long sizeBytes)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(drivePath), nameof(drivePath));
            Covenant.Requires<ArgumentException>(sizeBytes > 0, nameof(sizeBytes));

            var args = new CmdletArgs();

            args.AddArg("Path", drivePath);
            args.AddArg("SizeBytes", sizeBytes);

            Invoke<ResizeVhd>(args);
        }

        /// <inheritdoc/>
        public void SaveVm(string drivePath)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SetVm(string machineName, int? processorCount = null, long? startupMemoryBytes = null, bool? checkpointDrives = null)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void StartVm(string machineName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void StopVm(string machineName, bool turnOff = false)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<VirtualIPAddress> ListIPAddresses()
        {
            CheckDisposed();

            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public IEnumerable<string> ListHostAdapters()
        {
            CheckDisposed();

            throw new NotImplementedException();
        }
    }
}
