namespace Microsoft.Virtualization.Client.Management;

internal class VMGuestServiceInterfaceComponentSettingView : VMIntegrationComponentSettingView, IVMGuestServiceInterfaceComponentSetting, IVMIntegrationComponentSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.GuestServiceInterface;
}
