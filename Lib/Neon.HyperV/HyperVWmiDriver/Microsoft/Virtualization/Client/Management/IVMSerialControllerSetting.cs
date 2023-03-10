namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_SerialControllerSettingData")]
internal interface IVMSerialControllerSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
}
