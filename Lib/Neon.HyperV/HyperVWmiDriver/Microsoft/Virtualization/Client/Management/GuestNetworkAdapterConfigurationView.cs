namespace Microsoft.Virtualization.Client.Management;

internal class GuestNetworkAdapterConfigurationView : View, IGuestNetworkAdapterConfiguration, IVirtualizationManagementObject
{
    internal static class WmiPropertyNames
    {
        public const string IPAddresses = "IPAddresses";
    }

    public string[] IPAddresses => GetProperty<string[]>("IPAddresses");
}
