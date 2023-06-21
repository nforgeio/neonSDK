using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Virtualization.Client.Management;

internal class VirtualEthernetSwitchManagementCapabilitiesView : View, IVirtualEthernetSwitchManagementCapabilities, IVirtualizationManagementObject
{
    private static class WmiMemberNames
    {
        public const string IOVSupport = "IOVSupport";

        public const string IOVSupportReasons = "IOVSupportReasons";
    }

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "IOV", Justification = "This is by spec.")]
    public bool IOVSupport => GetProperty<bool>("IOVSupport");

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "IOV", Justification = "This is by spec.")]
    public string[] IOVSupportReasons => GetProperty<string[]>("IOVSupportReasons");
}
