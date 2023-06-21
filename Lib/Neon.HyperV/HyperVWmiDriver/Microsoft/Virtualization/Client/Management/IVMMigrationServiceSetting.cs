using System.Collections.Generic;

namespace Microsoft.Virtualization.Client.Management;

[WmiName("Msvm_VirtualSystemMigrationServiceSettingData")]
internal interface IVMMigrationServiceSetting : IVirtualizationManagementObject, IPutableAsync, IPutable
{
    bool EnableVirtualSystemMigration { get; set; }

    long MaximumActiveVirtualSystemMigration { get; set; }

    long MaximumActiveStorageMigration { get; set; }

    int AuthenticationType { get; set; }

    bool EnableCompression { get; set; }

    bool EnableSmbTransport { get; set; }

    bool SmbTransportOptionAvailable { get; }

    bool CompressionOptionAvailable { get; }

    IEnumerable<IVMMigrationNetworkSetting> NetworkSettings { get; }

    IEnumerable<IVMMigrationNetworkSetting> GetUserManagedNetworkSettings();
}
