namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchOffloadStatusView : EthernetSwitchStatusView, IEthernetSwitchOffloadStatus, IEthernetSwitchStatus, IEthernetStatus, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string IovQueuePairCapacity = "IovQueuePairCapacity";

        public const string IovQueuePairUsage = "IovQueuePairUsage";

        public const string IovVfCapacity = "IovVfCapacity";

        public const string IovVfUsage = "IovVfUsage";

        public const string IPsecSACapacity = "IPsecSACapacity";

        public const string IPsecSAUsage = "IPsecSAUsage";

        public const string VmqCapacity = "VmqCapacity";

        public const string VmqUsage = "VmqUsage";

        public const string PacketDirectInUse = "PacketDirectInUse";

        public const string DefaultQueueVrssEnabled = "DefaultQueueVrssEnabled";

        public const string DefaultQueueVmmqEnabled = "DefaultQueueVmmqEnabled";

        public const string DefaultQueueVmmqQueuePairs = "DefaultQueueVmmqQueuePairs";

        public const string DefaultQueueVrssMinQueuePairs = "DefaultQueueVrssMinQueuePairs";

        public const string DefaultQueueVrssQueueSchedulingMode = "DefaultQueueVrssQueueSchedulingMode";

        public const string DefaultQueueVrssExcludePrimaryProcessor = "DefaultQueueVrssExcludePrimaryProcessor";

        public const string DefaultQueueVrssIndependentHostSpreading = "DefaultQueueVrssIndependentHostSpreading";
    }

    public uint IovQueuePairCapacity => GetProperty<uint>("IovQueuePairCapacity");

    public uint IovQueuePairUsage => GetProperty<uint>("IovQueuePairUsage");

    public uint IovVfCapacity => GetProperty<uint>("IovVfCapacity");

    public uint IovVfUsage => GetProperty<uint>("IovVfUsage");

    public uint IPsecSACapacity => GetProperty<uint>("IPsecSACapacity");

    public uint IPsecSAUsage => GetProperty<uint>("IPsecSAUsage");

    public uint VmqCapacity => GetProperty<uint>("VmqCapacity");

    public uint VmqUsage => GetProperty<uint>("VmqUsage");

    public bool PacketDirectInUse => GetPropertyOrDefault("PacketDirectInUse", defaultValue: false);

    public bool DefaultQueueVrssEnabled => GetProperty<bool>("DefaultQueueVrssEnabled");

    public bool DefaultQueueVmmqEnabled => GetProperty<bool>("DefaultQueueVmmqEnabled");

    public uint DefaultQueueVmmqQueuePairs => GetProperty<uint>("DefaultQueueVmmqQueuePairs");

    public uint DefaultQueueVrssMinQueuePairs => GetProperty<uint>("DefaultQueueVrssMinQueuePairs");

    public uint DefaultQueueVrssQueueSchedulingMode => GetProperty<uint>("DefaultQueueVrssQueueSchedulingMode");

    public bool DefaultQueueVrssExcludePrimaryProcessor => GetProperty<bool>("DefaultQueueVrssExcludePrimaryProcessor");

    public bool DefaultQueueVrssIndependentHostSpreading => GetProperty<bool>("DefaultQueueVrssIndependentHostSpreading");
}
