namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_EmulatedEthernetPortSettingData")]
internal interface IEmulatedEthernetPortSetting : IEthernetPortSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
}
