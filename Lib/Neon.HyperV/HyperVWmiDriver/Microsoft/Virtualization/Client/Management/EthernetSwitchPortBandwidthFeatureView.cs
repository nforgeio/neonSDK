namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchPortBandwidthFeatureView : EthernetSwitchPortFeatureView, IEthernetSwitchPortBandwidthFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string BurstLimit = "BurstLimit";

        public const string BurstSize = "BurstSize";

        public const string Limit = "Limit";

        public const string Reservation = "Reservation";

        public const string Weight = "Weight";
    }

    public long Limit
    {
        get
        {
            return NumberConverter.UInt64ToInt64(GetProperty<ulong>("Limit"));
        }
        set
        {
            ulong num = NumberConverter.Int64ToUInt64(value);
            SetProperty("Limit", num);
        }
    }

    public long BurstLimit
    {
        get
        {
            return NumberConverter.UInt64ToInt64(GetProperty<ulong>("BurstLimit"));
        }
        set
        {
            ulong num = NumberConverter.Int64ToUInt64(value);
            SetProperty("BurstLimit", num);
        }
    }

    public long BurstSize
    {
        get
        {
            return NumberConverter.UInt64ToInt64(GetProperty<ulong>("BurstSize"));
        }
        set
        {
            ulong num = NumberConverter.Int64ToUInt64(value);
            SetProperty("BurstSize", num);
        }
    }

    public long Reservation
    {
        get
        {
            return NumberConverter.UInt64ToInt64(GetProperty<ulong>("Reservation"));
        }
        set
        {
            ulong num = NumberConverter.Int64ToUInt64(value);
            SetProperty("Reservation", num);
        }
    }

    public long Weight
    {
        get
        {
            return NumberConverter.UInt64ToInt64(GetProperty<ulong>("Weight"));
        }
        set
        {
            ulong num = NumberConverter.Int64ToUInt64(value);
            SetProperty("Weight", num);
        }
    }

    public override EthernetFeatureType FeatureType => EthernetFeatureType.Bandwidth;
}
