namespace Microsoft.Virtualization.Client.Management;

internal class EthernetSwitchFeatureView : EthernetFeatureView, IEthernetSwitchFeature, IEthernetFeature, IVirtualizationManagementObject
{
    public override EthernetFeatureType FeatureType => EthernetFeatureType.Unknown;
}
