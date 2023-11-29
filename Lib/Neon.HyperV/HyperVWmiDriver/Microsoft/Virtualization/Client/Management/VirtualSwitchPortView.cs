namespace Microsoft.Virtualization.Client.Management;

internal class VirtualSwitchPortView : View, IVirtualSwitchPort, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string FriendlyName = "ElementName";
    }

    public string Name => GetProperty<string>("Name");

    public string FriendlyName => GetProperty<string>("ElementName");
}
