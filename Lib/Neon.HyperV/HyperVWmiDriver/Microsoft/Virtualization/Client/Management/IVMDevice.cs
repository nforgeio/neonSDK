namespace Microsoft.Virtualization.Client.Management;

[WmiName("CIM_LogicalDevice")]
internal interface IVMDevice : IVirtualizationManagementObject
{
    string FriendlyName { get; }

    [Key]
    string DeviceId { get; }

    IVMComputerSystem VirtualComputerSystem { get; }

    IVMDeviceSetting VirtualDeviceSetting { get; }
}
