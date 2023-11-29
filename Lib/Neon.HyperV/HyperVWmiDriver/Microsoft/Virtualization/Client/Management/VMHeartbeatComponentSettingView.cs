namespace Microsoft.Virtualization.Client.Management;

internal class VMHeartbeatComponentSettingView : VMIntegrationComponentSettingView, IVMHeartbeatComponentSetting, IVMIntegrationComponentSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.Heartbeat;
}
