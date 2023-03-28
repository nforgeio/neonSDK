namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourceAllocationSettingData", PrimaryMapping = false)]
internal interface IVMScsiControllerSetting : IVMDriveControllerSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
}
