using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Remove", "VMGroupMember", DefaultParameterSetName = "VM Using Name", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMGroup) })]
internal sealed class RemoveVMGroupMember : VirtualizationCmdlet<Guid>, ISupportsPassthrough
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VM Using Name")]
	[Parameter(ParameterSetName = "VMGroup Using Name")]
	[Parameter(ParameterSetName = "VM Using ID")]
	[Parameter(ParameterSetName = "VMGroup Using ID")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VM Using Name")]
	[Parameter(ParameterSetName = "VMGroup Using Name")]
	[Parameter(ParameterSetName = "VM Using ID")]
	[Parameter(ParameterSetName = "VMGroup Using ID")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VM Using Name")]
	[Parameter(ParameterSetName = "VMGroup Using Name")]
	[Parameter(ParameterSetName = "VM Using ID")]
	[Parameter(ParameterSetName = "VMGroup Using ID")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[Parameter(ParameterSetName = "VM Using Name", Mandatory = true, Position = 0)]
	[Parameter(ParameterSetName = "VMGroup Using Name", Mandatory = true, Position = 0)]
	[ValidateNotNullOrEmpty]
	public string Name { get; set; }

	[Parameter(ParameterSetName = "VM Using ID", Mandatory = true, Position = 0)]
	[Parameter(ParameterSetName = "VMGroup Using ID", Mandatory = true, Position = 0)]
	[ValidateNotNullOrEmpty]
	public Guid Id { get; set; }

	[Parameter(ParameterSetName = "VM Using InputObject", Mandatory = true, Position = 0)]
	[Parameter(ParameterSetName = "VMGroup Using InputObject", Mandatory = true, Position = 0)]
	[ValidateNotNullOrEmpty]
	public VMGroup VMGroup { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VM Using Name", Mandatory = true, Position = 1)]
	[Parameter(ParameterSetName = "VM Using ID", Mandatory = true, Position = 1)]
	[Parameter(ParameterSetName = "VM Using InputObject", Mandatory = true, Position = 1)]
	[ValidateNotNullOrEmpty]
	public VirtualMachine[] VM { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMGroup Using Name", Mandatory = true, Position = 1)]
	[Parameter(ParameterSetName = "VMGroup Using ID", Mandatory = true, Position = 1)]
	[Parameter(ParameterSetName = "VMGroup Using InputObject", Mandatory = true, Position = 1)]
	[ValidateNotNullOrEmpty]
	public VMGroup[] VMGroupMember { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<Guid> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("VM Using InputObject") || CurrentParameterSetIs("VMGroup Using InputObject"))
		{
			return new Guid[1] { VMGroup.InstanceId };
		}
		if (CurrentParameterSetIs("VM Using ID") || CurrentParameterSetIs("VMGroup Using ID"))
		{
			return new Guid[1] { Id };
		}
		IEnumerable<Server> servers = ParameterResolvers.GetServers(this, operationWatcher);
		List<string> groupName = new List<string> { Name };
		return (from g in VMGroup.GetVMGroupsByName(servers, groupName, operationWatcher)
			select g.InstanceId).ToList();
	}

	internal override void ValidateOperandList(IList<Guid> operands, IOperationWatcher operationWatcher)
	{
		base.ValidateOperandList(operands, operationWatcher);
		if (!operands.Any())
		{
			throw ExceptionHelper.CreateObjectNotFoundException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMGroupMember_NoneFound, Name), null);
		}
		if (operands.Count > 1)
		{
			throw ExceptionHelper.CreateObjectNotFoundException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.VMGroupMember_MoreThanOneFound, Name), null);
		}
	}

	internal override void ProcessOneOperand(Guid operand, IOperationWatcher operationWatcher)
	{
		if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMGroupMember, Name)))
		{
			return;
		}
		if (CurrentParameterSetIs("VM Using Name") || CurrentParameterSetIs("VM Using ID") || CurrentParameterSetIs("VM Using InputObject"))
		{
			VirtualMachine[] vM = VM;
			for (int i = 0; i < vM.Length; i++)
			{
				VMGroup.RemoveGroupMemberById(vM[i], operand, operationWatcher);
			}
		}
		else
		{
			VMGroup[] vMGroupMember = VMGroupMember;
			for (int i = 0; i < vMGroupMember.Length; i++)
			{
				VMGroup.RemoveGroupMemberById(vMGroupMember[i], operand, operationWatcher);
			}
		}
		if (Passthru.IsPresent)
		{
			operationWatcher.WriteObject(operand);
		}
	}
}
