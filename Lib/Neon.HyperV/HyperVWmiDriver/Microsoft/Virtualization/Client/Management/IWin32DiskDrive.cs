namespace Microsoft.Virtualization.Client.Management;

[WmiName("Win32_DiskDrive")]
internal interface IWin32DiskDrive : IVirtualizationManagementObject
{
    uint DiskNumber { get; }

    ushort LunId { get; }

    uint PathId { get; }

    ushort PortNumber { get; }

    ushort TargetId { get; }

    string DeviceId { get; }

    IMountedStorageImage GetMountedStorageImage();
}
