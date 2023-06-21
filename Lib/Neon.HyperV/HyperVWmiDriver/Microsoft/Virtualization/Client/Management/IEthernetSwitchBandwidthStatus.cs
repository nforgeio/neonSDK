namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchBandwidthData")]
internal interface IEthernetSwitchBandwidthStatus : IEthernetSwitchStatus, IEthernetStatus, IVirtualizationManagementObject
{
    ulong DefaultFlowReservation { get; }

    uint DefaultFlowReservationPercentage { get; }

    ulong DefaultFlowWeight { get; }
}
