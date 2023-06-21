namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_SerialPortSettingData")]
internal interface IVMSerialPortSetting : IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable
{
    string AttachedResourcePath { get; set; }

    bool DebuggerMode { get; set; }
}
