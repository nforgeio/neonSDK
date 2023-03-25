namespace Microsoft.Virtualization.Client.Management;

[WmiName("CIM_LogicalDevice", PrimaryMapping = false)]
internal interface IVMDrive : IVMDevice, IVirtualizationManagementObject
{
}
