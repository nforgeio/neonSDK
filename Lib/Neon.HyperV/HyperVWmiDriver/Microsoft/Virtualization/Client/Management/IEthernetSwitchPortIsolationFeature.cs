namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchPortIsolationSettingData")]
internal interface IEthernetSwitchPortIsolationFeature : IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
	IsolationMode IsolationMode { get; set; }

	bool AllowUntaggedTraffic { get; set; }

	int DefaultIsolationID { get; set; }

	bool IsMultiTenantStackEnabled { get; set; }
}
