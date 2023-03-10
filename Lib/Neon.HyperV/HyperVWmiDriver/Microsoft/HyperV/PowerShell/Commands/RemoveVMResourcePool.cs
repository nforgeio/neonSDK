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

[Cmdlet("Remove", "VMResourcePool", SupportsShouldProcess = true, DefaultParameterSetName = "ComputerName")]
[OutputType(new Type[] { typeof(VMResourcePool) })]
internal sealed class RemoveVMResourcePool : VirtualizationCmdlet<VMResourcePool>, ISupportsPassthrough
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ValueFromPipelineByPropertyName = true, ParameterSetName = "CimSession")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "ComputerName")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "ComputerName")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
	public string Name { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
	public VMResourcePoolType[] ResourcePoolType { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMResourcePool> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		string[] poolNames = new string[1] { Name };
		List<VMResourcePool> list = ParameterResolvers.GetServers(this, operationWatcher).SelectManyWithLogging((Server server) => VMResourcePool.GetVMResourcePools(server, poolNames, allowWildcards: true, ResourcePoolType), operationWatcher).ToList();
		if (list.Count == 0)
		{
			throw ExceptionHelper.CreateObjectNotFoundException(CmdletErrorMessages.GetVMResourcePool_NoneFound, null);
		}
		return list;
	}

	internal override void ProcessOneOperand(VMResourcePool operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMResourcePool, operand.Name, operand.ResourcePoolType)))
		{
			((IRemovable)operand).Remove(operationWatcher);
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(operand);
			}
		}
	}
}
