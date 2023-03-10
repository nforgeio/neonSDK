namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourceAllocationSettingData", PrimaryMapping = false)]
internal interface IVMSyntheticMouseControllerSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
}
