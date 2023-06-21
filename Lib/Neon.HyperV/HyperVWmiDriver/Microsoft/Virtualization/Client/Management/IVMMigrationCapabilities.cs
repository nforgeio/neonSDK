using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualSystemMigrationCapabilities")]
internal interface IVMMigrationCapabilities : IVirtualizationManagementObject
{
    IEnumerable<IVMMigrationSetting> MigrationSettings { get; }
}
