using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Rename", "VMGroup", DefaultParameterSetName = "Name", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMGroup) })]
internal sealed class RenameVMGroup : VirtualizationCmdlet<VMGroup>, ISupportsPassthrough
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "Name")]
	[Parameter(ParameterSetName = "Id")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "Name")]
	[Parameter(ParameterSetName = "Id")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "Name")]
	[Parameter(ParameterSetName = "Id")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "Name", Mandatory = true, Position = 0)]
	public string[] Name { get; set; }

	[Parameter(ParameterSetName = "Id", Mandatory = true, Position = 0)]
	[ValidateNotNullOrEmpty]
	public Guid Id { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "InputObject", Mandatory = true, Position = 0)]
	[ValidateNotNullOrEmpty]
	public VMGroup[] VMGroup { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "Name")]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "Id")]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "InputObject")]
	public string NewName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMGroup> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("InputObject"))
		{
			return VMGroup;
		}
		IEnumerable<Server> servers = ParameterResolvers.GetServers(this, operationWatcher);
		if (CurrentParameterSetIs("Name"))
		{
			return global::Microsoft.HyperV.PowerShell.VMGroup.GetVMGroupsByName(servers, Name, operationWatcher);
		}
		return global::Microsoft.HyperV.PowerShell.VMGroup.GetVMGroupsById(servers, Id, operationWatcher);
	}

	internal override void ProcessOneOperand(VMGroup operand, IOperationWatcher operationWatcher)
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
