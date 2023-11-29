using Microsoft.Management.Infrastructure;

namespace Microsoft.Virtualization.Client.Management;

internal class SettingsDefineCapabilitiesView : View, ISettingsDefineCapabilities, IVirtualizationManagementObject
{
    internal static class WmiPropertyNames
    {
        public const string PartComponent = "PartComponent";

        public const string ValueRole = "ValueRole";

        public const string ValueRange = "ValueRange";

        public const string SupportStatement = "SupportStatement";
    }

    public IVirtualizationManagementObject PartComponent
    {
        get
        {
            WmiObjectPath path = new WmiObjectPath(base.ManagementPath.ServerName, base.ManagementPath.NamespaceName, GetProperty<CimInstance>("PartComponent").ToICimInstance());
            return ObjectLocator.GetVirtualizationManagementObject(base.Server, path);
        }
    }

    public CapabilitiesValueRole ValueRole => (CapabilitiesValueRole)GetProperty<ushort>("ValueRole");

    public CapabilitiesValueRange ValueRange => (CapabilitiesValueRange)GetProperty<ushort>("ValueRange");

    public CapabilitiesSupportStatement SupportStatement => (CapabilitiesSupportStatement)GetPropertyOrDefault("SupportStatement", (ushort)0);
}
