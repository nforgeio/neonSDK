namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourceAllocationSettingData", PrimaryMapping = false)]
internal interface IVMPmemControllerSetting : IVMDriveControllerSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
}
