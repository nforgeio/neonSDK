namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_PersistentMemoryController")]
internal interface IVMPmemController : IVMDriveController, IVMDevice, IVirtualizationManagementObject
{
}
