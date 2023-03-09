namespace Microsoft.Virtualization.Client.Management;

internal class VMShutdownComponentSettingView : VMIntegrationComponentSettingView, IVMShutdownComponentSetting, IVMIntegrationComponentSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.Shutdown;
}
