namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_GuestServiceInterfaceComponentSettingData")]
internal interface IVMGuestServiceInterfaceComponentSetting : IVMIntegrationComponentSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
}
