namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualEthernetSwitchBandwidthSettingData")]
internal interface IEthernetSwitchBandwidthFeature : IEthernetSwitchFeature, IEthernetFeature, IVirtualizationManagementObject
{
	long DefaultFlowReservation { get; set; }

	long DefaultFlowWeight { get; set; }
}
