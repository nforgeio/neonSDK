using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMHardDiskDrive", DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(HardDiskDrive) })]
internal sealed class GetVMHardDiskDrive : VirtualizationCmdlet<HardDiskDrive>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMSnapshotCmdlet
{
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

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string[] VMName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMSnapshot")]
	[Alias(new string[] { "VMCheckpoint" })]
	public VMSnapshot VMSnapshot { get; set; }

	[Parameter]
	[ValidateNotNull]
	public int? ControllerLocation { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMDriveController")]
	public VMDriveController[] VMDriveController { get; set; }

	[ValidateNotNull]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMSnapshot")]
	public int? ControllerNumber { get; set; }

	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMSnapshot")]
	public ControllerType? ControllerType { get; set; }

	internal override IList<HardDiskDrive> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IEnumerable<HardDiskDrive> source;
		if (CurrentParameterSetIs("VMDriveController"))
		{
			source = VMDriveController.SelectManyWithLogging((VMDriveController controller) => controller.Drives, operationWatcher).OfType<HardDiskDrive>();
			if (ControllerLocation.HasValue)
			{
				source = source.Where((HardDiskDrive drive) => drive.ControllerLocation == ControllerLocation.Value);
			}
		}
		else
		{
			IEnumerable<VirtualMachineBase> inputs = ((!CurrentParameterSetIs("VMSnapshot")) ? ((IEnumerable<VirtualMachineBase>)ParameterResolvers.ResolveVirtualMachines(this, operationWatcher)) : ((IEnumerable<VirtualMachineBase>)new VirtualMachineBase[1] { VMSnapshot }));
			source = inputs.SelectManyWithLogging((VirtualMachineBase vm) => vm.FindDrives(ControllerType, ControllerNumber, ControllerLocation), operationWatcher).OfType<HardDiskDrive>();
		}
		return source.ToList();
	}

	internal override void ProcessOneOperand(HardDiskDrive operand, IOperationWatcher operationWatcher)
	{
		operationWatcher.WriteObject(operand);
	}
}
