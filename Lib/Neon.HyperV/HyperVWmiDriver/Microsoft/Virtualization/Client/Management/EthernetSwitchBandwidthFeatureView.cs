namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchBandwidthFeatureView : EthernetSwitchFeatureView, IEthernetSwitchBandwidthFeature, IEthernetSwitchFeature, IEthernetFeature, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string DefaultFlowReservation = "DefaultFlowReservation";

		public const string DefaultFlowWeight = "DefaultFlowWeight";
	}

	public override EthernetFeatureType FeatureType => EthernetFeatureType.SwitchBandwidth;

	public long DefaultFlowReservation
	{
		get
		{
			return GetProperty<long>("DefaultFlowReservation");
		}
		set
		{
			SetProperty("DefaultFlowReservation", value);
		}
	}

	public long DefaultFlowWeight
	{
		get
		{
			return GetProperty<long>("DefaultFlowWeight");
		}
		set
		{
			SetProperty("DefaultFlowWeight", value);
		}
	}
}
