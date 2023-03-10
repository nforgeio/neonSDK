namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_BatterySettingData")]
internal interface IVMBatterySetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
}
