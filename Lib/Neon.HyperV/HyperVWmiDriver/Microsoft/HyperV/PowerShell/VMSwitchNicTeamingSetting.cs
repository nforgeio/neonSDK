using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMSwitchNicTeamingSetting : VMSwitchFeatureBase
{
    public uint TeamingMode
    {
        get
        {
            return ((IEthernetSwitchNicTeamingFeature)m_FeatureSetting).TeamingMode;
        }
        internal set
        {
            ((IEthernetSwitchNicTeamingFeature)m_FeatureSetting).TeamingMode = value;
        }
    }

    public uint LoadBalancingAlgorithm
    {
        get
        {
            return ((IEthernetSwitchNicTeamingFeature)m_FeatureSetting).LoadBalancingAlgorithm;
        }
        internal set
        {
            ((IEthernetSwitchNicTeamingFeature)m_FeatureSetting).LoadBalancingAlgorithm = value;
        }
    }

    internal VMSwitchNicTeamingSetting(IEthernetSwitchNicTeamingFeature nicTeaming, VMSwitch parentSwitch)
        : base(nicTeaming, parentSwitch, isTemplate: false)
    {
    }

    private VMSwitchNicTeamingSetting(VMSwitch parentSwitch)
        : base(parentSwitch, EthernetFeatureType.SwitchNicTeaming)
    {
    }

    internal static VMSwitchNicTeamingSetting CreateTemplateSwitchNicTeamingSetting(VMSwitch parentSwitch)
    {
        return new VMSwitchNicTeamingSetting(parentSwitch);
    }
}
