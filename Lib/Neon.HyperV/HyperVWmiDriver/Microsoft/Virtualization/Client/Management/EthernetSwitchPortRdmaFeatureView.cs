namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchPortRdmaFeatureView : EthernetSwitchPortFeatureView, IEthernetSwitchPortRdmaFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string RdmaOffloadWeight = "RdmaOffloadWeight";
    }

    public int RdmaOffloadWeight
    {
        get
        {
            return NumberConverter.UInt32ToInt32(GetProperty<uint>("RdmaOffloadWeight"));
        }
        set
        {
            uint num = NumberConverter.Int32ToUInt32(value);
            SetProperty("RdmaOffloadWeight", num);
        }
    }

    public override EthernetFeatureType FeatureType => EthernetFeatureType.Rdma;
}
