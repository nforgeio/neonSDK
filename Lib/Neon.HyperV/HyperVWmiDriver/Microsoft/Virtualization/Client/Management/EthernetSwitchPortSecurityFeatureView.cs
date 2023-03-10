namespace Microsoft.Virtualization.Client.Management;

internal sealed class EthernetSwitchPortSecurityFeatureView : EthernetSwitchPortFeatureView, IEthernetSwitchPortSecurityFeature, IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
	internal static class WmiMemberNames
	{
		public const string AllowMacSpoofing = "AllowMacSpoofing";

		public const string EnableDhcpGuard = "EnableDhcpGuard";

		public const string EnableRouterGuard = "EnableRouterGuard";

		public const string MonitorMode = "MonitorMode";

		public const string AllowNicTeaming = "AllowTeaming";

		public const string AllowIeeePriorityTag = "AllowIeeePriorityTag";

		public const string DynamicIPAddressLimit = "DynamicIPAddressLimit";

		public const string StormLimit = "StormLimit";

		public const string VirtualSubnetId = "VirtualSubnetId";

		public const string EnableFixedSpeed10G = "EnableFixSpeed10G";
	}

	public bool AllowMacSpoofing
	{
		get
		{
			return GetProperty<bool>("AllowMacSpoofing");
		}
		set
		{
			SetProperty("AllowMacSpoofing", value);
		}
	}

	public bool EnableDhcpGuard
	{
		get
		{
			return GetProperty<bool>("EnableDhcpGuard");
		}
		set
		{
			SetProperty("EnableDhcpGuard", value);
		}
	}

	public bool EnableRouterGuard
	{
		get
		{
			return GetProperty<bool>("EnableRouterGuard");
		}
		set
		{
			SetProperty("EnableRouterGuard", value);
		}
	}

	public SwitchPortMonitorMode MonitorMode
	{
		get
		{
			return (SwitchPortMonitorMode)GetProperty<byte>("MonitorMode");
		}
		set
		{
			byte b = (byte)value;
			SetProperty("MonitorMode", b);
		}
	}

	public bool AllowNicTeaming
	{
		get
		{
			return GetProperty<bool>("AllowTeaming");
		}
		set
		{
			SetProperty("AllowTeaming", value);
		}
	}

	public uint VirtualSubnetId
	{
		get
		{
			return GetProperty<uint>("VirtualSubnetId");
		}
		set
		{
			SetProperty("VirtualSubnetId", value);
		}
	}

	public uint DynamicIPAddressLimit
	{
		get
		{
			return GetProperty<uint>("DynamicIPAddressLimit");
		}
		set
		{
			SetProperty("DynamicIPAddressLimit", value);
		}
	}

	public uint StormLimit
	{
		get
		{
			return GetProperty<uint>("StormLimit");
		}
		set
		{
			SetProperty("StormLimit", value);
		}
	}

	public bool EnableFixedSpeed10G
	{
		get
		{
			return GetProperty<bool>("EnableFixSpeed10G");
		}
		set
		{
			SetProperty("EnableFixSpeed10G", value);
		}
	}

	public bool EnableIeeePriorityTag
	{
		get
		{
			return GetProperty<bool>("AllowIeeePriorityTag");
		}
		set
		{
			SetProperty("AllowIeeePriorityTag", value);
		}
	}

	public override EthernetFeatureType FeatureType => EthernetFeatureType.Security;
}
