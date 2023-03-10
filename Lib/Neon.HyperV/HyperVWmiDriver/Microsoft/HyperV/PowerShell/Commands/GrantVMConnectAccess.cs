using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Grant", "VMConnectAccess", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMConnectAce) })]
internal sealed class GrantVMConnectAccess : VirtualizationCmdlet<VirtualMachine>, IVmByVMIdCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMId", ValueFromPipeline = true, ValueFromPipelineByPropertyName = true, Position = 0, Mandatory = true)]
	public Guid[] VMId { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string[] VMName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true)]
	[Alias(new string[] { "UserId", "Sid" })]
	public string[] UserName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
	}

	internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
	{
		if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_GrantVMConnectAccess, operand.Name)))
		{
			return;
		}
		operand.GrantVMConnectAccess(UserName, operationWatcher);
		if ((bool)Passthru)
		{
			string[] userName = UserName;
			foreach (string userName2 in userName)
			{
				operationWatcher.WriteObject(new VMConnectAce(operand, userName2));
			}
		}
	}
}
