namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchPortBandwidthSettingData")]
internal interface IEthernetSwitchPortBandwidthFeature : IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
    long BurstLimit { get; set; }

    long BurstSize { get; set; }

    long Limit { get; set; }

    long Reservation { get; set; }

    long Weight { get; set; }
}
