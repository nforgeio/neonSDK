using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMNetworkAdapterIsolationSetting : VMNetworkAdapterFeatureBase
{
	public VMNetworkAdapterIsolationMode IsolationMode
	{
		get
		{
			return (VMNetworkAdapterIsolationMode)((IEthernetSwitchPortIsolationFeature)m_FeatureSetting).IsolationMode;
		}
		internal set
		{
			((IEthernetSwitchPortIsolationFeature)m_FeatureSetting).IsolationMode = (IsolationMode)value;
		}
	}

	public bool AllowUntaggedTraffic
	{
		get
		{
			return ((IEthernetSwitchPortIsolationFeature)m_FeatureSetting).AllowUntaggedTraffic;
		}
		internal set
		{
			((IEthernetSwitchPortIsolationFeature)m_FeatureSetting).AllowUntaggedTraffic = value;
		}
	}

	public int DefaultIsolationID
	{
		get
		{
			return ((IEthernetSwitchPortIsolationFeature)m_FeatureSetting).DefaultIsolationID;
		}
		internal set
		{
			((IEthernetSwitchPortIsolationFeature)m_FeatureSetting).DefaultIsolationID = value;
		}
	}

	public OnOffState MultiTenantStack
	{
		get
		{
			return ((IEthernetSwitchPortIsolationFeature)m_FeatureSetting).IsMultiTenantStackEnabled.ToOnOffState();
		}
		internal set
		{
			((IEthernetSwitchPortIsolationFeature)m_FeatureSetting).IsMultiTenantStackEnabled = value.ToBool();
		}
	}

	internal VMNetworkAdapterIsolationSetting(IEthernetSwitchPortIsolationFeature isolationSetting, VMNetworkAdapterBase parentAdapter)
		: base(isolationSetting, parentAdapter, isTemplate: false)
	{
	}

	private VMNetworkAdapterIsolationSetting(VMNetworkAdapterBase parentAdapter)
		: base(parentAdapter, EthernetFeatureType.Isolation)
	{
	}

	internal void ClearSettings()
	{
		IEthernetSwitchPortIsolationFeature ethernetSwitchPortIsolationFeature = (IEthernetSwitchPortIsolationFeature)VMNetworkingFeatureBase.GetDefaultFeatureSettingInstance(base.Server, EthernetFeatureType.Isolation);
		IEthernetSwitchPortIsolationFeature obj = (IEthernetSwitchPortIsolationFeature)m_FeatureSetting;
		obj.IsolationMode = ethernetSwitchPortIsolationFeature.IsolationMode;
		obj.AllowUntaggedTraffic = ethernetSwitchPortIsolationFeature.AllowUntaggedTraffic;
		obj.DefaultIsolationID = ethernetSwitchPortIsolationFeature.DefaultIsolationID;
		obj.IsMultiTenantStackEnabled = ethernetSwitchPortIsolationFeature.IsMultiTenantStackEnabled;
	}

	internal static VMNetworkAdapterIsolationSetting CreateTemplateIsolationSetting(VMNetworkAdapterBase parentAdapter)
	{
		return new VMNetworkAdapterIsolationSetting(parentAdapter);
	}
}
