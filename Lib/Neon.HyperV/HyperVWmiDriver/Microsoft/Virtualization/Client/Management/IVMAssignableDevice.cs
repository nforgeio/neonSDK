namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_PciExpress")]
internal interface IVMAssignableDevice : IVMDevice, IVirtualizationManagementObject
{
    string DeviceInstancePath { get; }

    string LocationPath { get; }

    IVMAssignableDevice LogicalIdentity { get; }
}
