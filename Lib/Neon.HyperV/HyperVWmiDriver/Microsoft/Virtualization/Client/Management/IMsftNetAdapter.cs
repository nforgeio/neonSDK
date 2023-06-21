namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msft_NetAdapter")]
internal interface IMsftNetAdapter : IVirtualizationManagementObject
{
    string DeviceId { get; }
}
