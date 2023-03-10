using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Commands;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.Vhd.PowerShell.Cmdlets;

[Cmdlet("Get", "VHD", ConfirmImpact = ConfirmImpact.Medium, DefaultParameterSetName = "Path")]
[OutputType(new Type[] { typeof(VirtualHardDisk) })]
internal sealed class GetVHD : VirtualizationCmdlet<VirtualHardDisk>, IServerParameters
{
	private static class ParameterSetNames
	{
		public const string Disk = "Disk";

		public const string Path = "Path";

		public const string VMId = "VMId";
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

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec. Also arrary is more user friendly.")]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "Id" })]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, ParameterSetName = "VMId")]
	public Guid[] VMId { get; set; }

	internal override IList<VirtualHardDisk> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		List<VirtualHardDisk> list = new List<VirtualHardDisk>();
		IEnumerable<Server> servers = ParameterResolvers.GetServers(this, operationWatcher);
		if (CurrentParameterSetIs("Path"))
		{
			foreach (Server item in servers)
			{
				Server currentServer2 = item;
				IEnumerable<string> paths = Path.SelectManyWithLogging((string path) => VhdPathResolver.GetVirtualHardDiskFullPath(currentServer2, path, base.CurrentFileSystemLocation, base.InvokeProvider), operationWatcher);
				list.AddRange(VhdUtilities.GetVirtualHardDisks(currentServer2, paths, operationWatcher));
			}
			return list;
		}
		if (CurrentParameterSetIs("VMId"))
		{
			foreach (Server item2 in servers)
			{
				Server currentServer = item2;
				IEnumerable<string> paths2 = from drive in VMId.SelectWithLogging((Guid id) => VirtualizationObjectLocator.GetVirtualMachineById(currentServer, id), operationWatcher).SelectMany((VirtualMachine vm) => vm.HardDrives)
					select drive.Path into path
					where path != null
					select path;
				list.AddRange(VhdUtilities.GetVirtualHardDisks(currentServer, paths2, operationWatcher));
			}
			return list;
		}
		list.AddRange(servers.SelectWithLogging((Server server) => VhdUtilities.GetVirtualHardDiskByDiskNumber(server, DiskNumber), operationWatcher));
		return list;
	}

	internal override void ProcessOneOperand(VirtualHardDisk operand, IOperationWatcher operationWatcher)
	{
		operationWatcher.WriteObject(operand);
	}
}
