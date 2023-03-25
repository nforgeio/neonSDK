using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;

namespace Microsoft.HyperV.PowerShell.Commands;

[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fibre", Justification = "This is per spec.")]
[Cmdlet("Remove", "VMFibreChannelHba", DefaultParameterSetName = "VMFibreChannelHba", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMFibreChannelHba) })]
internal sealed class RemoveVMFibreChannelHba : VirtualizationCmdlet<VMFibreChannelHba>, ISupportsPassthrough, IVmBySingularVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName and WWN", Mandatory = true, Position = 0)]
	public string VMName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fibre", Justification = "This is per spec.")]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMFibreChannelHba")]
	public VMFibreChannelHba[] VMFibreChannelHba { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(Mandatory = true, ParameterSetName = "VMName and WWN", Position = 1)]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "Wwnn1" })]
	public string WorldWideNodeNameSetA { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(Mandatory = true, ParameterSetName = "VMName and WWN", Position = 2)]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "Wwpn1" })]
	public string WorldWidePortNameSetA { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(Mandatory = true, ParameterSetName = "VMName and WWN", Position = 3)]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "Wwnn2" })]
	public string WorldWideNodeNameSetB { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "WorldWide", Justification = "This is per spec.")]
	[Parameter(Mandatory = true, ParameterSetName = "VMName and WWN", Position = 4)]
	[ValidateNotNullOrEmpty]
	[Alias(new string[] { "Wwpn2" })]
	public string WorldWidePortNameSetB { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMFibreChannelHba> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (VMFibreChannelHba != null)
		{
			return VMFibreChannelHba;
		}
		return (from hba in ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.FibreChannelHostBusAdapters, operationWatcher)
			where string.Equals(hba.WorldWideNodeNameSetA, WorldWideNodeNameSetA, StringComparison.OrdinalIgnoreCase) && string.Equals(hba.WorldWidePortNameSetA, WorldWidePortNameSetA, StringComparison.OrdinalIgnoreCase) && string.Equals(hba.WorldWideNodeNameSetB, WorldWideNodeNameSetB, StringComparison.OrdinalIgnoreCase) && string.Equals(hba.WorldWidePortNameSetB, WorldWidePortNameSetB, StringComparison.OrdinalIgnoreCase)
			select hba).ToList();
	}

	internal override void ProcessOneOperand(VMFibreChannelHba operand, IOperationWatcher operationWatcher)
	{
		VirtualMachineBase parentAs = operand.GetParentAs<VirtualMachineBase>();
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMFibreChannelHba, operand.Name, parentAs.Name)))
		{
			((IRemovable)operand).Remove(operationWatcher);
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(operand);
			}
		}
	}
}
