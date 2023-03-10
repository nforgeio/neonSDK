using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMSan")]
[OutputType(new Type[] { typeof(VMSan) })]
internal sealed class GetVMSan : VirtualizationCmdlet<VMSan>
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Position = 0, ValueFromPipelineByPropertyName = true)]
	[Alias(new string[] { "SanName" })]
	public string[] Name { get; set; }

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
	}

	internal override IList<VMSan> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		List<VMSan> list = ParameterResolvers.GetServers(this, operationWatcher).SelectManyWithLogging((Server server) => VMSan.GetVMSans(server, Name, allowWildcards: true), operationWatcher).ToList();
		if (list.Count == 0)
		{
			operationWatcher.WriteWarning(CmdletErrorMessages.GetVMSan_NoneFound);
		}
		return list;
	}

	internal override void ProcessOneOperand(VMSan operand, IOperationWatcher operationWatcher)
	{
		operationWatcher.WriteObject(operand);
	}
}
