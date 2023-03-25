namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_GuestServiceInterfaceComponent")]
internal interface IVMGuestServiceInterfaceComponent : IVMIntegrationComponent, IVMDevice, IVirtualizationManagementObject
{
	IVMGuestFileService FileService { get; }
}
