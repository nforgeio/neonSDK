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

[Cmdlet("Remove", "VMPmemController", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMPmemController) })]
internal sealed class RemoveVMPmemController : VirtualizationCmdlet<VMPmemController>, IVmByVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, ISupportsPassthrough
{
	internal static class ParameterSetNames
	{
		public const string VMName = "VMName";

		public const string Object = "Object";
	}

	internal const int ValidControllerNumber = 0;

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Need this to hide the inherited parameter.")]
	[SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility", Justification = "Need this to hide the inherited parameter.")]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName")]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Need this to hide the inherited parameter.")]
	[SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility", Justification = "Need this to hide the inherited parameter.")]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName")]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Need this to hide the inherited parameter.")]
	[SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility", Justification = "Need this to hide the inherited parameter.")]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName")]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "Object")]
	public VMPmemController[] VMPmemController { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ParameterSetName = "VMName")]
	public string[] VMName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMPmemController> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (VMPmemController != null)
		{
			return VMPmemController;
		}
		return (from controller in ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectManyWithLogging((VirtualMachine vm) => vm.GetPmemControllers(), operationWatcher)
			where controller.ControllerNumber == 0
			select controller).ToList();
	}

	internal override void ProcessOneOperand(VMPmemController operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_RemoveVMPmemController, operand.Name, operand.VMName)))
		{
			((IRemovable)operand).Remove(operationWatcher);
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(operand);
			}
		}
	}
}
