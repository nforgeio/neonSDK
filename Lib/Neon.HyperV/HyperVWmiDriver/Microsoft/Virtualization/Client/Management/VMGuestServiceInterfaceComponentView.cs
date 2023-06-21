namespace Microsoft.Virtualization.Client.Management;

internal class VMGuestServiceInterfaceComponentView : VMIntegrationComponentView, IVMGuestServiceInterfaceComponent, IVMIntegrationComponent, IVMDevice, IVirtualizationManagementObject
{
    public IVMGuestFileService FileService => GetRelatedObject<IVMGuestFileService>(base.Associations.GuestServiceInterfaceComponentToGuestFileService);
}
