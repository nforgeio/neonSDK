namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchPortIsolationFeatureView : EthernetSwitchPortFeatureView, IEthernetSwitchPortIsolationFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string AllowUntaggedTraffic = "AllowUntaggedTraffic";

        public const string DefaultIsolationID = "DefaultIsolationID";

        public const string IsMultiTenantStackEnabled = "EnableMultiTenantStack";

        public const string IsolationMode = "IsolationMode";
    }

    public override EthernetFeatureType FeatureType => EthernetFeatureType.Isolation;

    public bool AllowUntaggedTraffic
    {
        get
        {
            return GetProperty<bool>("AllowUntaggedTraffic");
        }
        set
        {
            SetProperty("AllowUntaggedTraffic", value);
        }
    }

    public int DefaultIsolationID
    {
        get
        {
            return NumberConverter.UInt32ToInt32(GetProperty<uint>("DefaultIsolationID"));
        }
        set
        {
            uint num = NumberConverter.Int32ToUInt32(value);
            SetProperty("DefaultIsolationID", num);
        }
    }

    public bool IsMultiTenantStackEnabled
    {
        get
        {
            return GetProperty<bool>("EnableMultiTenantStack");
        }
        set
        {
            SetProperty("EnableMultiTenantStack", value);
        }
    }

    public IsolationMode IsolationMode
    {
        get
        {
            return (IsolationMode)GetProperty<uint>("IsolationMode");
        }
        set
        {
            SetProperty("IsolationMode", (uint)value);
        }
    }
}
