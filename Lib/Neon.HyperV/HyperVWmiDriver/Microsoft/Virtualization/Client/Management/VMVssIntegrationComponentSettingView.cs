namespace Microsoft.Virtualization.Client.Management;

internal class VMVssIntegrationComponentSettingView : VMIntegrationComponentSettingView, IVMVssIntegrationComponentSetting, IVMIntegrationComponentSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    public override VMDeviceSettingType VMDeviceSettingType => VMDeviceSettingType.VssIntegration;
}
