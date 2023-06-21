namespace Microsoft.Virtualization.Client.Management;

internal class VMHardDiskDriveView : VMDriveView, IVMHardDiskDrive, IVMDrive, IVMDevice, IVirtualizationManagementObject
{
    internal static class WmiPropertyNames
    {
        public const string PhysicalDiskNumber = "DriveNumber";
    }

    public uint? PhysicalDiskNumber => GetProperty<uint?>("DriveNumber");
}
