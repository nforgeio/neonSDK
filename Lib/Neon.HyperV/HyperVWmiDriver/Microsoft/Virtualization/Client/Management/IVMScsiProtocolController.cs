namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_SCSIProtocolController")]
internal interface IVMScsiProtocolController : IVMDriveController, IVMDevice, IVirtualizationManagementObject
{
}
