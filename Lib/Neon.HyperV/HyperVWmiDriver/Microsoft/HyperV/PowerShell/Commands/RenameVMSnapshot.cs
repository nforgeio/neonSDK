using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Rename", "VMSnapshot", DefaultParameterSetName = "SnapshotName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMSnapshot) })]
internal sealed class RenameVMSnapshot : VirtualizationCmdlet<VMSnapshot>, ISupportsPassthrough, IVmBySingularObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmBySingularVMNameCmdlet, IVMSnapshotCmdlet
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "SnapshotName")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "SnapshotName")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "SnapshotName")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[ValidateNotNull]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SnapshotObject")]
	[Alias(new string[] { "VMCheckpoint" })]
	public VMSnapshot VMSnapshot { get; set; }

	[ValidateNotNull]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VM")]
	public VirtualMachine VM { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ParameterSetName = "SnapshotName")]
	[Parameter(Mandatory = true, ParameterSetName = "VM")]
	public string Name { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "SnapshotName")]
	public string VMName { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 2)]
	public string NewName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMSnapshot> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IEnumerable<VMSnapshot> source;
		if (CurrentParameterSetIs("SnapshotObject"))
		{
			source = new VMSnapshot[1] { VMSnapshot };
		}
		else
		{
			source = ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectMany((VirtualMachine vm) => vm.GetVMSnapshots());
			if (!string.IsNullOrEmpty(Name))
			{
				WildcardPattern pattern = new WildcardPattern(Name, WildcardOptions.Compiled | WildcardOptions.IgnoreCase);
				source = source.Where((VMSnapshot snapshot) => pattern.IsMatch(snapshot.Name));
			}
		}
		return source.ToList();
	}

	internal override void ValidateOperandList(IList<VMSnapshot> operands, IOperationWatcher operationWatcher)
	{
		base.ValidateOperandList(operands, operationWatcher);
		if (!operands.Any())
		{
			throw ExceptionHelper.CreateObjectNotFoundException(ErrorMessages.SnapshotNotFound, null);
		}
		if (operands.Count > 1)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(ErrorMessages.MoreThanOneSnapshotFound);
		}
	}

	internal override void ProcessOneOperand(VMSnapshot operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RenameVMSnapshot, operand.Name, NewName)))
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
