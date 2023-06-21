namespace Microsoft.Virtualization.Client.Management;

internal interface IVirtualSwitch : IVirtualizationManagementObject, IPutable
{
    [Key]
    string InstanceId { get; }

    string FriendlyName { get; }
}
