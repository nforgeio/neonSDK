using Microsoft.Virtualization.Client.Management;

namespace Microsoft.HyperV.PowerShell;

internal sealed class VMNetworkAdapterOffloadSetting : VMNetworkAdapterFeatureBase
{
    public uint VMQWeight
    {
        get
        {
            return NumberConverter.Int32ToUInt32(((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VMQOffloadWeight);
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VMQOffloadWeight = NumberConverter.UInt32ToInt32(value);
        }
    }

    public uint IovWeight
    {
        get
        {
            return NumberConverter.Int32ToUInt32(((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).IOVOffloadWeight);
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).IOVOffloadWeight = NumberConverter.UInt32ToInt32(value);
        }
    }

    public uint IPSecOffloadMaxSA
    {
        get
        {
            return NumberConverter.Int32ToUInt32(((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).IPSecOffloadLimit);
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).IPSecOffloadLimit = NumberConverter.UInt32ToInt32(value);
        }
    }

    public uint IOVQueuePairsRequested
    {
        get
        {
            return ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).IOVQueuePairsRequested;
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).IOVQueuePairsRequested = value;
        }
    }

    public IovInterruptModerationValue IovInterruptModeration
    {
        get
        {
            return (IovInterruptModerationValue)((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).InterruptModerationMode;
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).InterruptModerationMode = (IovInterruptModerationMode)value;
        }
    }

    public uint PacketDirectNumProcs
    {
        get
        {
            return ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).PacketDirectNumProcs;
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).PacketDirectNumProcs = value;
        }
    }

    public uint PacketDirectModerationCount
    {
        get
        {
            return ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).PacketDirectModerationCount;
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).PacketDirectModerationCount = value;
        }
    }

    public uint PacketDirectModerationInterval
    {
        get
        {
            return ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).PacketDirectModerationInterval;
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).PacketDirectModerationInterval = value;
        }
    }

    public bool VrssEnabled
    {
        get
        {
            return ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VrssEnabled;
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VrssEnabled = value;
        }
    }

    public bool VmmqEnabled
    {
        get
        {
            return ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VmmqEnabled;
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VmmqEnabled = value;
        }
    }

    public uint VrssMaxQueuePairs
    {
        get
        {
            return ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VmmqQueuePairs;
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VmmqQueuePairs = value;
        }
    }

    public uint VrssMinQueuePairs
    {
        get
        {
            return ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VrssMinQueuePairs;
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VrssMinQueuePairs = value;
        }
    }

    public VrssQueueSchedulingModeType VrssQueueSchedulingMode
    {
        get
        {
            return (VrssQueueSchedulingModeType)((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VrssQueueSchedulingMode;
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VrssQueueSchedulingMode = (uint)value;
        }
    }

    public bool VrssExcludePrimaryProcessor
    {
        get
        {
            return ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VrssExcludePrimaryProcessor;
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VrssExcludePrimaryProcessor = value;
        }
    }

    public bool VrssIndependentHostSpreading
    {
        get
        {
            return ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VrssIndependentHostSpreading;
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VrssIndependentHostSpreading = value;
        }
    }

    public VrssVmbusChannelAffinityPolicyType VrssVmbusChannelAffinityPolicy
    {
        get
        {
            return (VrssVmbusChannelAffinityPolicyType)((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VrssVmbusChannelAffinityPolicy;
        }
        internal set
        {
            ((IEthernetSwitchPortOffloadFeature)m_FeatureSetting).VrssVmbusChannelAffinityPolicy = (uint)value;
        }
    }

    internal VMNetworkAdapterOffloadSetting(IEthernetSwitchPortOffloadFeature offloadSetting, VMNetworkAdapterBase parentAdapter)
        : base(offloadSetting, parentAdapter, isTemplate: false)
    {
    }

    private VMNetworkAdapterOffloadSetting(VMNetworkAdapterBase parentAdapter)
        : base(parentAdapter, EthernetFeatureType.Offload)
    {
    }

    internal static VMNetworkAdapterOffloadSetting CreateTemplateOffloadSetting(VMNetworkAdapterBase parentAdapter)
    {
        return new VMNetworkAdapterOffloadSetting(parentAdapter);
    }
}
