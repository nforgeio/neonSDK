namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_ResourceAllocationSettingData", PrimaryMapping = false)]
internal interface IVMSyntheticKeyboardControllerSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
}
