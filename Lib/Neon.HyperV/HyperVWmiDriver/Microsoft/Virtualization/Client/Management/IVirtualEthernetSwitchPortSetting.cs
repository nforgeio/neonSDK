namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EthernetPortAllocationSettingData", PrimaryMapping = false)]
internal interface IVirtualEthernetSwitchPortSetting : IEthernetPortAllocationSettingData, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    IVirtualEthernetSwitchPort VirtualSwitchPort { get; }
}
