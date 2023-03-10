namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_TimeSyncComponentSettingData")]
internal interface IVMTimeSyncComponentSetting : IVMIntegrationComponentSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
}
