namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchPortOffloadFeatureView : EthernetSwitchPortFeatureView, IEthernetSwitchPortOffloadFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string VMQOffloadWeight = "VMQOffloadWeight";

        public const string IOVOffloadWeight = "IOVOffloadWeight";

        public const string IPSecOffloadLimit = "IPSecOffloadLimit";

        public const string IovQueuePairsRequested = "IOVQueuePairsRequested";

        public const string IovInterruptModeration = "IOVInterruptModeration";

        public const string PacketDirectModerationCount = "PacketDirectModerationCount";

        public const string PacketDirectNumProcs = "PacketDirectNumProcs";

        public const string PacketDirectModerationInterval = "PacketDirectModerationInterval";

        public const string VrssEnabled = "VrssEnabled";

        public const string VmmqEnabled = "VmmqEnabled";

        public const string VmmqQueuePairs = "VmmqQueuePairs";

        public const string VrssMinQueuePairs = "VrssMinQueuePairs";

        public const string VrssQueueSchedulingMode = "VrssQueueSchedulingMode";

        public const string VrssExcludePrimaryProcessor = "VrssExcludePrimaryProcessor";

        public const string VrssIndependentHostSpreading = "VrssIndependentHostSpreading";

        public const string VrssVmbusChannelAffinityPolicy = "VrssVmbusChannelAffinityPolicy";
    }

    public int VMQOffloadWeight
    {
        get
        {
            return NumberConverter.UInt32ToInt32(GetProperty<uint>("VMQOffloadWeight"));
        }
        set
        {
            uint num = NumberConverter.Int32ToUInt32(value);
            SetProperty("VMQOffloadWeight", num);
        }
    }

    public int IOVOffloadWeight
    {
        get
        {
            return NumberConverter.UInt32ToInt32(GetProperty<uint>("IOVOffloadWeight"));
        }
        set
        {
            uint num = NumberConverter.Int32ToUInt32(value);
            SetProperty("IOVOffloadWeight", num);
        }
    }

    public int IPSecOffloadLimit
    {
        get
        {
            return NumberConverter.UInt32ToInt32(GetProperty<uint>("IPSecOffloadLimit"));
        }
        set
        {
            uint num = NumberConverter.Int32ToUInt32(value);
            SetProperty("IPSecOffloadLimit", num);
        }
    }

    public IovInterruptModerationMode InterruptModerationMode
    {
        get
        {
            return (IovInterruptModerationMode)GetProperty<uint>("IOVInterruptModeration");
        }
        set
        {
            SetProperty("IOVInterruptModeration", (uint)value);
        }
    }

    public uint IOVQueuePairsRequested
    {
        get
        {
            return GetProperty<uint>("IOVQueuePairsRequested");
        }
        set
        {
            SetProperty("IOVQueuePairsRequested", value);
        }
    }

    public uint PacketDirectNumProcs
    {
        get
        {
            return GetPropertyOrDefault("PacketDirectNumProcs", 0u);
        }
        set
        {
            SetProperty("PacketDirectNumProcs", value);
        }
    }

    public uint PacketDirectModerationCount
    {
        get
        {
            return GetPropertyOrDefault("PacketDirectModerationCount", 0u);
        }
        set
        {
            SetProperty("PacketDirectModerationCount", value);
        }
    }

    public uint PacketDirectModerationInterval
    {
        get
        {
            return GetPropertyOrDefault("PacketDirectModerationInterval", 0u);
        }
        set
        {
            SetProperty("PacketDirectModerationInterval", value);
        }
    }

    public override EthernetFeatureType FeatureType => EthernetFeatureType.Offload;

    public bool VrssEnabled
    {
        get
        {
            return GetPropertyOrDefault("VrssEnabled", defaultValue: true);
        }
        set
        {
            SetProperty("VrssEnabled", value);
        }
    }

    public bool VmmqEnabled
    {
        get
        {
            return GetPropertyOrDefault("VmmqEnabled", defaultValue: false);
        }
        set
        {
            SetProperty("VmmqEnabled", value);
        }
    }

    public uint VmmqQueuePairs
    {
        get
        {
            return GetPropertyOrDefault("VmmqQueuePairs", 1u);
        }
        set
        {
            SetProperty("VmmqQueuePairs", value);
        }
    }

    public uint VrssMinQueuePairs
    {
        get
        {
            return GetPropertyOrDefault("VrssMinQueuePairs", 1u);
        }
        set
        {
            SetProperty("VrssMinQueuePairs", value);
        }
    }

    public uint VrssQueueSchedulingMode
    {
        get
        {
            return GetPropertyOrDefault("VrssQueueSchedulingMode", 2u);
        }
        set
        {
            SetProperty("VrssQueueSchedulingMode", value);
        }
    }

    public bool VrssExcludePrimaryProcessor
    {
        get
        {
            return GetPropertyOrDefault("VrssExcludePrimaryProcessor", defaultValue: false);
        }
        set
        {
            SetProperty("VrssExcludePrimaryProcessor", value);
        }
    }

    public bool VrssIndependentHostSpreading
    {
        get
        {
            return GetPropertyOrDefault("VrssIndependentHostSpreading", defaultValue: false);
        }
        set
        {
            SetProperty("VrssIndependentHostSpreading", value);
        }
    }

    public uint VrssVmbusChannelAffinityPolicy
    {
        get
        {
            return GetPropertyOrDefault("VrssVmbusChannelAffinityPolicy", 3u);
        }
        set
        {
            SetProperty("VrssVmbusChannelAffinityPolicy", value);
        }
    }
}
