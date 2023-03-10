namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_FcPortAllocationSettingData")]
internal interface IFcPoolAllocationSetting : IGsmPoolAllocationSetting, IResourcePoolAllocationSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
}
