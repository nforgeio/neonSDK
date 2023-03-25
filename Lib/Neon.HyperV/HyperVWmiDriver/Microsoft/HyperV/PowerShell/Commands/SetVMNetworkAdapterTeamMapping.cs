using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;
using Microsoft.Management.Infrastructure;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMNetworkAdapterTeamMapping", SupportsShouldProcess = true, DefaultParameterSetName = "VMName")]
[OutputType(new Type[] { typeof(VMNetworkAdapterTeamMappingSetting) })]
internal sealed class SetVMNetworkAdapterTeamMapping : VirtualizationCmdlet<VMNetworkAdapterBase>, IVmBySingularVMNameCmdlet, IVirtualMachineCmdlet, IServerParameters, IParameterSet, IVmBySingularObjectCmdlet, IVMInternalNetworkAdapterBySwitchNameCmdlet, IVMNetworkAdapterBaseCmdlet, ISupportsPassthrough
{
	[Parameter(ParameterSetName = "ManagementOS", Mandatory = true)]
	public SwitchParameter ManagementOS { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	public override CimSession[] CimSession { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	[CredentialArray]
	public override PSCredential[] Credential { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMName", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public string VMName { get; set; }

	[ValidateNotNull]
	[Parameter(ParameterSetName = "ResourceObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VMNetworkAdapterBase VMNetworkAdapter { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(ParameterSetName = "VMObject", ValueFromPipeline = true, Position = 0, Mandatory = true)]
	public VirtualMachine VM { get; set; }

	[Parameter(ParameterSetName = "VMObject")]
	[Parameter(ParameterSetName = "VMName")]
	[Parameter(ParameterSetName = "ManagementOS")]
	public string VMNetworkAdapterName { get; set; }

	[Parameter(ParameterSetName = "ManagementOS")]
	[ValidateNotNullOrEmpty]
	public string SwitchName { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true)]
	public string PhysicalNetAdapterName { get; set; }

	[ValidateNotNullOrEmpty]
	[Parameter]
	public DisableOnFailoverFeature DisableOnFailover { get; set; }

	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is a standard PowerShell term.")]
	[Parameter]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMNetworkAdapterBase> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		if (CurrentParameterSetIs("ResourceObject"))
		{
			return new VMNetworkAdapterBase[1] { VMNetworkAdapter };
		}
		return ParameterResolvers.ResolveNetworkAdapters(this, VMNetworkAdapterName, operationWatcher);
	}

	internal override void ProcessOneOperand(VMNetworkAdapterBase operand, IOperationWatcher operationWatcher)
	{
		if (operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMNetworkAdapterTeamMapping, operand.Name)))
		{
			operand.PrepareForModify(operationWatcher);
			ConfigureTeamMappingSettings(operand, operationWatcher);
			if (Passthru.IsPresent)
			{
				operationWatcher.WriteObject(operand);
			}
		}
	}

	private void ConfigureTeamMappingSettings(VMNetworkAdapterBase adapter, IOperationWatcher operationWatcher)
	{
		VMNetworkAdapterTeamMappingSetting vMNetworkAdapterTeamMappingSetting = adapter.TeamMappingSetting;
		VMSwitch connectedSwitch = adapter.GetConnectedSwitch();
		if (connectedSwitch == null)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(CmdletErrorMessages.InvalidParameter_InvalidVirtualAdapterName);
		}
		string netAdapterDeviceId = NetworkingUtilities.FindExternalPortDeviceId(connectedSwitch.Server, PhysicalNetAdapterName);
		if (vMNetworkAdapterTeamMappingSetting == null)
		{
			vMNetworkAdapterTeamMappingSetting = VMNetworkAdapterTeamMappingSetting.CreateTemplateTeamMappingSetting(adapter);
		}
		vMNetworkAdapterTeamMappingSetting.NetAdapterName = PhysicalNetAdapterName;
		vMNetworkAdapterTeamMappingSetting.NetAdapterDeviceId = netAdapterDeviceId;
		if (IsParameterSpecified("DisableOnFailover"))
		{
			vMNetworkAdapterTeamMappingSetting.DisableOnFailover = DisableOnFailover;
		}
		adapter.AddOrModifyOneFeatureSetting(vMNetworkAdapterTeamMappingSetting, operationWatcher);
	}
}
