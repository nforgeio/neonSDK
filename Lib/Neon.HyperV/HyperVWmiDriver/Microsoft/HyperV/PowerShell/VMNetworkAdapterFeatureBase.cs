using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal abstract class VMNetworkAdapterFeatureBase : VMNetworkingFeatureBase
{
	protected readonly VMNetworkAdapterBase m_ParentAdapter;

	public VMNetworkAdapterBase ParentAdapter => m_ParentAdapter;

	protected sealed override string DescriptionForPut => TaskDescriptions.SetVMNetworkAdapterFeature;

	protected sealed override string DescriptionForRemove => TaskDescriptions.RemoveVMNetworkAdapterFeature;

	protected VMNetworkAdapterFeatureBase(IEthernetSwitchPortFeature featureSetting, VMNetworkAdapterBase parentAdapter, bool isTemplate)
		: base(featureSetting, parentAdapter.FeatureService, isTemplate)
	{
		m_ParentAdapter = parentAdapter;
	}

	protected VMNetworkAdapterFeatureBase(VMNetworkAdapterBase parentAdapter, EthernetFeatureType featureType)
		: this((IEthernetSwitchPortFeature)VMNetworkingFeatureBase.GetDefaultFeatureSettingInstance(parentAdapter.Server, featureType), parentAdapter, isTemplate: true)
	{
	}

	protected sealed override void ResetParentFeatureCache()
	{
		m_ParentAdapter.InvalidateFeatureCache();
	}

	internal static VMNetworkAdapterFeatureBase Create(VMNetworkAdapterBase parentAdapter, IEthernetSwitchPortFeature featureSetting)
	{
		VMNetworkAdapterFeatureBase result = null;
		if (featureSetting is IEthernetSwitchPortAclFeature)
		{
			result = new VMNetworkAdapterAclSetting((IEthernetSwitchPortAclFeature)featureSetting, parentAdapter);
		}
		else if (featureSetting is IEthernetSwitchPortBandwidthFeature)
		{
			result = new VMNetworkAdapterBandwidthSetting((IEthernetSwitchPortBandwidthFeature)featureSetting, parentAdapter);
		}
		else if (featureSetting is IEthernetSwitchPortExtendedAclFeature)
		{
			result = new VMNetworkAdapterExtendedAclSetting((IEthernetSwitchPortExtendedAclFeature)featureSetting, parentAdapter);
		}
		else if (featureSetting is IEthernetSwitchPortIsolationFeature)
		{
			result = new VMNetworkAdapterIsolationSetting((IEthernetSwitchPortIsolationFeature)featureSetting, parentAdapter);
		}
		else if (featureSetting is IEthernetSwitchPortOffloadFeature)
		{
			result = new VMNetworkAdapterOffloadSetting((IEthernetSwitchPortOffloadFeature)featureSetting, parentAdapter);
		}
		else if (featureSetting is IEthernetSwitchPortRdmaFeature)
		{
			result = new VMNetworkAdapterRdmaSetting((IEthernetSwitchPortRdmaFeature)featureSetting, parentAdapter);
		}
		else if (featureSetting is IEthernetSwitchPortRoutingDomainFeature)
		{
			result = new VMNetworkAdapterRoutingDomainSetting((IEthernetSwitchPortRoutingDomainFeature)featureSetting, parentAdapter);
		}
		else if (featureSetting is IEthernetSwitchPortSecurityFeature)
		{
			result = new VMNetworkAdapterSecuritySetting((IEthernetSwitchPortSecurityFeature)featureSetting, parentAdapter);
		}
		else if (featureSetting is IEthernetSwitchPortVlanFeature)
		{
			result = new VMNetworkAdapterVlanSetting((IEthernetSwitchPortVlanFeature)featureSetting, parentAdapter);
		}
		else if (featureSetting is IEthernetSwitchPortTeamMappingFeature)
		{
			result = new VMNetworkAdapterTeamMappingSetting((IEthernetSwitchPortTeamMappingFeature)featureSetting, parentAdapter);
		}
		return result;
	}
}
