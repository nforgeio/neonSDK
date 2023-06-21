namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_GuestNetworkAdapterConfiguration")]
internal interface IGuestNetworkAdapterConfiguration : IVirtualizationManagementObject
{
    string[] IPAddresses { get; }
}
