namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchPortSecuritySettingData")]
internal interface IEthernetSwitchPortSecurityFeature : IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
	bool AllowMacSpoofing { get; set; }

	bool EnableDhcpGuard { get; set; }

	bool EnableRouterGuard { get; set; }

	SwitchPortMonitorMode MonitorMode { get; set; }

	bool AllowNicTeaming { get; set; }

	uint VirtualSubnetId { get; set; }

	uint DynamicIPAddressLimit { get; set; }

	uint StormLimit { get; set; }

	bool EnableIeeePriorityTag { get; set; }

	bool EnableFixedSpeed10G { get; set; }
}
