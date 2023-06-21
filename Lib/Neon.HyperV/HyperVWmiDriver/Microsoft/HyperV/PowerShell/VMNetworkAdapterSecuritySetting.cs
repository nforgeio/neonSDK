using Microsoft.HyperV.PowerShell.ExtensionMethods;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMNetworkAdapterSecuritySetting : VMNetworkAdapterFeatureBase
{
    public OnOffState MacAddressSpoofing
    {
        get
        {
            return ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).AllowMacSpoofing.ToOnOffState();
        }
        internal set
        {
            ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).AllowMacSpoofing = value.ToBool();
        }
    }

    public OnOffState DhcpGuard
    {
        get
        {
            return ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).EnableDhcpGuard.ToOnOffState();
        }
        internal set
        {
            ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).EnableDhcpGuard = value.ToBool();
        }
    }

    public OnOffState RouterGuard
    {
        get
        {
            return ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).EnableRouterGuard.ToOnOffState();
        }
        internal set
        {
            ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).EnableRouterGuard = value.ToBool();
        }
    }

    public VMNetworkAdapterPortMirroringMode PortMirroringMode
    {
        get
        {
            return (VMNetworkAdapterPortMirroringMode)((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).MonitorMode;
        }
        internal set
        {
            ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).MonitorMode = (SwitchPortMonitorMode)value;
        }
    }

    public OnOffState IeeePriorityTag
    {
        get
        {
            return ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).EnableIeeePriorityTag.ToOnOffState();
        }
        internal set
        {
            ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).EnableIeeePriorityTag = value.ToBool();
        }
    }

    public uint DynamicIPAddressLimit
    {
        get
        {
            return ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).DynamicIPAddressLimit;
        }
        internal set
        {
            ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).DynamicIPAddressLimit = value;
        }
    }

    public uint StormLimit
    {
        get
        {
            return ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).StormLimit;
        }
        internal set
        {
            ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).StormLimit = value;
        }
    }

    public uint VirtualSubnetId
    {
        get
        {
            return ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).VirtualSubnetId;
        }
        internal set
        {
            ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).VirtualSubnetId = value;
        }
    }

    public OnOffState AllowTeaming
    {
        get
        {
            return ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).AllowNicTeaming.ToOnOffState();
        }
        internal set
        {
            ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).AllowNicTeaming = value.ToBool();
        }
    }

    public OnOffState FixSpeed10G
    {
        get
        {
            return ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).EnableFixedSpeed10G.ToOnOffState();
        }
        internal set
        {
            ((IEthernetSwitchPortSecurityFeature)m_FeatureSetting).EnableFixedSpeed10G = value.ToBool();
        }
    }

    internal VMNetworkAdapterSecuritySetting(IEthernetSwitchPortSecurityFeature securitySetting, VMNetworkAdapterBase parentAdapter)
        : base(securitySetting, parentAdapter, isTemplate: false)
    {
    }

    private VMNetworkAdapterSecuritySetting(VMNetworkAdapterBase parentAdapter)
        : base(parentAdapter, EthernetFeatureType.Security)
    {
    }

    internal static VMNetworkAdapterSecuritySetting CreateTemplateSecuritySetting(VMNetworkAdapterBase parentAdapter)
    {
        return new VMNetworkAdapterSecuritySetting(parentAdapter);
    }
}
