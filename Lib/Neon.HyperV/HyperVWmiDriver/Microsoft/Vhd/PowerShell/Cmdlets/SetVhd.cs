using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Commands;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell.Cmdlets;

[Cmdlet("Set", "VHD", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true, DefaultParameterSetName = "Parent")]
[OutputType(new Type[] { typeof(VirtualHardDisk) })]
internal sealed class SetVhd : VirtualizationCmdlet<Tuple<Server, string>>, ISupportsForce, ISupportsPassthrough
{
	private static class ParameterSetNames
	{
		public const string Parent = "Parent";

		public const string PhysicalSectorSize = "PhysicalSectorSize";

		public const string DiskIdentifier = "DiskIdentifier";
	}

	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "FullName" })]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
	public string Path { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "Parent")]
	public string ParentPath { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "Parent")]
	public string LeafPath { get; set; }

	[ValidateNotNullOrEmpty]
	[ValidateSet(new string[] { "512", "4096" })]
	[Parameter(Mandatory = true, ParameterSetName = "PhysicalSectorSize")]
	public uint PhysicalSectorSizeBytes { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, ParameterSetName = "DiskIdentifier")]
	public SwitchParameter ResetDiskIdentifier { get; set; }

	[Parameter(ParameterSetName = "DiskIdentifier")]
	public SwitchParameter Force { get; set; }

	[Parameter(ParameterSetName = "Parent")]
	public SwitchParameter IgnoreIdMismatch { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<Tuple<Server, string>> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		List<Tuple<Server, string>> list = new List<Tuple<Server, string>>();
		foreach (Server server in ParameterResolvers.GetServers(this, operationWatcher))
		{
			try
			{
				string singleVirtualHardDiskFullPath = VhdPathResolver.GetSingleVirtualHardDiskFullPath(server, Path, base.CurrentFileSystemLocation, base.InvokeProvider);
				list.Add(Tuple.Create(server, singleVirtualHardDiskFullPath));
			}
			catch (Exception e)
			{
				ExceptionHelper.DisplayErrorOnException(e, operationWatcher);
			}
		}
		return list;
	}

	internal override void ProcessOneOperand(Tuple<Server, string> operand, IOperationWatcher operationWatcher)
	{
		Server item = operand.Item1;
		string item2 = operand.Item2;
		if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVHD, item2)))
		{
			return;
		}
		if (CurrentParameterSetIs("Parent"))
		{
			string leafPath = null;
			string singleVirtualHardDiskFullPath = VhdPathResolver.GetSingleVirtualHardDiskFullPath(item, ParentPath, base.CurrentFileSystemLocation, base.InvokeProvider);
			if (!string.IsNullOrEmpty(LeafPath))
			{
				leafPath = VhdPathResolver.GetSingleVirtualHardDiskFullPath(item, LeafPath, base.CurrentFileSystemLocation, base.InvokeProvider);
			}
			VhdUtilities.ReconnectParentVirtualHardDisk(item, item2, singleVirtualHardDiskFullPath, leafPath, IgnoreIdMismatch.IsPresent, operationWatcher);
		}
		else if (CurrentParameterSetIs("PhysicalSectorSize"))
		{
			long physicalSectorSize = PhysicalSectorSizeBytes;
			VhdUtilities.SetVirtualHardDiskSettingData(item, item2, physicalSectorSize, null, operationWatcher);
		}
		else
		{
			string diskId = Guid.NewGuid().ToString();
			if (operationWatcher.ShouldContinue(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldContinue_SetVhdDiskId, item2)))
			{
				VhdUtilities.SetVirtualHardDiskSettingData(item, item2, 0L, diskId, operationWatcher);
			}
		}
		if (Passthru.IsPresent)
		{
			VirtualHardDisk virtualHardDisk = VhdUtilities.GetVirtualHardDisk(item, item2);
			operationWatcher.WriteObject(virtualHardDisk);
		}
	}
}
