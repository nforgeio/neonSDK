namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_RdvComponentSettingData")]
internal interface IVMRdvComponentSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
}
