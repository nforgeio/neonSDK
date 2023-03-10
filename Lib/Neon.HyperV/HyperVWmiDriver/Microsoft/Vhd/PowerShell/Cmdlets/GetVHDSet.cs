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

[Cmdlet("Get", "VHDSet", ConfirmImpact = ConfirmImpact.Medium)]
[OutputType(new Type[] { typeof(VHDSetInfo) })]
internal sealed class GetVHDSet : VirtualizationCmdlet<VHDSetInfo>
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec. Also arrary is more user friendly.")]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "FullName" })]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
	public string[] Path { get; set; }

	[Parameter]
	public SwitchParameter GetAllPaths { get; set; }

	internal override IList<VHDSetInfo> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		List<VHDSetInfo> list = new List<VHDSetInfo>();
		foreach (Server server in ParameterResolvers.GetServers(this, operationWatcher))
		{
			Server currentServer = server;
			IEnumerable<string> inputs = Path.SelectManyWithLogging((string path) => VhdPathResolver.GetVirtualHardDiskFullPath(currentServer, path, base.CurrentFileSystemLocation, base.InvokeProvider), operationWatcher);
			list.AddRange(inputs.SelectWithLogging((string path) => VhdUtilities.GetVHDSetInfo(currentServer, path, GetAllPaths), operationWatcher).ToList());
		}
		return list;
	}

	internal override void ProcessOneOperand(VHDSetInfo operand, IOperationWatcher operationWatcher)
	{
		operationWatcher.WriteObject(operand);
	}
}
