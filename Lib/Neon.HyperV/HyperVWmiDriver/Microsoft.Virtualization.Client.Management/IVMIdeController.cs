namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_IDEController")]
internal interface IVMIdeController : IVMDriveController, IVMDevice, IVirtualizationManagementObject
{
}
