namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchPortTeamMappingSettingData")]
internal interface IEthernetSwitchPortTeamMappingFeature : IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
	string NetAdapterName { get; set; }

	string NetAdapterDeviceId { get; set; }

	DisableOnFailoverFeature DisableOnFailover { get; set; }
}
