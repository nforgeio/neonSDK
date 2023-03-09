using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Remove", "VMSnapshot", SupportsShouldProcess = true, DefaultParameterSetName = "SnapshotName")]
[OutputType(new Type[] { typeof(VMSnapshot) })]
internal sealed class RemoveVMSnapshot : VirtualizationCmdlet<VMSnapshot>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsAsJob, ISupportsPassthrough, IVMMultipleSnapshotCmdlet
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, plus array is easier to use for users.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ParameterSetName = "SnapshotName")]
	public string[] VMName { get; set; }

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

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Arrays are more user friendly.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SnapshotObject")]
	[Alias(new string[] { "VMCheckpoint" })]
	public VMSnapshot[] VMSnapshot { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, plus array is easier to use for users.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Position = 1, ValueFromPipeline = true, ParameterSetName = "SnapshotName")]
	[Parameter(Position = 1, ParameterSetName = "VMObject")]
	public string[] Name { get; set; }

	[Parameter]
	[Alias(new string[] { "IncludeAllChildCheckpoints" })]
	public SwitchParameter IncludeAllChildSnapshots { get; set; }

	[Parameter]
	public SwitchParameter AsJob { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMSnapshot> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IEnumerable<VMSnapshot> source;
		if (CurrentParameterSetIs("SnapshotObject"))
		{
			source = VMSnapshot;
		}
		else
		{
			source = ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectMany((VirtualMachine vm) => vm.GetVMSnapshots());
			if (Name != null)
			{
				WildcardPatternMatcher matcher = new WildcardPatternMatcher(Name);
				source = source.Where((VMSnapshot snapshot) => matcher.MatchesAny(snapshot.Name));
			}
		}
		return source.ToList();
	}

	internal override void ProcessOneOperand(VMSnapshot operand, IOperationWatcher operationWatcher)
	{
		string name = operand.Name;
		bool flag = false;
		if (IncludeAllChildSnapshots.IsPresent)
		{
			if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMSnapshot_IncludeAllChildSnapshots, name)))
			{
				operand.DeleteTree(operationWatcher);
				flag = true;
			}
		}
		else if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMSnapshot_SnapshotOnly, name)))
		{
			((IRemovable)operand).Remove(operationWatcher);
			flag = true;
		}
		if (flag && Passthru.IsPresent)
		{
			operationWatcher.WriteObject(operand);
		}
	}
}
