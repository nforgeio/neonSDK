namespace Microsoft.Virtualization.Client.Management;

internal class VMTimeSyncComponentSettingView : VMIntegrationComponentSettingView, IVMTimeSyncComponentSetting, IVMIntegrationComponentSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.TimeSync;
}
