namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_HeartbeatComponentSettingData")]
internal interface IVMHeartbeatComponentSetting : IVMIntegrationComponentSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
}
