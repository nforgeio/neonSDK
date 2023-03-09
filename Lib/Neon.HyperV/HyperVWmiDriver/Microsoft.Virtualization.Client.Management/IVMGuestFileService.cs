namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_GuestFileService")]
internal interface IVMGuestFileService : IVMIntegrationComponent, IVMDevice, IVirtualizationManagementObject
{
	IVMTask BeginCopyFilesToGuest(string sourcePath, string destinationPath, bool overwriteExisting, bool createFullPath);

	void EndCopyFilesToGuest(IVMTask task);
}
