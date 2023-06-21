namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchOffloadFeatureView : EthernetSwitchFeatureView, IEthernetSwitchOffloadFeature, IEthernetSwitchFeature, IEthernetFeature, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string DefaultQueueVrssEnabled = "DefaultQueueVrssEnabled";

        public const string DefaultQueueVmmqEnabled = "DefaultQueueVmmqEnabled";

        public const string DefaultQueueVmmqQueuePairs = "DefaultQueueVmmqQueuePairs";

        public const string DefaultQueueVrssMinQueuePairs = "DefaultQueueVrssMinQueuePairs";

        public const string DefaultQueueVrssQueueSchedulingMode = "DefaultQueueVrssQueueSchedulingMode";

        public const string DefaultQueueVrssExcludePrimaryProcessor = "DefaultQueueVrssExcludePrimaryProcessor";

        public const string DefaultQueueVrssIndependentHostSpreading = "DefaultQueueVrssIndependentHostSpreading";

        public const string SoftwareRscEnabled = "SoftwareRscEnabled";
    }

    public override EthernetFeatureType FeatureType => EthernetFeatureType.SwitchOffload;

    public bool DefaultQueueVrssEnabled
    {
        get
        {
            return GetProperty<bool>("DefaultQueueVrssEnabled");
        }
        set
        {
            SetProperty("DefaultQueueVrssEnabled", value);
        }
    }

    public bool DefaultQueueVmmqEnabled
    {
        get
        {
            return GetProperty<bool>("DefaultQueueVmmqEnabled");
        }
        set
        {
            SetProperty("DefaultQueueVmmqEnabled", value);
        }
    }

    public uint DefaultQueueVmmqQueuePairs
    {
        get
        {
            return GetProperty<uint>("DefaultQueueVmmqQueuePairs");
        }
        set
        {
            SetProperty("DefaultQueueVmmqQueuePairs", value);
        }
    }

    public uint DefaultQueueVrssMinQueuePairs
    {
        get
        {
            return GetProperty<uint>("DefaultQueueVrssMinQueuePairs");
        }
        set
        {
            SetProperty("DefaultQueueVrssMinQueuePairs", value);
        }
    }

    public uint DefaultQueueVrssQueueSchedulingMode
    {
        get
        {
            return GetProperty<uint>("DefaultQueueVrssQueueSchedulingMode");
        }
        set
        {
            SetProperty("DefaultQueueVrssQueueSchedulingMode", value);
        }
    }

    public bool DefaultQueueVrssExcludePrimaryProcessor
    {
        get
        {
            return GetProperty<bool>("DefaultQueueVrssExcludePrimaryProcessor");
        }
        set
        {
            SetProperty("DefaultQueueVrssExcludePrimaryProcessor", value);
        }
    }

    public bool DefaultQueueVrssIndependentHostSpreading
    {
        get
        {
            return GetProperty<bool>("DefaultQueueVrssIndependentHostSpreading");
        }
        set
        {
            SetProperty("DefaultQueueVrssIndependentHostSpreading", value);
        }
    }

    public bool SoftwareRscEnabled
    {
        get
        {
            return GetPropertyOrDefault("SoftwareRscEnabled", defaultValue: false);
        }
        set
        {
            SetProperty("SoftwareRscEnabled", value);
        }
    }
}
