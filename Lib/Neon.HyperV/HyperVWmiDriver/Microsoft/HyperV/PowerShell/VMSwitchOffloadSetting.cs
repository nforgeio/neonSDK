using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMSwitchOffloadSetting : VMSwitchFeatureBase
{
    public bool DefaultQueueVrssEnabled
    {
        get
        {
            return ((IEthernetSwitchOffloadFeature)m_FeatureSetting).DefaultQueueVrssEnabled;
        }
        internal set
        {
            ((IEthernetSwitchOffloadFeature)m_FeatureSetting).DefaultQueueVrssEnabled = value;
        }
    }

    public bool DefaultQueueVmmqEnabled
    {
        get
        {
            return ((IEthernetSwitchOffloadFeature)m_FeatureSetting).DefaultQueueVmmqEnabled;
        }
        internal set
        {
            ((IEthernetSwitchOffloadFeature)m_FeatureSetting).DefaultQueueVmmqEnabled = value;
        }
    }

    public uint DefaultQueueVrssMaxQueuePairs
    {
        get
        {
            return ((IEthernetSwitchOffloadFeature)m_FeatureSetting).DefaultQueueVmmqQueuePairs;
        }
        internal set
        {
            ((IEthernetSwitchOffloadFeature)m_FeatureSetting).DefaultQueueVmmqQueuePairs = value;
        }
    }

    public uint DefaultQueueVrssMinQueuePairs
    {
        get
        {
            return ((IEthernetSwitchOffloadFeature)m_FeatureSetting).DefaultQueueVrssMinQueuePairs;
        }
        internal set
        {
            ((IEthernetSwitchOffloadFeature)m_FeatureSetting).DefaultQueueVrssMinQueuePairs = value;
        }
    }

    public VrssQueueSchedulingModeType DefaultQueueVrssQueueSchedulingMode
    {
        get
        {
            return (VrssQueueSchedulingModeType)((IEthernetSwitchOffloadFeature)m_FeatureSetting).DefaultQueueVrssQueueSchedulingMode;
        }
        internal set
        {
            ((IEthernetSwitchOffloadFeature)m_FeatureSetting).DefaultQueueVrssQueueSchedulingMode = (uint)value;
        }
    }

    public bool DefaultQueueVrssExcludePrimaryProcessor
    {
        get
        {
            return ((IEthernetSwitchOffloadFeature)m_FeatureSetting).DefaultQueueVrssExcludePrimaryProcessor;
        }
        internal set
        {
            ((IEthernetSwitchOffloadFeature)m_FeatureSetting).DefaultQueueVrssExcludePrimaryProcessor = value;
        }
    }

    public bool SoftwareRscEnabled
    {
        get
        {
            return ((IEthernetSwitchOffloadFeature)m_FeatureSetting).SoftwareRscEnabled;
        }
        internal set
        {
            ((IEthernetSwitchOffloadFeature)m_FeatureSetting).SoftwareRscEnabled = value;
        }
    }

    internal VMSwitchOffloadSetting(IEthernetSwitchOffloadFeature offloadFeature, VMSwitch parentSwitch)
        : base(offloadFeature, parentSwitch, isTemplate: false)
    {
    }

    private VMSwitchOffloadSetting(VMSwitch parentSwitch)
        : base(parentSwitch, EthernetFeatureType.SwitchOffload)
    {
    }

    internal static VMSwitchOffloadSetting CreateTemplateSwitchOffloadSetting(VMSwitch parentSwitch)
    {
        return new VMSwitchOffloadSetting(parentSwitch);
    }
}
