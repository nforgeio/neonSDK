using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Disable", "VMMigration", SupportsShouldProcess = true, DefaultParameterSetName = "ComputerName")]
[OutputType(new Type[] { typeof(VMHost) })]
internal sealed class DisableVMMigration : VirtualizationCmdlet<VMHost>, ISupportsPassthrough
{
	[Parameter(ParameterSetName = "CimSession", Position = 0, Mandatory = true)]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[Parameter(ParameterSetName = "ComputerName", Position = 0)]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[Parameter(ParameterSetName = "ComputerName", Position = 1)]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMHost> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return VirtualizationObjectLocator.GetVMHosts(ParameterResolvers.GetServers(this, operationWatcher), operationWatcher);
	}

	internal override void ProcessOneOperand(VMHost operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_DisableVMMigration, operand.Server)))
		{
			operand.VirtualMachineMigrationEnabled = false;
			((IUpdatable)operand).Put(operationWatcher);
			if ((bool)Passthru)
			{
				operationWatcher.WriteObject(operand);
			}
		}
	}
}
