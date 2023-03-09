namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_SyntheticKeyboard")]
internal interface IVMSyntheticKeyboardController : IVMDevice, IVirtualizationManagementObject
{
}
[WmiName("Msvm_ResourceAllocationSettingData", PrimaryMapping = false)]
internal interface IVMSyntheticKeyboardControllerSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
}
