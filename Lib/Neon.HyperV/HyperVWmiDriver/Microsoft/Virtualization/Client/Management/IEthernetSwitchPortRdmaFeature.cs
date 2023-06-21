namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetSwitchPortRdmaSettingData")]
internal interface IEthernetSwitchPortRdmaFeature : IEthernetSwitchPortFeature, IEthernetFeature, IVirtualizationManagementObject
{
    int RdmaOffloadWeight { get; set; }
}
