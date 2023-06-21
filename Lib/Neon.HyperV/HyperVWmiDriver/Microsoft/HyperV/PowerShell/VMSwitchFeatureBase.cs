using Microsoft.HyperV.PowerShell.Common;
using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal abstract class VMSwitchFeatureBase : VMNetworkingFeatureBase
{
    protected readonly VMSwitch m_Switch;

    public VMSwitch ParentSwitch => m_Switch;

    protected sealed override string DescriptionForPut => TaskDescriptions.SetVMSwitchFeature;

    protected sealed override string DescriptionForRemove => TaskDescriptions.RemoveVMSwitchFeature;

    protected VMSwitchFeatureBase(IEthernetSwitchFeature featureSetting, VMSwitch parentSwitch, bool isTemplate)
        : base(featureSetting, ObjectLocator.GetVirtualSwitchManagementService(parentSwitch.Server), isTemplate)
    {
        m_Switch = parentSwitch;
    }

    protected VMSwitchFeatureBase(VMSwitch parentSwitch, EthernetFeatureType featureType)
        : this((IEthernetSwitchFeature)VMNetworkingFeatureBase.GetDefaultFeatureSettingInstance(parentSwitch.Server, featureType), parentSwitch, isTemplate: true)
    {
    }

    protected sealed override void ResetParentFeatureCache()
    {
        m_Switch.InvalidateFeatureCache();
    }

    internal static VMSwitchFeatureBase Create(VMSwitch parentSwitch, IEthernetSwitchFeature featureSetting)
    {
        VMSwitchFeatureBase result = null;
        if (featureSetting is IEthernetSwitchBandwidthFeature)
        {
            result = new VMSwitchBandwidthSetting((IEthernetSwitchBandwidthFeature)featureSetting, parentSwitch);
        }
        else if (featureSetting is IEthernetSwitchNicTeamingFeature)
        {
            result = new VMSwitchNicTeamingSetting((IEthernetSwitchNicTeamingFeature)featureSetting, parentSwitch);
        }
        else if (featureSetting is IEthernetSwitchOffloadFeature)
        {
            result = new VMSwitchOffloadSetting((IEthernetSwitchOffloadFeature)featureSetting, parentSwitch);
        }
        return result;
    }
}
