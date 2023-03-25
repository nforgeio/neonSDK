using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMResourcePool", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMResourcePool) })]
internal sealed class SetVMResourcePool : VirtualizationCmdlet<VMResourcePool>, ISupportsPassthrough
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ValueFromPipelineByPropertyName = true)]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
	public string[] Name { get; set; }

	[Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
	public VMResourcePoolType ResourcePoolType { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 2)]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly, and this is by spec.")]
	public string[] ParentName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "Passthru is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMResourcePool> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		VMResourcePoolType[] poolTypes = new VMResourcePoolType[1] { ResourcePoolType };
		List<VMResourcePool> list = ParameterResolvers.GetServers(this, operationWatcher).SelectManyWithLogging((Server server) => VMResourcePool.GetVMResourcePools(server, Name, allowWildcards: true, poolTypes), operationWatcher).ToList();
		if (list.Count == 0)
		{
			operationWatcher.WriteWarning(CmdletErrorMessages.GetVMResourcePool_NoneFound);
		}
		return list;
	}

	internal override void ProcessOneOperand(VMResourcePool operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMResourcePool, string.Format(CultureInfo.CurrentCulture, CmdletResources.ResourcePoolFullName, operand.Name, operand.ResourcePoolType))))
		{
			if (ParentName != null)
			{
				operand.SetParentNames(ParentName, operationWatcher);
			}
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(operand);
			}
		}
	}
}
