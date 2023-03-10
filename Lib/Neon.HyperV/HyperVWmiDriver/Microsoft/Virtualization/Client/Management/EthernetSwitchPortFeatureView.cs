namespace Microsoft.Virtualization.Client.Management;

internal class EthernetSwitchPortFeatureView : EthernetFeatureView, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
	public override EthernetFeatureType FeatureType => EthernetFeatureType.Unknown;
}
