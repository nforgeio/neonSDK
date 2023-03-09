using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Commands;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell.Cmdlets;

[Cmdlet("Dismount", "VHD", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true, DefaultParameterSetName = "Path")]
[OutputType(new Type[] { typeof(VirtualHardDisk) })]
internal sealed class DismountVHD : VirtualizationCmdlet<MountedDiskImage>, ISupportsPassthrough
{
	private static class ParameterSetNames
	{
		public const string Disk = "Disk";

		public const string Path = "Path";
	}

	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "Number" })]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, ParameterSetName = "Disk")]
	public uint DiskNumber { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec. Also arrary is more user friendly.")]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "FullName" })]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "Path")]
	public string[] Path { get; set; }

	[Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "Path")]
	public Guid? SnapshotId { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		if (!CurrentParameterSetIs("Path") || !SnapshotId.HasValue)
		{
			return;
		}
		string[] path = Path;
		foreach (string text in path)
		{
			if (!System.IO.Path.GetExtension(text).Equals(".VHDS", StringComparison.OrdinalIgnoreCase))
			{
				throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_SnapshotIDOnlyValidForVHDS, text));
			}
		}
	}

	internal override IList<MountedDiskImage> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IEnumerable<Server> servers = ParameterResolvers.GetServers(this, operationWatcher);
		if (CurrentParameterSetIs("Path"))
		{
			List<MountedDiskImage> list = new List<MountedDiskImage>();
			List<string> snapshotIdString = new List<string>();
			if (SnapshotId.HasValue)
			{
				snapshotIdString.Add(SnapshotId.ToString());
			}
			{
				foreach (Server item in servers)
				{
					Server currentServer = item;
					IEnumerable<string> inputs = Path.SelectManyWithLogging((string path) => VhdPathResolver.GetVirtualHardDiskFullPath(currentServer, path, base.CurrentFileSystemLocation, base.InvokeProvider), operationWatcher);
					if (SnapshotId.HasValue)
					{
						inputs = inputs.SelectManyWithLogging((string path) => VhdUtilities.GetVHDSnapshotInfo(currentServer, path, snapshotIdString, getParentPaths: false), operationWatcher).SelectWithLogging((VHDSnapshotInfo snapshot) => VhdPathResolver.GetSingleVirtualHardDiskFullPath(currentServer, snapshot.SnapshotPath, snapshot.FilePath, base.InvokeProvider), operationWatcher);
					}
					list.AddRange(inputs.SelectWithLogging((string path) => MountedDiskImage.FindByPath(currentServer, path), operationWatcher));
				}
				return list;
			}
		}
		return servers.SelectWithLogging((Server server) => MountedDiskImage.FindByDiskNumber(server, DiskNumber), operationWatcher).ToList();
	}

	internal override void ProcessOneOperand(MountedDiskImage operand, IOperationWatcher operationWatcher)
	{
		string path = operand.Path;
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_DismountVHD, path)))
		{
			operand.Dismount(operationWatcher);
			if (Passthru.IsPresent)
			{
				VirtualHardDisk virtualHardDisk = VhdUtilities.GetVirtualHardDisk(operand.Server, path);
				operationWatcher.WriteObject(virtualHardDisk);
			}
		}
	}
}
