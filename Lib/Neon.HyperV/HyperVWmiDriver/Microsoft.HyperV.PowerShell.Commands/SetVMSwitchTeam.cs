using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using Microsoft.HyperV.PowerShell.Commands.Resources;

namespace Microsoft.HyperV.PowerShell.Commands;

[Cmdlet("Set", "VMSwitchTeam", SupportsShouldProcess = true, DefaultParameterSetName = "SwitchName_NetAdapterName")]
[OutputType(new Type[] { typeof(VMSwitch) })]
internal sealed class SetVMSwitchTeam : VirtualizationCmdlet<VMSwitch>, ISupportsPassthrough
{
	internal static class ParameterSetNames
	{
		public const string SwitchNameNetAdapterInterfaceDescription = "SwitchName_NetAdapterInterfaceDescription";

		public const string SwitchNameNetAdapterName = "SwitchName_NetAdapterName";

		public const string SwitchObjectNetAdapterName = "SwitchObject_NetAdapterName";

		public const string SwitchObjectNetAdapterInterfaceDescription = "SwitchObject_NetAdapterInterfaceDescription";
	}

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "SwitchName_NetAdapterInterfaceDescription", ValueFromPipelineByPropertyName = true)]
	[Parameter(ParameterSetName = "SwitchName_NetAdapterName", ValueFromPipelineByPropertyName = true)]
	[Parameter(ParameterSetName = "SwitchObject_NetAdapterInterfaceDescription", ValueFromPipelineByPropertyName = true)]
	[Parameter(ParameterSetName = "SwitchObject_NetAdapterName", ValueFromPipelineByPropertyName = true)]
	[Alias(new string[] { "PSComputerName" })]
	public override string[] ComputerName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec, and array is more user friendly as this is an user input parameter.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchObject_NetAdapterName")]
	[Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ParameterSetName = "SwitchObject_NetAdapterInterfaceDescription")]
	public VMSwitch[] VMSwitch { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Array is more user friendly, and this is a user input parameter.")]
	[ValidateNotNullOrEmpty]
	[Parameter(Mandatory = true, Position = 0, ParameterSetName = "SwitchName_NetAdapterInterfaceDescription")]
	[Parameter(Mandatory = true, Position = 0, ParameterSetName = "SwitchName_NetAdapterName")]
	[Alias(new string[] { "SwitchName" })]
	public string[] Name { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Alias(new string[] { "InterfaceAlias" })]
	[Parameter(ParameterSetName = "SwitchName_NetAdapterName")]
	[Parameter(ParameterSetName = "SwitchObject_NetAdapterName")]
	public string[] NetAdapterName { get; set; }

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is by spec.")]
	[Parameter(ParameterSetName = "SwitchName_NetAdapterInterfaceDescription")]
	[Parameter(ParameterSetName = "SwitchObject_NetAdapterInterfaceDescription")]
	public string[] NetAdapterInterfaceDescription { get; set; }

	[ValidateNotNull]
	[Parameter]
	public VMSwitchTeamingMode? TeamingMode { get; set; }

	[ValidateNotNull]
	[Parameter]
	public VMSwitchLoadBalancingAlgorithm? LoadBalancingAlgorithm { get; set; }

	[Parameter]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Passthru", Justification = "This is per spec. Also Passthru is a standard PowerShell cmdlet parameter name.")]
	public SwitchParameter Passthru { get; set; }

	internal override IList<VMSwitch> EnumerateOperands(IOperationWatcher operationWatcher)
	{
		IList<VMSwitch> list = ((!IsParameterSpecified("VMSwitch")) ? Microsoft.HyperV.PowerShell.VMSwitch.GetSwitchesByNamesAndServers(ParameterResolvers.GetServers(this, operationWatcher), Name, allowWildcards: true, ErrorDisplayMode.WriteError, operationWatcher) : VMSwitch);
		VMSwitch vMSwitch = list.FirstOrDefault((VMSwitch vmSwitchTeam) => !vmSwitchTeam.EmbeddedTeamingEnabled);
		if (vMSwitch != null)
		{
			throw ExceptionHelper.CreateInvalidArgumentException(string.Format(CultureInfo.CurrentCulture, CmdletErrorMessages.InvalidParameter_VMSwitchIsNotTeamingEnabled, vMSwitch.Name));
		}
		return list;
	}

	internal override void ProcessOneOperand(VMSwitch operand, IOperationWatcher operationWatcher)
	{
		if (!operationWatcher.ShouldProcess(string.Format(CultureInfo.CurrentCulture, CmdletResources.ShouldProcess_SetVMSwitchTeam, operand.Name)))
		{
			return;
		}
		if (NetAdapterName != null || NetAdapterInterfaceDescription != null)
		{
			operand.ConfigureConnections(operand.SwitchType, operand.AllowManagementOS, NetAdapterName, NetAdapterInterfaceDescription, operationWatcher);
		}
		if (LoadBalancingAlgorithm.HasValue || TeamingMode.HasValue)
		{
			VMSwitchNicTeamingSetting vMSwitchNicTeamingSetting = operand.NicTeamingSetting;
			if (vMSwitchNicTeamingSetting == null)
			{
				vMSwitchNicTeamingSetting = VMSwitchNicTeamingSetting.CreateTemplateSwitchNicTeamingSetting(operand);
			}
			bool flag = false;
			if (TeamingMode.HasValue)
			{
				vMSwitchNicTeamingSetting.TeamingMode = (uint)TeamingMode.Value;
				flag = true;
			}
			if (LoadBalancingAlgorithm.HasValue)
			{
				vMSwitchNicTeamingSetting.LoadBalancingAlgorithm = (uint)LoadBalancingAlgorithm.Value;
				flag = true;
			}
			if (flag)
			{
				operand.AddOrModifyOneFeatureSetting(vMSwitchNicTeamingSetting, operationWatcher);
			}
			((IUpdatable)operand).Put(operationWatcher);
		}
		if (Passthru.IsPresent)
		{
			operationWatcher.WriteObject(operand);
		}
	}
}
