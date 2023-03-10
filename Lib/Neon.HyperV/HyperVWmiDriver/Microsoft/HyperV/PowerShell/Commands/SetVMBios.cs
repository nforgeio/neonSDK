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

[Cmdlet("Set", "VMBios", DefaultParameterSetName = "VMName", SupportsShouldProcess = true)]
[OutputType(new Type[] { typeof(VMBios) })]
internal sealed class SetVMBios : VirtualizationCmdlet<VMBios>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
	private static class ParameterSetNames
	{
		public const string BiosObject = "VMBios";
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

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string[] VMName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMBios", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VMBios[] VMBios { get; set; }

	[Parameter]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Num")]
	public SwitchParameter DisableNumLock { get; set; }

	[Parameter]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Num")]
	public SwitchParameter EnableNumLock { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter]
	[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "Need to accept input for the parameter.")]
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Usability is more important than the slight gain in efficiency here.")]
	public BootDevice[] StartupOrder { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	protected override void ValidateParameters()
	{
		base.ValidateParameters();
		ValidateMutuallyExclusiveParameters("DisableNumLock", "EnableNumLock");
	}

	internal override IList<VMBios> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("VMBios"))
		{
			return VMBios;
		}
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher).SelectWithLogging((VirtualMachine vm) => vm.GetBios(), operationWatcher).ToList();
	}

	internal override void ProcessOneOperand(VMBios bios, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMBios, bios.GetParentAs<VirtualMachineBase>().Name)))
		{
			if (DisableNumLock.IsPresent)
			{
				bios.NumLockEnabled = false;
			}
			else if (EnableNumLock.IsPresent)
			{
				bios.NumLockEnabled = true;
			}
			if (StartupOrder != null)
			{
				bios.StartupOrder = StartupOrder;
			}
			((IUpdatable)bios).Put(operationWatcher);
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(bios);
			}
		}
	}
}
