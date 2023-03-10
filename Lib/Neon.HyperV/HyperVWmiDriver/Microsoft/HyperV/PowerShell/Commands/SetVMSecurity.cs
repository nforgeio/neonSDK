using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMSecurity", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VirtualMachine) })]
internal sealed class SetVMSecurity : VirtualizationCmdlet<VirtualMachine>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
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

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string[] VMName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	[Parameter]
	public bool EncryptStateAndVmMigrationTraffic { get; set; }

	[Parameter]
	public bool VirtualizationBasedSecurityOptOut { get; set; }

	internal override IList<VirtualMachine> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		return ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
	}

	internal override void ProcessOneOperand(VirtualMachine vm, IOperationWatcher operationWatcher)
	{
		if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMSecurity, vm.Name)))
		{
			return;
		}
		VMSecurity security = vm.GetSecurity();
		if (IsParameterSpecified("EncryptStateAndVmMigrationTraffic"))
		{
			if (!EncryptStateAndVmMigrationTraffic && security.Shielded)
			{
				throw ExceptionHelper.CreateInvalidOperationException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidOperation_ShieldingIsEnabled, "EncryptStateAndVmMigrationTraffic"), null, security);
			}
			security.EncryptStateAndVmMigrationTraffic = EncryptStateAndVmMigrationTraffic;
		}
		if (IsParameterSpecified("VirtualizationBasedSecurityOptOut"))
		{
			security.VirtualizationBasedSecurityOptOut = VirtualizationBasedSecurityOptOut;
		}
		((IUpdatable)security).Put(operationWatcher);
		if ((bool)Passthru)
		{
			operationWatcher.WriteObject(vm);
		}
	}
}
