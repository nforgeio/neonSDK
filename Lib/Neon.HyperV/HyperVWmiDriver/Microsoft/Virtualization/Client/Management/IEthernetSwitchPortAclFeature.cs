namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchPortAclSettingData")]
internal interface IEthernetSwitchPortAclFeature : IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject, IMetricMeasurableElement
{
	AclAction Action { get; set; }

	AclDirection Direction { get; set; }

	string LocalAddress { get; set; }

	byte LocalAddressPrefixLength { get; set; }

	string RemoteAddress { get; set; }

	byte RemoteAddressPrefixLength { get; set; }

	bool IsRemote { get; set; }

	AclAddressType AddressType { get; set; }
}
