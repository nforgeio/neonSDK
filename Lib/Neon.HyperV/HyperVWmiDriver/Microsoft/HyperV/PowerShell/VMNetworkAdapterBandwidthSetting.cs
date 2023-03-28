using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMNetworkAdapterBandwidthSetting : VMNetworkAdapterFeatureBase
{
	public long? MinimumBandwidthAbsolute
	{
		get
		{
			return ((IEthernetSwitchPortBandwidthFeature)m_FeatureSetting).Reservation;
		}
		internal set
		{
			IEthernetSwitchPortBandwidthFeature ethernetSwitchPortBandwidthFeature = (IEthernetSwitchPortBandwidthFeature)m_FeatureSetting;
			if (value.HasValue)
			{
				ethernetSwitchPortBandwidthFeature.Reservation = value.Value;
			}
		}
	}

	public long? MinimumBandwidthWeight
	{
		get
		{
			return ((IEthernetSwitchPortBandwidthFeature)m_FeatureSetting).Weight;
		}
		internal set
		{
			IEthernetSwitchPortBandwidthFeature ethernetSwitchPortBandwidthFeature = (IEthernetSwitchPortBandwidthFeature)m_FeatureSetting;
			if (value.HasValue)
			{
				ethernetSwitchPortBandwidthFeature.Weight = value.Value;
			}
		}
	}

	public long? MaximumBandwidth
	{
		get
		{
			return ((IEthernetSwitchPortBandwidthFeature)m_FeatureSetting).Limit;
		}
		internal set
		{
			IEthernetSwitchPortBandwidthFeature ethernetSwitchPortBandwidthFeature = (IEthernetSwitchPortBandwidthFeature)m_FeatureSetting;
			if (value.HasValue)
			{
				ethernetSwitchPortBandwidthFeature.Limit = value.Value;
			}
		}
	}

	internal VMNetworkAdapterBandwidthSetting(IEthernetSwitchPortBandwidthFeature bandwidthFeature, VMNetworkAdapterBase parentAdapter)
		: base(bandwidthFeature, parentAdapter, isTemplate: false)
	{
	}

	private VMNetworkAdapterBandwidthSetting(VMNetworkAdapterBase parentAdapter)
		: base(parentAdapter, EthernetFeatureType.Bandwidth)
	{
	}

	internal static VMNetworkAdapterBandwidthSetting CreateTemplateBandwidthSetting(VMNetworkAdapterBase parentAdapter)
	{
		return new VMNetworkAdapterBandwidthSetting(parentAdapter);
	}
}
