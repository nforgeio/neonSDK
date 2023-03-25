using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Commands;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell.Cmdlets;

[Cmdlet("Remove", "VHDSnapshot", ConfirmImpact = ConfirmImpact.Medium, SupportsShouldProcess = true, DefaultParameterSetName = "Path")]
internal sealed class RemoveVHDSnapshot : VirtualizationCmdlet<Tuple<Server, string, Guid>>, ISupportsAsJob
{
	private static class ParameterSetNames
	{
		public const string Path = "Path";

		public const string VHDSnapshot = "VHDSnapshot";
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec. Also array is more user friendly.")]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "FullName" })]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "Path")]
	public string[] Path { get; set; }

	[Parameter]
	public SwitchParameter PersistReferencePoint { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec. Also array is more user friendly.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, ParameterSetName = "Path")]
	public Guid[] SnapshotId { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec. Also array is more user friendly.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VHDSnapshot")]
	public VHDSnapshotInfo[] VHDSnapshot { get; set; }

	[Parameter]
	public SwitchParameter AsJob { get; set; }

	internal override IList<Tuple<Server, string, Guid>> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		List<Tuple<Server, string, Guid>> list = new List<Tuple<Server, string, Guid>>();
		if (CurrentParameterSetIs("Path"))
		{
			foreach (Server server in ParameterResolvers.GetServers(this, operationWatcher))
			{
				Server currentServer = server;
				List<string> inputs = Path.SelectManyWithLogging((string path) => VhdPathResolver.GetVirtualHardDiskFullPath(currentServer, path, base.CurrentFileSystemLocation, base.InvokeProvider), operationWatcher).ToList();
				Guid[] snapshotId2 = SnapshotId;
				foreach (Guid snapshotId in snapshotId2)
				{
					list.AddRange(inputs.SelectWithLogging((string path) => Tuple.Create(currentServer, path, snapshotId), operationWatcher));
				}
			}
			return list;
		}
		list.AddRange(VHDSnapshot.SelectWithLogging((VHDSnapshotInfo vhdSnapshot) => Tuple.Create(vhdSnapshot.Server, vhdSnapshot.FilePath, vhdSnapshot.SnapshotId), operationWatcher));
		return list;
	}

	internal override void ProcessOneOperand(Tuple<Server, string, Guid> operand, IOperationWatcher operationWatcher)
	{
		Server item = operand.Item1;
		string item2 = operand.Item2;
		Guid item3 = operand.Item3;
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVHDSnapshot, item3, item2)))
		{
			VhdUtilities.DeleteVHDSnapshot(item, item2, item3.ToString(), PersistReferencePoint, operationWatcher);
		}
	}
}
