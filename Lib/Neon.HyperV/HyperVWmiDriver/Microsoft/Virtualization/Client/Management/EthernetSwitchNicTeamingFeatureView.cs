namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchNicTeamingFeatureView : EthernetSwitchFeatureView, IEthernetSwitchNicTeamingFeature, IEthernetSwitchFeature, IEthernetFeature, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string TeamingMode = "TeamingMode";

        public const string LoadBalancingAlgorithm = "LoadBalancingAlgorithm";
    }

    public override EthernetFeatureType FeatureType => EthernetFeatureType.SwitchNicTeaming;

    public uint TeamingMode
    {
        get
        {
            return GetProperty<uint>("TeamingMode");
        }
        set
        {
            SetProperty("TeamingMode", value);
        }
    }

    public uint LoadBalancingAlgorithm
    {
        get
        {
            return GetProperty<uint>("LoadBalancingAlgorithm");
        }
        set
        {
            SetProperty("LoadBalancingAlgorithm", value);
        }
    }
}
