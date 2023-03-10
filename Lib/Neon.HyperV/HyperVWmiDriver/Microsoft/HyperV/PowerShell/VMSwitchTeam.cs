using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMSwitchTeam
{
	internal VMSwitch m_VMSwitch;

	[VirtualizationObjectIdentifier(IdentifierFlags.FriendlyName)]
	public string Name => m_VMSwitch.Name;

	[VirtualizationObjectIdentifier(IdentifierFlags.UniqueIdentifier)]
	public Guid Id => m_VMSwitch.Id;

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public string[] NetAdapterInterfaceDescription => m_VMSwitch.NetAdapterInterfaceDescriptions;

	[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "This is per spec.")]
	public Guid[] NetAdapterInterfaceGuid => m_VMSwitch.NetAdapterInterfaceGuid;

	public VMSwitchTeamingMode? TeamingMode => (VMSwitchTeamingMode?)m_VMSwitch.NicTeamingSetting?.TeamingMode;

	public VMSwitchLoadBalancingAlgorithm? LoadBalancingAlgorithm => (VMSwitchLoadBalancingAlgorithm?)m_VMSwitch.NicTeamingSetting?.LoadBalancingAlgorithm;

	internal VMSwitchTeam(VMSwitch vmswitch)
	{
		m_VMSwitch = vmswitch;
	}
}
