namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchPortTeamMappingFeatureView : EthernetSwitchPortFeatureView, IEthernetSwitchPortTeamMappingFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string NetAdapterName = "NetAdapterName";

		public const string NetAdapterDeviceId = "NetAdapterDeviceId";

		public const string DisableOnFailover = "DisableOnFailover";
	}

	public string NetAdapterName
	{
		get
		{
			return GetProperty<string>("NetAdapterName");
		}
		set
		{
			SetProperty("NetAdapterName", value);
		}
	}

	public string NetAdapterDeviceId
	{
		get
		{
			return GetProperty<string>("NetAdapterDeviceId");
		}
		set
		{
			SetProperty("NetAdapterDeviceId", value);
		}
	}

	public DisableOnFailoverFeature DisableOnFailover
	{
		get
		{
			return (DisableOnFailoverFeature)GetPropertyOrDefault("DisableOnFailover", 0u);
		}
		set
		{
			SetProperty("DisableOnFailover", (uint)value);
		}
	}

	public override EthernetFeatureType FeatureType => EthernetFeatureType.TeamMapping;
}
