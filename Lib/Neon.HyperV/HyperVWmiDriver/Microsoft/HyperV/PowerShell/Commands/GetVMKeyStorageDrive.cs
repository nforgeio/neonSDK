using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Get", "VMKeyStorageDrive", DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(KeyStorageDrive) })]
internal sealed class GetVMKeyStorageDrive : VirtualizationCmdlet<KeyStorageDrive>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet
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
	public int? ControllerNumber { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMDriveController")]
	public VMDriveController[] VMDriveController { get; set; }

	internal override IList<KeyStorageDrive> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IEnumerable<KeyStorageDrive> source;
		if (CurrentParameterSetIs("VMDriveController"))
		{
			source = VMDriveController.SelectManyWithLogging((VMDriveController controller) => controller.Drives, operationWatcher).OfType<KeyStorageDrive>();
			if (ControllerLocation.HasValue)
			{
				source = source.Where((KeyStorageDrive drive) => drive.ControllerLocation == ControllerLocation.Value);
			}
		}
		else
		{
			source = ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectManyWithLogging((VirtualMachineBase vm) => vm.FindDrives(ControllerType.IDE, ControllerNumber, ControllerLocation), operationWatcher).OfType<KeyStorageDrive>();
		}
		return source.ToList();
	}

	internal override void ProcessOneOperand(KeyStorageDrive operand, IOperationWatcher operationWatcher)
	{
		operationWatcher.WriteObject(operand);
	}
}
