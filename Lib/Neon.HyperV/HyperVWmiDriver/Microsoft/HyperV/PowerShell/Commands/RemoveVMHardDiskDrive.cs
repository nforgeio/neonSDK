using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Remove", "VMHardDiskDrive", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(HardDiskDrive) })]
internal sealed class RemoveVMHardDiskDrive : VirtualizationCmdlet<HardDiskDrive>, ISupportsPassthrough, IVmBySingularVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet
{
	internal static class ParameterSetNames
	{
		public const string VMName = "VMName";

		public const string VMHardDiskDrive = "VMHardDiskDrive";
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ParameterSetName = "VMName")]
	public string VMName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec and arrays are easier to use.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ParameterSetName = "VMHardDiskDrive", ValueFromPipeline = true)]
	public HardDiskDrive[] VMHardDiskDrive { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 1, ParameterSetName = "VMName")]
	public ControllerType? ControllerType { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 2, ParameterSetName = "VMName")]
	public int? ControllerNumber { get; set; }

	[Parameter(Mandatory = true, Position = 3, ParameterSetName = "VMName")]
	public int? ControllerLocation { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<HardDiskDrive> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (VMHardDiskDrive != null)
		{
			return VMHardDiskDrive;
		}
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.FindDrives(ControllerType, ControllerNumber, ControllerLocation), operationWatcher).OfType<HardDiskDrive>()
			.ToList();
	}

	internal override void ProcessOneOperand(HardDiskDrive hardDrive, IOperationWatcher operationWatcher)
	{
		VirtualMachineBase parentAs = hardDrive.GetParentAs<VirtualMachineBase>();
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMHardDiskDrive, hardDrive.Name, parentAs.Name)))
		{
			((IRemovable)hardDrive).Remove(operationWatcher);
			if (parentAs.IsClustered)
			{
				ClusterUtilities.UpdateClusterVMConfiguration(parentAs, base.InvokeCommand, operationWatcher);
			}
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(hardDrive);
			}
		}
	}
}
