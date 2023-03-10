namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_MountedStorageImage")]
internal interface IMountedStorageImage : IVirtualizationManagementObject
{
	string ImagePath { get; }

	byte LunId { get; }

	byte PathId { get; }

	byte PortNumber { get; }

	byte TargetId { get; }

	IWin32DiskDrive GetDiskDrive();

	IVMTask BeginDetachVirtualHardDisk();

	void EndDetachVirtualHardDisk(IVMTask task);
}
