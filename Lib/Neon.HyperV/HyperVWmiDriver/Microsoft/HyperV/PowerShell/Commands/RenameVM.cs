using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Rename", "VM", DefaultParameterSetName = "Name", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VirtualMachine) })]
internal sealed class RenameVM : VirtualizationCmdlet<VirtualMachine>, IVMObjectOrNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByNameCmdlet, ISupportsPassthrough
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "Name")]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "Name")]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "Name")]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "VMName" })]
	[Parameter(ParameterSetName = "Name", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string[] Name { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1)]
	public string NewName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
	}

	internal override void ProcessOneOperand(VirtualMachine operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, operand.Name, NewName)))
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
