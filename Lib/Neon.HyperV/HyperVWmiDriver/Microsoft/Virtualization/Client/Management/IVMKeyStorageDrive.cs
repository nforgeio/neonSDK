namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualLogicalUnitSettingData")]
internal interface IVMKeyStorageDrive : IVMDriveSetting, IVMDeviceSetting, IVirtualizationManagementObject, IPutableAsync, IPutable, IDeleteableAsync, IDeleteable
{
}
