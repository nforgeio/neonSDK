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

[Cmdlet("Enable", "VMIntegrationService", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMIntegrationComponent) })]
internal sealed class EnableVMIntegrationService : VirtualizationCmdlet<VMIntegrationComponent>, IVMObjectOrVMNameCmdlet, IVmByObjectCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmByVMNameCmdlet, ISupportsPassthrough
{
	private static class ParameterSetNames
	{
		public const string VMIntegrationService = "VMIntegrationService";
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
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "VMIntegrationService")]
	public VMIntegrationComponent[] VMIntegrationService { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ParameterSetName = "VMObject")]
	[Parameter(Mandatory = true, Position = 0, ParameterSetName = "VMName")]
	public string[] Name { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 1, Mandatory = true)]
	public VirtualMachine[] VM { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 1, Mandatory = true)]
	public string[] VMName { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMIntegrationComponent> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("VMIntegrationService"))
		{
			return VMIntegrationService;
		}
		IList<VirtualMachine> inputs = ParameterResolvers.ResolveVirtualMachines(this, operationWatcher);
		WildcardPatternMatcher matcher = null;
		if (Name != null && Name.Length != 0)
		{
			matcher = new WildcardPatternMatcher(Name);
		}
		return inputs.SelectManyWithLogging((VirtualMachine vmOrSnapshot) => VMIntegrationComponent.GetIntegrationComponents(vmOrSnapshot, matcher), operationWatcher).ToList();
	}

	internal override void ProcessOneOperand(VMIntegrationComponent integrationComponent, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_EnableVMIntegrationService, integrationComponent.Name, integrationComponent.VMName)))
		{
			integrationComponent.Enabled = true;
			((IUpdatable)integrationComponent).Put(operationWatcher);
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(integrationComponent);
			}
		}
	}
}
