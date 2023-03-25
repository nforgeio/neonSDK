using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMSwitchBandwidthSetting : VMSwitchFeatureBase
{
	public long DefaultFlowReservation
	{
		get
		{
			return ((IEthernetSwitchBandwidthFeature)m_FeatureSetting).DefaultFlowReservation;
		}
		internal set
		{
			((IEthernetSwitchBandwidthFeature)m_FeatureSetting).DefaultFlowReservation = value;
		}
	}

	public long DefaultFlowWeight
	{
		get
		{
			return ((IEthernetSwitchBandwidthFeature)m_FeatureSetting).DefaultFlowWeight;
		}
		internal set
		{
			((IEthernetSwitchBandwidthFeature)m_FeatureSetting).DefaultFlowWeight = value;
		}
	}

	internal VMSwitchBandwidthSetting(IEthernetSwitchBandwidthFeature bandwidthFeature, VMSwitch parentSwitch)
		: base(bandwidthFeature, parentSwitch, isTemplate: false)
	{
	}

	private VMSwitchBandwidthSetting(VMSwitch parentSwitch)
		: base(parentSwitch, EthernetFeatureType.SwitchBandwidth)
	{
	}

	internal static VMSwitchBandwidthSetting CreateTemplateSwitchBandwidthSetting(VMSwitch parentSwitch)
	{
		return new VMSwitchBandwidthSetting(parentSwitch);
	}
}
