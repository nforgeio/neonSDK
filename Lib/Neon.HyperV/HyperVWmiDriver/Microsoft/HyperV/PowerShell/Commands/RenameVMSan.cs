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

[Cmdlet("Rename", "VMSan", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMSan) })]
internal sealed class RenameVMSan : VirtualizationCmdlet<VMSan>, ISupportsPassthrough
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ValueFromPipelineByPropertyName = true)]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true)]
	[Alias(new string[] { "SanName" })]
	public string Name { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1)]
	public string NewName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
	}

	internal override IList<VMSan> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		string[] sanNames = new string[1] { Name };
		List<VMSan> list = ParameterResolvers.GetServers(this, operationWatcher).SelectManyWithLogging((Server server) => VMSan.GetVMSans(server, sanNames, allowWildcards: false), operationWatcher).ToList();
		if (list.Count == 0)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.GetVMSan_NoneFound);
		}
		return list;
	}

	internal override void ProcessOneOperand(VMSan operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RenameVMSan, operand.Name, NewName)))
		{
			operand.Name = NewName;
			((IUpdatable)operand).Put(operationWatcher);
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(operand);
			}
		}
	}
}
