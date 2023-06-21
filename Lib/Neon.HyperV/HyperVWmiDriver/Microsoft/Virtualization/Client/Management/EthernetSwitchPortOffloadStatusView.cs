namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchPortOffloadStatusView : EthernetPortStatusView, IEthernetSwitchPortOffloadStatus, IEthernetPortStatus, IEthernetStatus, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string IovOffloadActive = "IovVfDataPathActive";

        public const string IovOffloadUsage = "IovOffloadUsage";

        public const string IovQueuePairUsage = "IovQueuePairUsage";

        public const string IovVirtualFunctionId = "IovVfId";

        public const string IpsecCurrentOffloadSaCount = "IpsecCurrentOffloadSaCount";

        public const string VmqId = "VMQId";

        public const string VmqOffloadUsage = "VMQOffloadUsage";

        public const string VrssEnabled = "VrssEnabled";

        public const string VmmqEnabled = "VmmqEnabled";

        public const string VmmqQueuePairs = "VmmqQueuePairs";

        public const string VrssMinQueuePairs = "VrssMinQueuePairs";

        public const string VrssQueueSchedulingMode = "VrssQueueSchedulingMode";

        public const string VrssExcludePrimaryProcessor = "VrssExcludePrimaryProcessor";

        public const string VrssIndependentHostSpreading = "VrssIndependentHostSpreading";

        public const string VrssVmbusChannelAffinityPolicy = "VrssVmbusChannelAffinityPolicy";
    }

    public int VmqOffloadUsage => NumberConverter.UInt32ToInt32(GetProperty<uint>("VMQOffloadUsage"));

    public int IovOffloadUsage => NumberConverter.UInt32ToInt32(GetProperty<uint>("IovOffloadUsage"));

    public uint IovQueuePairUsage => GetProperty<uint>("IovQueuePairUsage");

    public bool IovOffloadActive => GetProperty<bool>("IovVfDataPathActive");

    public uint IpsecCurrentOffloadSaCount => GetProperty<uint>("IpsecCurrentOffloadSaCount");

    public uint VmqId => GetProperty<uint>("VMQId");

    public ushort IovVirtualFunctionId => GetProperty<ushort>("IovVfId");

    public bool VrssEnabled => GetPropertyOrDefault("VrssEnabled", defaultValue: true);

    public bool VmmqEnabled => GetPropertyOrDefault("VmmqEnabled", defaultValue: false);

    public uint VmmqQueuePairs => GetPropertyOrDefault("VmmqQueuePairs", 1u);

    public uint VrssMinQueuePairs => GetPropertyOrDefault("VrssMinQueuePairs", 1u);

    public uint VrssQueueSchedulingMode => GetPropertyOrDefault("VrssQueueSchedulingMode", 2u);

    public bool VrssExcludePrimaryProcessor => GetPropertyOrDefault("VrssExcludePrimaryProcessor", defaultValue: false);

    public bool VrssIndependentHostSpreading => GetPropertyOrDefault("VrssIndependentHostSpreading", defaultValue: false);

    public uint VrssVmbusChannelAffinityPolicy => GetPropertyOrDefault("VrssVmbusChannelAffinityPolicy", 3u);
}
