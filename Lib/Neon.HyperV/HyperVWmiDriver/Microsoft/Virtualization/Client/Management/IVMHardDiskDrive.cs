namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_DiskDrive")]
internal interface IVMHardDiskDrive : IVMDrive, IVMDevice, IVirtualizationManagementObject
{
    uint? PhysicalDiskNumber { get; }
}
