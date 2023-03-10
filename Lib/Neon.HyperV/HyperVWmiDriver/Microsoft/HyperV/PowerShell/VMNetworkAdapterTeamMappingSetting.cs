using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMNetworkAdapterTeamMappingSetting : VMNetworkAdapterFeatureBase
{
	public string NetAdapterName
	{
		get
		{
			return ((IEthernetSwitchPortTeamMappingFeature)m_FeatureSetting).NetAdapterName;
		}
		internal set
		{
			((IEthernetSwitchPortTeamMappingFeature)m_FeatureSetting).NetAdapterName = value;
		}
	}

	public string NetAdapterDeviceId
	{
		get
		{
			return ((IEthernetSwitchPortTeamMappingFeature)m_FeatureSetting).NetAdapterDeviceId;
		}
		internal set
		{
			((IEthernetSwitchPortTeamMappingFeature)m_FeatureSetting).NetAdapterDeviceId = value;
		}
	}

	public DisableOnFailoverFeature DisableOnFailover
	{
		get
		{
			return ((IEthernetSwitchPortTeamMappingFeature)m_FeatureSetting).DisableOnFailover;
		}
		internal set
		{
			((IEthernetSwitchPortTeamMappingFeature)m_FeatureSetting).DisableOnFailover = value;
		}
	}

	internal VMNetworkAdapterTeamMappingSetting(IEthernetSwitchPortTeamMappingFeature offloadSetting, VMNetworkAdapterBase parentAdapter)
		: base(offloadSetting, parentAdapter, isTemplate: false)
	{
	}

	private VMNetworkAdapterTeamMappingSetting(VMNetworkAdapterBase parentAdapter)
		: base(parentAdapter, EthernetFeatureType.TeamMapping)
	{
	}

	internal static VMNetworkAdapterTeamMappingSetting CreateTemplateTeamMappingSetting(VMNetworkAdapterBase parentAdapter)
	{
		return new VMNetworkAdapterTeamMappingSetting(parentAdapter);
	}
}
