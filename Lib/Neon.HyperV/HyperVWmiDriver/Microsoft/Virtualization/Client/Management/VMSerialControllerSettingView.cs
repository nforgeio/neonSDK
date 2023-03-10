namespace Microsoft.Virtualization.Client.Management;

internal class VMSerialControllerSettingView : VMDeviceSettingView, IVMSerialControllerSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
	public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.SerialController;
}
