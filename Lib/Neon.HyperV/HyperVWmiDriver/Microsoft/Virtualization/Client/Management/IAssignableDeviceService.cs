namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_AssignableDeviceService")]
internal interface IAssignableDeviceService : IVirtualizationManagementObject
{
    void DismountAssignableDevice(VMDismountSetting dismountSettingData, out string newDeviceInstanceId);

    void MountAssignableDevice(string instanceId, string locationPath, out string newDeviceInstanceId);
}
