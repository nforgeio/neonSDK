namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VssComponent")]
internal interface IVMVssComponent : IVMIntegrationComponent, IVMDevice, IVirtualizationManagementObject
{
}
