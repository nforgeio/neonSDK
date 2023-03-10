using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMDvdDrive", DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(DvdDrive) })]
internal sealed class GetVMDvdDrive : VirtualizationCmdlet<DvdDrive>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, IVMSnapshotCmdlet
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

	[ValidateNotNull]
	[Parameter]
	public int? ControllerLocation { get; set; }

	[ValidateNotNull]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMSnapshot")]
	public int? ControllerNumber { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMDriveController")]
	public VMDriveController[] VMDriveController { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMSnapshot")]
	[Alias(new string[] { "VMCheckpoint" })]
	public VMSnapshot VMSnapshot { get; set; }

	internal override IList<DvdDrive> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IEnumerable<DvdDrive> source;
		if (CurrentParameterSetIs("VMDriveController"))
		{
			source = VMDriveController.SelectManyWithLogging((VMDriveController controller) => controller.Drives, operationWatcher).OfType<DvdDrive>();
			if (ControllerLocation.HasValue)
			{
				source = source.Where((DvdDrive drive) => drive.ControllerLocation == ControllerLocation.Value);
			}
		}
		else
		{
			IEnumerable<VirtualMachineBase> inputs = ((!CurrentParameterSetIs("VMSnapshot")) ? ((IEnumerable<VirtualMachineBase>)ParameterResolvers.ResolveVirtualMachines(this, operationWatcher)) : ((IEnumerable<VirtualMachineBase>)new VirtualMachineBase[1] { VMSnapshot }));
			source = inputs.SelectManyWithLogging((VirtualMachineBase vm) => vm.FindDrives((vm.Generation == 2) ? ControllerType.SCSI : ControllerType.IDE, ControllerNumber, ControllerLocation), operationWatcher).OfType<DvdDrive>();
		}
		return source.ToList();
	}

	internal override void ProcessOneOperand(DvdDrive operand, IOperationWatcher operationWatcher)
	{
		operationWatcher.WriteObject(operand);
	}
}
