namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VssComponentSettingData")]
internal interface IVMVssIntegrationComponentSetting : IVMIntegrationComponentSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
}
