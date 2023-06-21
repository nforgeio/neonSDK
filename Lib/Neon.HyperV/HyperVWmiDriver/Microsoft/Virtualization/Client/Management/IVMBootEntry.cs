namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_BootSourceSettingData")]
internal interface IVMBootEntry : IVirtualizationManagementObject
{
    string Description { get; }

    BootEntryType SourceType { get; }

    string DevicePath { get; }

    string FilePath { get; }

    IVMDeviceSetting GetBootDeviceSetting();
}
