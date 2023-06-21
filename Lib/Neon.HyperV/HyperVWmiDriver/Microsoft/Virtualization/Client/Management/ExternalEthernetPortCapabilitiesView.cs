using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

internal class ExternalEthernetPortCapabilitiesView : View, IExternalEthernetPortCapabilities, IVirtualizationManagementObject
{
    internal static class WmiMemberNames
    {
        public const string SupportsIov = "IOVSupport";

        public const string IovSupportReasons = "IOVSupportReasons";
    }

    public IReadOnlyList<string> IovSupportReasons => GetProperty<string[]>("IOVSupportReasons");

    public bool SupportsIov => GetProperty<bool>("IOVSupport");
}
