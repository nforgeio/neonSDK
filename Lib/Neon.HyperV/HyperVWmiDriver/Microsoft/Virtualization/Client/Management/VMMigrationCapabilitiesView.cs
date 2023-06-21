using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualSystemMigrationCapabilities")]
internal class VMMigrationCapabilitiesView : View, IVMMigrationCapabilities, IVirtualizationManagementObject
{
    public IEnumerable<IVMMigrationSetting> MigrationSettings => GetRelatedObjects<IVMMigrationSetting>(base.Associations.SettingsDefineCapabilities);
}
