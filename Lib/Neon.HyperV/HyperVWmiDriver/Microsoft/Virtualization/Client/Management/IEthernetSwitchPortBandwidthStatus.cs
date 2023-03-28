namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchPortBandwidthData")]
internal interface IEthernetSwitchPortBandwidthStatus : IEthernetPortStatus, IEthernetStatus, IVirtualizationManagementObject
{
	uint CurrentBandwidthReservationPercentage { get; }
}
